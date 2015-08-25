namespace TeamCity.ActionTypes
{
    using System.Collections.Generic;
    using TeamCity.DomainEntities;

    public interface IAgents
    {
        List<Agent> All();
    }
}