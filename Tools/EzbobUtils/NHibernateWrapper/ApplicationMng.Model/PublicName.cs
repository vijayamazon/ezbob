using Iesi.Collections.Generic;
using System;
namespace ApplicationMng.Model
{
	public class PublicName
	{
		private ISet<PublicNameStrategy> _publicNameStrategies = new HashedSet<PublicNameStrategy>();
		public virtual int Id
		{
			get;
			set;
		}
		public virtual string Name
		{
			get;
			set;
		}
		public virtual int? IsStopped
		{
			get;
			set;
		}
		public virtual int? IsDeleted
		{
			get;
			set;
		}
		public virtual ISet<PublicNameStrategy> PublicNameStrategies
		{
			get
			{
				return this._publicNameStrategies;
			}
			set
			{
				this._publicNameStrategies = value;
			}
		}
		public virtual System.DateTime? TerminationDate
		{
			get;
			set;
		}
		public virtual string DisplayNameWithTermDate()
		{
			return (!this.TerminationDate.HasValue) ? this.Name : string.Concat(new object[]
			{
				this.Name,
				"( ",
				this.TerminationDate.Value,
				" )"
			});
		}
	}
}
