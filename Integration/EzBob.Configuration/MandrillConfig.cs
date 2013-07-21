using Scorto.Configuration;

namespace EzBob.Configuration
{
    public interface IMandrillConfig
    {
        bool Enable { get; }
        string Key { get; }
    }

    public class MandrillConfig : ConfigurationRoot, IMandrillConfig
    {
        public bool Enable
        {
			get
			{ GetValueWithDefault<bool>("Enable", "False"); }
        }
		public virtual string Key
		{
			get { return GetValue<string>("Key"); }
		}
    }
}