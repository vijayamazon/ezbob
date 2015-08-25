namespace TeamCity.DomainEntities
{
    using System.Collections.Generic;

    public class VcsRootEntries
    {
        public override string ToString()
        {
            return "vcs-root-entries";
        }

        public List<VcsRootEntry> VcsRootEntry { get; set; }
    }
}