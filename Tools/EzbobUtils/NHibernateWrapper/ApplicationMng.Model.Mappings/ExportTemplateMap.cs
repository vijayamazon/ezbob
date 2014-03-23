using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class ExportTemplateMap : ClassMap<ExportTemplate>
	{
		public ExportTemplateMap()
		{
			base.Table("EXPORT_TEMPLATESLIST");
			this.Id((ExportTemplate x) => (object)x.Id, "ID").GeneratedBy.Native("SEQ_EXPORT_TEMPLATELIST");
			base.Map((ExportTemplate x) => x.FileName, "FILENAME");
			base.Map((ExportTemplate x) => x.Description, "DESCRIPTION");
			base.Map((ExportTemplate x) => x.VariablesXml, "VARIABLESXML");
			base.Map((ExportTemplate x) => (object)x.UploadDate, "UPLOADDATE");
			base.Map((ExportTemplate x) => (object)x.IsDeleted, "ISDELETED");
			base.Map((ExportTemplate x) => x.BinaryBody, "BINARYBODY").Length(8000).LazyLoad();
			base.Map((ExportTemplate x) => (object)x.ExceptionType, "EXCEPTIONTYPE");
			base.Map((ExportTemplate x) => x.DisplayName, "DISPLAYNAME");
			base.Map((ExportTemplate x) => (object)x.TerminationDate, "TERMINATIONDATE");
			base.Map((ExportTemplate x) => x.SignedDocument, "SIGNEDDOCUMENT").CustomType("StringClob").LazyLoad();
			base.Map((ExportTemplate x) => x.SignedDocumentDelete, "DELSIGNEDDOCUMENT").CustomType("StringClob").LazyLoad();
			base.References<User>((ExportTemplate x) => x.Creator, "USERID");
			base.References<User>((ExportTemplate x) => x.Deleter, "DELETERUSERID");
			base.HasManyToMany<Strategy>((ExportTemplate x) => x.Strategies).AsSet().Table("EXPORT_TEMPLATESTRATREL").ParentKeyColumn("TEMPLATEID").ChildKeyColumn("STRATEGYID").Cascade.None().Inverse().Cache.Region("LongTerm").ReadWrite();
		}
	}
}
