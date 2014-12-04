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

		public decimal OnlineTurnover1M { get { return GetTurnover(m_oOnlineTurnover, 1); } }
		public decimal OnlineTurnover3M { get { return GetTurnover(m_oOnlineTurnover, 3); } }
		public decimal OnlineTurnover1Y { get { return GetTurnover(m_oOnlineTurnover, 12); } }

		public DateTime? OnlineUpdateTime { get; private set; }
		public bool HasOnline { get; private set; }

		public bool HasHmrc { get; private set; }
		public DateTime? HmrcUpdateTime { get; private set; }

		public decimal HmrcTurnover3M { get { return GetTurnover(m_oHmrcTurnover, 3); } }
		public decimal HmrcTurnover6M { get { return GetTurnover(m_oHmrcTurnover, 6); } }
		public decimal HmrcTurnover1Y { get { return GetTurnover(m_oHmrcTurnover, 12); } }

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
			HasHmrc = false;
			HasOnline = false;
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

			SetAvailableFunds(oFunds.Available, oFunds.Reserved);

			SetDirectorNames(oDirectorNames);
			SetHmrcBusinessNames(oHmrcBusinessNames);

			SetTurnoverData(oTurnover);
		} // FullInit

		#endregion method FullInit

		#region method SetTurnoverData

		public void SetTurnoverData(CalculatedTurnover oTurnover) {
			SetOnlineTurnover(1, oTurnover.GetOnline(1));
			SetOnlineTurnover(3, oTurnover.GetOnline(3));
			SetOnlineTurnover(6, oTurnover.GetOnline(6));
			SetOnlineTurnover(12, oTurnover.GetOnline(12));

			OnlineUpdateTime = oTurnover.OnlineUpdateTime;
			HasOnline = oTurnover.HasOnline;

			HasHmrc = oTurnover.HasHmrc;
			HmrcUpdateTime = oTurnover.HmrcUpdateTime;

			SetHmrcTurnover(1, oTurnover.GetHmrc(1));
			SetHmrcTurnover(3, oTurnover.GetHmrc(3));
			SetHmrcTurnover(6, oTurnover.GetHmrc(6));
			SetHmrcTurnover(12, oTurnover.GetHmrc(12));
		} // SetTurnoverData

		#endregion method SetTurnoverData

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

		#region method IsOnlineTurnoverTooOld

		public bool IsOnlineTurnoverTooOld() {
			return IsTurnoverTooOld(OnlineUpdateTime, Configuration.OnlineTurnoverAge);
		} // IsOnlineTurnoverTooOld

		#endregion method IsOnlineTurnoverTooOld

		#region method IsOnlineTurnoverGood

		public bool IsOnlineTurnoverGood(int nMonthCount) {
			decimal nRatio;

			switch (nMonthCount) {
			case 1:
				nRatio = Configuration.OnlineTurnoverDropMonthRatio;
				break;

			case 3:
				nRatio = Configuration.OnlineTurnoverDropQuarterRatio;
				break;

			default:
				return false;
			} // switch

			return IsTurnoverGood(
				GetTurnover(m_oOnlineTurnover, nMonthCount),
				nMonthCount,
				GetTurnover(m_oOnlineTurnover, 12),
				12,
				nRatio
			);
		} // IsOnlineTurnoverGood

		#endregion method IsOnlineTurnoverGood

		#region method IsHmrcTurnoverGood

		public bool IsHmrcTurnoverGood(int nMonthCount) {
			decimal nRatio;

			switch (nMonthCount) {
			case 6:
				nRatio = Configuration.HmrcTurnoverDropHalfYearRatio;
				break;

			case 3:
				nRatio = Configuration.HmrcTurnoverDropQuarterRatio;
				break;

			default:
				return false;
			} // switch

			return IsTurnoverGood(
				GetTurnover(m_oHmrcTurnover, nMonthCount),
				nMonthCount,
				GetTurnover(m_oHmrcTurnover, 12),
				12,
				nRatio
			);
		} // IsHmrcTurnoverGood

		#endregion method IsHmrcTurnoverGood

		#region method IsHmrcTurnoverTooOld

		public bool IsHmrcTurnoverTooOld() {
			return IsTurnoverTooOld(HmrcUpdateTime, Configuration.HmrcTurnoverAge);
		} // IsHmrcTurnoverTooOld

		#endregion method IsHmrcTurnoverTooOld

		#endregion public

		#region private

		private Arguments m_oArguments;

		private SortedDictionary<int, decimal> m_oOnlineTurnover;
		private SortedDictionary<int, decimal> m_oHmrcTurnover;

		#region method GetTurnover

		private static decimal GetTurnover(SortedDictionary<int, decimal> dic, int nMonthCount) {
			if (dic == null)
				return 0;

			if (!dic.ContainsKey(nMonthCount))
				return 0;

			return dic[nMonthCount];
		} // GetTurnover

		#endregion method GetTurnover

		#region method IsTurnoverTooOld

		private bool IsTurnoverTooOld(DateTime? oDate, int nMonthCount) {
			if (oDate == null)
				return true;

			return oDate.Value < DataAsOf.AddMonths(-nMonthCount);
		} // IsTurnoverTooOld

		#endregion method IsTurnoverTooOld

		#region method IsTurnoverGood

		private static bool IsTurnoverGood(
			decimal nPeriodTurnover,
			int nPeriodLength,
			decimal nYearTurnover,
			int nYearLength,
			decimal nDropRatio
		) {
			if (nPeriodLength == 0)
				return false;

			return nPeriodTurnover / nPeriodLength * nYearLength > nYearTurnover * nDropRatio;
		} // IsTurnoverGood

		#endregion method IsTurnoverGood

		#region method SetOnlineTurnover

		private void SetOnlineTurnover(int nMonthCount, decimal nTurnover) {
			if (m_oOnlineTurnover == null)
				m_oOnlineTurnover = new SortedDictionary<int, decimal>();

			m_oOnlineTurnover[nMonthCount] = nTurnover;
		} // SetOnlineTurnover

		#endregion method SetTurnover

		#region method SetHmrcTurnover

		private void SetHmrcTurnover(int nMonthCount, decimal nTurnover) {
			if (m_oHmrcTurnover == null)
				m_oHmrcTurnover = new SortedDictionary<int, decimal>();

			m_oHmrcTurnover[nMonthCount] = nTurnover;
		} // SetHmrcTurnover

		#endregion method SetHmrcTurnover

		#endregion private
	} // class ApprovalInputData
} // namespace
