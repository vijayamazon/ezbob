using System;
using System.Runtime.Serialization;

namespace EzBob.Web.Areas.Customer.Controllers.Exceptions
{
    [Serializable]
    public class NameValidationException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NameValidationException()
        {
        }

        public NameValidationException(string message) : base(message)
        {
        }

        public NameValidationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NameValidationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}