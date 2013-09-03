using System;
using EZBob.DatabaseLib.Model.Database;
using OTP.Workplace.Code.jqGrid;
using PluginWebApp.Code.jqGrid;
using PluginWebApp.Code.jqGrid.SearchOperators;

namespace EzBob.Web.Areas.Underwriter
{
    public static class GridHelpers
    {
        public static void CreateStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = " Customer Status",
                    Name = "CreditResult",
                    Index = "CustomerStatus",
                    DataType = TypeCode.String,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 120,
                    Data = x => x.CustomerStatus
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
                    Width = 80,
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
                    Width = 85,
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
                    Width = 50,
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
                    Search = true,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Sortable = true,
                    Fixed = false,
                    Width = 75,
                    DataType = TypeCode.Decimal,
                    Data = x => x.SystemCalculatedSum
                });
        }

        public static void CreateManualyApprovedSum(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Approved Sum",
                    Name = "ManualAmount",
                    Index = "ManagerApprovedSum",
                    Search = true,
                    Sortable = true,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 105,
                    DataType = TypeCode.Decimal,
                    Data = x => x.ManagerApprovedSum
                });
        }

        public static void CreateAmountTaken(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Amount Taken",
                    Name = "AmountTaken",
                    Index = "AmountTaken",
                    Search = true,
                    Sortable = true,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 80,
                    DataType = TypeCode.Decimal,
                    Data = x => x.AmountTaken
                });
        }
        public static void CreateLateAmount(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Late Amount",
                    Name = "LateAmount",
                    Index = "LateAmount",
                    Search = true,
                    Sortable = true,
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 80,
                    DataType = TypeCode.Decimal,
                    Data = x => x.LateAmount
                });
        }

        public static void CreateNumApprovals(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Approves#",
                    Name = "NumApprovals",
                    Index = "NumApproves",
                    Search = true,
                    Sortable = true,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 65,
                    DataType = TypeCode.Int32,
                    Data = x => x.NumApproves
                });
        }

        public static void CreateNumRejections(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Rejects#",
                    Name = "Numrejections",
                    Index = "NumRejects",
                    Search = true,
                    Sortable = true,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 55,
                    DataType = TypeCode.Int32,
                    Data = x => x.NumRejects
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
                    Search = true,
                    Formatter = "profileLink",
                    DataType = TypeCode.String,
                    Data = x => new {text = (x.PersonalInfo != null) ? x.PersonalInfo.Fullname : " - ", id = x.Id}
                });
        }

        public static void CreateEscalationReasonColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Escalation Reason",
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
                    Caption = "Underwriter Name",
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
                    Caption = "Escalation Date",
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
					Formatter = "withScrollbar",
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
                    Search = true,
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
                    Name = "WizardStep",
					Index = "WizardStep",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = false,
                    Width = 87,
                    DataType = TypeCode.Boolean,
                    Data = x => x.WizardStep != WizardStepType.AllStep ? "registered" : "credit calculation"
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
                    Caption = "eBay Status",
                    Name = "EbayStatus",
                    Index = "EbayStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = true,
                    Width = 65,
                    DataType = TypeCode.String,
                    Data = x => x.EbayStatus
                });
        }

        public static void CreateAmazonStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Amazon Status",
                    Name = "AmazonStatus",
                    Index = "AmazonStatus",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Width = 85,
                    Search = true,
                    DataType = TypeCode.String,
                    Data = x => x.AmazonStatus
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
				Search = true,
				Width = 75,
				DataType = TypeCode.String,
				Data = x => x.PayPalStatus
			});
		}

		public static void CreateEkmStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Ekm Status",
				Name = "EkmStatus",
				Index = "EkmStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 65,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.EkmStatus
			});
		}

		public static void CreateVolusionStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Volusion Status",
				Name = "VolusionStatus",
				Index = "VolusionStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 90,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.VolusionStatus
			});
		}

		public static void CreatePayPointStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "PayPoint Status",
				Name = "PayPointStatus",
				Index = "PayPointStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 85,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.PayPointStatus
			});
		}

		public static void CreatePlayStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Play Status",
				Name = "PlayStatus",
				Index = "PlayStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 60,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.PlayStatus
			});
		}

		public static void CreateYodleeStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Yodlee Status",
				Name = "YodleeStatus",
				Index = "YodleeStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 75,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.YodleeStatus
			});
		}

		public static void CreateFreeAgentStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "FreeAgent Status",
				Name = "FreeAgentStatus",
				Index = "FreeAgentStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 95,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.FreeAgentStatus
			});
		}

		public static void CreateSageStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Sage Status",
				Name = "SageStatus",
				Index = "SageStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 95,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.SageStatus
			});
		}

		public static void CreateShopifyStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Shopify Status",
				Name = "ShopifyStatus",
				Index = "ShopifyStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 80,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.ShopifyStatus
			});
		}

		public static void CreateXeroStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Xero Status",
				Name = "XeroStatus",
				Index = "XeroStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Width = 65,
				Search = true,
				DataType = TypeCode.String,
				Data = x => x.XeroStatus
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
                    DataType = TypeCode.Int32,
                    Data = x => x.WizardStep == WizardStepType.AllStep ? (object) "Passed" : x.WizardStep
                });
        }

        public static void CreateDelinquencyColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
                {
                    Caption = "Delinquency",
                    Name = "Delinquency",
                    Index = "Delinquency",
                    Resizable = false,
                    Align = Align.Center,
                    Title = false,
                    Hidden = false,
                    Fixed = false,
                    Search = true,
                    Sortable = true,
                    Width = 70,
                    DataType = TypeCode.Int32,
                    Data = x => x.Delinquency
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
                    Data = x=>x.MpList
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
                    Search = true,
                    Sortable = true,
                    Width = 90,
                    DataType = TypeCode.String,
                    Data = x => x.LastStatus
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
                    Search = true,
                    Sortable = true,
                    Width = 65,
                    DataType = TypeCode.Decimal,
                    Data = x => x.OutstandingBalance
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
                Index = "FirstLoanDate",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = true,
                Sortable = true,
                Width = 85,
                DataType = TypeCode.DateTime,
                Data = x => x.FirstLoanDate,
                Formatter = "dateNative"
            });
        }

        public static void CreateLastLoanDateColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Last Loan Date",
                Name = "LastLoanDate",
                Index = "LastLoanDate",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = true,
                Sortable = true,
                Width = 85,
                DataType = TypeCode.DateTime,
                Data = x => x.LastLoanDate,
                Formatter = "dateNative"
            });
        }

        public static void CreateLastLoanAmountColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Last Loan Amount",
                Name = "LastLoanAmount",
                Index = "LastLoanAmount",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = true,
                Sortable = true,
                Width = 105,
                DataType = TypeCode.Decimal,
                Data = x => x.LastLoanAmount
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
                Search = true,
                Sortable = true,
                Width = 115,
                DataType = TypeCode.Decimal,
                Data = x => x.TotalPrincipalRepaid
            });
        }

        public static void CreateNextRepaymentDateColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Next Repayment Date",
                Name = "NextRepaymentDate",
                Index = "NextRepaymentDate",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = true,
                Sortable = true,
                Width = 115,
                DataType = TypeCode.DateTime,
                Formatter = "dateNative",
                Data = x => x.NextRepaymentDate
            });
        }

        public static void DateOfLatePayment(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Date of Late Payment",
                Name = "DateOfLate",
                Index = "DateOfLate",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = true,
                Sortable = true,
                Width = 125,
                DataType = TypeCode.DateTime,
                Formatter = "dateNative",
                Data = x => x.DateOfLate
            });
        }

        public static void CreateManualyOfferDate(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Offer Date",
                Name = "OfferDate",
                Index = "OfferDate",
                Resizable = false,
                Align = Align.Center,
                Title = false,
                Hidden = false,
                Fixed = false,
                Search = true,
                Sortable = true,
                Width = 60,
                DataType = TypeCode.DateTime,
                Formatter = "dateNative",
                Data = x => x.OfferDate
            });
        }

		public static void CreateLatestCRMstatus(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "CRM status",
				Name = "LatestCRMstatus",
				Index = "LatestCRMstatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Search = true,
				Sortable = true,
				Width = 65,
				DataType = TypeCode.String,
				Data = x => x.LatestCRMstatus ?? string.Empty
			});
		}

		public static void CreateLatestCRMComment(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "CRM Comment",
				Name = "LatestCRMComment",
				Index = "LatestCRMComment",
				Formatter = "withScrollbar",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Search = true,
				Sortable = true,
				Width = 100,
				DataType = TypeCode.String,
				Data = x => x.LatestCRMComment ?? string.Empty
			});
		}

		public static void CreateAmountOfInteractions(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Interactions",
				Name = "AmountOfInteractions",
				Index = "AmountOfInteractions",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Search = true,
				Sortable = true,
				Width = 85,
				DataType = TypeCode.Decimal,
				Data = x => x.AmountOfInteractions
			});
		}

		public static void CreateCollectionStatusColumn(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
		{
			gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
			{
				Caption = "Collection Status",
				Name = "CollectionStatus",
				Index = "CollectionStatus",
				Resizable = false,
				Align = Align.Center,
				Title = false,
				Hidden = false,
				Fixed = false,
				Search = false,
				Sortable = false,
				Width = 95,
				DataType = TypeCode.String,
				Data = x => x.CollectionStatus.CurrentStatus.Name
			});
		}

        public static void CreatePaymentDemeanor(GridModel<EZBob.DatabaseLib.Model.Database.Customer> gridModel)
        {
            gridModel.AddColumn(new CriteriaColumn<EZBob.DatabaseLib.Model.Database.Customer>
            {
                Caption = "Payment Demeanor",
                Name = "PaymentDemeanor",
                Title = false,
                Hidden = true,
                Fixed = false,
                Search = false,
                Sortable = false,
                Width = 95,
                DataType = TypeCode.String,
                Formatter = "ColorCell",
                Data = x => x.PaymentDemenaor != PaymentdemeanorType.Ok ? "#ffdfdf" : ""
            });
        }
    }
}