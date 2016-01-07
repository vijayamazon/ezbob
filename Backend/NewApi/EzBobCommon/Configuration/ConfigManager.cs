namespace EzBobCommon.Configuration {
    using System;
    using System.IO;
    using System.Linq;
    using Common.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles configuration (currently json)
    /// </summary>
    public class ConfigManager {

        /// <summary>
        /// The configuration object suffix (convention that help to distinguish configuration objects)
        /// </summary>
        public static readonly string ConfigSuffix = "Config";

        private JsonSerializerSettings jsonSerializerSettings;

        private static JsonMergeSettings mergeSettings = new JsonMergeSettings() {
            MergeArrayHandling = MergeArrayHandling.Replace
        };

        /// <summary>
        /// holds all configurations
        /// </summary>
        private JObject configJObject = new JObject();

        #region Private

        /// <summary>
        /// Gets the serializer settings.
        /// </summary>
        /// <returns></returns>
        private JsonSerializerSettings GetSerializerSettings() {
            if (this.jsonSerializerSettings == null) {
                this.jsonSerializerSettings = new JsonSerializerSettings {
                    ContractResolver = new ConfigManagerContractResolver()
                };
            }

            return this.jsonSerializerSettings;
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <returns></returns>
        private JsonSerializer GetSerializer() {
            return JsonSerializer.CreateDefault(GetSerializerSettings());
        }

        #endregion

        #region Public

        /// <summary>
        /// Gets or sets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Adds the configuration file paths.
        /// </summary>
        /// <param name="configPaths">The configuration paths.</param>
        public void AddConfigFilePaths(params string[] configPaths) {
            if (configPaths == null || configPaths.Length == 0) {
                Log.Error("got empty config file path");
                return;
            }

            //merges all configurations to one
            this.configJObject = configPaths.Where(File.Exists)
                .Select(path => JObject.Parse(File.ReadAllText(path)))
                .Aggregate(this.configJObject, (jo1, jo2) => Merge(jo1, jo2));
        }

        /// <summary>
        /// Adds the configuration json string.
        /// </summary>
        /// <param name="configJson">The configuration json.</param>
        public void AddConfigJsonString(String configJson) {

            if (string.IsNullOrEmpty(configJson)) {
                Log.Error("got empty config json string");
                return;
            }

            try {
                JObject config = JObject.Parse(configJson);
                Merge(this.configJObject, config);
            } catch (Exception ex) {
                Log.Error("could not parse json configuration: " + configJson.Substring(0, 20), ex);
            }
        }

        /// <summary>
        /// Merges the two JObjects.
        /// </summary>
        /// <param name="into">The into.</param>
        /// <param name="what">The what.</param>
        /// <returns></returns>
        private JObject Merge(JObject into, JObject what) {
            into.Merge(what, mergeSettings);
            return into;
        }

        /// <summary>
        /// Gets the configuration object.
        /// !! Assumes that configuration object's type name ends with 'Config' and
        /// that configuration object's type name corresponds to section name in configuration file
        /// (with or without 'Config' suffix)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfigObject<T>() where T : class, new() {
            string sectionName = typeof(T).Name;
            string shortSectionName = null;

            int indexOfConfigSuffix = sectionName.LastIndexOf(ConfigSuffix, StringComparison.InvariantCultureIgnoreCase);

            if (indexOfConfigSuffix > -1) {
                shortSectionName = sectionName.Remove(indexOfConfigSuffix, ConfigSuffix.Length);
            }

            JToken section;

            if (shortSectionName == null || !this.configJObject.TryGetValue(shortSectionName, StringComparison.InvariantCultureIgnoreCase, out section)) {
                if (!this.configJObject.TryGetValue(sectionName, StringComparison.InvariantCultureIgnoreCase, out section)) {
                    string errMsg = String.Format("got unknown section: {0}", sectionName);
                    Log.Error(errMsg);
                    throw new InvalidDataException(errMsg);
                }
            }

            return section.ToObject<T>(GetSerializer());
        }

        #endregion
    }
}