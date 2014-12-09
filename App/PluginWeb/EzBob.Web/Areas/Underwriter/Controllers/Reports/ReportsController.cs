namespace EzBob.Web.Areas.Underwriter.Controllers.Reports
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Models.Reports;
	using Code;
	using StructureMap;

    public class ReportsController : Controller
    {
        //
        // GET: /Underwriter/Reports/
        public class SimpleData
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        private enum ReportType
        {
            PerformencePerUnderwriterCount,
            PerformencePerUnderwriterAmount,
            PerformencePerMedalCount,
            PerformencePerMedalAmount,
            ExposurePerUnderwriterCount,
            ExposurePerUnderwriterAmount,
            ExposurePerMedalCount,
            ExposurePerMedalAmount,
            MedalStatistic,
            DailyReport,
        }

        private readonly Dictionary<ReportType, List<ReportTableColumn>> _columns = new Dictionary<ReportType, List<ReportTableColumn>>();

        public ReportsController()
        {
            foreach (ReportType rp in Enum.GetValues(typeof(ReportType)))
            {
                if(IsAmountCountReport(rp))
                    AddPair(rp);
            }
            _columns.Add(ReportType.MedalStatistic, GetMedalStatisticColumns().ToList());
            _columns.Add(ReportType.DailyReport, GetDailyReportColumns().ToList());
        }

        private bool IsAmountCountReport(ReportType type)
        {
            switch (type)
            {
                case ReportType.PerformencePerUnderwriterCount:
                case ReportType.ExposurePerMedalCount:
                case ReportType.PerformencePerMedalCount:
                case ReportType.ExposurePerUnderwriterCount:
                case ReportType.PerformencePerMedalAmount:
                case ReportType.ExposurePerMedalAmount:
                case ReportType.PerformencePerUnderwriterAmount:
                case ReportType.ExposurePerUnderwriterAmount:
                    return true;
                default:
                    return false;
            }
        }

        private void AddPair(ReportType type)
        {
            _columns.Add(type, CreateColumns(type));
        }

        private List<ReportTableColumn> CreateColumns(ReportType type)
        {
            Func<IEnumerable<ReportTableColumn>> a = GetColumnFunction(type);
            return IsMedalReport(type) ? CreateAllColumns("Medal", "Medal", a, new CartImageProvider()) : CreateAllColumns("Underwriter", "Underwriter", a);
        }

        private Func<IEnumerable<ReportTableColumn>> GetColumnFunction(ReportType type)
        {
            switch (type)
            {
                case ReportType.PerformencePerUnderwriterCount:
                case ReportType.PerformencePerMedalCount:
                    return GetPerformanceCountColumns;
                case ReportType.ExposurePerMedalCount:
                case ReportType.ExposurePerUnderwriterCount:
                    return GetExposureCountColumns;
                case ReportType.PerformencePerUnderwriterAmount:
                case ReportType.PerformencePerMedalAmount:
                    return GetPerformanceAmountColumns;
                case ReportType.ExposurePerMedalAmount:
                case ReportType.ExposurePerUnderwriterAmount:
                    return GetExposureAmountColumns;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private bool IsMedalReport(ReportType type)
        {
            switch (type)
            {
                case ReportType.PerformencePerUnderwriterCount:
                case ReportType.PerformencePerUnderwriterAmount:
                case ReportType.ExposurePerUnderwriterCount:
                case ReportType.ExposurePerUnderwriterAmount:
                    return false;
                case ReportType.PerformencePerMedalCount:
                case ReportType.PerformencePerMedalAmount:
                case ReportType.ExposurePerMedalCount:
                case ReportType.ExposurePerMedalAmount:
                case ReportType.MedalStatistic:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private List<ReportTableColumn> CreateAllColumns(string caption, string fieldName, Func<IEnumerable<ReportTableColumn>> a, IImageProvider imgProvider = null)
        {
            var res = new List<ReportTableColumn> { new ReportTableColumn { Caption = caption, FieldName = fieldName, ImageProvider = imgProvider} };
            res.AddRange(a());
            return res;
        }

        private IEnumerable<ReportTableColumn> GetMedalStatisticColumns()
        {
            return new List<ReportTableColumn>
                       {
                           new ReportTableColumn {Caption = "Medals", FieldName = "Medal", ImageProvider = new CartImageProvider()},
                           new ReportTableColumn
                               {
                                   Caption = "Ebay stores",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "#", FieldName = "EbayStoresCount", DataType = ReportTableCreator.DataType.IntNumber},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Avg.",
                                                            FieldName = "EbayStoresAverage",
                                                            DataType = ReportTableCreator.DataType.Amount
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Amazon stores",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "#", FieldName = "AmazonStoresCount"},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Avg.",
                                                            FieldName = "AmazonStoresAverage",
                                                            DataType = ReportTableCreator.DataType.Amount
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "PayPal account",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "#", FieldName = "PayPalStoresCount"},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Avg.",
                                                            FieldName = "PayPalStoresAverage",
                                                            DataType = ReportTableCreator.DataType.Amount
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Experian Rating",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "Range", FieldName = "ExperianRatingRange"},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Avg.",
                                                            FieldName = "ExperianRatingAverage",
                                                            DataType = ReportTableCreator.DataType.Amount
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Annual Turnover",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "Range", FieldName = "AnualTurnoverRange"},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Avg.",
                                                            FieldName = "AnualTurnoverAverage",
                                                            DataType = ReportTableCreator.DataType.Amount
                                                        }
                                                }

                               },
                               new ReportTableColumn
                               {
                                   Caption = "Ebay Reviews",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "# Reviews", FieldName = "EbayReviews"},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Rating avg.",
                                                            FieldName = "EbayRating",
                                                        }
                                                }

                               },
                               new ReportTableColumn
                               {
                                   Caption = "Amazon Reviews",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "# Reviews", FieldName = "AmazonReviews"},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Rating avg.",
                                                            FieldName = "AmazonRating",
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Ezbob Rating",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "Range", FieldName = "ScorePointsRange"},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "Avg.",
                                                            FieldName = "ScorePointsAverage",
                                                            DataType = ReportTableCreator.DataType.Amount
                                                        }
                                                }

                               }
                       };
        }

        private IEnumerable<ReportTableColumn> GetExposureAmountColumns()
        {
            return new List<ReportTableColumn>
                       {
                           new ReportTableColumn {Caption = "Processed", FieldName = "ProcessedAmount", DataType = ReportTableCreator.DataType.Amount},
                           new ReportTableColumn
                               {
                                   Caption = "Approved",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "ApprovedAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "ApprovedAmountToProcessedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Paid",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "PaidAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "PaidAmountToApprovedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Late 1-30",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "Late30Amount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "Late30AmountToApprovedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Late 31-60",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "£", FieldName = "Late60Amount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "%",
                                                            FieldName = "Late60AmountToApprovedAmount", DataType = ReportTableCreator.DataType.Percents
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Late 61-90",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "Late90Amount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "Late90AmountToApprovedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Defaults 90+",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "DefaultsAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "DefaultsAmountToApprovedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Exposure",
                                   FieldName = "Exposure",
                                   DataType = ReportTableCreator.DataType.Amount
                               },
                           new ReportTableColumn
                               {
                                   Caption = "Open Credit Line",
                                   FieldName = "OpenCreditLine",
                                   DataType = ReportTableCreator.DataType.Amount
                               }
                       };
        }

        private IEnumerable<ReportTableColumn> GetExposureCountColumns()
        {
            return new List<ReportTableColumn>
                       {
                           new ReportTableColumn {Caption = "Processed", FieldName = "Processed", DataType = ReportTableCreator.DataType.IntNumber},
                           new ReportTableColumn
                               {
                                   Caption = "Approved",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "#", FieldName = "Approved", DataType = ReportTableCreator.DataType.IntNumber},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "ApprovedToProcessed", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Paid",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "#", FieldName = "Paid", DataType = ReportTableCreator.DataType.IntNumber},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "PaidToApproved", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Late 1-30",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "#", FieldName = "Late30", DataType = ReportTableCreator.DataType.IntNumber},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "Late30ToApproved", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Late 31-60",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "#", FieldName = "Late60", DataType = ReportTableCreator.DataType.IntNumber},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "%",
                                                            FieldName = "Late60ToApproved", DataType = ReportTableCreator.DataType.Percents
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Late 61-90",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "#", FieldName = "Late90", DataType = ReportTableCreator.DataType.IntNumber},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "Late90ToApproved", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Defaults 90+",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "#", FieldName = "Defaults", DataType = ReportTableCreator.DataType.IntNumber},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "DefaultsToApproved", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Exposure",
                                   FieldName = "Exposure",
                                   DataType = ReportTableCreator.DataType.Amount

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Open Credit Line",
                                   FieldName = "OpenCreditLine",
                                   DataType = ReportTableCreator.DataType.Amount
                               }
                       };
        }

        private IEnumerable<ReportTableColumn> GetPerformanceAmountColumns()
        {
            return new List<ReportTableColumn>
                       {
                           new ReportTableColumn {Caption = "Processed", FieldName = "ProcessedAmount", DataType = ReportTableCreator.DataType.Amount},
                           new ReportTableColumn
                               {
                                   Caption = "Approved",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "ApprovedAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "ApprovedAmountToProcessedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Reject",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "RejectedAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "RejectedAmountToProcessedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Escalated",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "EscalatedAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "EscalatedAmountToProcessedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "LatePayments",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn
                                                        {Caption = "£", FieldName = "LatePaymentsAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {
                                                            Caption = "%",
                                                            FieldName = "LatePaymentsAmountToApprovedAmount", DataType = ReportTableCreator.DataType.Percents
                                                        }
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Defaults",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "DefaultsAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "DefaultsAmountToApprovedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "HighSide",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "HighSideAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "HighSideAmountToProcessedAmount", DataType = ReportTableCreator.DataType.Percents}
                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "LowSide",
                                   Childs = new List<ReportTableColumn>
                                                {
                                                    new ReportTableColumn {Caption = "£", FieldName = "LowSideAmount", DataType = ReportTableCreator.DataType.Amount},
                                                    new ReportTableColumn
                                                        {Caption = "%", FieldName = "LowSideAmountToProcessedAmount", DataType = ReportTableCreator.DataType.Percents}

                                                }

                               },
                           new ReportTableColumn
                               {
                                   Caption = "Max Time</br> (hours:minutes:seconds)",
                                   FieldName = "MaxTime",
                                   DataType = ReportTableCreator.DataType.TimeInSeconds
                               },
                           new ReportTableColumn
                               {
                                   Caption = "Avg Time</br> (hours:minutes:seconds)",
                                   FieldName = "AvgTime",
                                   DataType = ReportTableCreator.DataType.TimeInSeconds
                               }
                       };
        }

        private IEnumerable<ReportTableColumn> GetPerformanceCountColumns()
        {
            return new List<ReportTableColumn>
                       {
                           new ReportTableColumn {Caption = "Processed", FieldName = "Processed", DataType = ReportTableCreator.DataType.IntNumber},
                                  new ReportTableColumn {Caption = "Approved", Childs = new List<ReportTableColumn>{
                                            new ReportTableColumn {Caption = "#", FieldName = "Approved", DataType = ReportTableCreator.DataType.IntNumber},
                                            new ReportTableColumn {Caption = "%", FieldName = "ApprovedToProcessed", DataType = ReportTableCreator.DataType.Percents}}

                                  },
                                  new ReportTableColumn {Caption = "Reject", Childs = new List<ReportTableColumn>{
                                            new ReportTableColumn {Caption = "#", FieldName = "Rejected", DataType = ReportTableCreator.DataType.IntNumber},
                                            new ReportTableColumn {Caption = "%", FieldName = "RejectedToProcessed", DataType = ReportTableCreator.DataType.Percents}}

                                  },
                                  new ReportTableColumn {Caption = "Escalated", Childs = new List<ReportTableColumn>{
                                            new ReportTableColumn {Caption = "#", FieldName = "Escalated", DataType = ReportTableCreator.DataType.IntNumber},
                                            new ReportTableColumn {Caption = "%", FieldName = "EscalatedToProcessed", DataType = ReportTableCreator.DataType.Percents}}

                                  },
                                  new ReportTableColumn {Caption = "LatePayments", Childs = new List<ReportTableColumn>{
                                            new ReportTableColumn {Caption = "#", FieldName = "LatePayments", DataType = ReportTableCreator.DataType.IntNumber},
                                            new ReportTableColumn {Caption = "%", FieldName = "LatePaymentsToApproved", DataType = ReportTableCreator.DataType.Percents}}

                                  },
                                  new ReportTableColumn {Caption = "Defaults", Childs = new List<ReportTableColumn>{
                                            new ReportTableColumn {Caption = "#", FieldName = "Defaults", DataType = ReportTableCreator.DataType.IntNumber},
                                            new ReportTableColumn {Caption = "%", FieldName = "DefaultsToApproved", DataType = ReportTableCreator.DataType.Percents}}

                                  },
                                  new ReportTableColumn {Caption = "HighSide", Childs = new List<ReportTableColumn>{
                                            new ReportTableColumn {Caption = "#", FieldName = "HighSide", DataType = ReportTableCreator.DataType.IntNumber},
                                            new ReportTableColumn {Caption = "%", FieldName = "HighSideToProcessed", DataType = ReportTableCreator.DataType.Percents}}

                                  },
                                  new ReportTableColumn {Caption = "LowSide", Childs = new List<ReportTableColumn>{
                                            new ReportTableColumn {Caption = "#", FieldName = "LowSide", DataType = ReportTableCreator.DataType.IntNumber},
                                            new ReportTableColumn {Caption = "%", FieldName = "LowSideToProcessed", DataType = ReportTableCreator.DataType.Percents}}

                                  },
                                  new ReportTableColumn {Caption = "Max Time</br> (hours:minutes:seconds)", FieldName = "MaxTime", DataType = ReportTableCreator.DataType.TimeInSeconds},
                                  new ReportTableColumn {Caption = "Avg Time</br> (hours:minutes:seconds)", FieldName = "AvgTime", DataType = ReportTableCreator.DataType.TimeInSeconds}
                       };
        }

        private IEnumerable<ReportTableColumn> GetDailyReportColumns()
        {
            return new List<ReportTableColumn>
                       {
                            new ReportTableColumn {Caption = "Date", FieldName = "Date", DataType = ReportTableCreator.DataType.Date},
                            new ReportTableColumn {Caption = "Loan ref. #", FieldName = "LoanRef"},
                            new ReportTableColumn {Caption = "Origination date", FieldName = "OriginationDate", DataType = ReportTableCreator.DataType.Date},
                            new ReportTableColumn {Caption = "Loan amount (£)", FieldName = "LoanAmount", DataType = ReportTableCreator.DataType.Amount},
                            new ReportTableColumn {Caption = "Customer name", FieldName = "CustomerName"},
                            new ReportTableColumn {Caption = "Expected (£)", FieldName = "Expected", DataType = ReportTableCreator.DataType.Amount},
                            new ReportTableColumn {Caption = "Paid (£)", FieldName = "Paid", DataType = ReportTableCreator.DataType.Amount},
                            new ReportTableColumn {Caption = "Loan balance (£)", FieldName = "LoanBalance", DataType = ReportTableCreator.DataType.Amount},
                            new ReportTableColumn {Caption = "Status", FieldName = "Status"}
                       };
        }

        [HttpGet]
        public ActionResult PerformenceReportPerUnderWriter()
        {
            var df = new DateFilter { From = DateTime.Now.AddMonths(-1), To = DateTime.Now };
            return View(CreatePerformencePerUnderwriterReportModel(df));
        }

        [HttpPost]
        public ActionResult PerformenceReportPerUnderWriter(ReportCountAmountModel<PerformencePerUnderwriterData> model)
        {
            return View(CreatePerformencePerUnderwriterReportModel(model.DateFilter));
        }

        [HttpGet]
        public ActionResult PerformenceReportPerMedal()
        {
            var df = new DateFilter { From = DateTime.Now.AddMonths(-1), To = DateTime.Now };
            return View(CreatePerformencePerMedalReportModel(df));
        }

        [HttpPost]
        public ActionResult PerformenceReportPerMedal(ReportCountAmountModel<PerformencePerMedalData> model)
        {
            return View(CreatePerformencePerMedalReportModel(model.DateFilter));
        }

        [HttpGet]
        public ActionResult ExposureReportPerMedal()
        {
            var df = new DateFilter { From = DateTime.Now.AddMonths(-1), To = DateTime.Now };
            return View(CreateExposurePerMedalReportModel(df));
        }

        [HttpPost]
        public ActionResult ExposureReportPerMedal(ReportCountAmountModel<ExposurePerMedalData> model)
        {
            return View(CreateExposurePerMedalReportModel(model.DateFilter));
        }

        [HttpGet]
        public ActionResult ExposureReportPerUnderwriter()
        {
            var df = new DateFilter { From = DateTime.Now.AddMonths(-1), To = DateTime.Now };
            return View(CreateExposurePerUnderwriterReportModel(df));
        }

        [HttpPost]
        public ActionResult ExposureReportPerUnderwriter(ReportCountAmountModel<ExposurePerUnderwriterData> model)
        {
            return View(CreateExposurePerUnderwriterReportModel(model.DateFilter));
        }

        [HttpGet]
        public ActionResult MedalStatisticReport()
        {
            var df = new DateFilter { From = DateTime.Now.AddMonths(-1), To = DateTime.Now };
            return View(CreateMedalStatisticReportModel(df));
        }

        [HttpPost]
        public ActionResult MedalStatisticReport(ReportCountAmountModel<ExposurePerUnderwriterData> model)
        {
            return View(CreateMedalStatisticReportModel(model.DateFilter));
        }

        [HttpGet]
        public ActionResult DailyReport()
        {
            var dateN = DateTime.Now;
            var date = new DateTime(dateN.Year, dateN.Month, dateN.Day);
            return View(CreateDailyReportModel(date));
        }

        [HttpPost]
        public ActionResult DailyReport(ReportModelDay<DailyReportData> model)
        {
            return View(CreateDailyReportModel(model.DayDate.Day.Value));
        }

        [HttpGet]
        public ActionResult ExpectationReport()
        {
            var dateN = DateTime.Now;
            var data = CreateExpectationReportModel(dateN);
            return View(data);
        }

        [HttpPost]
        public ActionResult ExpectationReport(ReportModelDay<DailyReportData> model)
        {
            var data = CreateExpectationReportModel(model.DayDate.Day.Value);
            return View(data);
        }

        public FileResult DownloadExpectationReport(DayDate date)
        {
            var report = CreateExpectationReportModel(date.Day.Value);
            return File(ReportTableCreator.CreateExcelFile(report.Model, false).ToArray(), "application/ms-excel", "Expectation_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        private ReportModelDay<DailyReportData> CreateDailyReportModel(DateTime date)
        {
            var rm = new ReportManager();
            var data = rm.CreateDayliReport(date);
            var model = new ReportModelDay<DailyReportData>();
            model.Model = new ReportTableModel<DailyReportData>();
            model.DayDate = new DayDate { Day = date };
            model.Model.Columns = _columns[ReportType.DailyReport];
            model.Model.Data = data;
            model.DownloadAction = "DownloadDayReport";
            return model;
        }

        private ReportModelDay<ExpectationReportData> CreateExpectationReportModel(DateTime date)
        {
            var builder = ObjectFactory.GetInstance<DailyReportBuilder>();
            var model = new ReportModelDay<ExpectationReportData>();
            var data = builder.GenerateReport(date.Year, date.Month, date.Day);

            var columns = new List<ReportTableColumn>(){
                            new ReportTableColumn {Caption = "Customer name", FieldName = "CustomerName", DataType = ReportTableCreator.DataType.Overall},
                            new ReportTableColumn {Caption = "Loan ref. #", FieldName = "LoanRef"},
                            new ReportTableColumn {Caption = "Origination date", FieldName = "OriginationDate", DataType = ReportTableCreator.DataType.Date},

                            new ReportTableColumn(){Caption = "Current outstanding amount brought forward(Before)", Childs = new List<ReportTableColumn>()
                                {
                                    new ReportTableColumn {Caption = "Principal", FieldValue = (o) => (double)((ExpectationReportData)o).Before.Principal, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Interest", FieldValue = (o) => (double)((ExpectationReportData)o).Before.Interest, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Fees", FieldValue = (o) => (double)((ExpectationReportData)o).Before.Fees, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Total", FieldValue = (o) => (double)((ExpectationReportData)o).Before.Balance, DataType = ReportTableCreator.DataType.Amount}
                                }},

                            new ReportTableColumn(){Caption = "Expected (£)", Childs = new List<ReportTableColumn>()
                                {
                                    new ReportTableColumn {Caption = "Principal", FieldValue = (o) => (double)((ExpectationReportData)o).Expected.Principal, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Interest", FieldValue = (o) => (double)((ExpectationReportData)o).Expected.Interest, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Fees", FieldValue = (o) => (double)((ExpectationReportData)o).Expected.Fees, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Total", FieldValue = (o) => (double)((ExpectationReportData)o).Expected.Total, DataType = ReportTableCreator.DataType.Amount}
                                }},

                            new ReportTableColumn(){Caption = "Paid (£)", Childs = new List<ReportTableColumn>()
                                {
                                    new ReportTableColumn {Caption = "Principal", FieldValue = (o) => (double)((ExpectationReportData)o).Paid.Principal, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Interest", FieldValue = (o) => (double)((ExpectationReportData)o).Paid.Interest, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Fees", FieldValue = (o) => (double)((ExpectationReportData)o).Paid.Fees, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Total", FieldValue = (o) => (double)((ExpectationReportData)o).Paid.Total, DataType = ReportTableCreator.DataType.Amount}
                                }},

                            new ReportTableColumn(){Caption = "Current outstanding amount brought forward(After)", Childs = new List<ReportTableColumn>()
                                {
                                    new ReportTableColumn {Caption = "Principal", FieldValue = (o) => (double)((ExpectationReportData)o).After.Principal, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Interest", FieldValue = (o) => (double)((ExpectationReportData)o).After.Interest, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Fees", FieldValue = (o) => (double)((ExpectationReportData)o).After.Fees, DataType = ReportTableCreator.DataType.Amount},
                                    new ReportTableColumn {Caption = "Total", FieldValue = (o) => (double)((ExpectationReportData)o).After.Total, DataType = ReportTableCreator.DataType.Amount}
                                }},

                            new ReportTableColumn {Caption = "Variance", FieldValue = (o) => (double)((ExpectationReportData)o).Variance, DataType = ReportTableCreator.DataType.Amount},
                            new ReportTableColumn {Caption = "Status", FieldValue = (o) => ((ExpectationReportData)o).Status, DataType = ReportTableCreator.DataType.Overall}
            };

            model.Model = new ReportTableModel<ExpectationReportData>();
            model.DayDate = new DayDate { Day = date };
            model.Model.Columns = columns;
            model.Model.Data = data;
            model.DownloadAction = "DownloadExpectationReport";
            return model;
        }

        public FileResult DownloadDayReport(DayDate date)
        {
            var report = CreateDailyReportModel( date.Day.Value );
            return File(ReportTableCreator.CreateExcelFile(report.Model, false).ToArray(), "application/ms-excel", "Daily_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadMedalStatisticReport(DateFilter df)
        {
            var report = CreateMedalStatisticReportModel(df);
            return File(ReportTableCreator.CreateExcelFile(report.Model).ToArray(), "application/ms-excel", "Medal_statistic_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadExposurePerUnderwriterCountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportExposureDataByUnderwriter(df), ReportType.ExposurePerUnderwriterCount, GetTotalExposureCount, "Underwriter");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Exposure_per_underwriter_count_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadExposurePerUnderwriterAmountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportExposureDataByUnderwriter(df), ReportType.ExposurePerUnderwriterAmount, GetTotalExposureAmount, "Underwriter");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Exposure_per_underwriter_amount_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadExposurePerMedalCountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportExposureDataByMedal(df), ReportType.ExposurePerMedalCount, GetTotalExposureCount, "Medal");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Exposure_per_medal_count_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadExposurePerMedalAmountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportExposureDataByMedal(df), ReportType.ExposurePerMedalAmount, GetTotalExposureAmount, "Medal");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Exposure_per_medal_amount_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadPerformencePerUnderwriterCountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportPerformenceData(df), ReportType.PerformencePerUnderwriterCount, GetTotalPerformenceCount, "Underwriter");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Performence_per_underwriter_count_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadPerformencePerUnderwriterAmountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportPerformenceData(df), ReportType.PerformencePerUnderwriterAmount, GetTotalPerformenceAmount, "Underwriter");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Performence_per_underwriter_amount_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadPerformencePerMedalCountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportPerformenceDataByMedal(df), ReportType.PerformencePerMedalCount, GetTotalPerformenceCount, "Medal");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Performence_per_medal_count_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        public FileResult DownloadPerformencePerMedalAmountReport(DateFilter df)
        {
            var rm = new ReportManager();
            var model = CreateCountReport(rm.CreateReportPerformenceDataByMedal(df), ReportType.PerformencePerMedalAmount, GetTotalPerformenceAmount, "Medal");
            return File(ReportTableCreator.CreateExcelFile(model).ToArray(), "application/ms-excel", "Performence_per_medal_amount_Report_" + DateTime.Now.ToShortDateString() + '_' + DateTime.Now.ToShortTimeString() + ".xls");
        }

        private ReportCountAmountModel<T> CreateReportModel<T>(DateFilter df, ReportType type1, ReportType type2, Func<DateFilter, List<T>> createreport, Func<List<T>, T> createTotalCount, Func<List<T>, T> createTotalAmount, string totalProperty, string downloadCountAction, string downloadAmountAction)
            where T : new() 
        {
            var res = createreport(df);
            var modelCount = CreateCountReport(res, type1, createTotalCount, totalProperty);
            var modelAmount = CreateAmountReport(res, type2, createTotalAmount, totalProperty);
            return new ReportCountAmountModel<T> { AmountModel = modelAmount, CountModel = modelCount, DateFilter = df, DownloadAmountAction = downloadAmountAction, DownloadCountAction = downloadCountAction};
        }

        private ReportTableModel<T> CreateCountReport<T>(List<T> data, ReportType type, Func<List<T>, T> createTotal, string totalProperty)
            where T : new() 
        {
            var modelCount = new ReportTableModel<T>();
            modelCount.Columns = _columns[type];
            modelCount.Data = data;
            modelCount.Total = createTotal(data);
            var p = modelCount.Total.GetType().GetProperty(totalProperty);
            p.SetValue(modelCount.Total, "Total", null);
            return modelCount;
        }

        private ReportTableModel<T> CreateAmountReport<T>(List<T> data, ReportType type, Func<List<T>, T> createTotal, string totalProperty)
            where T : new() 
        {
            var modelAmount = new ReportTableModel<T>();
            modelAmount.Columns = _columns[type];
            modelAmount.Data = data;
            T pd = data.Count > 0 ? createTotal(data) : new T();
            modelAmount.Total = pd;
            var pa = modelAmount.Total.GetType().GetProperty(totalProperty);
            pa.SetValue(modelAmount.Total, "Total", null);
            return modelAmount;
        }

        private ReportModel<MedalStatisticData> CreateMedalStatisticReportModel(DateFilter dateFilter)
        {
            var rm = new ReportManager();
            var data = rm.CreateMedalStatisticReport(dateFilter);
            var model = new ReportModel<MedalStatisticData>();
            model.Model = new ReportTableModel<MedalStatisticData>();
            model.DateFilter = dateFilter;
            model.Model.Columns = _columns[ReportType.MedalStatistic];
            model.Model.Data = data;
            model.Model.Total = GetTotalMedalStatistic(data);
            model.DownloadAction = "DownloadMedalStatisticReport";
            return model;
        }

        //TODO Переделать последующие 4 через параметризированную функцию
        private ReportCountAmountModel<PerformencePerMedalData> CreatePerformencePerMedalReportModel(DateFilter df)
        {
            var rm = new ReportManager();
            return CreateReportModel(df, ReportType.PerformencePerMedalCount,
                                                              ReportType.PerformencePerMedalAmount,
                                                              rm.CreateReportPerformenceDataByMedal,
                                                              GetTotalPerformenceCount, GetTotalPerformenceAmount, "Medal", "DownloadPerformencePerMedalCountReport", "DownloadPerformencePerMedalAmountReport");
        }

        private ReportCountAmountModel<PerformencePerUnderwriterData> CreatePerformencePerUnderwriterReportModel(DateFilter df)
        {
            var rm = new ReportManager();
            return CreateReportModel(df, ReportType.PerformencePerUnderwriterCount,
                                                              ReportType.PerformencePerUnderwriterAmount,
                                                              rm.CreateReportPerformenceData,
                                                              GetTotalPerformenceCount, GetTotalPerformenceAmount, "Underwriter", "DownloadPerformencePerUnderwriterCountReport", "DownloadPerformencePerUnderwriterAmountReport");
        }

        private ReportCountAmountModel<ExposurePerMedalData> CreateExposurePerMedalReportModel(DateFilter df)
        {
            var rm = new ReportManager();
            return CreateReportModel(df, ReportType.ExposurePerMedalCount,
                                                              ReportType.ExposurePerMedalAmount,
                                                              rm.CreateReportExposureDataByMedal,
                                                              GetTotalExposureCount, GetTotalExposureAmount, "Medal", "DownloadExposurePerMedalCountReport", "DownloadExposurePerMedalAmountReport");
        }

        private ReportCountAmountModel<ExposurePerUnderwriterData> CreateExposurePerUnderwriterReportModel(DateFilter df)
        {
            var rm = new ReportManager();
            return CreateReportModel(df, ReportType.ExposurePerUnderwriterCount,
                                                              ReportType.ExposurePerUnderwriterAmount,
                                                              rm.CreateReportExposureDataByUnderwriter,
                                                              GetTotalExposureCount, GetTotalExposureAmount, "Underwriter", "DownloadExposurePerUnderwriterCountReport", "DownloadExposurePerUnderwriterAmountReport");
        }

        private T GetTotalExposureAmount<T>(IEnumerable<T> res)
            where T : ExposureDataBase, new()
        {
            var result = new T();
            if (!res.Any())
                return result;
            var lst = new List<ExposureDataBase>(res);
            result.ApprovedAmount = lst.Sum(x => x.ApprovedAmount);
            result.ProcessedAmount = lst.Sum(x => x.ProcessedAmount);
            result.PaidAmount = lst.Sum(x => x.PaidAmount);
            result.Late30Amount = lst.Sum(x => x.Late30Amount);
            result.Late60Amount = lst.Sum(x => x.Late60Amount);
            result.Late90Amount = lst.Sum(x => x.Late90Amount);
            result.DefaultsAmount = lst.Sum(x => x.DefaultsAmount);
            result.Exposure = (long) lst.Sum(x => x.Exposure);
            result.OpenCreditLine = lst.Sum(x => x.OpenCreditLine);
            return result;
        }

        private T GetTotalExposureCount<T>(IEnumerable<T> res)
            where T : ExposureDataBase, new()
        {
            var result = new T();
            if (!res.Any())
                return result;
            var lst = new List<ExposureDataBase>(res);
            result.ApprovedAmount = lst.Sum(x => x.Approved);
            result.ProcessedAmount = lst.Sum(x => x.Processed);
            result.PaidAmount = lst.Sum(x => x.Paid);
            result.Late30Amount = lst.Sum(x => x.Late30);
            result.Late60Amount = lst.Sum(x => x.Late60);
            result.Late90Amount = lst.Sum(x => x.Late90);
            result.DefaultsAmount = lst.Sum(x => x.Defaults);
            result.Exposure = (long)lst.Sum(x => x.Exposure);
            result.OpenCreditLine = lst.Sum(x => x.OpenCreditLine);
            return result;
        }

        private T GetTotalPerformenceAmount<T>(IEnumerable<T> res)
            where T : PerformenceDataBase, new()
        {
            var result = new T();
            if (!res.Any())
                return result;
            var lst = new List<PerformenceDataBase>(res);
            result.ApprovedAmount = lst.Sum(x => x.ApprovedAmount);
            result.ProcessedAmount = lst.Sum(x => x.ProcessedAmount);
            result.RejectedAmount = lst.Sum(x => x.RejectedAmount);
            result.EscalatedAmount = lst.Sum(x => x.EscalatedAmount);
            result.HighSideAmount = lst.Sum(x => x.HighSideAmount);
            result.LowSideAmount = lst.Sum(x => x.LowSideAmount);
            result.LatePaymentsAmount = lst.Sum(x => x.LatePaymentsAmount);
            result.DefaultsAmount = lst.Sum(x => x.DefaultsAmount);
            result.MaxTime = lst.Max(x => x.MaxTime);
            result.AvgTime = (long) lst.Average(x => x.AvgTime);
            return result;
        }

        private T GetTotalPerformenceCount<T>(IEnumerable<T> res)
            where T : PerformenceDataBase, new()
        {
            var result = new T();
            if (!res.Any())
                return result;
            var lst = new List<PerformenceDataBase>(res);
            result.Approved = lst.Sum(x => x.Approved);
            result.Processed = lst.Sum(x => x.Processed);
            result.Rejected = lst.Sum(x => x.Rejected);
            result.Escalated = lst.Sum(x => x.Escalated);
            result.HighSide = lst.Sum(x => x.HighSide);
            result.LowSide = lst.Sum(x => x.LowSide);
            result.LatePayments = lst.Sum(x => x.LatePayments);
            result.Defaults = lst.Sum(x => x.Defaults);
            result.MaxTime = lst.Max(x => x.MaxTime);
            result.AvgTime = (long)lst.Average(x => x.AvgTime);
            return result;
        }

        private MedalStatisticData GetTotalMedalStatistic(IEnumerable<MedalStatisticData> res)
        {
            var result = new MedalStatisticData();
            if (!res.Any())
                return result;
            result.Medal = "Total";
            result.EbayStoresCount = res.Sum(x => x.EbayStoresCount);
            result.EbayStoresAverage = res.Average(x => x.EbayStoresAverage);
            result.AmazonStoresCount = res.Sum(x => x.AmazonStoresCount);
            result.AmazonStoresAverage = res.Average(x => x.AmazonStoresAverage);
            result.PayPalStoresCount = res.Sum(x => x.PayPalStoresCount);
            result.PayPalStoresAverage = res.Average(x => x.PayPalStoresAverage);
            result.ExperianRatingAverage = res.Sum(x => x.ExperianRatingAverage);
            result.ExperianRatingMax = res.Max(x => x.ExperianRatingMax);
            result.ExperianRatingMin = res.Min(x => x.ExperianRatingMin);
            result.AnualTurnoverAverage = res.Average(x => x.AnualTurnoverAverage);
            result.AnualTurnoverMin = res.Min(x => x.AnualTurnoverMin);
            result.AnualTurnoverMax = res.Min(x => x.AnualTurnoverMax);
            result.EbayStoresAverage = res.Average(x => x.EbayStoresAverage);
            result.EbayStoresCount = res.Sum(x => x.EbayStoresCount);
            result.AmazonStoresAverage = res.Average(x => x.AmazonStoresAverage);
            result.AmazonStoresCount = res.Sum(x => x.AmazonStoresCount);
            result.ScorePointsAverage = res.Average(x => x.ScorePointsAverage);
            result.ScorePointsMax = res.Max(x => x.ScorePointsMax);
            result.ScorePointsMin = res.Min(x => x.ScorePointsMin);
            return result;
        }
    }
}
