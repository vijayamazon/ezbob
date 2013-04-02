using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ReportManager
    {
        public List<PerformencePerUnderwriterData> CreateReportPerformenceData(DateFilter df)
        {
            var rep = ObjectFactory.GetInstance<IPerformencePerUnderwriterReportRepository>();
            var underwriters = rep.GetData(df.FormatFromDate(), df.FormatToDate()).Select(x => new PerformencePerUnderwriterData(x));
            return underwriters.ToList();
        }

        public bool IsHighScored(CashRequest data)
        {
            return data.UnderwriterDecision == CreditResultStatus.Approved && data.SystemDecision != SystemDecision.Approve;
        }

        public bool IsLowSide(CashRequest data)
        {
            return data.UnderwriterDecision == CreditResultStatus.Rejected && data.SystemDecision != SystemDecision.Reject;
        }

        public long GetTime(CashRequest data)
        {
            return data.SystemDecisionDate != null
                       ? (data.UnderwriterDecisionDate.Value - data.SystemDecisionDate.Value).Ticks : 0;
        }

        public List<PerformencePerMedalData> CreateReportPerformenceDataByMedal(DateFilter df)
        {
            var rep = ObjectFactory.GetInstance<IPerformencePerMedalReportRepository>();
            var medals = rep.GetData(df.FormatFromDate(), df.FormatToDate()).Select(x => new PerformencePerMedalData(x));
            return medals.ToList();
        }

        public List<ExposurePerMedalData> CreateReportExposureDataByMedal(DateFilter df)
        {
            var rep = ObjectFactory.GetInstance<IExposurePerMedalReportRepository>();
            var medals = rep.GetData(df.FormatFromDate(), df.FormatToDate()).Select(x => new ExposurePerMedalData(x));
            return medals.ToList();
        }

        public List<ExposurePerUnderwriterData> CreateReportExposureDataByUnderwriter(DateFilter df)
        {
            var rep = ObjectFactory.GetInstance<IExposurePerUnderwriterReportRepository>();
            var medals = rep.GetData(df.FormatFromDate(), df.FormatToDate()).Select(x => new ExposurePerUnderwriterData(x));
            return medals.ToList();
        }

        public List<MedalStatisticData> CreateMedalStatisticReport(DateFilter df)
        {
            var rep = ObjectFactory.GetInstance<IMedalStatisticReportRepository>();
            var medals = rep.GetData(df.FormatFromDate(), df.FormatToDate()).Select(x => new MedalStatisticData(x));
            return medals.ToList();
        }

        public List<DailyReportData> CreateDayliReport(DateTime date)
        {
            var rep = ObjectFactory.GetInstance<IDailyReportRepository>();
            var data = rep.GetData(date).Select(x => new DailyReportData(x));
            return data.ToList();
        }
    }
}