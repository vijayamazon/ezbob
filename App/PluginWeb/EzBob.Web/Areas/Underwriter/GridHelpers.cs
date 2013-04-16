using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using OTP.Workplace.Code.jqGrid;
using PluginWebApp.Code.jqGrid;
using System.Linq;

namespace EzBob.Web.Areas.Underwriter
{
    public static class GridHelpers
    {
        /*
     * Helpers
     */

        public static int GetDelinquency(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var result = 0;

            var loans = customer.Loans.Where(l => l.Status == LoanStatus.Late).ToList();
            if (!loans.Any()) return 0;

            var scheduleDate = DateTime.UtcNow;

            foreach (
                var loanScheduleItem in
                    loans.SelectMany(
                        loan =>
                        loan.Schedule.Where(
                            loanScheduleItem =>
                            loanScheduleItem.Date < scheduleDate && loanScheduleItem.Status == LoanScheduleStatus.Late))
                )
            {
                scheduleDate = loanScheduleItem.Date;
            }

            var currentDate = DateTime.UtcNow.ToUniversalTime();
            if (scheduleDate <= currentDate)
                result = (currentDate - scheduleDate).Days;

            return result;
        }

        public static string CalculateStatus(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var decisionHistory = customer.DecisionHistory.LastOrDefault();
            if (decisionHistory == null)
            {
                return "N/A";
            }
            return decisionHistory.Action.ToString();
        }

        public static DecisionHistory GetLastHistory(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            return customer.DecisionHistory.LastOrDefault();
        }

        /*
         * Columns
         */
        public static void CreateStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = " Customer Status",
                    Name = "CreditResult",
                    Index = "CreditResult",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 120,
                    DataType = TypeCode.String,
                    Data = x => x.CreditResult.ToString()
                });
        }

        public static void CreatePhoneColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Mobile Phone",
                    Name = "MobilePhone",
                    Index = "PersonalInfo.MobilePhone",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    DataType = TypeCode.String,
                    Data = x => x.PersonalInfo.MobilePhone
                });
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Daytime Phone",
                    Name = "DaytimePhone",
                    Index = "PersonalInfo.DaytimePhone",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    DataType = TypeCode.String,
                    Data = x => x.PersonalInfo.DaytimePhone
                });
        }

        public static void CreateEmailColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "E-Mail",
                    Name = "Name",
                    Index = "Name",
                    Formatter = "withScrollbar",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 200,
                    DataType = TypeCode.String,
                    Data = x => x.Name
                });
        }

        public static void CreateCartColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel,
                                            bool showIcon = false)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Cart",
                    Name = "Medal",
                    Index = "Medal",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Width = 30,
                    DataType = TypeCode.String,
                    Formatter = showIcon ? "showMedalIcon" : "",
                    Data = x => x.Medal.HasValue ? x.Medal.ToString() : "-"
                });
        }

        public static void CreateDateApplyedColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Apply Date",
                    Name = "OfferStart",
                    Index = "OfferStart",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Sortable = true,
                    Search = true,
                    Width = 80,
                    DataType = TypeCode.DateTime,
                    Data = x => x.OfferStart,
                    DateColumn = true,
                    Formatter = "dateNative",
                });
        }

        public static void CreateSystemCalculatedSum(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Calc. Amount",
                    Name = "Amount",
                    Index = "SystemCalculatedSum",
                    Search = false,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Sortable = false,
                    Fixed = false,
                    Width = 75,
                    DataType = TypeCode.Decimal,
                    Data = x => x.LastCashRequest.SystemCalculatedSum ?? 0
                });
        }

        public static void CreateManualyApprovedSum(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Approved Manually",
                    Name = "ManualAmount",
                    Index = "ManualSum",
                    Search = false,
                    Sortable = false,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 100,
                    DataType = TypeCode.Decimal,
                    Data = x => x.LastCashRequest.ManagerApprovedSum ?? 0
                });
        }

        public static void CreateAmountTaken(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Amount Taken",
                    Name = "AmountTaken",
                    Index = "AmountTaken",
                    Search = false,
                    Sortable = false,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 80,
                    DataType = TypeCode.Decimal,
                    Data = x => x.Loans.Sum(y => y.LoanAmount)
                });
        }

        public static void CreateNumApprovals(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Approves#",
                    Name = "NumApprovals",
                    Index = "NumApprovals",
                    Search = false,
                    Sortable = false,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 65,
                    DataType = TypeCode.String,
                    Data = x => x.DecisionHistory.Count(y => y.Action == DecisionActions.Approve)
                });
        }

        public static void CreateNumRejections(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Rejects#",
                    Name = "Numrejections",
                    Index = "Numrejections",
                    Search = false,
                    Sortable = false,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 55,
                    DataType = TypeCode.String,
                    Data = x => x.DecisionHistory.Count(y => y.Action == DecisionActions.Reject)
                });
        }

        public static void CreateOfferExpiryDate(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Offer Expiry Date",
                    Name = "OfferValidUntil",
                    Index = "OfferValidUntil",
                    Search = true,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 90,
                    Formatter = "CheckDateWithNow",
                    DataType = TypeCode.DateTime,
                    Data = x => x.OfferValidUntil
                });
        }

        public static void CreateIdColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "#",
                    Name = "Id",
                    Index = "Id",
                    Width = 35,
                    Key = true,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Frozen = true,
                    Fixed = true,
                    DataType = TypeCode.Int32,
                    Formatter = "profileLink",
                    Data = x => x.Id
                });
        }

        public static void CreateRefNumColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "#",
                    Name = "RefNumber",
                    Index = "RefNumber",
                    Width = 60,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = true,
                    Fixed = true,
                    Formatter = "profileLink",
                    DataType = TypeCode.String,
                    Data = x => new {text = x.RefNumber, id = x.Id}
                });
        }

        public static void CreateRefNumWithoutLinkColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "#",
                    Name = "RefNumber",
                    Index = "RefNumber",
                    Width = 60,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = true,
                    DataType = TypeCode.String,
                    Data = x => x.RefNumber
                });
        }

        public static void CreateNameColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Name",
                    Name = "PersonalInfo.Fullname",
                    Index = "PersonalInfo.Fullname",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Frozen = true,
                    Fixed = false,
                    Formatter = "profileLink",
                    DataType = TypeCode.String,
                    Data = x => new {text = (x.PersonalInfo != null) ? x.PersonalInfo.Fullname : " - ", id = x.Id}
                });
        }

        public static void CreateEscalationReasonColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Escalation reason",
                    Name = "EscalationReason",
                    Index = "EscalationReason",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    DataType = TypeCode.String,
                    Data = x => x.EscalationReason
                });
        }

        public static void CreateUnderwriterNameColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Underwriter name",
                    Name = "UnderwriterName",
                    Index = "UnderwriterName",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    DataType = TypeCode.String,
                    Data = x => x.UnderwriterName
                });
        }

        public static void CreateDateEscalatedColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Date escalated",
                    Name = "DateEscalated",
                    Index = "DateEscalated",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 115,
                    DataType = TypeCode.DateTime,
                    Formatter = "dateNative",
                    Data = x => x.DateEscalated
                });
        }

        public static void CreateDateRejectedColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Reject Date",
                    Name = "DateRejected",
                    Index = "DateRejected",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 85,
                    DataType = TypeCode.DateTime,
                    Formatter = "dateNative",
                    Data = x => x.DateRejected
                });
        }

        public static void CreateRejectedReasonColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Rejected reason",
                    Name = "RejectedReason",
                    Index = "RejectedReason",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 115,
                    DataType = TypeCode.String,
                    Data = x => x.RejectedReason
                });
        }

        public static void CreateManagerNameColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Manager Name",
                    Name = "ManagerName",
                    Index = "ManagerName",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    DataType = TypeCode.String,
                    Data = x => x.ManagerName
                });
        }

        public static void CreateDateApprovedColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Approve Date",
                    Name = "DateApproved",
                    Index = "DateApproved",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 80,
                    DataType = TypeCode.DateTime,
                    Formatter = "dateNative",
                    Data = x => x.DateApproved
                });
        }

        public static void CreateMpStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "MP Status",
                    Name = "MPStatus",
                    Index = "MPStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Sortable = true,
                    Width = 85,
                    DataType = TypeCode.String,
                    Data = x => x.MPStatus
                });
        }

        public static void CreateUserStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "User Status",
                    Name = "IsSuccessfullyRegistered",
                    Index = "IsSuccessfullyRegistered",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Width = 87,
                    DataType = TypeCode.Boolean,
                    Data = x => !x.IsSuccessfullyRegistered ? "registered" : "credit calculation"
                });
        }

        public static void CreateRegisteredDateColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Reg. Date",
                    Name = "RegisteredDate",
                    Index = "GreetingMailSentDate",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = true,
                    Sortable = true,
                    Width = 73,
                    DataType = TypeCode.DateTime,
                    Data = x => x.GreetingMailSentDate,
                    Formatter = "dateNative"
                });
        }

        public static void CreateEbayStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "eBay status",
                    Name = "EbayStatus",
                    Index = "EbayStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Width = 85,
                    DataType = TypeCode.String,
                    Data = x => x.EbayStatus
                });
        }

        public static void CreateAmazonStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Amazon status",
                    Name = "AmazonStatus",
                    Index = "AmazonStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 105,
                    Search = false,
                    DataType = TypeCode.String,
                    Data = x => x.AmazonStatus
                });
        }

        public static void CreateEkmStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Ekm status",
                    Name = "EkmStatus",
                    Index = "EkmStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 75,
                    Search = false,
                    DataType = TypeCode.String,
                    Data = x => x.EkmStatus
                });
        }

        public static void CreatePayPalStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "PayPal Status",
                    Name = "PayPalStatus",
                    Index = "PayPalStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Width = 95,
                    DataType = TypeCode.String,
                    Data = x => x.PayPalStatus
                });
        }

        public static void CreateWizardStepColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Wizard Step",
                    Name = "WizardStep",
                    Index = "WizardStep",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Width = 95,
                    DataType = TypeCode.String,
                    Data = x => x.WizardStep == WizardStepType.AllStep ? (object) "Passed" : x.WizardStep
                });
        }

        public static void CreateDelinquencyColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Delinquency",
                    Name = "Delinquency",
                    Index = "Id",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Sortable = false,
                    Width = 70,
                    DataType = TypeCode.Int16,
                    Data = x => GetDelinquency(x)
                });
        }

        public static void CreateMpListColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "MPs",
                    Name = "MP",
                    Index = "Mp",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Sortable = false,
                    Width = 115,
                    DataType = TypeCode.String,
                    Formatter = "showMPsIcon",
                    Data = x => x.CustomerMarketPlaces.Select(y => y.Marketplace.Name).ToList()
                });
        }

        public static void CreateLastStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Current Status",
                    Name = "LastStatus",
                    Index = "LastStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Sortable = false,
                    Width = 65,
                    DataType = TypeCode.String,
                    Data = x => CalculateStatus(x)
                });
        }

        public static void CreateOutstandingBalanceColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "O/S Balance",
                    Name = "OutstandingBalance",
                    Index = "OutstandingBalance",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Sortable = false,
                    Width = 65,
                    DataType = TypeCode.String,
                    Data = x => x.Loans.Sum(y => y.Balance)
                });
        }

        public static void CreatePendingStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Pending Status",
                    Name = "PendingStatus",
                    Index = "PendingStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Sortable = false,
                    Width = 85,
                    DataType = TypeCode.String,
                    Data = x => x.PendingStatus.ToString()
                });
        }

        public static void CreateFirstLoanColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "First Loan Date",
                Name = "FirstLoanDate",
                Index = "Loans.Date",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = false,
                Sortable = false,
                Width = 85,
                DataType = TypeCode.String,
                Data = x => x.Loans.First().Date,
                Formatter = "dateNative"
            });
        }

        public static void CreateLastLoanDateColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Last Loan Date",
                Name = "LastLoanDate",
                Index = "Loans.Date",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = false,
                Sortable = false,
                Width = 85,
                DataType = TypeCode.String,
                Data = x => x.Loans.Last().Date,
                Formatter = "dateNative"
            });
        }

        public static void CreateLastLoanAmountColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Last Loan Amount",
                Name = "LastLoanAmount",
                Index = "Loans.Amount",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = false,
                Sortable = false,
                Width = 105,
                DataType = TypeCode.String,
                Data = x => x.Loans.Last().LoanAmount
            });
        }

        public static void CreateTotalPrincipalTakenColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Total Principal Taken",
                Name = "TotalPrincipalTaken",
                Index = "TotalPrincipalTaken",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = false,
                Sortable = false,
                Width = 115,
                DataType = TypeCode.String,
                Data = x => x.Loans.Sum(y=>y.LoanAmount)
            });
        }

        public static void CreateTotalPrincipalRepaidColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Total Principal Repaid",
                Name = "TotalPrincipalRepaid",
                Index = "TotalPrincipalRepaid",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = false,
                Sortable = false,
                Width = 115,
                DataType = TypeCode.String,
                Data = x => x.Loans.SelectMany(l => l.TransactionsWithPaypointSuccesefull).Sum(t => t.LoanRepayment)
            });
        }

        public static void CreateNextRepaymentDateColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Next Repayment Date",
                Name = "LastLoanAmount",
                Index = "Loans.Amount",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = false,
                Sortable = false,
                Width = 115,
                DataType = TypeCode.String,
                Formatter = "dateNative",
                Data = x => x.Loans.SelectMany(y => y.Schedule).Where(s => s.Status == LoanScheduleStatus.StillToPay || s.Status == LoanScheduleStatus.Late).OrderBy(s => s.Date).Select(s => s.Date).FirstOrDefault()
            });
        }
    }
}