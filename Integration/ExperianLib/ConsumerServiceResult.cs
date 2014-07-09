using System;
using EzBobIntegration.Web_References.Consumer;

namespace ExperianLib
{
	public class ConsumerServiceResult
	{
		public bool IsError
		{
			get { return !string.IsNullOrEmpty(Error); }
		}

		public bool IsExpirianError { get; set; }
		public string Error { get; set; }
		public OutputRoot Output { get; private set; }

		public double BureauScore { get; set; }
		public double CreditCardBalances { get; set; }
		public double NumCreditCards { get; set; }
		public double CreditLimitUtilisation { get; set; }
		public double CreditCardOverLimit { get; set; }
        public double CreditCardStatus { get; set; }
        public double CreditCardBalanceTrend { get; set; }
        public double UnsecuredLoans { get; set; }
        public double UnsecuredBorrowing { get; set; }
        public double PersonalLoanStatus { get; set; }
        public double PersonalLoans { get; set; }
		public double TotalAccountBalances { get; set; }
        public string NoAccountsReturned { get; set; }
		public double CCJLast2Years { get; set; }
		public double EnquiriesLast6Months { get; set; }
		public double EnquiriesLast3Months { get; set; }
		public double MortgageBalance { get; set; }
        public double StatusOnMortgageAccounts { get; set; }
		public double SumOfRepayements { get; set; }
        public double NumberCashWithdrawalsLastMonth { get; set; }
        public double NumberCCWithRepaymentFlag { get; set; }
        public double TimeLastAccountOpened { get; set; }
		public DateTime BirthDate { get; set; }

		public string SatisfiedJudgement { get; set; }
		public string Bankruptcy { get; set; }
		public double PublicData { get; set; }
		public double CAISDefaults { get; set; }
		public string BadDebt { get; set; }
		public string NOCAndNOD { get; set; }
		public double ConsumerIndebtedness { get; set; }
		public string CAISSpecialInstruction { get; set; }
		public string OtherBankruptcy { get; set; }
		public bool OtherPublicInfo { get; set; }

		public string ExperianResult { get; set; }
		public DateTime LastUpdateDate { get; set; }

		public ConsumerServiceResult()
		{
		}

		//-----------------------------------------------------------------------------------
		public ConsumerServiceResult(OutputRoot outputRoot, DateTime? defaultBirthDate)
		{
			Output = outputRoot;
			if (outputRoot == null)
			{
				Error = string.Format("Experian response is null");
				return;
			}

			if (outputRoot.Output.Error != null)
			{
				Error = string.Format("Error from service with code: {0}, severity: {1}, message: {2}", outputRoot.Output.Error.ErrorCode, outputRoot.Output.Error.Severity, outputRoot.Output.Error.Message);
				IsExpirianError = true;
			}
			else
			{
                TryRead(() => BureauScore = Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.Scoring.E5S051),"BureauScore");
                TryRead(() =>CreditCardBalances =Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA04),"CreditCardBalances");
                TryRead(() =>NumCreditCards =Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.APACSCCBehavrlData.NOMPMNPRL3M),"NumCreditCards");
                TryRead(() =>CreditLimitUtilisation =Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.APACSCCBehavrlData.CLUNPRL1M),"CreditLimitUtilisation");
                TryRead(() =>CreditCardOverLimit =Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPF131),"CreditCardOverLimit");
                TryRead(() => CreditCardStatus = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAIS.NDHAC04),"CreditCardStatus");
                TryRead(() =>
                            {
                                var ccbt =
                                    Convert.ToInt32(
                                        Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.
                                            Utilisationblock.SPB218);
                                switch (ccbt)
                                {
                                    case 9996:
                                    case 9999:
                                        CreditCardBalanceTrend = 0;
                                        break;
                                    case 9997:
                                    case 9998:
                                        CreditCardBalanceTrend = 100;
                                        break;
                                    default:
                                        CreditCardBalanceTrend = ccbt;
                                        break;
                                }
                            }, "CreditCardBalanceTrend");
                TryRead(
                    () =>
                    UnsecuredLoans =
                    Convert.ToDouble(
                        Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA02) -
                    Convert.ToDouble(
                        Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA04),
                    "UnsecuredLoans");
                TryRead(() => PersonalLoanStatus = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAIS.NDHAC05),"PersonalLoanStatus");
                TryRead(() => TotalAccountBalances = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAIS.E1B10),"TotalAccountBalances");
                TryRead(() => NoAccountsReturned = Output.Output.ConsumerSummary.Summary.CAIS.E1B01,"NoAccountsReturned");
				TryRead(() => CCJLast2Years = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.PublicInfo.E1A03),"CCJLast2Years");
                TryRead(() => EnquiriesLast6Months = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAPS.E1E02),"EnquiriesLast6Months");
                TryRead(() => EnquiriesLast3Months = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAPS.E1E01),"EnquiriesLast3Months");
                TryRead(() => MortgageBalance = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAIS.E1B11),"MortgageBalance");
				
				TryRead(() =>
				{
					if (null != Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04)
					{
						BirthDate = new DateTime(1900 + Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.YY,
												 Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.MM,
												 Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.EA4S04.DD);
					}
					else if (null != Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB)
					{
						BirthDate = new DateTime(1900 + Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.YY,
												 Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.MM,
												 Output.Output.ConsumerSummary.PremiumValueData.AgeDoB.NDDOB.DD);
					}
					else
					{
						BirthDate = defaultBirthDate.HasValue ? defaultBirthDate.Value : new DateTime(1900, 1, 1);
					}
				}, "BirthDate");
				
				TryRead(() => SumOfRepayements = 
					Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPH39) +
					Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPH40) + 
					Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPH41), "SumOfRepayements");
                TryRead(() => NumberCashWithdrawalsLastMonth = Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.APACSCCBehavrlData.NOCAL1M), "NumberCashWithdrawalsLastMonth");
                TryRead(() => NumberCCWithRepaymentFlag = Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.APACSCCBehavrlData.NOMPMNPRL3M), "NumberCCWithRepaymentFlag");
                TryRead(() => TimeLastAccountOpened = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAIS.E1B01), "TimeLastAccountOpened");
                TryRead(() => UnsecuredBorrowing = Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA02), "UnsecuredBorrowing");
                TryRead(() => PersonalLoans = Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA02) - Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.AdditDelphiBlocks.Utilisationblock.SPA04), "PersonalLoans");
				TryRead(() => Bankruptcy = Output.Output.ConsumerSummary.Summary.PublicInfo.EA1C01, "Bankruptcy");
				TryRead(() => OtherBankruptcy = Output.Output.ConsumerSummary.Summary.PublicInfo.EA2I01, "OtherBankruptcy");

				TryRead(() => SatisfiedJudgement = Output.Output.ConsumerSummary.Summary.PublicInfo.EA4Q06, "SatisfiedJudgement");
                TryRead(() => PublicData = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.PublicInfo.E1A02), "PublicData");
				
				TryRead(() => CAISDefaults = Convert.ToDouble(Output.Output.ConsumerSummary.Summary.CAIS.E1A05), "CAISDefaults");
				TryRead(() => BadDebt = Output.Output.ConsumerSummary.Summary.CAIS.E1B08, "BadDebt");
				TryRead(() => NOCAndNOD = Output.Output.ConsumerSummary.Summary.NOC.EA4Q05, "NOCAndNOD");
				TryRead(() => ConsumerIndebtedness = Convert.ToDouble(Output.Output.ConsumerSummary.PremiumValueData.CII.NDSPCII), "ConsumerIndebtedness");
				TryRead(() => CAISSpecialInstruction = Output.Output.ConsumerSummary.Summary.PublicInfo.EA4Q06, "CAISSpecialInstruction");
                TryRead(() => OtherBankruptcy = Output.Output.ConsumerSummary.Summary.PublicInfo.EA2I01, "OtherBankruptcy");
				TryRead(() =>
				{
					OtherPublicInfo = 
						Convert.ToInt32(Output.Output.ConsumerSummary.Summary.PublicInfo.E1A02) == 1 ||
						Convert.ToDouble(Output.Output.ConsumerSummary.Summary.PublicInfo.E2G02) > 0;
				}, "OtherPublicInfo");
			}
		}

		private void TryRead(Action a, string key)
		{
			try
			{
				a();
			}
			catch
			{
				Error += "Can`t read value for: " + key + Environment.NewLine;
			}
		}

	}
}
