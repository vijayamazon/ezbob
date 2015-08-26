namespace TeamCityModels.DomainEntities
{
    using System.Collections.Generic;

    public class BuildSteps
    {
        public override string ToString()
        {
            return "steps";
        }

        public List<BuildStep> Step { get; set; }
    }
}