namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PayPalServiceLib;
	using VatReturn;
	using YodleeLib.connector;

	public class GetAffordabilityData : AStrategy {
		#region public

		#region constructor

		public GetAffordabilityData(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oCustomer = DbHelper.GetCustomerInfo(nCustomerID);
			Result = new SortedSet<AffordabilityData>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "GetAffordabilityData"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			// int? nHmrcMpID = DB.ExecuteScalar<int?>("LoadCustomerHmrcAccounts", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", m_oCustomer.Id));

			m_oPaypal = new List<MP_CustomerMarketPlace>();
			m_oEcomm = new List<MP_CustomerMarketPlace>();
			m_oAccounting = new List<MP_CustomerMarketPlace>();

			int? nHmrcMpID = null;

			foreach (MP_CustomerMarketPlace mp in m_oCustomer.CustomerMarketPlaces) {
				if (mp.Marketplace.InternalId == ms_oYodleeID)
					continue;

				if (mp.Marketplace.InternalId == ms_oHmrcID) {
					if (!nHmrcMpID.HasValue)
						nHmrcMpID = mp.Id;

					continue;
				} // if

				if (mp.Marketplace.InternalId == ms_oPaypalID) {
					m_oPaypal.Add(mp);
					continue;
				} // if

				if (mp.Marketplace.IsPaymentAccount)
					m_oAccounting.Add(mp);
				else
					m_oEcomm.Add(mp);
			} // for each marketplace

			if (nHmrcMpID.HasValue)
				HmrcBank(nHmrcMpID.Value);
			else
				Bank();

			Psp();
			Ecomm();
			Accounting();

			Log.Debug("**************************************************************************");
			Log.Debug("*");
			Log.Debug("* Affordability data for customer {0} - begin:", m_oCustomer.Stringify());
			Log.Debug("*");
			Log.Debug("**************************************************************************");

			foreach (var a in Result)
				Log.Debug(a);

			Log.Debug("**************************************************************************");
			Log.Debug("*");
			Log.Debug("* Affordability data for customer {0} - end.", m_oCustomer.Stringify());
			Log.Debug("*");
			Log.Debug("**************************************************************************");
		} // Execute

		#endregion method Execute

		#region property Result

		public SortedSet<AffordabilityData> Result { get; private set; }

		#endregion property Result

		#endregion public

		#region private

		#region method HmrcBank

		private void HmrcBank(int nHmrcMpID) {
			var oVat = new LoadVatReturnFullData(m_oCustomer.Id, nHmrcMpID, DB, Log);
			oVat.Execute();

			if ((oVat.Summary != null) && (oVat.Summary.Length > 0)) {
				var oHmrc = oVat.Summary[0];

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

				Result.Add(new AffordabilityData {
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

				SaveBankStatement(oVat.BankStatement, oFrom, oTo);
			} // if
		} // HmrcBank

		#endregion method HmrcBank

		#region method Psp

		private void Psp() {
			if (m_oPaypal.Count < 1)
				return;

			bool bWasAnnualized = false;
			var oErrorMsgs = new List<string>();

			decimal nRevenue = 0;
			decimal nOpex = 0;

			foreach (MP_CustomerMarketPlace mp in m_oPaypal) {
				var builder = GetMpModelBuilder(mp);
				var oModel = builder.Create(mp, null);

				if (!string.IsNullOrWhiteSpace(mp.UpdateError))
					oErrorMsgs.Add(mp.UpdateError.Trim());

				Tuple<decimal, bool> res = ExtractValue(oModel.AnalysisDataInfo, PaypalRevenues);
				if (res.Item1 != 0) {
					nRevenue += res.Item1;

					if (res.Item2)
						bWasAnnualized = true;
				} // if

				Log.Debug(
					"Paypal account {0} ({1}): revenue = {2}, annualized = {3}.",
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

			Result.Add(oRes);
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

		#region method Ecomm

		private void Ecomm() {
			if (m_oEcomm.Count < 1)
				return;

			// TODO
		} // Ecomm

		#endregion method Ecomm

		#region method Accounting

		private void Accounting() {
			if (m_oAccounting.Count < 1)
				return;

			// TODO
		} // Accounting

		#endregion method Accounting

		#region method Bank

		private void Bank() {
			var oGetBankModel = new GetBankModel(m_oCustomer.Id, DB, Log);

			oGetBankModel.Execute();

			if (oGetBankModel.Result != null)
				SaveBankStatement(oGetBankModel.Result.Yodlee.BankStatementDataModel, null, null);
		} // Bank

		#endregion method Bank

		#region method SaveBankStatement

		private void SaveBankStatement(BankStatementDataModel oBank, DateTime? oFrom, DateTime? oTo) {
			Result.Add(new AffordabilityData {
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

		private readonly Customer m_oCustomer;

		private List<MP_CustomerMarketPlace> m_oPaypal; 
		private List<MP_CustomerMarketPlace> m_oEcomm; 
		private List<MP_CustomerMarketPlace> m_oAccounting;

		private const string PaypalRevenues = "TotalNetRevenues";
		private const string PaypalOpex = "TotalNetExpenses";

		private static readonly List<Tuple<string, int>> ms_oYearParts = new List<Tuple<string, int>> {
			new Tuple<string, int>("12M", 1),
			new Tuple<string, int>("6M", 2),
			new Tuple<string, int>("3M", 4),
			new Tuple<string, int>("1M", 12),
		};

		private static readonly Guid ms_oHmrcID = Integration.ChannelGrabberConfig.Configuration.Instance.Hmrc.Guid();
		private static readonly Guid ms_oPaypalID = new PayPalServiceInfo().InternalId;
		private static readonly Guid ms_oYodleeID = new YodleeServiceInfo().InternalId;

		#endregion private
	} // class GetAffordabilityData
} // namespace
