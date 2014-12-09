using System;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoCheckAuthenticationToken : ResultInfoByServerResponseBase
	{
		private readonly GeteBayOfficialTimeResponseType _Response;

		public ResultInfoCheckAuthenticationToken( GeteBayOfficialTimeResponseType response )
			: base( response )
		{
			_Response = response;			
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.GeteBayOfficialTime; }
		}

	}

	public class ResultInfoTokenStatus : ResultInfoByServerResponseBase
	{
		private readonly GetTokenStatusResponseType _Response;

		public ResultInfoTokenStatus(GetTokenStatusResponseType response) : base(response)
		{
			_Response = response;

		}

		public DateTime? Expred
		{
			get { return _Response.TokenStatus.ExpirationTimeSpecified? _Response.TokenStatus.ExpirationTime: (DateTime?)null; }
		}

		public TokenStatusCodeType Status
		{
			get { return _Response.TokenStatus.Status; }
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.TokenStatusInfo; }
		}
	}
}
