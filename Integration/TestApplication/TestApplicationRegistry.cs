using System;
using EZBob.DatabaseLib.Model;
using EzBob.AmazonLib;
using EzBob.PayPal;
using EzBob.PayPalServiceLib;
using EzBob.PayPalServiceLib.Common;
using EzBob.TeraPeakServiceLib;
using EzBob.eBayLib;
using NHibernate;
using StructureMap.Configuration.DSL;

namespace TestApplication
{
    public class TestApplicationRegistry : Registry
    {
        public TestApplicationRegistry()
        {
			For<ISession>().Use( NHibernateHelper.OpenSession  );
			For<IPayPalConfig>().Use( new PayPalConfigTestSandbox() );
			//For<IPayPalConfig>().Use( new PayPalConfigTestProduction() );
			For<IServiceEndPointFactory>().Use( new ServiceEndPointFactory() );
			For<ITeraPeakCredentionProvider>().Use( new TeraPeakCredentionProviderSandbox() );
			//For<ITeraPeakCredentionProvider>().Use( new TeraPeakCredentionProviderProduction() );
        }
    }
}