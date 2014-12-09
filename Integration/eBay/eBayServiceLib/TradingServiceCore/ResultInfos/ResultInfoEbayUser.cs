using System;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.DatabaseWrapper.UsersData;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoEbayUser : ResultInfoByServerResponseBase, IDatabaseEbayUserData
	{
		private readonly GetUserResponseType _Response;

		public ResultInfoEbayUser( GetUserResponseType response )
			: base( response )
		{
		    _Response = response;        
  		}

	    public bool? IDVerified
	    {
			get { return _Response.User != null && _Response.User.IDVerifiedSpecified? _Response.User.IDVerified : (bool?)null; }
	    }

	    public bool? NewUser
	    {
			get { return _Response.User != null && _Response.User.NewUserSpecified?  _Response.User.NewUser: (bool?)null; }
	    }

	    public string PayPalAccountStatus
	    {
			get { return _Response.User == null ? null: _Response.User.PayPalAccountStatus.ToString(); }
	    }

	    public string PayPalAccountType
	    {
	        get { return _Response.User == null ? null: _Response.User.PayPalAccountType.ToString(); }
	    }

	    public bool? QualifiesForSelling
	    {
			get { return _Response.User != null && _Response.User.QualifiesForSellingSpecified? _Response.User.QualifiesForSelling : (bool?)null; }
	    }

	    public bool QualifiesForB2BVAT
	    {
			get { return _Response.User != null && _Response.User.SellerInfo != null && _Response.User.SellerInfo.QualifiesForB2BVAT; }
	    }

	    public string SellerBusinessType
	    {
			get { return _Response.User == null || _Response.User.SellerInfo == null ? null : _Response.User.SellerInfo.SellerBusinessType.ToString(); }
	    }

	    public bool StoreOwner
	    {
			get { return _Response.User != null && _Response.User.SellerInfo != null && _Response.User.SellerInfo.StoreOwner; }
	    }

	    public string StoreSite
	    {
	        get {return _Response.User == null || _Response.User.SellerInfo == null ? null: _Response.User.SellerInfo.StoreSite.ToString(); }
	    }

	    public string StoreURL
	    {
			get { return _Response.User == null || _Response.User.SellerInfo == null ? null : _Response.User.SellerInfo.StoreURL; }
	    }

	    public bool? TopRatedSeller
	    {
			get { return _Response.User != null && _Response.User.SellerInfo != null && _Response.User.SellerInfo.TopRatedSellerSpecified? _Response.User.SellerInfo.TopRatedSeller: (bool?)null; }
	    }

		public string Site
	    {
	        get {return _Response.User == null ? null: _Response.User.Site.ToString(); }
	    }

	    public string SkypeID 
		{ 
			get 
			{
				return _Response.User == null || _Response.User.SkypeID == null ? null : string.Join( ", ", _Response.User.SkypeID.Select( s => s.ToString() ) );
			} 
		}

		public bool? FeedbackPrivate
	    {
			get { return _Response.User != null && _Response.User.FeedbackPrivateSpecified? _Response.User.FeedbackPrivate: (bool?)null; }
	    }

	    public string EIASToken
	    {
			get { return _Response.User == null ? null : _Response.User.EIASToken; }
	    }

	    public bool? eBayGoodStanding
	    {
			get { return _Response.User != null && _Response.User.eBayGoodStandingSpecified? _Response.User.eBayGoodStanding: (bool?)null; }
	    }

	    public string UserID
		{
			get { return _Response.User == null ? null : _Response.User.UserID; }
		}

		public int? FeedbackScore
		{
			get { return _Response.User != null && _Response.User.FeedbackScoreSpecified ? _Response.User.FeedbackScore : (int?)null; }
		}

		public string FeedbackRatingStar
		{
			get { return _Response.User == null ? null : _Response.User.FeedbackRatingStar.ToString(); }
		}

		public string EMail
		{
			get { return _Response.User == null ? null : _Response.User.Email; }
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.UserInfo; }
		}

		public DateTime? RegistrationDate
		{
			get { return _Response.User == null || !_Response.User.RegistrationDateSpecified ? (DateTime?)null : _Response.User.RegistrationDate.ToUniversalTime(); }
   		}

        public string BillingEmail
        {
			get { return _Response.User == null ? null : _Response.User.BillingEmail ; }
		}

		public bool? IDChanged
		{
			get { return _Response.User != null && _Response.User.UserIDChangedSpecified? _Response.User.UserIDChanged: (bool?)null; }
		}

		public DateTime? IDLastChanged
		{
			get { return _Response.User == null || !_Response.User.UserIDLastChangedSpecified || _Response.User.UserIDLastChanged == DateTime.MinValue ? (DateTime?)null : _Response.User.UserIDLastChanged.ToUniversalTime(); }
		}

		public string TopRatedProgram
		{
			get 
			{
				if ( _Response.User == null || _Response.User.SellerInfo == null || _Response.User.SellerInfo.TopRatedSellerDetails == null )
				{
					return null;
				}
				var topRatedProgram = _Response.User.SellerInfo.TopRatedSellerDetails.TopRatedProgram; 

				return string.Join( ", ", topRatedProgram.Select( p => p.ToString() ) );
			}
		}

		public DatabaseShipingAddress RegistrationAddress
		{
			get
			{
				if ( _Response.User == null || _Response.User.RegistrationAddress == null )
				{
					return null;
				}

				return _Response.User.RegistrationAddress.ConvertToDatabaseType();
			}
		}

		public DatabaseShipingAddress SellerPaymentAddress
		{
			get
			{
				if ( _Response.User == null || _Response.User.SellerInfo == null || _Response.User.SellerInfo.SellerPaymentAddress == null )
				{
					return null;
				}

				return _Response.User.SellerInfo.SellerPaymentAddress.ConvertToDatabaseType();
			}
		}

	}
}
