namespace AutomationCalculator.ProcessHistory.Trails {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using Newtonsoft.Json;

	public partial class ApprovalInputData : ITrailInputData {
		public static ApprovalInputData Deserialize(string json) {
			var aid = new ApprovalInputData();
			aid.FromJson(json);
			return aid;
		} // Deserialize

		public DateTime DataAsOf { get; private set; }

		public Configuration Configuration { get; private set; }
		public MetaData MetaData { get; private set; }

		public int CustomerID { get { return m_oArguments.CustomerID; } } // CustomerID
		public decimal SystemCalculatedAmount { get { return m_oArguments.SystemCalculatedAmount; } } // SystemCalculatedAmount
		public Medal Medal { get { return m_oArguments.Medal; } } // Medal

		public string WorstStatuses {
			get { return string.Join(",", WorstStatusList); }
		} // WorstStatuses

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

		public List<Name> DirectorNames { get; set; }
		public List<string> HmrcBusinessNames { get; set; }

		[JsonIgnore]
		public Name CustomerName {
			get {
				return new Name(MetaData.FirstName, MetaData.LastName);
			} // get
		} // CustomerName

		[JsonIgnore]
		public string CompanyName {
			get {
				if (m_bCompanyNameHasValue)
					return m_sCompanyName;

				m_bCompanyNameHasValue = true;

				m_sCompanyName = Utils.AdjustCompanyName(MetaData.ExperianCompanyName);
				if (m_sCompanyName == string.Empty)
					m_sCompanyName = Utils.AdjustCompanyName(MetaData.EnteredCompanyName);

				return m_sCompanyName;
			}
		} // CompanyName

		public ApprovalInputData() {
			Clean();
		} // constructor

		public string Serialize() {
			return JsonConvert.SerializeObject(new SerializationModel().InitFrom(this), Formatting.Indented);
		} // Serialize

		public void FromJson(string json) {
			JsonConvert.DeserializeObject<SerializationModel>(json).FlushTo(this);
		} // FromJson

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

		public void SetTurnoverData(CalculatedTurnover oTurnover) {
			SetOnlineTurnover(1, oTurnover.GetOnline(1));
			SetOnlineTurnover(3, oTurnover.GetOnline(3));
			SetOnlineTurnover(12, oTurnover.GetOnline(12));

			OnlineUpdateTime = oTurnover.OnlineUpdateTime;
			HasOnline = oTurnover.HasOnline;

			HasHmrc = oTurnover.HasHmrc;
			HmrcUpdateTime = oTurnover.HmrcUpdateTime;

			SetHmrcTurnover(3, oTurnover.GetHmrc(3));
			SetHmrcTurnover(6, oTurnover.GetHmrc(6));
			SetHmrcTurnover(12, oTurnover.GetHmrc(12));
		} // SetTurnoverData

		public void AddLatePayment(Payment oPayment) {
			LatePayments.Add(oPayment);
		} // AddLatePayment

		public void SetWorstStatuses(IEnumerable<string> oWorstStatuses) {
			if (WorstStatusList == null)
				WorstStatusList = new List<string>();

			if (oWorstStatuses != null)
				WorstStatusList.AddRange(oWorstStatuses);
		} // SetWorstStatuses

		public void SetSeniority(int v) {
			MarketplaceSeniority = v;
		} // SetSeniority

		public void SetArgs(int nCustomerID, decimal nAmount, Medal nMedal) {
			m_oArguments = new Arguments(nCustomerID, nAmount, nMedal);
		} // SetArgs

		public void SetDataAsOf(DateTime v) {
			DataAsOf = v;
		} // SetDataAsOf

		public void SetAvailableFunds(decimal nTotalAvailable, decimal nReserved) {
			AvailableFunds = nTotalAvailable - nReserved;
			ReservedFunds = nReserved;
		} // SetAvailableFunds

		public void SetConfiguration(Configuration oCfg) {
			Configuration = oCfg;
		} // SetConfiguration

		public void SetMetaData(MetaData oMetaData) {
			MetaData = oMetaData;
		} // SetMetaData

		public void SetDirectorNames(List<Name> oDirectorNames) {
			DirectorNames.Clear();

			if (oDirectorNames != null)
				DirectorNames.AddRange(oDirectorNames.Where(n => !n.IsEmpty));
		} // SetDirectorNames

		public void SetHmrcBusinessNames(List<string> oHmrcBusinessNames) {
			HmrcBusinessNames.Clear();

			if (oHmrcBusinessNames != null)
				HmrcBusinessNames.AddRange(oHmrcBusinessNames.Where(n => n != string.Empty));
		} // SetHmrcBusinessNames

		public bool IsOnlineTurnoverTooOld() {
			return IsTurnoverTooOld(OnlineUpdateTime, Configuration.OnlineTurnoverAge);
		} // IsOnlineTurnoverTooOld

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

		public bool IsHmrcTurnoverTooOld() {
			return IsTurnoverTooOld(HmrcUpdateTime, Configuration.HmrcTurnoverAge);
		} // IsHmrcTurnoverTooOld

		private void Clean() {
			m_bCompanyNameHasValue = false;
			LatePayments = new List<Payment>();
			m_oArguments = new Arguments();
			DirectorNames = new List<Name>();
			HmrcBusinessNames = new List<string>();
			HasHmrc = false;
			HasOnline = false;

			DataAsOf = default(DateTime);
			Configuration = null;
			MetaData = null;
			WorstStatusList = null;

			MarketplaceSeniority = 0;

			AvailableFunds = 0;
			ReservedFunds = 0;

			m_oOnlineTurnover = null;
			m_oHmrcTurnover = null;

			OnlineUpdateTime = default(DateTime);
			HasOnline = false;

			HasHmrc = false;
			HmrcUpdateTime = default(DateTime);
		} // Clean

		private static decimal GetTurnover(SortedDictionary<int, decimal> dic, int nMonthCount) {
			if (dic == null)
				return 0;

			if (!dic.ContainsKey(nMonthCount))
				return 0;

			return dic[nMonthCount];
		} // GetTurnover

		private bool IsTurnoverTooOld(DateTime? oDate, int nMonthCount) {
			if (oDate == null)
				return true;

			return oDate.Value < DataAsOf.AddMonths(-nMonthCount);
		} // IsTurnoverTooOld

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

		private void SetOnlineTurnover(int nMonthCount, decimal nTurnover) {
			if (m_oOnlineTurnover == null)
				m_oOnlineTurnover = new SortedDictionary<int, decimal>();

			m_oOnlineTurnover[nMonthCount] = nTurnover;
		} // SetOnlineTurnover

		private void SetHmrcTurnover(int nMonthCount, decimal nTurnover) {
			if (m_oHmrcTurnover == null)
				m_oHmrcTurnover = new SortedDictionary<int, decimal>();

			m_oHmrcTurnover[nMonthCount] = nTurnover;
		} // SetHmrcTurnover

		private Arguments m_oArguments;
		private bool m_bCompanyNameHasValue;
		private string m_sCompanyName;
		private SortedDictionary<int, decimal> m_oOnlineTurnover;
		private SortedDictionary<int, decimal> m_oHmrcTurnover;
	} // class ApprovalInputData
} // namespace
