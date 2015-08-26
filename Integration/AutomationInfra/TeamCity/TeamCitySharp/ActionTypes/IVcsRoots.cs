namespace TeamCityModels.ActionTypes
{
    using System.Collections.Generic;
    using TeamCityModels.DomainEntities;
    using TeamCityModels.Locators;

    public interface IVcsRoots
    {
        List<VcsRoot> All();
        VcsRoot ById(string vcsRootId);
        VcsRoot AttachVcsRoot(BuildTypeLocator locator, VcsRoot vcsRoot);
        void DetachVcsRoot(BuildTypeLocator locator, string vcsRootId);
        void SetVcsRootField(VcsRoot vcsRoot, VcsRootField field, object value);
    }
}