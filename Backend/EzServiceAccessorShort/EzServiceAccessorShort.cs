namespace EzServiceShortcut {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.VatReturn;
	using EzServiceAccessor;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Utils;

	public class EzServiceAccessorShort : IEzServiceAccessor {
		public ElapsedTimeInfo SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths
		) {
			var stra = new SaveVatReturnData(nCustomerMarketplaceID, nHistoryRecordID, nSourceID, oVatReturn, oRtiMonths);
			stra.Execute();
			return stra.ElapsedTimeInfo;
		} // SaveVatReturnData

		public VatReturnFullData LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID) {
			var stra = new LoadVatReturnFullData(nCustomerID, nCustomerMarketplaceID);
			stra.Execute();

			return new VatReturnFullData {
				VatReturnRawData = stra.VatReturnRawData,
				RtiTaxMonthRawData = stra.RtiTaxMonthRawData,
				Summary = stra.Summary,
				BankStatement = stra.BankStatement,
				BankStatementAnnualized = stra.BankStatementAnnualized,
			};
		}

		public ExperianConsumerData ParseExperianConsumer(long nServiceLogID)
		{
			var stra = new ParseExperianConsumerData(nServiceLogID);
			stra.Execute();
			return stra.Result;
		}

		public ExperianConsumerData LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogId)
		{
			var stra = new LoadExperianConsumerData(customerId, directorId, nServiceLogId);
			stra.Execute();
			return stra.Result;
		}

// LoadVatReturnFullData

		public ExperianLtd ParseExperianLtd(long nServiceLogID) {
			var stra = new ParseExperianLtd(nServiceLogID);
			stra.Execute();
			return stra.Result;
		} // ParseExperianLtd

		public ExperianLtd LoadExperianLtd(long nServiceLogID) {
			var stra = new LoadExperianLtd(null, nServiceLogID);
			stra.Execute();
			return stra.Result;
		} // LoadExperianLtd

		public ExperianLtd CheckLtdCompanyCache(int userId, string sCompanyRefNum) {
			var stra = new LoadExperianLtd(sCompanyRefNum, 0);
			stra.Execute();
			return stra.Result;
		} // CheckLtdCompanyCache

		public void EmailHmrcParsingErrors(int nCustomerID, int nCustomerMarketplaceID, SortedDictionary<string, string> oErrorsToEmail) {
			new EmailHmrcParsingErrors(nCustomerID, nCustomerMarketplaceID, oErrorsToEmail).Execute();
		} // EmailHmrcParsingErrors

		public CompanyData GetNonLimitedData(int underwriterId, string refNumber) {
			GetCompanyDataForCompanyScore strategyInstance = new GetCompanyDataForCompanyScore(refNumber);
			
			strategyInstance.Execute();
			return strategyInstance.Data;
		}
		
		public CompanyDataForCreditBureau GetCompanyDataForCreditBureau(int underwriterId, string refNumber) {
			GetCompanyDataForCreditBureau strategyInstance = new GetCompanyDataForCreditBureau(refNumber);

			strategyInstance.Execute();

			return new CompanyDataForCreditBureau {
				LastUpdate = strategyInstance.LastUpdate,
				Score = strategyInstance.Score,
				Errors = strategyInstance.Errors
			};
		}

        public WriteToLogPackage.OutputData ServiceLogWriter(WriteToLogPackage package)
        {
            var stra = new ServiceLogWriter(package);
            stra.Execute();
            return stra.Package.Out;
        }
	

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="customerId"></param>
		/// <param name="nlModel"></param>
		/// <returns></returns>
		public NL_Model AddPayment(NL_Model nlModel) {
			var strategy = new AddPayment(nlModel);
			try {
				strategy.Execute();
				return strategy.NLModel;
			} catch (Exception) {
				Console.WriteLine("xxx");
				//this.ServiceLogWriter(new WriteToLogPackage(new WriteToLogPackage.InputData("xxxx")));
			}

			return null;
		}
	} // class EzServiceAccessorShort
} // namespace EzServiceShortcut
