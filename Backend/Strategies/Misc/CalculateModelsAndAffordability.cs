namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Utils;
	using CompanyFiles;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using PayPalServiceLib;
	using StructureMap;
	using VatReturn;
	using YodleeLib.connector;

	public class CalculateModelsAndAffordability : AStrategy {
		#region public

		#region constructor

		public CalculateModelsAndAffordability(int nCustomerID, DateTime? oHistory, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oTimeCounter = new TimeCounter("CalculateModelsAndAffordability elapsed times");

			using (m_oTimeCounter.AddStep("Constructor time")) {
				m_nCustomerID = nCustomerID;
				m_oHistory = oHistory;
				m_oMundMs = new List<LocalMp>();
				Affordability = new SortedSet<AffordabilityData>();
				m_oRepo = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>();
			} // using
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Calculate Models And Affordability"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			using (m_oTimeCounter.AddStep("Total mp and affordability strategy execute time")) {
				using (m_oTimeCounter.AddStep("All marketplaces build time"))
					GetAllModels();

				var oPaypal = new List<LocalMp>();
				var oEcomm = new List<LocalMp>();
				var oAccounting = new List<LocalMp>();

				MarketPlaceModel oHmrc = null;

				var oYodlee = new List<LocalMp>();

				foreach (LocalMp mm in m_oMundMs) {
					if (mm.Marketplace.Disabled) {
						continue;
					}

					if (mm.Marketplace.Marketplace.InternalId == ms_oCompanyFilesID) {
						continue;
					}

					if (mm.Marketplace.Marketplace.InternalId == ms_oYodleeID) {
						oYodlee.Add(mm);
						continue;
					} // if

					if (mm.Marketplace.Marketplace.InternalId == ms_oHmrcID) {
						if (oHmrc == null) {
							oHmrc = mm.Model;
						}
						continue;
					} // if

					if (mm.Marketplace.Marketplace.InternalId == ms_oPaypalID) {
						oPaypal.Add(mm);
						continue;
					} // if

					if (mm.Marketplace.Marketplace.IsPaymentAccount) {
						oAccounting.Add(mm);
					}
					else {
						oEcomm.Add(mm);
					}
				} // for each marketplace

				if (oHmrc != null) {
					using (m_oTimeCounter.AddStep("HMRC affordability build time")) {
						HmrcBank(oHmrc);
					}
				} // if

				if (oYodlee.Any()) {
					using (m_oTimeCounter.AddStep("Yodlee affordability build time")) {
						SaveBankStatement(oYodlee);
					}
				} // if

				using (m_oTimeCounter.AddStep("PayPal affordability build time")) {
					Psp(oPaypal);
				}

				using (m_oTimeCounter.AddStep("Ecomm affordability build time")) {
					EcommAccounting(oEcomm, AffordabilityType.Ecomm);
				}

				using (m_oTimeCounter.AddStep("Accounting affordability build time")) {
					EcommAccounting(oAccounting, AffordabilityType.Accounting);
				}

				using (m_oTimeCounter.AddStep("Logging affordability time")) {
					Log.Debug("**************************************************************************");
					Log.Debug("*");
					Log.Debug("* Affordability data for customer {0} - begin:", m_nCustomerID);
					Log.Debug("*");
					Log.Debug("**************************************************************************");

					foreach (var a in Affordability) {
						Log.Debug(a);
					}
					Log.Debug("**************************************************************************");
					Log.Debug("*");
					Log.Debug("* Affordability data for customer {0} - end.", m_nCustomerID);
					Log.Debug("*");
					Log.Debug("**************************************************************************");
				} // using
			} // using total timer

			m_oTimeCounter.Log(Log);
		} // Execute

		#endregion method Execute

		#region property Models

		public string Models {
			get {
				return JsonConvert.SerializeObject(
					m_oMundMs.Select(mm => mm.Model).ToArray()
				);
			} // get
		} // Models

		#endregion property Models

		#region property Affordability

		public SortedSet<AffordabilityData> Affordability { get; private set; }

		#endregion property Affordability

		#endregion public

		#region private

		#region method HmrcBank

		private void HmrcBank(MarketPlaceModel oModel) {
			if (oModel.HmrcData == null) {
				Log.Debug("There is no VAT return data for customer {0}.", m_nCustomerID);
				return;
			} // if

			var oVat = oModel.HmrcData;

			if (oVat == null) {
				Log.Debug("There is no VAT return data for customer {0}.", m_nCustomerID);
				return;
			} // if

			if ((oVat.VatReturnSummary != null) && (oVat.VatReturnSummary.Length > 0)) {
				var oHmrc = oVat.VatReturnSummary[0];

				DateTime? oFrom = null;
				DateTime? oTo = null;

				if ((oHmrc.Quarters != null) && (oHmrc.Quarters.Count > 0)) {
					int nIdx = oHmrc.Quarters.Count - 1;

					oTo = oHmrc.Quarters[nIdx].DateTo;

					nIdx -= 3;
					if (nIdx < 0)
						nIdx = 0;

					oFrom = oHmrc.Quarters[nIdx].DateFrom;
				} // if

				Affordability.Add(new AffordabilityData {
					Type = AffordabilityType.Hmrc,
					DateFrom = oFrom,
					DateTo = oTo,
					Ebitda = oHmrc.Ebida,
					FreeCashFlow = oHmrc.FreeCashFlow,
					LoanRepayment = oHmrc.ActualLoanRepayment,
					Opex = oHmrc.Opex,
					Revenues = oHmrc.Revenues,
					Salaries = oHmrc.Salaries,
					Tax = oHmrc.Tax,
					ValueAdded = oHmrc.TotalValueAdded,
				});
			} // if
		} // HmrcBank

		#endregion method HmrcBank

		#region method Psp

		private void Psp(List<LocalMp> oPayPals) {
			if ((oPayPals == null) || (oPayPals.Count < 1))
				return;

			bool bWasAnnualized = false;
			var oErrorMsgs = new List<string>();

			decimal nRevenue = 0;
			decimal nOpex = 0;

			foreach (var mm in oPayPals) {
				var mp = mm.Marketplace;
				var oModel = mm.Model;

				if (!string.IsNullOrWhiteSpace(mp.UpdateError))
					oErrorMsgs.Add(mp.UpdateError.Trim());

				Tuple<decimal, bool> res = ExtractValue(oModel.AnalysisDataInfo, PaypalRevenues);
				if (res.Item1 != 0) {
					nRevenue += res.Item1;

					if (res.Item2)
						bWasAnnualized = true;
				} // if

				Log.Debug(
					"PayPal account {0} ({1}): revenue = {2}, annualized = {3}.",
					mp.DisplayName,
					mp.Id,
					res.Item1,
					res.Item2 ? "yes" : "no"
				);

				res = ExtractValue(oModel.AnalysisDataInfo, PaypalOpex);
				if (res.Item1 != 0) {
					nOpex += Math.Abs(res.Item1);

					if (res.Item2)
						bWasAnnualized = true;
				} // if

				Log.Debug(
					"PayPal account {0} ({1}): OPEX = {2}, annualized = {3}.",
					mp.DisplayName,
					mp.Id,
					res.Item1,
					res.Item2 ? "yes" : "no"
				);
			} // for each account

			var oRes = new AffordabilityData {
				Type = AffordabilityType.Psp,
				Revenues = nRevenue,
				Opex = nOpex,
				IsAnnualized = bWasAnnualized,
				ErrorMsgs = string.Join(" ", oErrorMsgs).Trim(),
			};

			oRes.Fill();

			Affordability.Add(oRes);
		} // Psp

		#endregion method Psp

		#region method ExtractValue

		private Tuple<decimal /*value*/, bool /*isAnnualized*/> ExtractValue(Dictionary<string, string> oValues, string sKeyBase, bool isAnnualized = false) {
			if ((oValues == null) || (oValues.Count < 1))
				return new Tuple<decimal, bool>(0, false);

			string sValue = string.Empty;
			int nFactor = 0;

			foreach (var yp in ms_oYearParts) {
				string sKey = sKeyBase + yp.Item1;

				if (!oValues.ContainsKey(sKey))
					continue;

				sValue = oValues[sKey];

				if (sValue == "0")
					continue;

				nFactor = isAnnualized ? 1 : yp.Item2;
				break;
			} // for each

			if (nFactor == 0)
				return new Tuple<decimal, bool>(0, false);

			decimal nValue;

			if (!decimal.TryParse(sValue, out nValue))
				return new Tuple<decimal, bool>(0, false);

			return new Tuple<decimal, bool>(nValue * nFactor, nFactor != 1 || isAnnualized);
		} // ExtractValue

		#endregion method ExtractValue

		#region method EcommAccounting

		private void EcommAccounting(List<LocalMp> oModels, AffordabilityType nType) {
			if ((oModels == null) || (oModels.Count < 1))
				return;

			bool bWasAnnualized = false;
			var oErrorMsgs = new List<string>();

			decimal nRevenue = 0;
			int nCount = 0;

			foreach (var mm in oModels) {
				nCount++;
				var mp = mm.Marketplace;
				var oModel = mm.Model;

				if (!string.IsNullOrWhiteSpace(mp.UpdateError))
					oErrorMsgs.Add(mp.UpdateError.Trim());

				Tuple<decimal, bool> res = ExtractValue(oModel.AnalysisDataInfo, CommonRevenuesAnnualized, isAnnualized: true);

				if (res.Item1 == 0)
					res = ExtractValue(oModel.AnalysisDataInfo, CommonRevenues);

				if (res.Item1 != 0) {
					nRevenue += res.Item1;

					if (res.Item2)
						bWasAnnualized = true;
				} // if

				Log.Debug(
					"{4} account {0} ({1}): revenue = {2}, annualized = {3}.",
					mp.DisplayName,
					mp.Id,
					res.Item1,
					res.Item2 ? "yes" : "no",
					mp.Marketplace.Name
				);
			} // for each account

			if (nCount > 0) {
				var oRes = new AffordabilityData {
					Type = nType,
					Revenues = nRevenue,
					IsAnnualized = bWasAnnualized,
					ErrorMsgs = string.Join(" ", oErrorMsgs).Trim(),
				};

				oRes.Fill();

				Affordability.Add(oRes);
			} // if
		} // EcommAccounting

		#endregion method EcommAccounting

		#region method SaveBankStatement

		private void SaveBankStatement(List<LocalMp> yodlees) {
			var affordability = new AffordabilityData {
				Type = AffordabilityType.Bank,
				Ebitda = 0,
				FreeCashFlow = 0,
				LoanRepayment = 0,
				Opex = 0,
				Revenues = 0,
				Salaries = 0,
				Tax = 0,
				ValueAdded = 0,
			};

			foreach (var yodlee in yodlees) {
				if (yodlee.Model != null && yodlee.Model.Yodlee != null && yodlee.Model.Yodlee.BankStatementAnnualizedModel != null) {
					affordability.DateFrom = affordability.DateFrom ?? yodlee.Model.Yodlee.BankStatementAnnualizedModel.DateFrom;
					affordability.DateTo = affordability.DateTo ?? yodlee.Model.Yodlee.BankStatementAnnualizedModel.DateTo;
					affordability.Ebitda += (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.Ebida;
					affordability.FreeCashFlow += (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.FreeCashFlow;
					affordability.IsAnnualized = true;
					affordability.LoanRepayment += (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.ActualLoansRepayment;
					affordability.Opex = (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.Opex;
					affordability.Revenues += (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.Revenues;
					affordability.Salaries += (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.Salaries;
					affordability.Tax += (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.Tax;
					affordability.ValueAdded += (decimal) yodlee.Model.Yodlee.BankStatementAnnualizedModel.TotalValueAdded;
				}
			}
			
			if (yodlees.Count() > 1) {
				affordability.ErrorMsgs = "More than one bank data";
			}

			Affordability.Add(affordability);
		} // SaveBankStatement

		#endregion method SaveBankStatement

		#region method GetAllModels

		private void GetAllModels() {
			List<MP_CustomerMarketPlace> marketplaces = m_oHistory.HasValue
				? m_oRepo.GetAllByCustomer(m_nCustomerID).Where(mp => mp.Created.HasValue && mp.Created.Value.Date <= m_oHistory.Value.Date).ToList()
				: m_oRepo.GetAllByCustomer(m_nCustomerID).ToList();

			Log.Debug(
				"Loading mp models for customer {0}{1}: {2} model{3} loaded.",
				m_nCustomerID,
				m_oHistory.HasValue
					? string.Format(" that were created not after {0}", m_oHistory.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture))
					: string.Empty,
				marketplaces.Count,
				marketplaces.Count == 1 ? string.Empty : "s"
			);

			foreach (var mp in marketplaces) {
				MarketPlaceModel model;

				try {
					if (mp.Disabled) {
						model = GetDefaultModel(mp);
						Log.Debug("Marketplace {0} of type {1} is disabled, default model is used.", mp.Id, mp.Marketplace.Name);
					}
					else {
						var builder = GetMpModelBuilder(mp);

						Log.Debug(
							"Model builder of type {0} has been created for marketplace {1} of type {2}.",
							builder.GetType(),
							mp.Id,
							mp.Marketplace.Name
						);

						using (m_oTimeCounter.AddStep(
							"Model build time for mp {0}: {1} of type {2}", mp.Id, mp.DisplayName, mp.Marketplace.Name
						)) {
							model = builder.Create(mp, m_oHistory);

							Log.Debug(
								"Model has been built for marketplace {0} of type {1}.",
								mp.Id,
								mp.Marketplace.Name
							);
						} // using

						using (m_oTimeCounter.AddStep(
							"Payment model build time for mp {0}: {1} of type {2}",
							mp.Id, mp.DisplayName, mp.Marketplace.Name
						)) {
							model.PaymentAccountBasic = builder.GetPaymentAccountModel(mp, model, m_oHistory);

							Log.Debug(
								"Payment account model has been built for marketplace {0} of type {1}.",
								mp.Id,
								mp.Marketplace.Name
							);
						} // using
					} // if
				}
				catch (Exception e) {
					Log.Warn(e, "Something went wrong while building marketplace model for marketplace id {0} of type {1}, default model is used.", mp.Id, mp.Marketplace.Name);
					model = GetDefaultModel(mp);
				} // try

				m_oMundMs.Add(new LocalMp(model, mp));
			} // for each mp

			try {
				if (m_oMundMs.Any(x => x.Model.Name == "HMRC") && m_oMundMs.Any(x => x.Model.Name == "Yodlee")) {
					foreach (var mp in m_oMundMs.Where(x => x.Model.Name == "HMRC")) {
						var returnData = new LoadVatReturnFullData(m_nCustomerID, mp.Model.Id, DB, Log);
						returnData.CalculateBankStatements(mp.Model.HmrcData.VatReturn.LastOrDefault(),
						                                   m_oMundMs.First(x => x.Model.Name == "Yodlee")
						                                            .Model.Yodlee.BankStatementDataModel);

						mp.Model.HmrcData.BankStatement = returnData.BankStatement;
						mp.Model.HmrcData.BankStatementAnnualized = returnData.BankStatementAnnualized;
					} // for each HMRC model
				} // if
			}
			catch (Exception ex) {
				Log.Warn(ex, "Failed to build bank statement for hmrc");
			}
		} // GetAllModels

		private MarketPlaceModel GetDefaultModel(MP_CustomerMarketPlace mp) {
			return new MarketPlaceModel
			{
				Id = mp.Id,
				Type = mp.DisplayName,
				Name = mp.Marketplace.Name,
				IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
				PaymentAccountBasic = new PaymentAccountsModel
				{
					displayName = mp.DisplayName,
				},
				Disabled = mp.Disabled
			};
		}

		#endregion method GetAllModels

		#region class LocalMp

		private class LocalMp {
			public LocalMp(MarketPlaceModel oModel, MP_CustomerMarketPlace mp) {
				Model = oModel;
				Marketplace = mp;
			} // constructor

			public MarketPlaceModel Model { get; private set; }
			public MP_CustomerMarketPlace Marketplace { get; private set; }
		} // LocalMp

		#endregion class LocalMp

		#region fields

		private readonly CustomerMarketPlaceRepository m_oRepo;
		private readonly int m_nCustomerID;
		private readonly DateTime? m_oHistory;

		private readonly List<LocalMp> m_oMundMs;

		private readonly TimeCounter m_oTimeCounter;

		#endregion fields

		#region constants

		private const string PaypalRevenues = "TotalNetRevenues";
		private const string PaypalOpex = "TotalNetExpenses";

		private const string CommonRevenues = "TotalSumofOrders";
		private const string CommonRevenuesAnnualized = "TotalSumofOrdersAnnualized";

		private static readonly List<Tuple<string, int>> ms_oYearParts = new List<Tuple<string/*period*/, int/*factor*/>> {
			new Tuple<string, int>("12M", 1),
			new Tuple<string, int>("6M", 2),
			new Tuple<string, int>("3M", 4),
			new Tuple<string, int>("1M", 12),
		};

		private static readonly Guid ms_oHmrcID = Integration.ChannelGrabberConfig.Configuration.Instance.Hmrc.Guid();
		private static readonly Guid ms_oPaypalID = new PayPalServiceInfo().InternalId;
		private static readonly Guid ms_oYodleeID = new YodleeServiceInfo().InternalId;
		private static readonly Guid ms_oCompanyFilesID = new CompanyFilesServiceInfo().InternalId;

		#endregion constants

		#endregion private
	} // class CalculateModelsAndAffordability
} // namespace
