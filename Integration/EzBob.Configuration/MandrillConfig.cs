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
			{ return true;  GetValueWithDefault<bool>("Enable", "False"); }
        }
		public virtual string Key
		{
			get { return "Z95NpOsNNMy4LMLMH9mUjw"; return GetValue<string>("Key"); }
		}
    }
}