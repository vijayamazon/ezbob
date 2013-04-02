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
        public static void CreateStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Status",
                    Name = "CreditResult",
                    Index = "CreditResult",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = true,
                    Fixed = false,
                    DataType = TypeCode.String,
                    Data = x => x.CreditResult
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

        public static void CreateCartColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
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
                    Width = 50,
                    DataType = TypeCode.String,
                    Data = x => x.Medal.HasValue ? x.Medal.ToString() : "-"
                });
        }

        public static void CreateDateApplyedColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Date Applyed",
                    Name = "OfferStart",
                    Index = "OfferStart",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Sortable = true,
                    Search = true,
                    Width = 115,
                    DataType = TypeCode.DateTime,
                    Data = x => x.OfferStart,
                    DateColumn = true,
                    Formatter = "dateNative",
                });
        }

        public static void CreateLoanAmountColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Loan amount",
                    Name = "CreditSum",
                    Index = "CreditSum",
                    Search = true,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 100,
                    Formatter = "tooltipText",
                    DataType = TypeCode.Decimal,
                    Data = x => x.CreditSum ?? 0
                });
        }

        public static void CreateIdColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "#",
                Name = "Id",
                Index = "Id",
                Width = 60,
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
                    Data = x => new { text = x.RefNumber, id = x.Id }
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
                Data = x =>  x.RefNumber
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
                    Data = x => new{text= (x.PersonalInfo != null) ? x.PersonalInfo.Fullname : " - ", id= x.Id}
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
                    Caption = "Date approved",
                    Name = "DateApproved",
                    Index = "DateApproved",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 105,
                    DataType = TypeCode.DateTime,
                    Formatter = "dateNative",
                    Data = x => x.DateApproved
                });
        }

        public static void CreateMPStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
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
                Width = 95,
                DataType = TypeCode.Boolean,
                Data = x => !x.IsSuccessfullyRegistered ? "registered" : "credit calculation"
            });
        }
        public static void CreateRegisteredDateColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Registration Date",
                Name = "RegisteredDate",
                Index = "GreetingMailSentDate",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = false,
                Width = 130,
                DataType = TypeCode.DateTime,
                Data = x => x.GreetingMailSentDate,
                Formatter = "dateNative"
            });
        }

        public static void CreateEbayStatus(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
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
                Width = 95,
                DataType = TypeCode.String,
                Data = x => x.EbayStatus
            });
        }
        
        public static void CreateAmazonStatus(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
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
                Width = 115,
                Search = false,
                DataType = TypeCode.String,
                Data = x => x.AmazonStatus
            });
        }

        public static void CreatePayPalStatus(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
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
                Width = 110,
                DataType = TypeCode.String,
                Data = x => x.PayPalStatus
            });
        }

        public static void CreateWizardStep(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
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
                Width = 100,
                DataType = TypeCode.String,
                Data = x => x.WizardStep == WizardStepType.AllStep ? (object) "Passed" : x.WizardStep
            });
        }

        public static void CreateDelinquency(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
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
                Width = 100,
                DataType = TypeCode.Int16,
                Data = x => GetDelinquency(x)
            });
        }

        public static int GetDelinquency(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var result = 0;
            
            var loans = customer.Loans.Where(l=>l.Status == LoanStatus.Late).ToList();
            if (!loans.Any()) return 0;

            var scheduleDate=DateTime.UtcNow;

            foreach (var loanScheduleItem in loans.SelectMany(loan => loan.Schedule.Where(loanScheduleItem => loanScheduleItem.Date < scheduleDate && loanScheduleItem.Status == LoanScheduleStatus.Late)))
            {
                scheduleDate = loanScheduleItem.Date;
            }
            
            var currentDate = DateTime.UtcNow.ToUniversalTime();
            if (scheduleDate <= currentDate)
                result = (currentDate - scheduleDate).Days;

            return result;
        }
    }
}