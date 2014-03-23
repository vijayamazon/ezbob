using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class ExportJournal
	{
		public enum JournalTypes
		{
			ApplicationsOperationJournal,
			BaseEntitiesOperationJournalStrategy,
			BaseEntitiesOperationJournalPublishStrategy,
			BaseEntitiesOperationJournalNode,
			BaseEntitiesOperationJournalCreditProduct,
			BaseEntitiesOperationJournalTemplate,
			BaseEntitiesOperationJournalSmsTemplate,
			BaseEntitiesOperationJournalDictionary,
			BaseEntitiesOperationJournalExport,
			BaseEntitiesOperationJournalDataSource,
			BaseEntitiesOperationJournalModel,
			Unknown
		}
		private System.DateTime? _actionDateTime;
		private string _operationType;
		public virtual int Id
		{
			get;
			set;
		}
		public virtual User User
		{
			get;
			set;
		}
		public virtual System.DateTime ActionDateTime
		{
			get
			{
				return (!this._actionDateTime.HasValue) ? System.DateTime.Now : this._actionDateTime.Value;
			}
			set
			{
				this._actionDateTime = new System.DateTime?(value);
			}
		}
		public virtual byte[] BinaryBody
		{
			get;
			set;
		}
		public virtual string ContentName
		{
			get;
			set;
		}
		public virtual string ContentType
		{
			get;
			set;
		}
		public virtual string OperationType
		{
			get
			{
				return string.IsNullOrEmpty(this._operationType) ? "Export" : this._operationType;
			}
			set
			{
				this._operationType = value;
			}
		}
		public virtual ExportJournal.JournalTypes JournalType
		{
			get;
			set;
		}
	}
}
