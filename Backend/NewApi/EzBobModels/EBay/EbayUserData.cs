namespace EzBobModels.EBay {
    using System;

    /// <summary>
    ///DTO for MP_EbayUserData
    /// </summary>
    public class EbayUserData {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public string UserID { get; set; }
        public string BillingEmail { get; set; }
        public bool? eBayGoodStanding { get; set; }
        public string EIASToken { get; set; }
        public string EMail { get; set; }
        public bool? FeedbackPrivate { get; set; }
        public int? FeedbackScore { get; set; }
        public string FeedbackRatingStar { get; set; }
        public bool? IdVerified { get; set; }
        public bool? NewUser { get; set; }
        public string PayPalAccountStatus { get; set; }
        public string PayPalAccountType { get; set; }
        public bool? QualifiesForSelling { get; set; }
        public int? RegistrationAddressId { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool? SellerInfoQualifiesForB2BVAT { get; set; }
        public string SellerInfoSellerBusinessType { get; set; }
        public int? SellerInfoSellerPaymentAddressId { get; set; }
        public bool? SellerInfoStoreOwner { get; set; }
        public string SellerInfoStoreSite { get; set; }
        public string SellerInfoStoreURL { get; set; }
        public bool? SellerInfoTopRatedSeller { get; set; }
        public string SellerInfoTopRatedProgram { get; set; }
        public string Site { get; set; }
        public string SkypeID { get; set; }
        public bool? IDChanged { get; set; }
        public DateTime? IDLastChanged { get; set; }
        public int CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
    }
}
