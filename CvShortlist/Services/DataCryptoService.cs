using System.Security.Cryptography;
using CvShortlist.POCOs;
using CvShortlist.Services.Contracts;

namespace CvShortlist.Services;

public class DataCryptoService : IDataCryptoService
{
	public byte[] EncryptData(byte[] data, string passkey)
	{
		try
		{
			var initializationVector = GenerateInitializationVector();
			var salt = GenerateSalt();

			var encryptionKey = GetEncryptionKey(passkey, salt);

			var encryptedData = ExecuteCryptoTransform(
				data, initializationVector, encryptionKey, aes => aes.CreateEncryptor());

			var dataVault = new DataVault(initializationVector, salt, encryptedData);
			var byteArray = SerializeDataVaultToByteArray(dataVault);

			return byteArray;
		}
		catch (Exception ex)
		{
			throw new CryptographicException(EncryptionError, ex);
		}
	}

	public byte[] DecryptData(byte[] byteArray, string passkey)
	{
		try
		{
			var dataVault = DeserializeDataVaultFromByteArray(byteArray);

			var encryptionKey = GetEncryptionKey(passkey, dataVault.Salt);

			var data = ExecuteCryptoTransform(
				dataVault.EncryptedData, dataVault.InitializationVector, encryptionKey, aes => aes.CreateDecryptor());

			return data;
		}
		catch (Exception ex)
		{
			throw new CryptographicException(DecryptionError, ex);
		}
	}

	private const int KeySizeInBits = 256;
	private const int KeySizeInBytes = KeySizeInBits / 8;

	private const int InitializationVectorSizeInBytes = 16;
	private const int SaltSizeInBytes = 16;

	private const int PasskeyIterations = 600_000;

	private const string EncryptionError = "Data encryption error";
	private const string DecryptionError = "Data decryption error";

	private static byte[] GenerateInitializationVector()
		=> RandomNumberGenerator.GetBytes(InitializationVectorSizeInBytes);

	private static byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(SaltSizeInBytes);

	private static byte[] ExecuteCryptoTransform(
		byte[] data,
		byte[] initializationVector,
		byte[] encryptionKey,
		Func<Aes, ICryptoTransform> createCryptoTransform)
	{
		using var aes = Aes.Create();
		aes.IV = initializationVector;
		aes.KeySize = KeySizeInBits;
		aes.Key = encryptionKey;

		using var dataStream = new MemoryStream();
		using var encryptor = createCryptoTransform(aes);
		using (var cryptoStream = new CryptoStream(dataStream, encryptor, CryptoStreamMode.Write))
		{
			cryptoStream.Write(data);
		}

		var resultData = dataStream.ToArray();
		return resultData;
	}

	private static byte[] GetEncryptionKey(string passkey, byte[] salt)
	{
		var encryptionKey = Rfc2898DeriveBytes.Pbkdf2(
			passkey, salt, PasskeyIterations, HashAlgorithmName.SHA256, KeySizeInBytes);

		return encryptionKey;
	}

	private static byte[] SerializeDataVaultToByteArray(DataVault dataVault)
	{
		using var outputStream = new MemoryStream();
		using var binaryWriter = new BinaryWriter(outputStream);

		binaryWriter.Write(dataVault.InitializationVector.Length);
		binaryWriter.Write(dataVault.InitializationVector);

		binaryWriter.Write(dataVault.Salt.Length);
		binaryWriter.Write(dataVault.Salt);

		binaryWriter.Write(dataVault.EncryptedData.Length);
		binaryWriter.Write(dataVault.EncryptedData);

		var outputStreamData = outputStream.ToArray();
		return outputStreamData;
	}

	private static DataVault DeserializeDataVaultFromByteArray(byte[] binaryData)
	{
		using var inputStream = new MemoryStream(binaryData);
		using var binaryReader = new BinaryReader(inputStream);

		var initializationVectorLength = binaryReader.ReadInt32();
		var initializationVector = binaryReader.ReadBytes(initializationVectorLength);

		var saltLength = binaryReader.ReadInt32();
		var salt = binaryReader.ReadBytes(saltLength);

		var encryptedDataLength = binaryReader.ReadInt32();
		var encryptedData = binaryReader.ReadBytes(encryptedDataLength);

		var dataVault = new DataVault(initializationVector, salt, encryptedData);
		return dataVault;
	}
}
