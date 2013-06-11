using System;

namespace MailApi.Model
{
    public class MandrillException : Exception
    {
        public ErrorResponseModel Error { get; private set; }

        public MandrillException() { }

        public MandrillException(string message)
            : base(message)
        {
        }

        public MandrillException(ErrorResponseModel error, string message)
            : base(message)
        {
            Error = error;
        }

        public MandrillException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
