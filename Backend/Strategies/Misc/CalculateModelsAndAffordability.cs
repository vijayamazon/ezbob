namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using CompanyFiles;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using PayPalServiceLib;
	using VatReturn;
	using YodleeLib.connector;

	public class CalculateModelsAndAffordability : AStrategy {
		#region public

		#region constructor

		public CalculateModelsAndAffordability(int nCustomerID, DateTime? oHistory, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			_timeElapsed = new List<Tuple<string, double>>();
			Stopwatch sw = Stopwatch.StartNew();
			m_nCustomerID = nCustomerID;
			m_oHistory = oHistory;
			m_oMundMs = new List<LocalMp>();
			Affordability = new SortedSet<AffordabilityData>();
			m_oCustomer = DbHelper.GetCustomerInfo(nCustomerID);
			sw.Stop();
			_timeElapsed.Add(new Tuple<string, double>("Constructor time", sw.Elapsed.TotalMilliseconds));
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "GetAffordabilityData"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (m_oCustomer == null) {
				Log.Warn("Could not find customer by id {0}.", m_nCustomerID);
				return;
			} // if
			Stopwatch sw = Stopwatch.StartNew();
			Stopwatch totalSw = Stopwatch.StartNew();
			GetAllModels();
			sw.Stop();
			_timeElapsed.Add(new Tuple<string, double>("All marketplaces build time", sw.Elapsed.TotalMilliseconds));
			
			var oPaypal = new List<LocalMp>();
			var oEcomm = new List<LocalMp>();
			var oAccounting = new List<LocalMp>();

			MarketPlaceModel oHmrc = null;
			MarketPlaceModel oYodlee = null;

			foreach (LocalMp mm in m_oMundMs) {
				if (mm.Marketplace.Marketplace.InternalId == ms_oCompanyFilesID)
				{
					continue;
				}

				if (mm.Marketplace.Marketplace.InternalId == ms_oYodleeID) {
					if (oYodlee == null)
					{
						oYodlee = mm.Model;
					}

					continue;
				} // if

				if (mm.Marketplace.Marketplace.InternalId == ms_oHmrcID) {
					if (oHmrc == null)
					{
						oHmrc = mm.Model;
					}

					continue;
				} // if

				if (mm.Marketplace.Marketplace.InternalId == ms_oPaypalID) {
					oPaypal.Add(mm);
					continue;
				} // if

				if (mm.Marketplace.Marketplace.IsPaymentAccount)
				{
					oAccounting.Add(mm);
				}
				else
				{
					oEcomm.Add(mm);
				}
			} // for each marketplace
			
			if (oHmrc != null)
			{
				sw.Restart();
				HmrcBank(oHmrc);
				sw.Stop();
				_timeElapsed.Add(new Tuple<string, double>("HMRC affordability build time", sw.Elapsed.TotalMilliseconds));
			}
			
			if (oYodlee != null)
			{
				sw.Restart();
				SaveBankStatement(oYodlee.Yodlee.BankStatementDataModel, null, null);
				sw.Stop();
				_timeElapsed.Add(new Tuple<string, double>("Yodlee affordability build time", sw.Elapsed.TotalMilliseconds));
			}

			sw.Restart();
			Psp(oPaypal);
			sw.Stop();
			_timeElapsed.Add(new Tuple<string, double>("PayPal affordability build time", sw.Elapsed.TotalMilliseconds));
			sw.Restart();
			EcommAccounting(oEcomm, AffordabilityType.Ecomm);
			sw.Stop();
			_timeElapsed.Add(new Tuple<string, double>("Ecomm affordability build time", sw.Elapsed.TotalMilliseconds));
			sw.Restart();
			EcommAccounting(oAccounting, AffordabilityType.Accounting);
			sw.Stop();
			_timeElapsed.Add(new Tuple<string, double>("Accounting affordability build time", sw.Elapsed.TotalMilliseconds));

			sw.Restart();
			Log.Debug("**************************************************************************");
			Log.Debug("*");
			Log.Debug("* Affordability data for customer {0} - begin:", m_oCustomer.Stringify());
			Log.Debug("*");
			Log.Debug("**************************************************************************");

			foreach (var a in Affordability)
			{
				Log.Debug(a);
			}

			Log.Debug("**************************************************************************");
			Log.Debug("*");
			Log.Debug("* Affordability data for customer {0} - end.", m_oCustomer.Stringify());
			Log.Debug("*");
			Log.Debug("**************************************************************************");
			sw.Stop();
			_timeElapsed.Add(new Tuple<string, double>("Logging affordability time", sw.Elapsed.TotalMilliseconds));
			totalSw.Stop();
			_timeElapsed.Add(new Tuple<string, double>("Total mp and affordability strategy execute time", totalSw.Elapsed.TotalMilliseconds));
			LogElapsedTimes();
		}

		private void LogElapsedTimes()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("CalculateModelsAndAffordability elapsed times \n");
			foreach (var time in _timeElapsed)
			{
				sb.AppendFormat("{0}: {1}ms \n", time.Item1, time.Item2);
			}
			Log.Debug(sb);
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
				Log.Debug("There is no VAT return data for customer {0}.", m_oCustomer.Stringify());
				return;
			} // if

			var oVat = oModel.HmrcData;

			if (oVat == null) {
				Log.Debug("There is no VAT return data for customer {0}.", m_oCustomer.Stringify());
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
			if (!oPayPals.Any())
			{
				return;
			}

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
					nOpex += res.Item1;

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

		private Tuple<decimal, bool> ExtractValue(Dictionary<string, string> oValues, string sKeyBase) {
			string sValue = string.Empty;
			int nFactor = 0;

			foreach (var yp in ms_oYearParts) {
				string sKey = sKeyBase + yp.Item1;

				if (!oValues.ContainsKey(sKey))
					continue;

				sValue = oValues[sKey];

				if (sValue == "0")
					continue;

				nFactor = yp.Item2;
				break;
			} // for each

			if (nFactor == 0)
				return new Tuple<decimal, bool>(0, false);

			decimal nValue;

			if (!decimal.TryParse(sValue, out nValue))
				return new Tuple<decimal, bool>(0, false);

			return new Tuple<decimal, bool>(nValue * nFactor, nFactor != 1);
		} // ExtractValue

		#endregion method ExtractValue

		#region method EcommAccounting

		private void EcommAccounting(List<LocalMp> oModels, AffordabilityType nType) {
			if (!oModels.Any())
			{
				return;
			}
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

				Tuple<decimal, bool> res = ExtractValue(oModel.AnalysisDataInfo, CommonRevenuesAnnualized);

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

		private void SaveBankStatement(BankStatementDataModel oBank, DateTime? oFrom, DateTime? oTo) {
			Affordability.Add(new AffordabilityData {
				Type = AffordabilityType.Bank,
				DateFrom = oFrom,
				DateTo = oTo,
				Ebitda = (decimal)oBank.Ebida,
				FreeCashFlow = (decimal)oBank.FreeCashFlow,
				LoanRepayment = (decimal)oBank.ActualLoansRepayment,
				Opex = (decimal)oBank.Opex,
				Revenues = (decimal)oBank.Revenues,
				Salaries = (decimal)oBank.Salaries,
				Tax = (decimal)oBank.Tax,
				ValueAdded = (decimal)oBank.TotalValueAdded,
			});
		} // SaveBankStatement

		#endregion method SaveBankStatement

		#region method GetAllModels

		private void GetAllModels() {
			List<MP_CustomerMarketPlace> marketplaces = m_oHistory.HasValue
				? m_oCustomer.CustomerMarketPlaces.Where(mp => mp.Created.HasValue && mp.Created.Value.Date <= m_oHistory.Value.Date).ToList()
				: m_oCustomer.CustomerMarketPlaces.ToList();

			foreach (var mp in marketplaces) {
				MarketPlaceModel model;
				try {
					var builder = GetMpModelBuilder(mp);
					Stopwatch sw = Stopwatch.StartNew();
					model = builder.Create(mp, m_oHistory);
					sw.Stop();
					_timeElapsed.Add(new Tuple<string, double>(string.Format("{0} MP model build  time",model.Name), sw.Elapsed.TotalMilliseconds));
					sw.Restart();
					model.PaymentAccountBasic = builder.GetPaymentAccountModel(mp, model, m_oHistory);
					sw.Stop();
					_timeElapsed.Add(new Tuple<string, double>(string.Format("{0} MP payment model build  time", model.Name), sw.Elapsed.TotalMilliseconds));
				}
				catch (Exception e) {
					new SafeILog(this).Warn(e, "Something went wrong while building marketplace model for marketplace id {0} of type {1}.", mp.Id, mp.Marketplace.Name);

					model = new MarketPlaceModel {
						Id = mp.Id,
						Type = mp.DisplayName,
						Name = mp.Marketplace.Name,
						IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
						PaymentAccountBasic = new PaymentAccountsModel {
							displayName = mp.DisplayName,
						},
					};
				} // try

				m_oMundMs.Add(new LocalMp(model, mp));
			} // for each mp

			if (m_oMundMs.Any(x => x.Model.Name == "HMRC") && m_oMundMs.Any(x => x.Model.Name == "Yodlee"))
			{
				
				foreach (var mp in m_oMundMs.Where(x => x.Model.Name == "HMRC"))
				{
					var returnData = new LoadVatReturnFullData(m_oCustomer.Id, mp.Model.Id, DB, Log);
					returnData.CalculateBankStatements(mp.Model.HmrcData.VatReturn.LastOrDefault(), m_oMundMs.First(x => x.Model.Name == "Yodlee").Model.Yodlee.BankStatementDataModel);
					mp.Model.HmrcData.BankStatement = returnData.BankStatement;
					mp.Model.HmrcData.BankStatementAnnualized = returnData.BankStatementAnnualized;
				}
			}
		} // GetAllModels

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

		private readonly Customer m_oCustomer;
		private readonly int m_nCustomerID;
		private readonly DateTime? m_oHistory;

		private readonly List<LocalMp> m_oMundMs;

		#endregion fields

		#region constants

		private const string PaypalRevenues = "TotalNetRevenues";
		private const string PaypalOpex = "TotalNetExpenses";

		private const string CommonRevenues = "TotalSumofOrders";
		private const string CommonRevenuesAnnualized = "TotalSumofOrdersAnnualized";

		private static readonly List<Tuple<string, int>> ms_oYearParts = new List<Tuple<string, int>> {
			new Tuple<string, int>("12M", 1),
			new Tuple<string, int>("6M", 2),
			new Tuple<string, int>("3M", 4),
			new Tuple<string, int>("1M", 12),
		};

		private static readonly Guid ms_oHmrcID = Integration.ChannelGrabberConfig.Configuration.Instance.Hmrc.Guid();
		private static readonly Guid ms_oPaypalID = new PayPalServiceInfo().InternalId;
		private static readonly Guid ms_oYodleeID = new YodleeServiceInfo().InternalId;
		private static readonly Guid ms_oCompanyFilesID = new CompanyFilesServiceInfo().InternalId;

		private List<Tuple<string, double>> _timeElapsed;

		#endregion constants

		#endregion private
	} // class CalculateModelsAndAffordability
} // namespace
