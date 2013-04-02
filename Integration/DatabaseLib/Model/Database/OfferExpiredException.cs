using System;
using System.Runtime.Serialization;

namespace EZBob.DatabaseLib.Model.Database
{
    [Serializable]
    public class OfferExpiredException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public OfferExpiredException()
        {
        }

        public OfferExpiredException(string message) : base(message)
        {
        }

        public OfferExpiredException(string message, Exception inner) : base(message, inner)
        {
        }

        protected OfferExpiredException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}