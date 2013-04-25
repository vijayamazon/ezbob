using System;
using System.Runtime.Serialization;

namespace EzBob.Web.Areas.Customer.Controllers
{
    [Serializable]
    public class LoanDelayViolationException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public LoanDelayViolationException()
        {
        }

        public LoanDelayViolationException(string message) : base(message)
        {
        }

        public LoanDelayViolationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LoanDelayViolationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}