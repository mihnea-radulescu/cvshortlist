namespace CvShortlist.SelfHosted.POCOs;

public class ReadFileMessage
{
	public ReadFileMessage(string messageType, string message)
	{
		MessageType = messageType;
		Message = message;
	}

	public string MessageType { get; }
	public string Message { get; }
}
