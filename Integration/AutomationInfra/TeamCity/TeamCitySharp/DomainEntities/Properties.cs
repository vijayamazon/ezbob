namespace TeamCityModels.DomainEntities
{
    using System.Collections.Generic;

    public class Properties
    {
        public override string ToString()
        {
            return "properties";
        }
        public List<Property> Property { get; set; }
    }
}