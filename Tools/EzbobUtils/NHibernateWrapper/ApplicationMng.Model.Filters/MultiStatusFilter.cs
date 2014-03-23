using NHibernate;
using NHibernate.Criterion;
using NHibernateWrapper.Web;
using System;
using System.Linq;
namespace ApplicationMng.Model.Filters
{
	public class MultiStatusFilter : FilterBase
	{
		protected string[] _statuses;
		protected int[] _states;
		public virtual string Statuses
		{
			get
			{
				return string.Join(";", this._statuses);
			}
			set
			{
				this._statuses = value.Split(new char[]
				{
					';'
				});
			}
		}
		public virtual string States
		{
			get
			{
				return string.Join(";", (
					from s in this._states
					select s.ToString()).ToArray<string>());
			}
			set
			{
				this._states = (
					from s in value.Split(new char[]
					{
						';'
					})
					select int.Parse(s)).ToArray<int>();
			}
		}
		public override System.Linq.IQueryable<Application> ApplyFilter(System.Linq.IQueryable<Application> applications, IWorkplaceContext context)
		{
			return 
				from a in applications
				where a.AdditionalData.Id != 0L && this._statuses.Contains(a.AdditionalData.Status.Name) && this._states.Contains((int)a.State)
				select a;
		}
		public override void ApplyFilter(ICriteria applications, IWorkplaceContext context)
		{
			applications.CreateAlias("AdditionalData", "ad");
			applications.CreateAlias("ad.Status", "status");
			applications.Add(Restrictions.In("State", this._states));
			applications.Add(Restrictions.In("status.Name", this._statuses));
			applications.Add(Restrictions.Not(Restrictions.Eq("Id", 0L)));
		}
	}
}
