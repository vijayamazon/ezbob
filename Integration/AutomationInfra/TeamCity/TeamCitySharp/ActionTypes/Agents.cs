namespace TeamCity.ActionTypes
{
    using System.Collections.Generic;
    using TeamCity.Connection;
    using TeamCity.DomainEntities;

    internal class Agents : IAgents
    {
        private readonly TeamCityCaller _caller;

        internal Agents(TeamCityCaller caller)
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