namespace TeamCityModels.ActionTypes
{
    using System.Collections.Generic;
    using System.Linq;
    using TeamCityModels.Connection;
    using TeamCityModels.DomainEntities;

    public class Changes : IChanges
    {
        private readonly TeamCityCaller _caller;

        public Changes(TeamCityCaller caller)
        {
            this._caller = caller;
        }

        public List<Change> All()
        {
            var changeWrapper = this._caller.Get<ChangeWrapper>("/app/rest/changes");

            return changeWrapper.Change;
        }

        public Change ByChangeId(string id)
        {
            var change = this._caller.GetFormat<Change>("/app/rest/changes/id:{0}", id);

            return change;
        }

        public List<Change> ByBuildConfigId(string buildConfigId)
        {
            var changeWrapper = this._caller.GetFormat<ChangeWrapper>("/app/rest/changes?buildType={0}", buildConfigId);

            return changeWrapper.Change;
        }

        public Change LastChangeDetailByBuildConfigId(string buildConfigId)
        {
            var changes = ByBuildConfigId(buildConfigId);

            return changes.FirstOrDefault();
        }

    }
}