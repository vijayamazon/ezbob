namespace TeamCity.ActionTypes
{
    using System.Collections.Generic;
    using TeamCity.DomainEntities;
    using TeamCity.Locators;

    public interface IVcsRoots
    {
        List<VcsRoot> All();
        VcsRoot ById(string vcsRootId);
        VcsRoot AttachVcsRoot(BuildTypeLocator locator, VcsRoot vcsRoot);
        void DetachVcsRoot(BuildTypeLocator locator, string vcsRootId);
        void SetVcsRootField(VcsRoot vcsRoot, VcsRootField field, object value);
    }
}