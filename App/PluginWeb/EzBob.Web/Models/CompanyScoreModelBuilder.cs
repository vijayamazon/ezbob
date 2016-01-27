namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.CompanyScore;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.ModelsWithDB.Experian.Attributes;
	using Ezbob.Logger;
	using Infrastructure;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class CompanyScoreModelBuilder {

		static CompanyScoreModelBuilder() {
			var oAttrTypes = typeof (ASrcAttribute).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ASrcAttribute)) && t != typeof (ASrcAttribute));

			SortedDictionary<int, string> oOrder = new SortedDictionary<int, string>();

			foreach (Type oType in oAttrTypes) {
				ConstructorInfo ci = oType.GetConstructors().FirstOrDefault();

				if (ci == null)
					continue;

				ASrcAttribute oAttr = (ASrcAttribute)ci.Invoke(new object[ci.GetParameters().Length]);

				if (!oAttr.IsCompanyScoreModel)
					continue;

				if (!oAttr.IsTopLevel)
					continue;

				if (oOrder.ContainsKey(oAttr.TargetDisplayPosition))
					throw new Exception("Company score display order of " + oAttr.TargetDisplayPosition + " is specified more than once.");

				oOrder[oAttr.TargetDisplayPosition] = oAttr.TargetDisplayGroup;
			} // for each

			ms_DisplayOrder = oOrder.Values.ToList();
		} // static constructor

		public CompanyScoreModelBuilder() {
			m_oServiceClient = new ServiceClient();
			m_oContext = ObjectFactory.GetInstance<IWorkplaceContext>();
		} // constructor

		public CompanyScoreModel Create(Customer customer) {
			var model = new CompanyScoreModel {CompanyDetails = BuildDetails(customer)};

			bool bHasCompany = false;
			var nBusinessType = TypeOfBusinessReduced.Personal;

			if (customer != null) {
				if (customer.Company != null) {
					nBusinessType = customer.Company.TypeOfBusiness.Reduce();
					bHasCompany = true;
				} // if
			} // if

			if (!bHasCompany) {
				model.result = "No data found.";
				model.DashboardModel = new ComapanyDashboardModel {Error = "No data found.",};
				return model;

			} // if

			if (nBusinessType != TypeOfBusinessReduced.Limited) {
				CompanyDataForCompanyScoreActionResult oUnlimAr = m_oServiceClient.Instance.GetCompanyDataForCompanyScore(
					m_oContext.UserId,
					customer.Company.ExperianRefNum
				);

				model.result = CompanyScoreModel.Ok;
				model.company_name = oUnlimAr.Data.BusinessName;
				model.company_ref_num = customer.Company.ExperianRefNum;
				model.Data = oUnlimAr.Data;
				model.DashboardModel = BuildNonLimitedDashboardModel(oUnlimAr.Data, customer.Company.ExperianRefNum);
				return model;
			} // if

			ExperianLtdActionResult oLtdAr;

			try {
				oLtdAr = m_oServiceClient.Instance.CheckLtdCompanyCache(m_oContext.UserId, customer.Company.ExperianRefNum);
				model.CompaniesHouseModel = oLtdAr.CompaniesHouse;
				FindCustomerOfficersAndAppointments(model);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load Experian parsed data for a company '{0}'.", customer.Company.ExperianRefNum);

				model.result = "Failed to load data.";
				model.DashboardModel = new ComapanyDashboardModel {Error = "Failed to load data.",};
				
				return model;
			} // try

			if (string.IsNullOrWhiteSpace(oLtdAr.Value.RegisteredNumber)) {
				model.result = "No data found.";
				model.DashboardModel = new ComapanyDashboardModel {Error = "No data found.",};
				return model;
			} // if

			CompanyScoreModel oResult = BuildLimitedScoreModel(oLtdAr);
			oResult.CompanyDetails = model.CompanyDetails;
			foreach (var oSha in oLtdAr.Value.GetChildren<ExperianLtdShareholders>())
				AddOwners(oResult, oSha.RegisteredNumberOfALimitedCompanyWhichIsAShareholder);

			AddOwners(oResult, oLtdAr.Value.RegisteredNumberOfTheCurrentUltimateParentCompany);

			return oResult;
		}// Create

		private void FindCustomerOfficersAndAppointments(CompanyScoreModel model) {
			if (model.CompanyDetails == null || 
				string.IsNullOrEmpty(model.CompanyDetails.CustomerFirstName) || 
				string.IsNullOrEmpty(model.CompanyDetails.CustomerSurname) || 
				model.CompaniesHouseModel == null || 
				model.CompaniesHouseModel.Officers == null) {
				return;
			}//if
				
			foreach (var officer in model.CompaniesHouseModel.Officers) {
				if (!string.IsNullOrEmpty(officer.Name) &&
					officer.Name.ToLowerInvariant().Contains(model.CompanyDetails.CustomerFirstName) &&
					officer.Name.ToLowerInvariant().Contains(model.CompanyDetails.CustomerSurname)) {
					officer.IsCustomer = true;
				}//if

				if (officer.AppointmentOrder == null || officer.AppointmentOrder.Appointments == null) {
					continue;
				}//if

				foreach (var appointment in officer.AppointmentOrder.Appointments.Where(appointment => !string.IsNullOrEmpty(appointment.Name) &&
					appointment.Name.ToLowerInvariant().Contains(model.CompanyDetails.CustomerFirstName) &&
					appointment.Name.ToLowerInvariant().Contains(model.CompanyDetails.CustomerSurname))) {
					appointment.IsCustomer = true;
				}//foreach
			}//foreach
		} //FindCustomerOfficersAndAppointments

		private CompanyDetails BuildDetails(Customer customer) {
			var details = new CompanyDetails{
				CustomerId = customer.Id,
			};

			if (customer.PersonalInfo != null) {
				details.TypeOfBusiness = customer.PersonalInfo.TypeOfBusiness.ToString();
				details.CustomerFirstName = customer.PersonalInfo.FirstName.ToLowerInvariant();
				details.CustomerSurname = customer.PersonalInfo.Surname.ToLowerInvariant();
			}

			if (customer.Company != null) {
				details.CompanyName = customer.Company.ExperianCompanyName ?? customer.Company.CompanyName;
				details.CompanyRefNum = customer.Company.ExperianRefNum ?? customer.Company.CompanyNumber;
				details.CompanyAddress = customer.Company.CompanyAddress.ToList();
			}
			else {
				details.CompanyAddress = customer.AddressInfo.PersonalAddress.ToList();
			}
			return details;
		}//BuildDetails

		public ComapanyDashboardModel BuildNonLimitedDashboardModel(CompanyData data, string refNumber)
		{
			var model = new ComapanyDashboardModel();
			model.IsLimited = false;

			model.CompanyName = data.BusinessName;
			model.CompanyRefNum = refNumber;
			if (string.IsNullOrEmpty(refNumber)) {
				model.Error = "No data found.";
			}

			if (refNumber == "NotFound") {
				model.Error = "Customer selected company not found.";
			}

			model.Score = data.RiskScore ?? 0;
			model.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(model.Score, CompanyScoreMax, CompanyScoreMin).Color;

			model.CcjMonths = data.AgeOfMostRecentJudgmentDuringOwnershipMonths ?? -1;
			model.CcjValue = data.TotalJudgmentValueLast24Months ?? 0 + data.TotalAssociatedJudgmentValueLast24Months ?? 0;
			model.Ccjs = data.TotalJudgmentCountLast24Months ?? 0 + data.TotalAssociatedJudgmentCountLast24Months ?? 0;
			model.CompanyHistories = new List<CompanyHistory>();
			model.OriginationDate = data.IncorporationDate;
			if (data.ScoreHistory != null)
			{
				foreach (ScoreAtDate scoreAtDate in data.ScoreHistory)
				{
					model.CompanyHistories.Add(new CompanyHistory {Score = scoreAtDate.Score, Date = scoreAtDate.Date, ServiceLogId = scoreAtDate.ServiceLogId});
				}
			}

			return model;
		}

		public ComapanyDashboardModel BuildLimitedDashboardModel(ExperianLtdActionResult data, ComapanyDashboardModel oModel = null) {
			if (oModel == null) {
				oModel = new ComapanyDashboardModel {
					FinDataHistories = new List<FinDataModel>(),
					LastFinData = new FinDataModel(),
					IsLimited = true,
					CompanyHistories = new List<CompanyHistory>()
				};
			} // if
			var oExperianLtd = data.Value;
			oModel.CompanyName = oExperianLtd.CompanyName;
			oModel.CompanyRefNum = oExperianLtd.RegisteredNumber;

			oModel.Score = oExperianLtd.GetCommercialDelphiScore();
			oModel.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(oModel.Score, 100, 0).Color;

			oModel.CcjMonths = oExperianLtd.GetAgeOfMostRecentCCJDecreeMonths();

			oModel.Ccjs = oExperianLtd.GetNumberOfCcjsInLast24Months();
			oModel.CcjValue = oExperianLtd.GetSumOfCcjsInLast24Months();
			oModel.OriginationDate = oExperianLtd.GetOriginationDate();
			if (data.History != null)
			{
				foreach (ScoreAtDate scoreAtDate in data.History) {
					oModel.CompanyHistories.Add(new CompanyHistory {
						Score = scoreAtDate.Score,
						Date = scoreAtDate.Date,
						ServiceLogId = scoreAtDate.ServiceLogId,
						Balance = scoreAtDate.Balance
					});
				}
			}

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
			oModel.LateStatus = CreditBureauModelBuilder.GetAccountStatusString(worstStatusAll, out date, true);

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

		private void AddOwners(CompanyScoreModel oPossession, string sOwnerRegNum) {
			if (string.IsNullOrWhiteSpace(sOwnerRegNum))
				return;

			ExperianLtdActionResult oLtdAr;

			try {
				oLtdAr = m_oServiceClient.Instance.CheckLtdCompanyCache(m_oContext.UserId, sOwnerRegNum);
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to load Experian parsed data for company '{0}'.", sOwnerRegNum);
				return;
			} // try

			if (!string.IsNullOrWhiteSpace(oLtdAr.Value.RegisteredNumber))
				oPossession.AddOwner(BuildLimitedScoreModel(oLtdAr));
		} // AddOwners

		private CompanyScoreModel BuildLimitedScoreModel(ExperianLtdActionResult oExperianLtd) {
			SortedDictionary<string, CompanyScoreModelItem> oDataset = oExperianLtd.Value.ToCompanyScoreModel();

			return new CompanyScoreModel {
				result = CompanyScoreModel.Ok,
				dataset = oDataset,
				dataset_display_order = ms_DisplayOrder,
				company_name = oExperianLtd.Value.CompanyName,
				company_ref_num = oExperianLtd.Value.RegisteredNumber,
				Data = null,
				DashboardModel = BuildLimitedDashboardModel(oExperianLtd),
				CompaniesHouseModel = oExperianLtd.CompaniesHouse
			};
		} // BuildLimitedScoreModel

		private readonly ServiceClient m_oServiceClient;
		private readonly IWorkplaceContext m_oContext;
		private static readonly SafeILog m_oLog = new SafeILog(typeof(CompanyScoreModelBuilder));

		private const int CompanyScoreMax = 100;
		private const int CompanyScoreMin = 0;

		private static readonly List<string> ms_DisplayOrder;

	} // class CompanyScoreModelBuilder
} // namespace
