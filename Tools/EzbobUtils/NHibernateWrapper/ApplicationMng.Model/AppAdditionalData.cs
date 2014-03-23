using ApplicationMng.Model.Commands;
using System;
namespace ApplicationMng.Model
{
	public class AppAdditionalData
	{
		public virtual long Id
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual AppStatus Status
		{
			get;
			set;
		}
		public virtual CommandsList CommandsList
		{
			get;
			set;
		}
		public virtual string PassportSeries
		{
			get;
			set;
		}
		public virtual string Name
		{
			get;
			set;
		}
		public virtual string Surname
		{
			get;
			set;
		}
		public virtual string Patronymic
		{
			get;
			set;
		}
		public virtual decimal? DesiredCreditSum
		{
			get;
			set;
		}
		public virtual decimal? ActualCreditSum
		{
			get;
			set;
		}
		public virtual string CreditProduct
		{
			get;
			set;
		}
		public virtual string ReadOnlyNodeName
		{
			get;
			set;
		}
		public virtual string AutoCreditTerm
		{
			get;
			set;
		}
		public virtual string AutoCreditFirstPayment
		{
			get;
			set;
		}
	}
}
