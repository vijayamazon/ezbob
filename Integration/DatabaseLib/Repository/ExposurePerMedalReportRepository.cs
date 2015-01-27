using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface IExposurePerUnderwriterReportRepository : IRepository<ExposurePerUnderwriterDataRow>
    {
        IList<ExposurePerUnderwriterDataRow> GetData(DateTime start, DateTime end);
    }

    public class ExposurePerUnderwriterReportRepository : NHibernateRepositoryBase<ExposurePerUnderwriterDataRow>, IExposurePerUnderwriterReportRepository
    {
        public ExposurePerUnderwriterReportRepository(ISession session)
            : base(session)
        {
        }

        public IList<ExposurePerUnderwriterDataRow> GetData(DateTime start, DateTime end)
        {
            return Session.GetNamedQuery("ExposurePerUnderwriterReport")
                .SetParameter("dateStart", start)
                .SetParameter("dateEnd", end)
                .List<ExposurePerUnderwriterDataRow>();
        }
    }
}
