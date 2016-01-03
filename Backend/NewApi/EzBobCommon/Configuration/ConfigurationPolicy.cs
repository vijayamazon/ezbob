namespace EzBobCommon.Configuration {
    using System;
    using StructureMap.Graph;
    using StructureMap.Pipeline;
    using StructureMap.TypeRules;

    /// <summary>
    /// Instructs 'StructureMap' how to inject configuration objects
    /// </summary>
    public class ConfigurationPolicy : IFamilyPolicy {

        private Func<Type, bool> isPass = type => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ConfigurationPolicy() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ConfigurationPolicy(Func<Type, bool> isPass) {
            this.isPass = isPass;
        }

        /// <summary>
        /// Allows you to create missing registrations for an unknown plugin type
        /// at runtime.
        /// Return null if this policy does not apply to the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PluginFamily Build(Type type) {

            if (!this.isPass(type)) {
                return null;
            }

            if (type.Name.EndsWith(ConfigManager.ConfigSuffix, StringComparison.InvariantCultureIgnoreCase) &&
                type.IsConcreteWithDefaultCtor()) {
                var family = new PluginFamily(type);
                var instance = BuildInstanceForType(type);
                family.SetDefault(instance);

                return family;
            }

            return null;
        }

        /// <summary>
        /// Should this policy be used to determine whether or not the Container has
        /// registrations for a plugin type in the PluginGraph.HasFamily(type) method
        /// </summary>
        public bool AppliesToHasFamilyChecks
        {
            get { return true; }
        }

        /// <summary>
        /// Builds the type of the instance for.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static Instance BuildInstanceForType(Type type) {
            var instanceType = typeof(ConfigInstance<>).MakeGenericType(type);
            var instance = Activator.CreateInstance(instanceType) as Instance;
            return instance;
        }
    }
}