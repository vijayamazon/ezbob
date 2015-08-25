namespace TeamCity.DomainEntities
{
    using System.Collections.Generic;

    public class BuildWrapper
    {
        public string Count { get; set; }
        public List<Build> Build { get; set; }
    }
}