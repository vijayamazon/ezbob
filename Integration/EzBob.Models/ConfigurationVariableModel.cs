using EZBob.DatabaseLib.Model;

namespace EzBob.Models
{
    public class ConfigurationVariableModel : ConfigurationVariable
    {
        public static ConfigurationVariableModel FromConfigurationVariable(ConfigurationVariable variable)
        {
            return variable == null
                       ? null
                       : new ConfigurationVariableModel
                             {
                                 Id = variable.Id,
                                 Name = variable.Name,
                                 Value = variable.Value,
                                 Description = variable.Description
                             };
        }
    }
}