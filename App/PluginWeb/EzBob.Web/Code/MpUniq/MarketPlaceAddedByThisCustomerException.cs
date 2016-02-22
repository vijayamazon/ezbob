namespace EzBob.Web.Code.MpUniq {
	using System;
	using System.Runtime.Serialization;

// For guidelines regarding the creation of new exception types, see
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
// and
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp

	[Serializable]
	public class MarketPlaceAddedByThisCustomerException : Exception {
		public MarketPlaceAddedByThisCustomerException() { }

		public MarketPlaceAddedByThisCustomerException(string message) : base(message) { }

		public MarketPlaceAddedByThisCustomerException(string message, Exception inner) : base(message, inner) { }

		protected MarketPlaceAddedByThisCustomerException(
			SerializationInfo info,
			StreamingContext context
		) : base(info, context) {
		}
	} // class MarketPlaceAddedByThisCustomerException
} // namespace
