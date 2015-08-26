namespace TeamCityModels.ActionTypes
{
    using System.Collections.Generic;
    using TeamCityModels.DomainEntities;

    public interface IAgents
    {
        List<Agent> All();
    }
}