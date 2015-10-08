using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCityEngine
{
    using TeamCityData;
    using TeamCityModels.Locators;

    interface ITeamCityEngine {
         void AddJob(string buildRegressionId, string teamcityIncludeParameterName, string includedCases);
    }
    public class TeamCityEngine : ITeamCityEngine
    {

        public void AddJob(string buildRegressionId, string teamcityIncludeParameterName, string includingCases)
        {
            if (!string.IsNullOrEmpty(includingCases)) {
                TeamCityManager.Instance.BuildConfigs.SetConfigurationParameter(BuildTypeLocator.WithId(buildRegressionId), teamcityIncludeParameterName, includingCases);
                TeamCityManager.Instance.Builds.Add2QueueBuildByBuildConfigId(buildRegressionId);                
            }
        }
    }
}
