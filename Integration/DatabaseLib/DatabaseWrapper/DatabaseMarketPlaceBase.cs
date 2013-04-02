using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EzBob.CommonLib;
using StructureMap;

namespace EZBob.DatabaseLib.DatabaseWrapper
{
	public abstract class DatabaseMarketplaceBaseBase : IDatabaseMarketplace
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

		public abstract IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper);
		public abstract IDatabaseFunction GetDatabaseFunctionById(Guid id);
	}

	public abstract class DatabaseMarketplaceBase<TEnum> : DatabaseMarketplaceBaseBase		
	{		
		/*protected static void InitDatabaseMarketplace<T>()
			where T : DatabaseMarketplaceBase<TEnum>, new()
		{
			var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			var mp = new T();
			helper.InitDatabaseMarketPlace(mp);
		}*/

		protected DatabaseMarketplaceBase( IMarketplaceServiceInfo marketplaceSeriveInfo ) 
			: base(marketplaceSeriveInfo)
		{
		}

		// for internal use only!!! 
		protected internal IEnumerable<IDatabaseFunction> DatabaseFunctionList
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

	public abstract class DatabaseMarketplaceBase<T, TEnum> : DatabaseMarketplaceBase<TEnum>
		where T : DatabaseMarketplaceBase<TEnum>, new()
	{	
		static DatabaseMarketplaceBase()
		{			
			InitDatabaseMarketplace(new T());
		}

		private static void InitDatabaseMarketplace(DatabaseMarketplaceBase<TEnum> mp)
		{
			var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();			
			helper.InitDatabaseMarketPlace(mp);
		}

		protected DatabaseMarketplaceBase(IMarketplaceServiceInfo marketplaceSeriveInfo) 
			: base(marketplaceSeriveInfo)
		{
		}
	}

	
}