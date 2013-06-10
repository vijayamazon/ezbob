using System.Xml.Serialization;

namespace EzBob.TeraPeakServiceLib
{
    public class Category
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public int Level { get; set; }
        public int ParentCategoryID { get; set; }
        
        [XmlElement("Statistics")]
        public CategoryStatistics Statistics { get; set; }
    }

    public class CategoryStatistics
    {
        public int Listings { get; set; }
        public int Successful { get; set; }
        public int ItemsSold { get; set; }
        public double Revenue { get; set; }
        public double SuccessRate { get; set; }
    }
}