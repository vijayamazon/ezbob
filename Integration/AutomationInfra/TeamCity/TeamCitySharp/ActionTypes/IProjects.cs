namespace TeamCity.ActionTypes
{
    using System.Collections.Generic;
    using TeamCity.DomainEntities;

    public interface IProjects
    {
        List<Project> All();
        Project ByName(string projectLocatorName);
        Project ById(string projectLocatorId);
        Project Details(Project project);
        Project Create(string projectName);
        void Delete(string projectName);
        void DeleteProjectParameter(string projectName, string parameterName);
        void SetProjectParameter(string projectName, string settingName, string settingValue);
    }
}