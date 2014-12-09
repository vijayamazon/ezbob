using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ReportModelDayBase
    {
        public DayDate DayDate { get; set; }
        public string DownloadAction { get; set; }
    }

    public class DayDate
    {
        public DateTime? Day { get; set; }
        public string StringDate
        {
            get
            {
                return Day != null ? Day.Value.ToShortDateString() : "";
            }
            set { Day = Convert.ToDateTime(value); }
        }

        public static DayDate FromString(string incomingData)
        {
            return new DayDate() {StringDate = incomingData};
        }
    }

    public class DayDateModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var incomingData = bindingContext.ValueProvider.GetValue("DayDate").AttemptedValue;
            return DayDate.FromString(incomingData);
        }
    }

    public class ReportModelDay<T> : ReportModelDayBase
    {
        public ReportTableModel<T> Model { get; set; }
    }

    public class ReportModelBase
    {
        public DateFilter DateFilter { get; set; }

        private static readonly List<SelectListItem> Sl = new List<SelectListItem>
                                                      {
                                                        new SelectListItem{ Text = "All range", Value = "all", Selected = true},
                                                        new SelectListItem{ Text = "This week", Value = "thisWeek"},
                                                        new SelectListItem{ Text = "Previous week", Value = "prevWeek"},
                                                        new SelectListItem{ Text = "This month", Value = "thisMonth"},
                                                        new SelectListItem{ Text = "Previous month", Value = "prevMonth"},
                                                        new SelectListItem{ Text = "Last 3 monthes", Value = "3Month"},
                                                        new SelectListItem{ Text = "Last 6 monthes", Value = "6Month"},
                                                        new SelectListItem{ Text = "Last 12 monthes", Value = "12Month"},
                                                        new SelectListItem{ Text = "This year", Value = "thisYear"},
                                                        new SelectListItem{ Text = "Previous year", Value = "prevYear"},
                                                        new SelectListItem{ Text = "---User defined---", Value = "user"}
                                                      };

        public static IEnumerable<SelectListItem> FilterTypes
        {
            get { return Sl; }
        }
    }

    public class ReportCountAmountModelBase : ReportModelBase
    {
        public bool AccountView { get; set; }
        public string DownloadCountAction { get; set; }
        public string DownloadAmountAction { get; set; }
    }

    public class ReportCountAmountModel<T> : ReportCountAmountModelBase
    {
        public ReportTableModel<T> CountModel { get; set; }
        public ReportTableModel<T> AmountModel { get; set; }
    }

    public class ReportModelStandartBase : ReportModelBase
    {
        public string DownloadAction { get; set; }
    }

    public class ReportModel<T> : ReportModelStandartBase
    {
        public ReportTableModel<T> Model { get; set; }
    }
}
