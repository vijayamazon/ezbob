using PostcodeAnywhere;
using Scorto.Configuration;

namespace EzBob.Web.Infrastructure
{
    public class PostcodeAnywhereConfig : ConfigurationRoot, IPostcodeAnywhereConfig
    {
        public virtual string Key { get { return GetValue<string>("Key"); } }

        public bool Enabled { get { return GetValueWithDefault<bool>("Enabled", "True"); } }
        public int MaxBankAccountValidationAttempts { get { return GetValueWithDefault<int>("MaxBankAccountValidationAttempts", "2"); } }
    }
}