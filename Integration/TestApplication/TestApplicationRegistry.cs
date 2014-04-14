namespace TestApplication
{
	using EzBob.PayPalServiceLib.Common;
	using EzBob.TeraPeakServiceLib;
	using NHibernate;
	using StructureMap.Configuration.DSL;

    public class TestApplicationRegistry : Registry
    {
        public TestApplicationRegistry()
        {
			For<ISession>().Use( NHibernateHelper.OpenSession  );
			For<IServiceEndPointFactory>().Use( new ServiceEndPointFactory() );
			For<ITeraPeakCredentionProvider>().Use( new TeraPeakCredentionProviderSandbox() );
        }
    }
}