using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface IPerformencePerMedalReportRepository : IRepository<PerformencePerMedalDataRow>
    {
        IList<PerformencePerMedalDataRow> GetData(DateTime start, DateTime end);
    }

    public class PerformencePerMedalReportRepository : NHibernateRepositoryBase<PerformencePerMedalDataRow>, IPerformencePerMedalReportRepository
    {
        public PerformencePerMedalReportRepository(ISession session)
            : base(session)
        {
        }

        public IList<PerformencePerMedalDataRow> GetData(DateTime start, DateTime end)
        {
            return Session.GetNamedQuery("PerformencePerMedalReport")
                .SetParameter("dateStart", start)
                .SetParameter("dateEnd", end)
                .List<PerformencePerMedalDataRow>();
        }
    }
}
