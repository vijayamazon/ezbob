using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class ScoringModelMap : ClassMap<ScoringModel>
	{
		public ScoringModelMap()
		{
			base.Table("ScoringModel");
			this.Id((ScoringModel x) => (object)x.Id).GeneratedBy.HiLo("1");
			base.Map((ScoringModel x) => (object)x.IsDeleted);
			base.Map((ScoringModel x) => (object)x.AllowSaveResults);
			base.Map((ScoringModel x) => (object)x.AllowWeightsEdit);
			base.References<User>((ScoringModel x) => x.CreateUser).Column("UserId").Cascade.None();
			base.Map((ScoringModel x) => (object)x.CreationDate);
			base.Map((ScoringModel x) => (object)x.CutOffPoint);
			base.Map((ScoringModel x) => x.Description);
			base.Map((ScoringModel x) => x.DisplayName);
			base.Map((ScoringModel x) => x.Guid);
			base.Map((ScoringModel x) => x.ModelTypeName);
			base.Map((ScoringModel x) => x.PmmlFile).CustomType("StringClob");
			base.Map((ScoringModel x) => x.SignedDocument).CustomType("StringClob");
			base.Map((ScoringModel x) => x.SignedDocumentDelete).CustomType("StringClob");
			base.Map((ScoringModel x) => (object)x.TerminationDate);
		}
	}
}
