namespace TeamCityModels.ActionTypes
{
    using System.Collections.Generic;
    using TeamCityModels.Connection;
    using TeamCityModels.DomainEntities;

    public class Agents : IAgents
    {
        private readonly TeamCityCaller _caller;

        public Agents(TeamCityCaller caller)
        {
            this._caller = caller;
        }

        public List<Agent> All()
        {
            var agentWrapper = this._caller.Get<AgentWrapper>("/app/rest/agents");

            return agentWrapper.Agent;
        }
    }
}