namespace AutomationCalculator.ProcessHistory.Trails {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public class ApprovalInputData : ITrailInputData {
		#region public

		#region method AdjustCompanyName

		public static string AdjustCompanyName(string companyName) {
			if (string.IsNullOrWhiteSpace(companyName))
				return string.Empty;

			return companyName.Trim()
				.ToLowerInvariant()
				.Replace("limited", "ltd")
				.Replace("the ", string.Empty)
				.Replace("&amp;", "&")
				.Replace(".", string.Empty)
				.Replace("#049;", "'");
		} // AdjustCompanyName

		#endregion method AdjustCompanyName

		public DateTime DataAsOf { get; private set; }

		public Configuration Configuration { get; private set; }
		public MetaData MetaData { get; private set; }
		public int CustomerID { get { return m_oArguments.CustomerID; } } // CustomerID
		public decimal SystemCalculatedAmount { get { return m_oArguments.SystemCalculatedAmount; } } // SystemCalculatedAmount

		[JsonConverter(typeof(StringEnumConverter))]
		public Medal Medal { get { return m_oArguments.Medal; } } // Medal

		public string WorstStatuses {
			get { return string.Join(",", WorstStatusList); }
		} // WorstStatuses

		[JsonIgnore]
		public List<string> WorstStatusList { get; private set; }

		public int MarketplaceSeniority { get; private set; }
		public List<Payment> LatePayments { get; private set; }
		public decimal Turnover1M { get { return m_oTurnover != null ? m_oTurnover[1] : 0; } } // Turnover1M
		public decimal Turnover3M { get { return m_oTurnover != null ? m_oTurnover[3] : 0; } } // Turnover3M
		public decimal Turnover1Y { get { return m_oTurnover != null ? m_oTurnover[12] : 0; } } // Turnover1Y

		public decimal AvailableFunds { get; private set; }
		public decimal ReservedFunds { get; private set; }

		#region property CustomerName

		public Name CustomerName {
			get {
				if (m_oCustomerName == null)
					m_oCustomerName = new Name(MetaData.FirstName, MetaData.LastName);

				return m_oCustomerName;
			} // get
		} // CustomerName

		private Name m_oCustomerName;
		
		#endregion property CustomerName

		public List<Name> DirectorNames { get; set; }
		public List<string> HmrcBusinessNames { get; set; }

		#region constructor

		public ApprovalInputData() {
			m_bCompanyNameHasValue = false;
			LatePayments = new List<Payment>();
			m_oArguments = new Arguments();
			DirectorNames = new List<Name>();
			HmrcBusinessNames = new List<string>();
		} // constructor

		#endregion constructor

		#region method Serialize

		public string Serialize() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		} // Serialize

		#endregion method Serialize

		#region method CompanyName

		public string CompanyName {
			get {
				if (m_bCompanyNameHasValue)
					return m_sCompanyName;

				m_bCompanyNameHasValue = true;

				m_sCompanyName = AdjustCompanyName(MetaData.ExperianCompanyName);
				if (m_sCompanyName == string.Empty)
					m_sCompanyName = AdjustCompanyName(MetaData.EnteredCompanyName);

				return m_sCompanyName;
			}
		} // CompanyName

		private bool m_bCompanyNameHasValue;
		private string m_sCompanyName;

		#endregion method CompanyName

		#region method FullInit

		public void FullInit(
			DateTime oDataAsOf,
			Configuration oCfg,
			Arguments oArgs,
			MetaData oMetaData,
			IEnumerable<string> oWorstStatuses,
			IEnumerable<Payment> oPayments,
			OriginationTime oOriginationTime,
			CalculatedTurnover oTurnover,
			AvailableFunds oFunds,
			List<Name> oDirectorNames,
			List<string> oHmrcBusinessNames
		) {
			SetDataAsOf(oDataAsOf);
			SetConfiguration(oCfg);
			m_oArguments = oArgs;
			SetMetaData(oMetaData);
			SetWorstStatuses(oWorstStatuses);

			if (oPayments != null)
				LatePayments.AddRange(oPayments);

			SetSeniority(oOriginationTime.Seniority);

			SetTurnover(1, oTurnover[1]);
			SetTurnover(3, oTurnover[3]);
			SetTurnover(12, oTurnover[12]);

			SetAvailableFunds(oFunds.Available, oFunds.Reserved);

			SetDirectorNames(oDirectorNames);
			SetHmrcBusinessNames(oHmrcBusinessNames);
		} // FullInit

		#endregion method FullInit

		#region method AddLatePayment

		public void AddLatePayment(Payment oPayment) {
			LatePayments.Add(oPayment);
		} // AddLatePayment

		#endregion method AddLatePayment

		#region method SetWorstStatuses

		public void SetWorstStatuses(IEnumerable<string> oWorstStatuses) {
			if (WorstStatusList == null)
				WorstStatusList = new List<string>();

			if (oWorstStatuses != null)
				WorstStatusList.AddRange(oWorstStatuses);
		} // SetWorstStatuses

		#endregion method SetWorstStatuses

		#region method SetSeniority

		public void SetSeniority(int v) {
			MarketplaceSeniority = v;
		} // SetSeniority

		#endregion method SetSeniority

		#region method SetArgs

		public void SetArgs(int nCustomerID, decimal nAmount, Medal nMedal) {
			m_oArguments = new Arguments(nCustomerID, nAmount, nMedal);
		} // SetArgs

		#endregion method SetArgs

		#region method SetDataAsOf

		public void SetDataAsOf(DateTime v) {
			DataAsOf = v;
		} // SetDataAsOf

		#endregion method SetDataAsOf

		#region method SetAvailableFunds

		public void SetAvailableFunds(decimal nTotalAvailable, decimal nReserved) {
			AvailableFunds = nTotalAvailable - nReserved;
			ReservedFunds = nReserved;
		} // SetAvailableFunds

		#endregion method SetAvailableFunds

		#region method SetTurnover

		public void SetTurnover(int nMonthCount, decimal nTurnover) {
			if (m_oTurnover == null)
				m_oTurnover = new SortedDictionary<int, decimal>();

			m_oTurnover[nMonthCount] = nTurnover;
		} // SetTurnover

		#endregion method SetTurnover

		#region method SetConfiguration

		public void SetConfiguration(Configuration oCfg) {
			Configuration = oCfg;
		} // SetConfiguration

		#endregion method SetConfiguration

		#region method SetMetaData

		public void SetMetaData(MetaData oMetaData) {
			MetaData = oMetaData;
		} // SetMetaData

		#endregion method SetMetaData

		#region method SetDirectorNames

		public void SetDirectorNames(List<Name> oDirectorNames) {
			DirectorNames.Clear();

			if (oDirectorNames != null)
				DirectorNames.AddRange(oDirectorNames.Where(n => !n.IsEmpty));
		} // SetDirectorNames

		#endregion method SetDirectorNames

		#region method SetHmrcBusinessNames

		public void SetHmrcBusinessNames(List<string> oHmrcBusinessNames) {
			HmrcBusinessNames.Clear();

			if (oHmrcBusinessNames != null)
				HmrcBusinessNames.AddRange(oHmrcBusinessNames.Where(n => n != string.Empty));
		} // SetHmrcBusinessNames

		#endregion method SetHmrcBusinessNames

		#endregion public

		#region private

		private Arguments m_oArguments;

		private SortedDictionary<int, decimal> m_oTurnover;

		#endregion private
	} // class ApprovalInputData
} // namespace
