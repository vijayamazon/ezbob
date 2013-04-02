using System;
using System.Runtime.Serialization;

namespace EzBob.Web.Code.MpUniq
{
    [Serializable]
    public class MarketPlaceAddedByThisCustomerException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MarketPlaceAddedByThisCustomerException()
        {
        }

        public MarketPlaceAddedByThisCustomerException(string message) : base(message)
        {
        }

        public MarketPlaceAddedByThisCustomerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MarketPlaceAddedByThisCustomerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}