using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class EzbobMailNodeAttachRelation
    {
        public virtual int Id { get; set; }
        public virtual string To { get; set; }
        public virtual ExportResult Export { get; set; }
		public virtual int? UserID { get; set; }
    }

    public class EzbobMailNodeAttachRelationMap : ClassMap<EzbobMailNodeAttachRelation>
    {
            public EzbobMailNodeAttachRelationMap()
            {
                Table("EzbobMailNodeAttachRelation");
                Id(x => x.Id).Column("Id");
                Map(x => x.To, "ToField").Length(200);
				Map(x => x.UserID);
                References(x => x.Export, "ExportId");
            }
    }
}
