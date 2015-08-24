namespace TeamCity.DomainEntities
{
    using System.Collections.Generic;

    public class Parameters
    {
        public override string ToString()
        {
            return "parameters";
        }

        public List<Property> Property { get; set; }
    }
}