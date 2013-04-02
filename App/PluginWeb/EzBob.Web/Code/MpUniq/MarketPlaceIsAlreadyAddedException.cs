using System;
using System.Runtime.Serialization;

namespace EzBob.Web.Code.MpUniq
{
    [Serializable]
    public class MarketPlaceIsAlreadyAddedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MarketPlaceIsAlreadyAddedException()
        {
        }

        public MarketPlaceIsAlreadyAddedException(string message) : base(message)
        {
        }

        public MarketPlaceIsAlreadyAddedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MarketPlaceIsAlreadyAddedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}