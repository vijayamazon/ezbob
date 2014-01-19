using System;
using System.Linq;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoEbayAccount : ResultInfoByServerResponseBase, IDatabaseEbayAccountInfo
	{
		private readonly GetAccountResponseType _response;
        
        public ResultInfoEbayAccount(GetAccountResponseType  response)
			: base( response )
		{
			_response = response;
		}

	    public string PaymentMethod
	    {
			get { return _response.AccountSummary == null ? null : _response.AccountSummary.PaymentMethod.ToString(); }
	    }

	    public bool PastDue
	    {
			get { return _response.AccountSummary != null && _response.AccountSummary.PastDue; }
	    }

	    public double? CurrentBalance
	    {
			get { return _response.AccountSummary == null ? 0d : _response.AccountSummary.CurrentBalance.Value; }
	    }

	    public DateTime? CreditCardModifyDate
	    {
			get { return _response.AccountSummary == null || !_response.AccountSummary.CreditCardModifyDateSpecified ? (DateTime?)null : _response.AccountSummary.CreditCardModifyDate.ToUniversalTime(); }
	    }

	    public string CreditCardInfo
	    {
			get { return _response.AccountSummary == null ? null : _response.AccountSummary.CreditCardInfo; }
	    }

	    public DateTime? CreditCardExpiration
	    {
			get { return _response.AccountSummary == null || !_response.AccountSummary.CreditCardExpirationSpecified ? (DateTime?)null : _response.AccountSummary.CreditCardExpiration.ToUniversalTime(); }
	    }

	    public DateTime? BankModifyDate
	    {
			get { return _response.AccountSummary == null || !_response.AccountSummary.BankModifyDateSpecified ? (DateTime?)null : _response.AccountSummary.BankModifyDate.ToUniversalTime(); }
	    }

	    public string AccountState
	    {
            get { return _response.AccountSummary == null? null: _response.AccountSummary.AccountState.ToString(); }
	    }

		public AmountInfo AmountPastDueValue
        {
			get 
			{
				if ( _response.AccountSummary == null || _response.AccountSummary == null)
				{
					return null;
				}
				return new AmountInfo
				{
					Value = _response.AccountSummary.AmountPastDue.Value,
					CurrencyCode = _response.AccountSummary.AmountPastDue.currencyID.ToString(),
				};
			}
        }

		public string BankAccountInfo
	    {
			get { return _response.AccountSummary == null ? null : _response.AccountSummary.BankAccountInfo; }
	    }

		public string AccountId
        {
            get { return _response.AccountID ?? string.Empty ; }
        }

	    public string Currency
	    {
            get { return _response.Currency.ToString(); }
	    }

		public override DataInfoTypeEnum DataInfoType
	    {
            get { return DataInfoTypeEnum.Token  ; }
	    }

		public DatabaseEbayAdditionalAccount[] AdditionalAccount
		{
			get 
			{
				if ( _response.AccountSummary == null || _response.AccountSummary.AdditionalAccount == null || _response.AccountSummary.AdditionalAccount.Length == 0 )
				{
					return null;
				}

				return _response.AccountSummary.AdditionalAccount.Select( a =>
					new DatabaseEbayAdditionalAccount
						{
							Currency = a.CurrencySpecified ? a.Currency.ToString() : null,
							AccountCode = a.AccountCode,
							Balance = a.Balance == null? null : new AmountInfo
								{
									CurrencyCode = a.Balance.currencyID.ToString(),
									Value = a.Balance.Value
								}

						} ).ToArray();
			}
		}
	}
}
