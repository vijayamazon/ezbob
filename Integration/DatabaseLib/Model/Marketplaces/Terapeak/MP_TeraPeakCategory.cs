using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_TeraPeakCategory
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string FullName { get; set; }
        public virtual int Level { get; set; }
        public virtual int ParentCategoryID { get; set; }
    }

    public class MP_TeraPeakCategoryMap : ClassMap<MP_TeraPeakCategory>
    {
        public MP_TeraPeakCategoryMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
            Map(x => x.FullName);
            Map(x => x.Level);
            Map(x => x.ParentCategoryID);
        }
    }
}