namespace CvShortlist.POCOs;

public record DataVault(byte[] InitializationVector, byte[] Salt, byte[] EncryptedData);
