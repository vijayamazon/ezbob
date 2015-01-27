using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public interface IExposurePerMedalReportRepository : IRepository<ExposurePerMedalDataRow>
    {
        IList<ExposurePerMedalDataRow> GetData(DateTime start, DateTime end);
    }

    public class ExposurePerMedalReportRepository : NHibernateRepositoryBase<ExposurePerMedalDataRow>, IExposurePerMedalReportRepository
    {
        public ExposurePerMedalReportRepository(ISession session)
            : base(session)
        {
        }

        public IList<ExposurePerMedalDataRow> GetData(DateTime start, DateTime end)
        {
            return Session.GetNamedQuery("ExposurePerMedalReport")
                .SetParameter("dateStart", start)
                .SetParameter("dateEnd", end)
                .List<ExposurePerMedalDataRow>();
        }
    }
}
