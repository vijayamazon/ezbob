using Scorto.Configuration;

namespace EzBob.Configuration
{
    public interface IMandrillConfig
    {
        string Key { get; }
        string BaseSecureUrl { get; }
        string SendTemplatePath { get; }
        string FinishWizardTemplateName { get; }
    }

    public class MandrillConfig : ConfigurationRoot, IMandrillConfig
    {
        public virtual string Key
        {
            get { return GetValue<string>("Key"); }
        }

        public virtual string BaseSecureUrl
        {
            get { return GetValue<string>("BaseSecureUrl"); }
        }

        public virtual string SendTemplatePath
        {
            get { return GetValue<string>("SendTemplatePath"); }
        }

        public virtual string FinishWizardTemplateName
        {
            get { return GetValue<string>("FinishWizardTemplateName"); }
        }
    }
}
