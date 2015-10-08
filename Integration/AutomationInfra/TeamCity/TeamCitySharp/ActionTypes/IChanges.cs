namespace TeamCityModels.ActionTypes
{
    using System.Collections.Generic;
    using TeamCityModels.DomainEntities;

    public interface IChanges
    {
        List<Change> All();
        Change ByChangeId(string id);
        Change LastChangeDetailByBuildConfigId(string buildConfigId);
        List<Change> ByBuildConfigId(string buildConfigId);
    }
}