﻿using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using StructureMap.Configuration.DSL;

namespace PayPoint
{
    public class PluginRegistry : Registry
    {
        public PluginRegistry()
        {
            For<IMarketplaceType>().Use<PayPointDatabaseMarketPlace>().Named("PayPoint");
            For<DatabaseMarketplaceBase<PayPointDatabaseFunctionType>>().Use<PayPointDatabaseMarketPlace>();
            For<IMarketplaceRetrieveDataHelper>().Use<PayPointRetrieveDataHelper>().Named("PayPoint");
        }
    }
}