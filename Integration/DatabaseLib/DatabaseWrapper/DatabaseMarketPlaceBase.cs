using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public abstract class DatabaseMarketplaceBaseBase : IMarketplaceType
	{		
		private readonly IMarketplaceServiceInfo _MarketplaceSeriveInfo;

		protected DatabaseMarketplaceBaseBase( IMarketplaceServiceInfo marketplaceSeriveInfo )
		{
			_MarketplaceSeriveInfo = marketplaceSeriveInfo;
		}

		public string DisplayName 
		{ 
			get { return _MarketplaceSeriveInfo.DisplayName; } 
		}

		public Guid InternalId
		{
			get { return _MarketplaceSeriveInfo.InternalId; }
		}

		public string Description
		{
			get { return _MarketplaceSeriveInfo.Description; }
		}

		public bool IsPaymentAccount
		{
			get { return _MarketplaceSeriveInfo.IsPaymentAccount; }
		}

	    public abstract IEnumerable<IDatabaseFunction> DatabaseFunctionList { get; }

	    public abstract IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper);
		public abstract IDatabaseFunction GetDatabaseFunctionById(Guid id);
	}

	public abstract class DatabaseMarketplaceBase<TEnum> : DatabaseMarketplaceBaseBase		
	{		
		protected DatabaseMarketplaceBase( IMarketplaceServiceInfo marketplaceSeriveInfo ) 
			: base(marketplaceSeriveInfo)
		{
		}

	    public override IEnumerable<IDatabaseFunction> DatabaseFunctionList
		{
			get
			{
				return (from TEnum enumItem in Enum.GetValues(typeof (TEnum)) select FunctionFactory.Create(enumItem));
			}
		}

		public abstract IDatabaseFunctionFactory<TEnum> FunctionFactory { get;}

		public override IDatabaseFunction GetDatabaseFunctionById(Guid id)
		{
			return FunctionFactory.GetById( id );
		}
	
	}
}