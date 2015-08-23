namespace UIAutomationTests.Core
{
    using System.Resources;
    using UIAutomationTests.configs.Brand;
    using UIAutomationTests.configs.Enviorment;

    class Resources
    {

        public static ResourceManager GetEnvironmentResourceManager(Environment resourceName)
        {
            switch (resourceName)
            {
                case Environment.QA:
                    return QA.ResourceManager;
                case Environment.Staging:
                    return Staging.ResourceManager;
                default:
                    return DefaultEnviorment.ResourceManager;
            }
        }

        public static ResourceManager GetBrandResourceManager(Brand resourceName)
        {
            switch (resourceName)
            {
                case Brand.Everline:
                    return Everline.ResourceManager;
                case Brand.Ezbob:
                    return Ezbob.ResourceManager;
                default:
                    return DefaultBrand.ResourceManager;
            }
        }
    }
}
