namespace Nasurino.SmartWallet.Service.Exceptions;

public sealed class CodedServiceException : ServiceException
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public CodedServiceException(string errorCode, string message, int statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
