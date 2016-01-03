namespace UIAutomationTests.Core {
    using System.Resources;
    using UIAutomationTests.configs.Brand;
    using UIAutomationTests.configs.Enviorment;
    using TestRailModels.Automation;

    class Resources {

        public static ResourceManager GetEnvironmentResourceManager(AutomationModels.Environment env) {
            switch (env) {
                case AutomationModels.Environment.QA:
                    return QA.ResourceManager;
                case AutomationModels.Environment.Staging:
                    return Staging.ResourceManager;
                default:
                    return DefaultEnviorment.ResourceManager;
            }
        }

        public static ResourceManager GetBrandResourceManager(AutomationModels.Brand brand) {
            switch (brand) {
                case AutomationModels.Brand.Everline:
                    return Everline.ResourceManager;
                case AutomationModels.Brand.Ezbob:
                    return Ezbob.ResourceManager;
                default:
                    return DefaultBrand.ResourceManager;
            }
        }
    }
}
