using System;
using System.Globalization;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public enum TimeFilterTypes { AllRange, ThisWeek, PrevWeek, ThisMonth, PrevMonth, Month3, Month6, Month12, ThisYear, PrevYear, User }

    public class DateFilter
    {
        public string StringFrom
        {
            get
            {
                return From != null ? From.Value.ToShortDateString() : "";
            }
            set { From = Convert.ToDateTime(value); } 
        }

        public string StringTo
        {
            get
            {
                return To != null ? To.Value.ToShortDateString() : "";
            }
            set { To = Convert.ToDateTime(value); }
        }

        public DateTime FormatFromDate()
        {
            var dt = DateTime.Now;
            switch (TimeFilter)
            {
                case TimeFilterTypes.AllRange:
                    return new DateTime(1753, 1, 1);
                case TimeFilterTypes.ThisWeek:
                    return GetFirstDayOfTheWeek(DateTime.Now);
                case TimeFilterTypes.PrevWeek:
                    return new DateTime(dt.Year, dt.Month, 1);
                case TimeFilterTypes.ThisMonth:
                    return new DateTime(dt.Year, dt.Month, 1);
                case TimeFilterTypes.PrevMonth:
                    return new DateTime(dt.Year, dt.Month, 1).AddMonths(-1);
                case TimeFilterTypes.Month3:
                    return new DateTime(dt.Year, dt.Month, dt.Day).AddMonths(-3);
                case TimeFilterTypes.Month6:
                    return new DateTime(dt.Year, dt.Month, dt.Day).AddMonths(-6);
                case TimeFilterTypes.Month12:
                    return new DateTime(dt.Year, dt.Month, dt.Day).AddYears(-1);
                case TimeFilterTypes.ThisYear:
                    return new DateTime(dt.Year, 1, 1);
                case TimeFilterTypes.PrevYear:
                    return new DateTime(dt.Year, 1, 1).AddYears(-1);
                case TimeFilterTypes.User:
                    return From.HasValue ? From.Value : new DateTime(1753, 1, 1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DateTime FormatToDate()
        {
            var dt = DateTime.Now;
            switch (TimeFilter)
            {
                case TimeFilterTypes.AllRange:
                    return new DateTime(9999, 12, 31);
                case TimeFilterTypes.ThisWeek:
                case TimeFilterTypes.ThisMonth:
                case TimeFilterTypes.ThisYear:
                case TimeFilterTypes.Month3:
                case TimeFilterTypes.Month6:
                case TimeFilterTypes.Month12:
                    return DateTime.Now;
                case TimeFilterTypes.PrevWeek:
                    return new DateTime(9999, 12, 31);
                case TimeFilterTypes.PrevMonth:
                    return new DateTime(dt.Year, dt.Month, 1).AddDays(-1);
                case TimeFilterTypes.PrevYear:
                    return new DateTime(dt.Year, 1, 1).AddDays(-1);
                case TimeFilterTypes.User:
                    return To.HasValue ? To.Value : new DateTime(9999, 12, 31);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DateTime GetFirstDayOfTheWeek(DateTime dt)
        {
            var firstDay = new CultureInfo("en-GB").DateTimeFormat.FirstDayOfWeek;
            var firstDayInWeek = new DateTime(dt.Year, dt.Month, dt.Day);
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);
            return firstDayInWeek;
        }

        public TimeFilterTypes TimeFilter { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string TimeFilterString
        {
            get
            {
                switch (TimeFilter)
                {
                    case TimeFilterTypes.AllRange:
                        return "all";
                    case TimeFilterTypes.ThisWeek:
                        return "thisWeek";
                    case TimeFilterTypes.PrevWeek:
                        return "prevWeek";
                    case TimeFilterTypes.ThisMonth:
                        return "thisMonth";
                    case TimeFilterTypes.PrevMonth:
                        return "prevMonth";
                    case TimeFilterTypes.Month3:
                        return "3Month";
                    case TimeFilterTypes.Month6:
                        return "6Month";
                    case TimeFilterTypes.Month12:
                        return "12Month";
                    case TimeFilterTypes.ThisYear:
                        return "thisYear";
                    case TimeFilterTypes.PrevYear:
                        return "prevYear";
                    case TimeFilterTypes.User:
                        return "user";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (value)
                {
                    case "all":
                        TimeFilter = TimeFilterTypes.AllRange;
                        break;
                    case "thisWeek":
                        TimeFilter = TimeFilterTypes.ThisWeek;
                        break;
                    case "prevWeek":
                        TimeFilter = TimeFilterTypes.PrevWeek;
                        break;
                    case "thisMonth":
                        TimeFilter = TimeFilterTypes.ThisMonth;
                        break;
                    case "prevMonth":
                        TimeFilter = TimeFilterTypes.PrevMonth;
                        break;
                    case "3Month":
                        TimeFilter = TimeFilterTypes.Month3;
                        break;
                    case "6Month":
                        TimeFilter = TimeFilterTypes.Month6;
                        break;
                    case "12Month":
                        TimeFilter = TimeFilterTypes.Month12;
                        break;
                    case "thisYear":
                        TimeFilter = TimeFilterTypes.ThisYear;
                        break;
                    case "prevYear":
                        TimeFilter = TimeFilterTypes.PrevYear;
                        break;
                    case "user":
                        TimeFilter = TimeFilterTypes.User;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}