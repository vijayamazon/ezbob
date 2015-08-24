namespace TeamCity.DomainEntities
{
    using System.Collections.Generic;

    public class AgentRequirements
    {
        public override string ToString()
        {
            return "agent-requirements";
        }

        public List<AgentRequirement> AgentRequirement { get; set; }    
    }
}