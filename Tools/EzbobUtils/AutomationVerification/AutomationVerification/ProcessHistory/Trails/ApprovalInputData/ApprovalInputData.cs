namespace AutomationCalculator.ProcessHistory.Trails {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using Newtonsoft.Json;

	public partial class ApprovalInputData : ITrailInputData {
		public decimal AvailableFunds { get; private set; }

		[JsonIgnore]
		public string CompanyName {
			get {
				if (this.m_bCompanyNameHasValue)
					return this.m_sCompanyName;

				this.m_bCompanyNameHasValue = true;

				this.m_sCompanyName = Utils.AdjustCompanyName(MetaData.ExperianCompanyName);
				if (this.m_sCompanyName == string.Empty)
					this.m_sCompanyName = Utils.AdjustCompanyName(MetaData.EnteredCompanyName);

				return this.m_sCompanyName;
			}
		}

		public Configuration Configuration { get; private set; }

		public int CustomerID {
			get {
				return this.m_oArguments.CustomerID;
			}
		}

		[JsonIgnore]
		public Name CustomerName {
			get {
				return new Name(MetaData.FirstName, MetaData.LastName);
			} // get
		}

		public DateTime DataAsOf { get; private set; }

		public List<Name> DirectorNames { get; set; }

		public bool HasHmrc { get; private set; }

		public bool HasOnline { get; private set; }

		public List<string> HmrcBusinessNames { get; set; }

		public decimal HmrcTurnover1Y {
			get {
				return GetTurnover(this.m_oHmrcTurnover, 12);
			}
		}

		public decimal HmrcTurnover3M {
			get {
				return GetTurnover(this.m_oHmrcTurnover, 3);
			}
		}

		public decimal HmrcTurnover6M {
			get {
				return GetTurnover(this.m_oHmrcTurnover, 6);
			}
		}

		public DateTime? HmrcUpdateTime { get; private set; }

		public List<Payment> LatePayments { get; private set; }

		public int MarketplaceSeniority { get; private set; }

		public Medal Medal {
			get {
				return this.m_oArguments.Medal;
			}
		}

		public MedalType MedalType {
			get {
				return this.m_oArguments.MedalType;
			}
		}

		public TurnoverType? TurnoverType {
			get {
				return this.m_oArguments.TurnoverType;
			}
		}

		public MetaData MetaData { get; private set; }

		public decimal OnlineTurnover1M {
			get {
				return GetTurnover(this.m_oOnlineTurnover, 1);
			}
		}

		public decimal OnlineTurnover1Y {
			get {
				return GetTurnover(this.m_oOnlineTurnover, 12);
			}
		}

		public decimal OnlineTurnover3M {
			get {
				return GetTurnover(this.m_oOnlineTurnover, 3);
			}
		}

		public DateTime? OnlineUpdateTime { get; private set; }

		public decimal ReservedFunds { get; private set; }

		public decimal SystemCalculatedAmount {
			get {
				return this.m_oArguments.SystemCalculatedAmount;
			}
		}

		public string WorstStatuses {
			get {
				return string.Join(",", WorstStatusList);
			}
		}

		public List<string> WorstStatusList { get; private set; }

		public ApprovalInputData() {
			Clean();
		}

		public static ApprovalInputData Deserialize(string json) {
			var aid = new ApprovalInputData();
			aid.FromJson(json);
			return aid;
		} // Deserialize

		public void AddLatePayment(Payment oPayment) {
			LatePayments.Add(oPayment);
		}

		public void FromJson(string json) {
			JsonConvert.DeserializeObject<SerializationModel>(json)
				.FlushTo(this);
		}

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
			this.m_oArguments = oArgs;
			SetMetaData(oMetaData);
			SetWorstStatuses(oWorstStatuses);

			if (oPayments != null)
				LatePayments.AddRange(oPayments);

			SetSeniority(oOriginationTime.Seniority);

			SetAvailableFunds(oFunds.Available, oFunds.Reserved);

			SetDirectorNames(oDirectorNames);
			SetHmrcBusinessNames(oHmrcBusinessNames);

			SetTurnoverData(oTurnover);
		}

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
				GetTurnover(this.m_oHmrcTurnover, nMonthCount),
				nMonthCount,
				GetTurnover(this.m_oHmrcTurnover, 12),
				12,
				nRatio
				);
		}

		public bool IsHmrcTurnoverTooOld() {
			return IsTurnoverTooOld(HmrcUpdateTime, Configuration.HmrcTurnoverAge);
		}

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
				GetTurnover(this.m_oOnlineTurnover, nMonthCount),
				nMonthCount,
				GetTurnover(this.m_oOnlineTurnover, 12),
				12,
				nRatio
				);
		}

		public bool IsOnlineTurnoverTooOld() {
			return IsTurnoverTooOld(OnlineUpdateTime, Configuration.OnlineTurnoverAge);
		}

		public string Serialize() {
			return JsonConvert.SerializeObject(new SerializationModel().InitFrom(this), Formatting.Indented);
		} // Serialize

		public void SetArgs(int nCustomerID, decimal nAmount, Medal nMedal, MedalType medalType, TurnoverType? turnoverType) {
			this.m_oArguments = new Arguments(nCustomerID, nAmount, nMedal, medalType, turnoverType);
		}

		public void SetAvailableFunds(decimal nTotalAvailable, decimal nReserved) {
			AvailableFunds = nTotalAvailable - nReserved;
			ReservedFunds = nReserved;
		}

		public void SetConfiguration(Configuration oCfg) {
			Configuration = oCfg;
		}

		public void SetDataAsOf(DateTime v) {
			DataAsOf = v;
		}

		public void SetDirectorNames(List<Name> oDirectorNames) {
			DirectorNames.Clear();

			if (oDirectorNames != null)
				DirectorNames.AddRange(oDirectorNames.Where(n => !n.IsEmpty));
		}

		public void SetHmrcBusinessNames(List<string> oHmrcBusinessNames) {
			HmrcBusinessNames.Clear();

			if (oHmrcBusinessNames != null)
				HmrcBusinessNames.AddRange(oHmrcBusinessNames.Where(n => n != string.Empty));
		}

		public void SetMetaData(MetaData oMetaData) {
			MetaData = oMetaData;
		}

		public void SetSeniority(int v) {
			MarketplaceSeniority = v;
		}

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

		public void SetWorstStatuses(IEnumerable<string> oWorstStatuses) {
			if (WorstStatusList == null)
				WorstStatusList = new List<string>();

			if (oWorstStatuses != null)
				WorstStatusList.AddRange(oWorstStatuses);
		} // SetWorstStatuses

		private static decimal GetTurnover(SortedDictionary<int, decimal> dic, int nMonthCount) {
			if (dic == null)
				return 0;

			if (!dic.ContainsKey(nMonthCount))
				return 0;

			return dic[nMonthCount];
		}

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
		}

		private void Clean() {
			this.m_bCompanyNameHasValue = false;
			LatePayments = new List<Payment>();
			this.m_oArguments = new Arguments();
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

			this.m_oOnlineTurnover = null;
			this.m_oHmrcTurnover = null;

			OnlineUpdateTime = default(DateTime);
			HasOnline = false;

			HasHmrc = false;
			HmrcUpdateTime = default(DateTime);
		} // Clean

		private bool IsTurnoverTooOld(DateTime? oDate, int nMonthCount) {
			if (oDate == null)
				return true;

			return oDate.Value < DataAsOf.AddMonths(-nMonthCount);
		} // IsTurnoverTooOld

		private void SetHmrcTurnover(int nMonthCount, decimal nTurnover) {
			if (this.m_oHmrcTurnover == null)
				this.m_oHmrcTurnover = new SortedDictionary<int, decimal>();

			this.m_oHmrcTurnover[nMonthCount] = nTurnover;
		}

		private void SetOnlineTurnover(int nMonthCount, decimal nTurnover) {
			if (this.m_oOnlineTurnover == null)
				this.m_oOnlineTurnover = new SortedDictionary<int, decimal>();

			this.m_oOnlineTurnover[nMonthCount] = nTurnover;
		} // SetOnlineTurnover

		private bool m_bCompanyNameHasValue;
		private Arguments m_oArguments;
		private SortedDictionary<int, decimal> m_oHmrcTurnover;
		private SortedDictionary<int, decimal> m_oOnlineTurnover;
		private string m_sCompanyName;
	} // class ApprovalInputData
} // namespace
