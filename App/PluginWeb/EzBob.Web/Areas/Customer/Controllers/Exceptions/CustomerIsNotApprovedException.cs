using System;
using System.Runtime.Serialization;

namespace EzBob.Web.Areas.Customer.Controllers.Exceptions
{
    [Serializable]
    public class CustomerIsNotApprovedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CustomerIsNotApprovedException()
        {
        }

        public CustomerIsNotApprovedException(string message) : base(message)
        {
        }

        public CustomerIsNotApprovedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CustomerIsNotApprovedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}