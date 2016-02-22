namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils.Extensions;
	using Ezbob.Backend.ModelsWithDB;
	using PaymentServices.Calculators;

	public class DecisionHistoryModel {
		public int Id { get; set; }
		public string Action { get; set; }
		public DateTime Date { get; set; }
		public string Comment { get; set; }
		public string UnderwriterName { get; set; }
		public decimal InterestRate { get; set; }
		public int RepaymentPeriod { get; set; }
		public decimal ApprovedSum { get; set; }
		public string LoanType { get; set; }
		public int IsLoanTypeSelectionAllowed { get; set; }
		public string Originator { get; set; }
		public decimal BrokerSetupFee { get; set; }
		public decimal TotalSetupFee { get; set; }
		public string DiscountPlan { get; set; }
		public string LoanSourceName { get; set; }
		public string IsOpenPlatform { get; set; }

		public static DecisionHistoryModel Create(DecisionHistoryDBModel item) {
			CashRequestOriginator originator;
			string originatorStr = item.Originator;

			if (Enum.TryParse(item.Originator, out originator))
				originatorStr = originator.DescriptionAttr();

			var fees = new SetupFeeCalculator(item.ManualSetupFeePercent, item.BrokerSetupFeePercent)
				.CalculateTotalAndBroker(item.ApprovedSum);

			return new DecisionHistoryModel {
				Id = item.DecisionHistoryID,
				Action = item.Action,
				Comment = item.Comment,
				Date = item.Date,
				UnderwriterName = item.UnderwriterName,
				LoanType = item.LoanType,
				DiscountPlan = item.LoanType,
				LoanSourceName = item.LoanSourceName,
				RepaymentPeriod = item.RepaymentPeriod,
				InterestRate = item.InterestRate,
				ApprovedSum = item.ApprovedSum,
				IsLoanTypeSelectionAllowed = item.IsLoanTypeSelectionAllowed,
				Originator = originatorStr,
				TotalSetupFee = fees.Total,
				BrokerSetupFee = fees.Broker,
				IsOpenPlatform = string.IsNullOrEmpty(item.FundingType) ? "No" : "Yes"
			};
		} // Create
	} // class DecisionHistoryModel
} // namespace
