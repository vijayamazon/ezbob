using System;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Simple;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Factory
{
	public static class DataProviderFactory
	{
		public static IDataProviderBaseTyped<TRezult, TParams> Create<TRezult, TParams>( IEbayServiceProvider serviceProvider, CallProcedureTypeSimple callProcedureType )
			where TRezult : IResultDataInfo 
			where TParams : IParamsDataInfo
		{
			switch (callProcedureType.Type)
			{
				case CallProcedureTypeEnum.ConfirmIdentity:
					return new DataProviderConfirmIdentity( serviceProvider ) as IDataProviderBaseTyped<TRezult, TParams>;

				case CallProcedureTypeEnum.FetchToken:
					return new DataProviderFetchToken( serviceProvider ) as IDataProviderBaseTyped<TRezult, TParams>;

				case CallProcedureTypeEnum.GetSessionId:
					return new DataProviderSessionID( serviceProvider ) as IDataProviderBaseTyped<TRezult, TParams>;

				default:
					throw new NotImplementedException();
			}
		}

		public static IDataProviderBaseTyped<TRezult, TParams> Create<TRezult, TParams>( IEbayServiceProvider serviceProvider, IServiceTokenProvider tokenProvider, CallProcedureTypeTokenDependent callProcedureType )
			where TRezult : IResultDataInfo
			where TParams : IParamsDataInfo
		{
			switch ( callProcedureType.Type )
			{
				case CallProcedureTypeEnum.ConfirmIdentity:
					return new DataProviderConfirmIdentity( serviceProvider ) as IDataProviderBaseTyped<TRezult, TParams>;

				default:
					throw new NotImplementedException();
			}
		}

	}
}