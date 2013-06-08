﻿namespace FreeAgent
{
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using StructureMap.Configuration.DSL;

    public class PluginRegistry : Registry
    {
        public PluginRegistry()
        {
			For<IMarketplaceType>().Use<FreeAgentDatabaseMarketPlace>().Named("FreeAgent");
			For<DatabaseMarketplaceBase<FreeAgentDatabaseFunctionType>>().Use<FreeAgentDatabaseMarketPlace>();
			For<IMarketplaceRetrieveDataHelper>().Use<FreeAgentRetrieveDataHelper>().Named("FreeAgent");
        }
    }
}