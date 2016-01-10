namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.Investor;
	using Ezbob.Backend.Strategies.LogicalGlue;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using EzService.ActionResults.Investor;

	partial class EzServiceImplementation : IEzServiceInvestor {
		
		public InvestorActionResult LoadInvestor(int underwriterID, int investorID) {
			LoadInvestor strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorID);
			return new InvestorActionResult {
				MetaData = metadata,
				Investor = strategy.Result
			};
		}

		public IntActionResult CreateInvestor(int underwriterID, InvestorModel investor, IEnumerable<InvestorContactModel> investorContacts, IEnumerable<InvestorBankAccountModel> investorBanks) {
			CreateInvestor strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investor, investorContacts, investorBanks);
			return new IntActionResult{
				MetaData = metadata, 
				Value = strategy.InvestorID
			};
		}

		public BoolActionResult ManageInvestorContact(int underwriterID, InvestorContactModel investorContact) {
			ManageInvestorContact strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorContact);
			return new BoolActionResult {
				MetaData = metadata,
				Value = strategy.Result
			};
		}

		public BoolActionResult ManageInvestorBankAccount(int underwriterID, InvestorBankAccountModel investorBank) {
			ManageInvestorBankAccount strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorBank);
			return new BoolActionResult {
				MetaData = metadata,
				Value = strategy.Result
			};
		}


		public AccountingDataResult LoadAccountingData(int underwriterID) {
			LoadAccountingData strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID);
			return new AccountingDataResult {
				MetaData = metadata,
				AccountingData = strategy.Result
			};
		}

		public TransactionsDataResult LoadTransactionsData(int underwriterID, int investorID, int bankAccountTypeID) {
			LoadTransactionsData strategy;
			var metadata = ExecuteSync(out strategy, null, underwriterID, investorID, bankAccountTypeID);
			return new TransactionsDataResult {
				MetaData = metadata,
				TransactionsData = strategy.Result
			};
		}

		public LogicalGlueResult GetLatestKnownInference(int underwriterID, int customerID, DateTime? date, bool includeTryouts) {
			GetLatestKnownInference strategy;
			var metadata = ExecuteSync(out strategy, customerID, underwriterID, customerID, date, includeTryouts);
			
			decimal? minScore = 0;
			decimal? maxScore = 0;
			try {
				if (strategy.Inference.Bucket.HasValue) {
					var grade = DB.Fill<I_Grade>("SELECT * FROM I_Grade", CommandSpecies.Text);
					int gradeID = (int)strategy.Inference.Bucket.Value;
					maxScore = grade.First(x => x.GradeID == gradeID)
						.UpperBound;
					if (gradeID > 1) {
						minScore = grade.First(x => x.GradeID == (gradeID - 1))
							.UpperBound;
					}

				}
			} catch (Exception ex) {
				Log.Error(ex, "Failed to retrieve min max grade scores for bucket {0}", strategy.Inference.Bucket);
			}

			var result =  new LogicalGlueResult {
				MetaData = metadata,
				Error = strategy.Inference.Error.Message,
				Date = strategy.Inference.ReceivedTime,
				Bucket = strategy.Inference.Bucket,
				BucketStr = strategy.Inference.Bucket.HasValue ? strategy.Inference.Bucket.ToString() : string.Empty,
				MonthlyRepayment = 0, //TODO
				FLScore = strategy.Inference.ModelOutputs.ContainsKey(ModelNames.FuzzyLogic) ? strategy.Inference.ModelOutputs[ModelNames.FuzzyLogic].Grade.Score : null,
				NNScore = strategy.Inference.ModelOutputs.ContainsKey(ModelNames.NeuralNetwork) ? strategy.Inference.ModelOutputs[ModelNames.NeuralNetwork].Grade.Score : null,
			};
			var b = (maxScore - minScore) ?? 0;
			var a = (result.NNScore - minScore) ?? 0;
			result.BucketPercent = b == 0 ? 0 : a / b;
			return result;
		}
	}
}

