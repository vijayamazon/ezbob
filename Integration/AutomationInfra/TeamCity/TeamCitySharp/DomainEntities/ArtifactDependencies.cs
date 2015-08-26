namespace TeamCityModels.DomainEntities
{
    using System.Collections.Generic;

    public class ArtifactDependencies
    {
        public override string ToString()
        {
            return "artifact-dependencies";
        }

        public List<ArtifactDependency> ArtifactDependency { get; set; }
    }
}