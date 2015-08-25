namespace TeamCity.DomainEntities
{
    using System.Collections.Generic;

    public class BuildTriggers
    {
        public override string ToString()
        {
            return "triggers";
        }

        public List<BuildTrigger> Trigger { get; set; }
    }
}