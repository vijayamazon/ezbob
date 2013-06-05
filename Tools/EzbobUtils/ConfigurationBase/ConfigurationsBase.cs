namespace ConfigurationBase
{
    using Logger;
    using DbConnection;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;

    public class ConfigurationsBase
    {
        private const string TypeNameInt = "Int32";
        private const string TypeNameString = "String";
        private const string TypeNameLong = "Int64";

        private static int readConfigurationsCounter;
        private static readonly Dictionary<string, PropertyInfo> configPropertyInfos = new Dictionary<string, PropertyInfo>();
        private static PropertyInfo[] propertyInfos;
        private static string spName;

        protected static void Init(Type derivedType, string spNameInput)
        {
            spName = spNameInput;
            propertyInfos = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (PropertyInfo pi in propertyInfos)
            {
                configPropertyInfos.Add(pi.Name, pi);
            }

            Read();
        }

        private static void Read()
        {
            Logger.Info("Reading configurations");

            try
            {
                DataTable dt = DbConnection.ExecuteSpReader(spName);

                foreach (DataRow row in dt.Rows)
                {
                    AddSingleConfiguration(row["CfgKey"].ToString(), row["CfgValue"].ToString());
                }

                if (readConfigurationsCounter != propertyInfos.Length)
                {
                    Logger.Error("Couldn't fill all properties");
                    Environment.Exit(-1);
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error reading configurations:{0}", e);
                Environment.Exit(-1);
            }
        }

        public static void Refresh()
        {
            Logger.Info("Refreshing configurations");

            try
            {
                DataTable dt = DbConnection.ExecuteSpReader(spName);

                foreach (DataRow row in dt.Rows)
                {
                    RefreshSingleConfiguration(row["CfgKey"].ToString(), row["CfgValue"].ToString());
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error reading configurations:{0}", e);
            }
        }

        private static void RefreshSingleConfiguration(string key, string value)
        {
            if (configPropertyInfos.ContainsKey(key))
            {
                if (configPropertyInfos[key].GetValue(null).ToString() != value)
                {
                    SetProperty(key, value);
                    Logger.InfoFormat("Refreshed configuration {0}: {1}", key, value);
                }
            }
            else
            {
                Logger.ErrorFormat("Unimplemented configuration was read:{0} with value:{1}.", key, value);
            }
        }

        private static void AddSingleConfiguration(string key, string value)
        {
            if (configPropertyInfos.ContainsKey(key))
            {
                SetProperty(key, value);
                readConfigurationsCounter++;
                Logger.InfoFormat("Added configuration {0}: {1}", key, value);
            }
            else
            {
                Logger.ErrorFormat("Unimplemented configuration was read:{0} with value:{1}.", key, value);
            }
        }

        private static void SetProperty(string key, string value)
        {
            switch (configPropertyInfos[key].PropertyType.Name)
            {
                case TypeNameInt:
                    int valueAsInt;
                    if (!int.TryParse(value, out valueAsInt))
                    {
                        Logger.ErrorFormat("Error parsing configuration:{0} with value:{1} as int", key, value);
                        Environment.Exit(-1);
                    }
                    configPropertyInfos[key].SetValue(null, valueAsInt);
                    break;
                case TypeNameLong:
                    long valueAsLong;
                    if (!long.TryParse(value, out valueAsLong))
                    {
                        Logger.ErrorFormat("Error parsing configuration:{0} with value:{1} as long", key, value);
                        Environment.Exit(-1);
                    }
                    configPropertyInfos[key].SetValue(null, valueAsLong);
                    break;
                case TypeNameString:
                    configPropertyInfos[key].SetValue(null, value);
                    break;
                default:
                    Logger.ErrorFormat("Error parsing configuration:{0} with value:{1}. Type is not implemented", key, value);
                    Environment.Exit(-1);
                    break;
            }
        }
    }

    // How to use
    // 1. Add reference to C:\DEV\Dlls\ConfigurationBase.dll
    // 2. Create a class like:
    //  namespace EzSupportAgent
    //  {
    //      using ConfigurationBase;

    //      public class ConfigurationSample : ConfigurationsBase
    //      {
    //          static ConfigurationSample()
    //          {
    //              Init(typeof (ConfigurationSample));
    //          }

    //          public static string PropertySample1 { get; private set; }
    //          public static int PropertySample2 { get; private set; }
    //      }
    //  }
    //
    // 3. (Optional) Use this line at the beginning of your code to make it initialized soon
    //  var touchToInit = ConfigurationSample.PropertySample1;
    //
    // 4. Can be accessed anywhere: 
    //  ConfigurationSample.PropertySample1
    //  ConfigurationSample.PropertySample2
}
