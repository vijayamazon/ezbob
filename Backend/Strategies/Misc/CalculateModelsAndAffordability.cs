namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.PayPalServiceLib;
	using Ezbob.Utils;
	using CompanyFiles;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using StructureMap;
	using YodleeLib.connector;

	public class CalculateModelsAndAffordability : AStrategy {
		public CalculateModelsAndAffordability(int nCustomerID, DateTime? oHistory) {
			this.m_oTimeCounter = new TimeCounter("CalculateModelsAndAffordability elapsed times");

			using (this.m_oTimeCounter.AddStep("Constructor time")) {
				this.m_nCustomerID = nCustomerID;
				this.m_oHistory = oHistory;
				this.m_oMundMs = new List<LocalMp>();

				MpModel = new MpModel {
					Affordability = new List<AffordabilityData>(),
					MarketPlaces = new List<MarketPlaceDataModel>(),
				};

				this.m_oRepo = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>();
			} // using
		} // constructor

		public override string Name {
			get { return "Calculate Models And Affordability"; }
		} // Name

		public override void Execute() {
			MpModel = new MpModel {
				Affordability = new List<AffordabilityData>(),
				MarketPlaces = new List<MarketPlaceDataModel>(),
			};

			using (this.m_oTimeCounter.AddStep("Total mp and affordability strategy execute time")) {
				try {
					using (this.m_oTimeCounter.AddStep("All marketplaces build time"))
						GetAllModels();

					var oPaypal = new List<LocalMp>();
					var oEcomm = new List<LocalMp>();
					var oAccounting = new List<LocalMp>();

					MarketPlaceDataModel oHmrc = null;

					var oYodlee = new List<LocalMp>();

					foreach (LocalMp mm in this.m_oMundMs) {
						if (mm.Marketplace.Disabled)
							continue;

						if (mm.Marketplace.Marketplace.InternalId == ms_oCompanyFilesID)
							continue;

						if (mm.Marketplace.Marketplace.InternalId == ms_oYodleeID) {
							oYodlee.Add(mm);
							continue;
						} // if

						if (mm.Marketplace.Marketplace.InternalId == ms_oHmrcID) {
							if (oHmrc == null)
								oHmrc = mm.Model;

							continue;
						} // if

						if (mm.Marketplace.Marketplace.InternalId == ms_oPaypalID) {
							oPaypal.Add(mm);
							continue;
						} // if

						if (mm.Marketplace.Marketplace.IsPaymentAccount)
							oAccounting.Add(mm);
						else
							oEcomm.Add(mm);
					} // for each marketplace

					if (oHmrc != null) {
						using (this.m_oTimeCounter.AddStep("HMRC affordability build time"))
							HmrcBank(oHmrc);
					} // if

					if (oYodlee.Any()) {
						using (this.m_oTimeCounter.AddStep("Yodlee affordability build time"))
							SaveBankStatement(oYodlee);
					} // if

					using (this.m_oTimeCounter.AddStep("PayPal affordability build time"))
						Psp(oPaypal);

					using (this.m_oTimeCounter.AddStep("Ecomm affordability build time"))
						EcommAccounting(oEcomm, AffordabilityType.Ecomm);

					using (this.m_oTimeCounter.AddStep("Accounting affordability build time"))
						EcommAccounting(oAccounting, AffordabilityType.Accounting);

					using (this.m_oTimeCounter.AddStep("Logging affordability time")) {
						Log.Debug("**************************************************************************");
						Log.Debug("*");
						Log.Debug("* Affordability data for customer {0} - begin:", this.m_nCustomerID);
						Log.Debug("*");
						Log.Debug("**************************************************************************");

						foreach (var a in MpModel.Affordability)
							Log.Debug(a);

						Log.Debug("**************************************************************************");
						Log.Debug("*");
						Log.Debug("* Affordability data for customer {0} - end.", this.m_nCustomerID);
						Log.Debug("*");
						Log.Debug("**************************************************************************");
					} // using
				} catch (Exception ex) {
					Log.Error(ex, "Failed calculation models and affordability for customer {0}", this.m_nCustomerID);
				}//try
			} // using total timer

			Log.Info(this.m_oTimeCounter.ToString());
		} // Execute

		public MpModel MpModel { get; set; }
		
		private void HmrcBank(MarketPlaceDataModel oModel) {
			/*if (oModel.HmrcData == null) {
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

				
			} // if*/ //todo

			MpModel.Affordability.Add(new AffordabilityData {
				Type = AffordabilityType.Hmrc,
				DateFrom = oModel.OriginationDate,
				DateTo = oModel.LastTransactionDate,
				
				Opex = oModel.TotalNetOutPayments,
				Revenues = oModel.TotalNetInPayments,
				ValueAdded = oModel.TotalNetInPayments - oModel.TotalNetOutPayments,
				
				Ebitda = oModel.TotalNetInPayments - oModel.TotalNetOutPayments, //todo
				FreeCashFlow = oModel.TotalNetInPayments - oModel.TotalNetOutPayments, //todo
				LoanRepayment = LoadLoanRepaymentsForHmrc(),
				Salaries = 0, //todo
				Tax = 0,//todo
				TurnoverTrend = oModel.TurnoverTrend
			});
		} // HmrcBank

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

				nRevenue += oModel.TotalNetInPayments;
				nOpex += Math.Abs(oModel.TotalNetOutPayments);
			} // for each account

			var trend = oPayPals
				.SelectMany(x => x.Model.TurnoverTrend)
				.GroupBy(x => x.TheMonth)
				.Select(t => new TurnoverTrend {
					TheMonth = t.Key,
					Turnover = t.Sum(s => s.Turnover)
				}).ToList();
				
			var oRes = new AffordabilityData {
				Type = AffordabilityType.Psp,
				Revenues = nRevenue,
				Opex = nOpex,
				IsAnnualized = bWasAnnualized,
				ErrorMsgs = string.Join(" ", oErrorMsgs).Trim(),
				TurnoverTrend = trend
			};

			oRes.Fill();
			oRes.DateFrom = oPayPals.Any(x => x.Model.OriginationDate.HasValue)
				? oPayPals.Min(x => x.Model.OriginationDate)
				: null;
			oRes.DateTo = oPayPals.Any(x => x.Model.LastTransactionDate.HasValue)
				? oPayPals.Max(x => x.Model.LastTransactionDate)
				: null;
			MpModel.Affordability.Add(oRes);
		} // Psp

		private Tuple<decimal /*value*/, bool /*isAnnualized*/> ExtractValue(
			Dictionary<string, string> oValues,
			string sKeyBase,
			bool isAnnualized = false
		) {
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

				nRevenue += oModel.AnnualSales;
			} // for each account

			var trend = oModels
				.SelectMany(x => x.Model.TurnoverTrend)
				.GroupBy(x => x.TheMonth)
				.Select(t => new TurnoverTrend {
					TheMonth = t.Key,
					Turnover = t.Sum(s => s.Turnover)
				})
				.ToList();

			if (nCount > 0) {
				var oRes = new AffordabilityData {
					Type = nType,
					Revenues = nRevenue,
					IsAnnualized = bWasAnnualized,
					ErrorMsgs = string.Join(" ", oErrorMsgs).Trim(),
					TurnoverTrend = trend
				};

				oRes.Fill();
				oRes.DateFrom = oModels.Any(x => x.Model.OriginationDate.HasValue)
					? oModels.Min(x => x.Model.OriginationDate)
					: null;
				oRes.DateTo = oModels.Any(x => x.Model.LastTransactionDate.HasValue)
					? oModels.Max(x => x.Model.LastTransactionDate)
					: null;
				MpModel.Affordability.Add(oRes);
			} // if
		} // EcommAccounting

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
				if (yodlee.Model != null) {
					affordability.DateFrom = affordability.DateFrom ?? yodlee.Model.OriginationDate;
					affordability.DateTo = affordability.DateTo ?? yodlee.Model.LastTransactionDate;
					affordability.Opex += yodlee.Model.TotalNetOutPayments;
					affordability.Revenues += yodlee.Model.TotalNetInPayments;
					affordability.ValueAdded += (yodlee.Model.TotalNetInPayments - yodlee.Model.TotalNetOutPayments); //todo fix
					affordability.FreeCashFlow += (yodlee.Model.TotalNetInPayments - yodlee.Model.TotalNetOutPayments); //todo fix
					affordability.Ebitda += (yodlee.Model.TotalNetInPayments - yodlee.Model.TotalNetOutPayments); //todo fix
					affordability.IsAnnualized = false;
					affordability.LoanRepayment += 0; // todo fix
					affordability.Salaries += 0;//todo fix
					affordability.Tax += 0;//todo fix
				} // if
			} // for each

			affordability.TurnoverTrend = yodlees
				.SelectMany(x => x.Model.TurnoverTrend)
				.GroupBy(x => x.TheMonth)
				.Select(t => new TurnoverTrend {
					TheMonth = t.Key,
					Turnover = t.Sum(s => s.Turnover)
				})
				.ToList();
			
			if (yodlees.Count() > 1)
				affordability.ErrorMsgs = "More than one bank data";

			MpModel.Affordability.Add(affordability);
		} // SaveBankStatement

		private void GetAllModels() {
			List<MP_CustomerMarketPlace> marketplaces = this.m_oHistory.HasValue
				? this.m_oRepo
					.GetAllByCustomer(this.m_nCustomerID)
					.Where(mp => mp.Created.HasValue && mp.Created.Value.Date <= this.m_oHistory.Value.Date)
					.ToList()
				: this.m_oRepo.GetAllByCustomer(this.m_nCustomerID).ToList();

			Log.Debug(
				"Loading mp models for customer {0}{1}: {2} model{3} loaded.",
				this.m_nCustomerID,
				this.m_oHistory.HasValue
					? string.Format(
						" that were created not after {0}",
						this.m_oHistory.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
					)
					: string.Empty,
				marketplaces.Count,
				marketplaces.Count == 1 ? string.Empty : "s"
			);

			foreach (var mp in marketplaces) {
				MarketPlaceDataModel model;

				try {
					if (mp.Disabled) {
						model = GetDefaultModel(mp);
						Log.Debug(
							"Marketplace {0} of type {1} is disabled, default model is used.",
							mp.Id,
							mp.Marketplace.Name
						);
					}
					else {
						var builder = GetMpModelBuilder(mp);

						Log.Debug(
							"Model builder of type {0} has been created for marketplace {1} of type {2}.",
							builder.GetType(),
							mp.Id,
							mp.Marketplace.Name
						);

						using (this.m_oTimeCounter.AddStep(
							"Model build time for mp {0}: {1} of type {2}", mp.Id, mp.DisplayName, mp.Marketplace.Name
						)) {
							model = builder.CreateLightModel(mp, this.m_oHistory);

							Log.Debug(
								"Model has been built for marketplace {0} of type {1}.",
								mp.Id,
								mp.Marketplace.Name
							);
						} // using
					} // if
				} catch (Exception e) {
					Log.Warn(
						e,
						"Something went wrong while building marketplace model for marketplace id {0} of type {1}, " +
						"default model is used.",
						mp.Id,
						mp.Marketplace.Name
					);
					model = GetDefaultModel(mp);
				} // try

				this.m_oMundMs.Add(new LocalMp(model, mp));
			} // for each mp

			MpModel.MarketPlaces.AddRange(this.m_oMundMs.Select(x => x.Model));
			/*
			try {
				if (m_oMundMs.Any(x => x.Model.Name == "HMRC") && m_oMundMs.Any(x => x.Model.Name == "Yodlee")) {
					foreach (var mp in m_oMundMs.Where(x => x.Model.Name == "HMRC")) {
						var returnData = new LoadVatReturnFullData(m_nCustomerID, mp.Model.Id);
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
			}*/ // todo bank statement model for hmrc
		} // GetAllModels

		private MarketPlaceDataModel GetDefaultModel(MP_CustomerMarketPlace mp) {
			return new MarketPlaceDataModel {
				Id = mp.Id,
				Type = mp.DisplayName,
				Name = mp.Marketplace.Name,
				IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
				Disabled = mp.Disabled,
				History = null,
			};
		} // GetDefaultModel

		private decimal LoadLoanRepaymentsForHmrc() {
			return DB.ExecuteScalar<decimal>(
				"LoadLoanRepaymentsForHmrc",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.m_nCustomerID),
				new QueryParameter("@Now", this.m_oHistory)
			) / 3.0m;
		} // LoadLoanRepaymentsForHmrc

		private class LocalMp {
			public LocalMp(MarketPlaceDataModel oModel, MP_CustomerMarketPlace mp) {
				Model = oModel;
				Marketplace = mp;
			} // constructor

			public MarketPlaceDataModel Model { get; private set; }
			public MP_CustomerMarketPlace Marketplace { get; private set; }
		} // LocalMp

		private readonly CustomerMarketPlaceRepository m_oRepo;
		private readonly int m_nCustomerID;
		private readonly DateTime? m_oHistory;

		private readonly List<LocalMp> m_oMundMs;

		private readonly TimeCounter m_oTimeCounter;

		private static readonly List<Tuple<string, int>> ms_oYearParts = new List<Tuple<string/*period*/, int/*factor*/>> {
			new Tuple<string, int>("12M", 1),
			new Tuple<string, int>("6M", 2),
			new Tuple<string, int>("3M", 4),
			new Tuple<string, int>("1M", 12),
		};

		private static readonly Guid ms_oHmrcID =
			global::Integration.ChannelGrabberConfig.Configuration.Instance.Hmrc.Guid();
		private static readonly Guid ms_oPaypalID = new PayPalServiceInfo().InternalId;
		private static readonly Guid ms_oYodleeID = new YodleeServiceInfo().InternalId;
		private static readonly Guid ms_oCompanyFilesID = new CompanyFilesServiceInfo().InternalId;
	} // class CalculateModelsAndAffordability
} // namespace
