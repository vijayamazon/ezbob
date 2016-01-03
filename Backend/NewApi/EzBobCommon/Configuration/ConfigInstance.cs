using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Configuration {
    using StructureMap.Pipeline;

    /// <summary>
    /// Uses registered ConfigManager to create configuration object
    /// used by <see cref="T:EzBobCommon.Configuration.ConfigurationPolicy"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConfigInstance<T> : LambdaInstance<T>
        where T : class, new() {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInstance{T}"/> class.
        /// </summary>
        public ConfigInstance()
            : base(string.Format("Building {0} from application settings", typeof(T).FullName),
                c => c.GetInstance<ConfigManager>()
                    .GetConfigObject<T>()) {}
    }
}
