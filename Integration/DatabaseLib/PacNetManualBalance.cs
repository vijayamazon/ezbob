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
		void DisableCurrents();
	}

    public class PacNetManualBalanceRepository : NHibernateRepositoryBase<PacNetManualBalance>, IPacNetManualBalanceRepository
    {
	    private PacNetBalanceRepository pacNetBalanceRepository;

		public PacNetManualBalanceRepository(ISession session)
			: base(session)
        {
			pacNetBalanceRepository = new PacNetBalanceRepository(session);
        }

		public int GetBalance()
		{
			return GetCurrentActive().Sum(row => row.Amount);
		}

		public void DisableCurrents()
		{
			foreach (PacNetManualBalance manualBalance in GetCurrentActive())
			{  
				manualBalance.Enabled = false;
				SaveOrUpdate(manualBalance);
			}
		}

		private IEnumerable<PacNetManualBalance> GetCurrentActive()
		{
			DateTime? lastReportTime = pacNetBalanceRepository.GetAll().Max(a => a.Date);
			DateTime lastNonManual = lastReportTime != null ? lastReportTime.Value.AddDays(1) : DateTime.Today;
			return _session.Query<PacNetManualBalance>().Where(a => a.Enabled && a.Date >= lastNonManual);
		}
    }
}