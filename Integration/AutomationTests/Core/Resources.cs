namespace UIAutomationTests.Core
{
    using System.Resources;
    using UIAutomationTests.configs.BrandConfigs;
    using UIAutomationTests.configs.EnviormentConfig;

    class Resources
    {

        public static ResourceManager GetEnvironmentResourceManager(Environment resourceName)
        {
            switch (resourceName)
            {
                case Environment.QA:
                    return QAConfig.ResourceManager;
                case Environment.Staging:
                    return  StagingConfig.ResourceManager;
                default:
                    return  EnvDefaultConfig.ResourceManager;
            }
        }

        public static ResourceManager GetBrandResourceManager(Brand resourceName)
        {
            switch (resourceName)
            {
                case Brand.Everline:
                    return EverlineConfig.ResourceManager;
                case Brand.Ezbob:
                    return  EzbobConfig.ResourceManager;
                default:
                    return BrandDefaultConfig.ResourceManager;
            }
        }
    }
}
