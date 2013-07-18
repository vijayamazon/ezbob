namespace EZBob.DatabaseLib
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Linq;

    public class PacNetManualBalance
    {
		public int Id { get; set; }
		public string Username { get; set; }
		public int Amount { get; set; }
		public DateTime Date { get; set; }
		public bool Enabled { get; set; }
    }

    public class PacNetManualBalanceMap : ClassMap<PacNetManualBalance>
    {
		public PacNetManualBalanceMap()
        {
            Not.LazyLoad();
			Table("PacNetManualBalance");
			ReadOnly(); // qqq - should it be readonly?
			Id(x => x.Id);
			Map(x => x.Username).Length(100);
			Map(x => x.Amount);
			Map(x => x.Date);
			Map(x => x.Enabled);
        }
    }

	public interface IPacNetManualBalanceRepository : IRepository<PacNetManualBalance>
	{
		int GetBalance();
		void DisableTodays();
	}

    public class PacNetManualBalanceRepository : NHibernateRepositoryBase<PacNetManualBalance>, IPacNetManualBalanceRepository
    {
		public PacNetManualBalanceRepository(ISession session)
			: base(session)
        {
        }

		public int GetBalance()
		{
			return GetTodaysActive().Sum(row => row.Amount);
		}

		public void DisableTodays()
		{
			foreach (PacNetManualBalance manualBalance in GetTodaysActive())
			{
				Delete(manualBalance);
				manualBalance.Enabled = false;
				SaveOrUpdate(manualBalance);
			}
		}

		private IEnumerable<PacNetManualBalance> GetTodaysActive()
		{
			DateTime today = DateTime.UtcNow;
			return _session.Query<PacNetManualBalance>().Where(a => a.Enabled && a.Date.Year == today.Year && a.Date.Month == today.Month && a.Date.Day == today.Day);
		}
    }

}