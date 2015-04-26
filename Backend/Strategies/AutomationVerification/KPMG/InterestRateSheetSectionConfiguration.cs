namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.Tasks.StatsForWeeklyMaamMedalAndPricing;

	internal class InterestRateSheetSectionConfiguration {
		public const string ManualDecisionType = "Manual";
		public const string MinOfferDecisionType ="Min offer";
		public const string MaxOfferDecisionType ="Max offer";

		public const string SetupFeeRate = "Setup fee rate";
		public const string SetupFeeAmount = "Setup fee amount";
		public const string InterestRate = "Interest rate";

		public InterestRateSheetSectionConfiguration(
			string title,
			string loanSourceColumn,
			string loanSourceCondition,
			string loanSourceEscapedCondition
		) {
			Title = title;
			LoanSourceColumn = loanSourceColumn;
			LoanSourceCondition = loanSourceCondition;
			LoanSourceEscapedCondition = loanSourceEscapedCondition;

			WeightColumnNames = new SortedDictionary<string, string> {
				{ ManualDecisionType,   "K" },
				{ MinOfferDecisionType, "BE" },
				{ MaxOfferDecisionType, "BP" },
			};

			ScenarioColumnNames = new SortedDictionary<string, string> {
				{ ManualDecisionType,   "CE" },
				{ MinOfferDecisionType, "CE" },
				{ MaxOfferDecisionType, "CF" },
			};

			DataColumnNames = new SortedDictionary<string, SortedDictionary<string, string>>();

			DataColumnNames[ManualDecisionType] = new SortedDictionary<string, string> {
				{ SetupFeeAmount, "O" },
				{ SetupFeeRate, "N" },
				{ InterestRate, "L" },
			};

			DataColumnNames[MinOfferDecisionType] = new SortedDictionary<string, string> {
				{ SetupFeeAmount, "AC" },
				{ SetupFeeRate, "AB" },
				{ InterestRate, "Z" },
			};

			DataColumnNames[MaxOfferDecisionType] = new SortedDictionary<string, string> {
				{ SetupFeeAmount, "AK" },
				{ SetupFeeRate, "AJ" },
				{ InterestRate, "AH" },
			};

			Formats = new SortedDictionary<string, string> {
				{ SetupFeeAmount, TitledValue.Format.Money },
				{ SetupFeeRate, TitledValue.Format.Percent },
				{ InterestRate, TitledValue.Format.Percent },
			};
		} // constructor

		public string Title { get; private set; }
		public string LoanSourceColumn { get; private set; }
		public string LoanSourceCondition { get; private set; }
		public string LoanSourceEscapedCondition { get; private set; }

		public SortedDictionary<string, SortedDictionary<string, string>> DataColumnNames { get; private set; }

		public SortedDictionary<string, string> WeightColumnNames { get; private set; }

		public SortedDictionary<string, string> Formats { get; private set; }

		public SortedDictionary<string, string> ScenarioColumnNames { get; private set; }
	} // class InterestRateSheetSectionConfiguration
} // namespace
