using System;
using System.Runtime.Serialization;

namespace EzBob.Web.Code.Email
{
    [Serializable]
    public class EmailConfirmationRequestNotFoundException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EmailConfirmationRequestNotFoundException()
        {
        }

        public EmailConfirmationRequestNotFoundException(string message) : base(message)
        {
        }

        public EmailConfirmationRequestNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EmailConfirmationRequestNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}