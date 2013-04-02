using System;
using EZBob.DatabaseLib.DatabaseWrapper.Order;

namespace EZBob.DatabaseLib.DatabaseWrapper.UsersData
{
	public interface IDatabaseEbayUserData
	{
		DateTime SubmittedDate { get; }
		bool? IDVerified { get; }
		bool? NewUser { get; }
		string PayPalAccountStatus { get; }
		string PayPalAccountType { get; }
		bool? QualifiesForSelling { get; }
		bool QualifiesForB2BVAT { get; }
		string SellerBusinessType { get; }
		bool StoreOwner { get; }
		string StoreSite { get; }
		string StoreURL { get; }
		bool? TopRatedSeller { get; }
		string Site { get; }
		string SkypeID { get; }
		bool? FeedbackPrivate { get; }
		string EIASToken { get; }
		bool? eBayGoodStanding { get; }
		string UserID { get; }
		int? FeedbackScore { get; }
		string FeedbackRatingStar { get; }
		string EMail { get; }
		DateTime? RegistrationDate { get; }
		string BillingEmail { get; }
		bool? IDChanged { get; }
		DateTime? IDLastChanged { get; }
		string TopRatedProgram { get; }
		DatabaseShipingAddress RegistrationAddress { get; }
		DatabaseShipingAddress SellerPaymentAddress { get; }
	}
}