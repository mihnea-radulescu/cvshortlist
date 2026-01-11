using System.Security.Cryptography;
using System.Text;
using Xunit;
using CvShortlist.Services;

namespace CvShortlist.Test;

public class DataCryptoServiceTest
{
	public DataCryptoServiceTest()
	{
		_dataCryptoService = new DataCryptoService();
	}

	[Fact]
	public void EncryptDecrypt_ShortMatchingPasskey_ReturnsInitialData()
	{
		// Arrange
		const string textReference = "plain text";
		var dataReference = GetByteArray(textReference);

		const string passkey = "passkey";

		// Act
		var encryptedData = _dataCryptoService.EncryptData(dataReference, passkey);
		var data = _dataCryptoService.DecryptData(encryptedData, passkey);
		var text = GetString(data);

		// Assert
		Assert.Equal(textReference, text);
	}

	[Fact]
	public void EncryptDecrypt_LongMatchingPasskey_ReturnsInitialData()
	{
		// Arrange
		const string textReference = "plain text";
		var dataReference = GetByteArray(textReference);

		const string passkey = "passkey_passkey_passkey_passkey";

		// Act
		var encryptedData = _dataCryptoService.EncryptData(dataReference, passkey);
		var data = _dataCryptoService.DecryptData(encryptedData, passkey);
		var text = GetString(data);

		// Assert
		Assert.Equal(textReference, text);
	}

	[Fact]
	public void EncryptDecrypt_ShortNotMatchingPasskey_ThrowsCryptographicException()
	{
		// Arrange
		const string textReference = "plain text";
		var dataReference = GetByteArray(textReference);

		const string encryptionPasskey = "encryption passkey";
		const string decryptionPasskey = "decryption passkey";

		// Act and Assert
		var encryptedData = _dataCryptoService.EncryptData(dataReference, encryptionPasskey);

		Assert.Throws<CryptographicException>(() => _dataCryptoService.DecryptData(encryptedData, decryptionPasskey));
	}

	[Fact]
	public void EncryptDecrypt_LongNotMatchingPasskey_ThrowsCryptographicException()
	{
		// Arrange
		const string textReference = "plain text";
		var dataReference = GetByteArray(textReference);

		const string encryptionPasskey = "encryption passkey_passkey_passkey_passkey";
		const string decryptionPasskey = "decryption passkey_passkey_passkey_passkey";

		// Act and Assert
		var encryptedData = _dataCryptoService.EncryptData(dataReference, encryptionPasskey);

		Assert.Throws<CryptographicException>(() => _dataCryptoService.DecryptData(encryptedData, decryptionPasskey));
	}

	private readonly DataCryptoService _dataCryptoService;

	private static readonly Encoding DefaultEncoding = Encoding.UTF8;

	private static byte[] GetByteArray(string text) => DefaultEncoding.GetBytes(text);
	private static string GetString(byte[] data) => DefaultEncoding.GetString(data);
}
