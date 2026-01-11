namespace CvShortlist.Services.Contracts;

public interface IDataCryptoService
{
	byte[] EncryptData(byte[] data, string passkey);

	byte[] DecryptData(byte[] byteArray, string passkey);
}
