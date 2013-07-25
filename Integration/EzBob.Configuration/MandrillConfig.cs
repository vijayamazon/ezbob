using Scorto.Configuration;

namespace EzBob.Configuration
{
    public interface IMandrillConfig
    {
        bool Enable { get; }
        string Key { get; }
        /// <summary>
        /// Used in sending messages without a template
        /// </summary>
        string From { get; }
    }

    public class MandrillConfig : ConfigurationRoot, IMandrillConfig
    {
        public bool Enable
        {
			get { return GetValueWithDefault<bool>("Enable", "False"); }
        }
		public virtual string Key
		{
			get { return GetValue<string>("Key"); }
		}
        public virtual string From
        {
            get { return GetValue<string>("From"); }
        }
    }
}