using ApplicationMng.Model;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class ExportTemplatesRepository : NHibernateRepositoryBase<ExportTemplate>
	{
		public ExportTemplatesRepository(ISession session) : base(session)
		{
		}
		public void DeleteTemplate(int id, int userId, string deleteDoc)
		{
			this.BeginTransaction();
			try
			{
				ExportTemplate exportTemplate = this.Get(id);
				exportTemplate.IsDeleted = new int?(exportTemplate.Id);
				exportTemplate.TerminationDate = new System.DateTime?(System.DateTime.Now);
				exportTemplate.SignedDocumentDelete = deleteDoc;
				exportTemplate.Deleter = this._session.Load<User>(userId);
				this._session.Update(exportTemplate);
				string displayName = exportTemplate.DisplayName;
				exportTemplate = (
					from t in this.GetAll()
					where t.DisplayName == displayName && t.IsDeleted == null && t.Id != id
					select t into exportTemplate1
					orderby exportTemplate1.Id descending
					select exportTemplate1).FirstOrDefault<ExportTemplate>();
				if (exportTemplate != null)
				{
					exportTemplate.TerminationDate = null;
					this._session.Update(exportTemplate);
				}
				this.CommitTransaction();
			}
			catch
			{
				this.RollbackTransaction();
				throw;
			}
		}
		public ExportTemplate GetByFileName(string filename)
		{
			ExportTemplate result;
			try
			{
				result = this.GetAll().Single((ExportTemplate t) => t.FileName == filename && t.IsDeleted == null);
			}
			catch (System.InvalidOperationException inner)
			{
				string message = string.Format("Template {0} was not found.", filename);
				throw new ExportTemplateNotFoundException(message, inner);
			}
			return result;
		}
		public System.Collections.Generic.List<ExportTemplate> GetByFileNames(params string[] filenames)
		{
			System.Collections.Generic.List<ExportTemplate> list = (
				from t in this.GetAll()
				where filenames.Contains(t.FileName) && t.IsDeleted == null
				select t).ToList<ExportTemplate>();
			string[] array = filenames.Except(
				from t in list
				select t.FileName).ToArray<string>();
			if (array.Length > 0)
			{
				string message = string.Format("Templates '{0}' were not found.", string.Join(", ", array));
				throw new ExportTemplateNotFoundException(message);
			}
			return list;
		}
		public string[] GetNonExistantTemplates(params string[] filenames)
		{
			System.Collections.Generic.List<ExportTemplate> source = (
				from t in this.GetAll()
				where filenames.Contains(t.FileName) && t.IsDeleted == null
				select t).ToList<ExportTemplate>();
			return filenames.Except(
				from t in source
				select t.FileName).ToArray<string>();
		}
		public string[] CheckTemplates(params string[] filenames)
		{
			string[] second = (
				from t in this.GetAll()
				where filenames.Contains(t.FileName) && t.IsDeleted == null
				select t.FileName).ToArray<string>();
			return filenames.Except(second).ToArray<string>();
		}
		public ExportTemplate GetByName(string name)
		{
			return this.GetAll().SingleOrDefault((ExportTemplate t) => t.DisplayName == name && t.IsDeleted == null && t.TerminationDate == (System.DateTime?)null);
		}
	}
}
