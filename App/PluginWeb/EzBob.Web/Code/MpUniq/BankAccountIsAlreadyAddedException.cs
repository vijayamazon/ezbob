using System;
using System.Runtime.Serialization;

namespace EzBob.Web.Code.MpUniq
{
    [Serializable]
    public class BankAccountIsAlreadyAddedException : Exception
    {
        public BankAccountIsAlreadyAddedException()
        {
        }

        public BankAccountIsAlreadyAddedException(string message) : base(message)
        {
        }

        public BankAccountIsAlreadyAddedException(string message, Exception inner) : base(message, inner)
        {
        }

		protected BankAccountIsAlreadyAddedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}