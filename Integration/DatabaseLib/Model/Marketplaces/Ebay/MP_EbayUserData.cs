using System;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserData
	{
		public virtual int Id { get; set; }
		public virtual int CustomerMarketPlaceId { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual string BillingEmail { get; set; }
		public virtual bool? eBayGoodStanding { get; set; }
		public virtual string EIASToken { get; set; }
		public virtual string EMail { get; set; }
		public virtual bool? FeedbackPrivate { get; set; }
		public virtual int? FeedbackScore { get; set; }
		public virtual string FeedbackRatingStar { get; set; }
		public virtual bool? IDVerified { get; set; }
		public virtual bool? NewUser { get; set; }
		public virtual string PayPalAccountStatus { get; set; }
		public virtual string PayPalAccountType { get; set; }
		public virtual bool? QualifiesForSelling { get; set; }
		public virtual MP_EbayUserAddressData RegistrationAddress { get; set; }
		public virtual DateTime? RegistrationDate { get; set; }
		public virtual EbaySellerInfo SellerInfo { get; set; }
		public virtual string Site { get; set; }
		public virtual string SkypeID { get; set; }
		public virtual string UserID { get; set; }
		public virtual bool? IDChanged { get; set; }
		public virtual DateTime? IDLastChanged { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}

	public class EbaySellerInfo
	{
		public bool? SellerInfoQualifiesForB2BVAT { get; set; }
		public string SellerInfoSellerBusinessType { get; set; }
		public MP_EbayUserAddressData SellerPaymentAddress { get; set; }
		public bool? SellerInfoStoreOwner { get; set; }
		public string SellerInfoStoreSite { get; set; }
		public string SellerInfoStoreURL { get; set; }
		public bool? SellerInfoTopRatedSeller { get; set; }
		public string SellerInfoTopRatedProgram { get; set; }
	}	
}
