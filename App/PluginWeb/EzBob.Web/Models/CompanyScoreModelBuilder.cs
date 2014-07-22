namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Text.RegularExpressions;
	using Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Infrastructure;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class CompanyScoreModelBuilder {
		#region public

		#region constructor

		public CompanyScoreModelBuilder() {
			m_oServiceClient = new ServiceClient();
			m_oDB = DbConnectionGenerator.Get(m_oLog);
			m_oContext = ObjectFactory.GetInstance<IWorkplaceContext>();
		} // constructor

		#endregion constructor

		#region method Create

		public CompanyScoreModel Create(Customer customer) {
			bool bHasCompany = false;
			TypeOfBusinessReduced nBusinessType = TypeOfBusinessReduced.Personal;

			if (customer != null) {
				if (customer.Company != null) {
					nBusinessType = customer.Company.TypeOfBusiness.Reduce();
					bHasCompany = true;
				} // if
			} // if

			if (!bHasCompany) {
				return new CompanyScoreModel {
					result = "No data found.",
					DashboardModel = new ComapanyDashboardModel { Error = "No data found.", },
				};
			} // if

			if (nBusinessType != TypeOfBusinessReduced.Limited) {
				CompanyDataForCompanyScoreActionResult oUnlimAr = m_oServiceClient.Instance.GetCompanyDataForCompanyScore(
					m_oContext.UserId,
					customer.Id,
					customer.Company.ExperianRefNum
				);

				return new CompanyScoreModel {
					result = CompanyScoreModel.Ok,
					company_name = oUnlimAr.Data.BusinessName,
					company_ref_num = customer.Company.ExperianRefNum,
					Data = oUnlimAr.Data,
				};
			} // if

			ExperianLtdActionResult oLtdAr;

			try {
				oLtdAr = m_oServiceClient.Instance.CheckLtdCompanyCache(customer.Company.ExperianRefNum);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load Experian parsed data for a company '{0}'.", customer.Company.ExperianRefNum);

				return new CompanyScoreModel {
					result = "Failed to load data.",
					DashboardModel = new ComapanyDashboardModel { Error = "Failed to load data.", },
				};
			} // try

			if (string.IsNullOrWhiteSpace(oLtdAr.Value.RegisteredNumber)) {
				return new CompanyScoreModel {
					result = "No data found.",
					DashboardModel = new ComapanyDashboardModel { Error = "No data found.", },
				};
			} // if

			CompanyScoreModel oResult = BuildLimitedScoreModel(oLtdAr.Value);

			foreach (var oSha in oLtdAr.Value.GetChildren<ExperianLtdShareholders>())
				AddOwners(oResult, oSha.RegisteredNumberOfALimitedCompanyWhichIsAShareholder);

			AddOwners(oResult, oLtdAr.Value.RegisteredNumberOfTheCurrentUltimateParentCompany);

			return oResult;
		} // Create

		#endregion method Create

		#region method BuildLimitedDashboardModel

		public ComapanyDashboardModel BuildLimitedDashboardModel(long nServiceLogID) {
			var oModel = new ComapanyDashboardModel {
				FinDataHistories = new List<FinDataModel>(),
				LastFinData = new FinDataModel(),
				IsLimited = true,
			};

			ExperianLtdActionResult ar = m_oServiceClient.Instance.LoadExperianLtd(nServiceLogID);

			if (ar.Value.ServiceLogID != nServiceLogID)
				return oModel;

			return BuildLimitedDashboardModel(ar.Value, oModel);
		} // BuildLimitedDashboardModel

		public ComapanyDashboardModel BuildLimitedDashboardModel(ExperianLtd oExperianLtd, ComapanyDashboardModel oModel = null) {
			if (oModel == null) {
				oModel = new ComapanyDashboardModel {
					FinDataHistories = new List<FinDataModel>(),
					LastFinData = new FinDataModel(),
					IsLimited = true,
				};
			} // if

			oModel.CompanyName = oExperianLtd.CompanyName;
			oModel.CompanyRefNum = oExperianLtd.RegisteredNumber;

			oModel.Score = oExperianLtd.GetCommercialDelphiScore();
			oModel.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(oModel.Score, 100, 0).Color;

			oModel.CcjMonths = oExperianLtd.GetAgeOfMostRecentCCJDecreeMonths();

			oModel.Ccjs = oExperianLtd.GetNumberOfCcjsInLast24Months();

			List<ExperianLtdDL97> oDL97List = new List<ExperianLtdDL97>();
			List<ExperianLtdDL99> oDL99List = new List<ExperianLtdDL99>();

			foreach (var oKid in oExperianLtd.Children) {
				if (typeof (ExperianLtdDL97) == oKid.GetType())
					oDL97List.Add((ExperianLtdDL97)oKid);
				else if (typeof (ExperianLtdDL99) == oKid.GetType()) {
					ExperianLtdDL99 dl99 = (ExperianLtdDL99)oKid;

					if (dl99.Date.HasValue)
						oDL99List.Add(dl99);
				} // if
			} // for each

			string worstStatusAll = "0";

			//Calc and add Cais Balance

			oModel.CaisBalance = 0;

			foreach (var cais in oDL97List) {
				var state = cais.AccountState;
				var balance = cais.CurrentBalance ?? 0;

				// Sum all accounts balance that are not settled
				if (!string.IsNullOrEmpty(state) && state[0] != 'S') {
					oModel.CaisBalance += balance;
					oModel.CaisAccounts++;
				} // if

				if (!string.IsNullOrEmpty(state) && state[0] == 'D') {
					oModel.DefaultAccounts++;
					oModel.DefaultAmount += cais.DefaultBalance ?? 0;
				}
				else {
					var status = cais.AccountStatusLast12AccountStatuses ?? string.Empty;
					var worstStatus = CreditBureauModelBuilder.GetWorstStatus(Regex.Split(status, string.Empty));
					if (worstStatus != "0") {
						oModel.LateAccounts++;
						worstStatusAll = CreditBureauModelBuilder.GetWorstStatus(worstStatusAll, worstStatus);
					} // if
				} // if
			} // for each

			string date;
			oModel.LateStatus = CreditBureauModelBuilder.GetAccountStatusString(worstStatusAll, out date);

			// Calc and add tangible equity and adjusted profit

			if (oDL99List.Count > 0) {
				// ReSharper disable PossibleInvalidOperationException
				oDL99List.Sort((a, b) => b.Date.Value.CompareTo(a.Date.Value));
				// ReSharper restore PossibleInvalidOperationException

				for (var i = 0; i < oDL99List.Count - 1; i++) {
					ExperianLtdDL99 oCurItem = oDL99List[i];

					decimal totalShareFund = oCurItem.TotalShareFund ?? 0;
					decimal inTngblAssets = oCurItem.InTngblAssets ?? 0;
					decimal debtorsDirLoans = oCurItem.DebtorsDirLoans ?? 0;
					decimal credDirLoans = oCurItem.CredDirLoans ?? 0;
					decimal onClDirLoans = oCurItem.OnClDirLoans ?? 0;

					decimal tangibleEquity = totalShareFund - inTngblAssets - debtorsDirLoans + credDirLoans + onClDirLoans;

					if (oDL99List.Count > 1) {
						var oNextItem = oDL99List[i + 1];

						decimal retainedEarnings = oCurItem.RetainedEarnings ?? 0;
						decimal retainedEarningsPrev = oNextItem.RetainedEarnings ?? 0;
						decimal fixedAssetsPrev = oNextItem.TngblAssets ?? 0;

						decimal adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev / 5;

						var fin = new FinDataModel {
							TangibleEquity = tangibleEquity,
							AdjustedProfit = adjustedProfit,
						};
						oModel.FinDataHistories.Add(fin);

						if (i == 0)
							oModel.LastFinData = fin;
					} // if
				} // for each
			} // if DL99 has data

			return oModel;
		} // BuildLimitedDashboardModel

		#endregion method BuildLimitedDashboardModel

		#endregion public

		#region method BuildNonLimitedDashboardModel

		private ComapanyDashboardModel BuildNonLimitedDashboardModel(int customerId, string refNumber) {
			var model = new ComapanyDashboardModel {
				FinDataHistories = new List<FinDataModel>(),
				LastFinData = new FinDataModel(),
				IsLimited = false,
				CompanyRefNum = refNumber,
			};

			DataTable dt = m_oDB.ExecuteReader(
				"GetNonLimitedCompanyDashboardDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));

			if (dt.Rows.Count == 1) {
				var sr = new SafeReader(dt.Rows[0]);

				model.CompanyName = sr["BusinessName"];
				model.Score = sr["RiskScore"];
				model.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(model.Score, 100, 0).Color;
				model.CcjMonths = sr["AgeOfMostRecentJudgmentDuringOwnershipMonths"];
				model.Ccjs = sr["TotalJudgmentCountLast24Months"];
				model.Ccjs += sr["TotalAssociatedJudgmentCountLast24Months"];

				DataTable scoreHistoryDataTable = m_oDB.ExecuteReader(
					"GetNonLimitedCompanyScoreHistory",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("RefNumber", refNumber));

				foreach (DataRow row in scoreHistoryDataTable.Rows) {
					var scoreHistorySafeReader = new SafeReader(row);
					try {
						model.NonLimScoreHistories.Add(new NonLimScoreHistory {
							Score = scoreHistorySafeReader["RiskScore"],
							ScoreDate = scoreHistorySafeReader["Date"]
						});
					}
					catch (Exception ex) {
						m_oLog.Warn(ex, "Failed to parse non limited score history.");
					}
				}
			}

			return model;
		} // BuildNonLimitedDashboardModel

		#endregion method BuildNonLimitedDashboardModel

		#region private

		#region method AddOwners

		private void AddOwners(CompanyScoreModel oPossession, string sOwnerRegNum) {
			if (string.IsNullOrWhiteSpace(sOwnerRegNum))
				return;

			ExperianLtdActionResult oLtdAr;

			try {
				oLtdAr = m_oServiceClient.Instance.CheckLtdCompanyCache(sOwnerRegNum);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load Experian parsed data for company '{0}'.", sOwnerRegNum);
				return;
			} // try

			if (!string.IsNullOrWhiteSpace(oLtdAr.Value.RegisteredNumber))
				oPossession.AddOwner(BuildLimitedScoreModel(oLtdAr.Value));
		} // AddOwners

		#endregion method AddOwners

		#region method BuildLimitedScoreModel

		private CompanyScoreModel BuildLimitedScoreModel(ExperianLtd oExperianLtd) {
			var oDataset = new Dictionary<string, CompanyScoreModelItem>();

			// TODO build dataset

			return new CompanyScoreModel {
				result = CompanyScoreModel.Ok,
				dataset = oDataset,
				company_name = oExperianLtd.CompanyName,
				company_ref_num = oExperianLtd.RegisteredNumber,
				Data = null,
				DashboardModel = BuildLimitedDashboardModel(oExperianLtd),
			};
		} // BuildLimitedScoreModel

		#endregion method BuildLimitedScoreModel

		#region fields

		private readonly ServiceClient m_oServiceClient;
		private readonly IWorkplaceContext m_oContext;
		private readonly AConnection m_oDB;

		private static readonly SafeILog m_oLog = new SafeILog(typeof(CompanyScoreModelBuilder));

		#endregion fields

		#endregion private
	} // class CompanyScoreModelBuilder
} // namespace
