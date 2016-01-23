namespace AutomationCalculator.ProcessHistory.Trails.ApprovalInput {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using Newtonsoft.Json;

	public class ApprovalInputData : ITrailInputData {
		public int CustomerID { get { return this.m_oArguments.CustomerID; } }

		public long? CashRequestID { get { return this.m_oArguments.CashRequestID; } }

		public long? NlCashRequestID { get { return this.m_oArguments.NlCashRequestID; } }

		public Configuration Configuration { get; private set; }

		public DateTime DataAsOf { get; private set; }
		public decimal AvailableFunds { get; private set; }
		public List<Name> DirectorNames { get; set; }
		public List<NameForComparison> HmrcBusinessNames { get; set; }
		public decimal Turnover1Y { get { return GetTurnover(12); } }
		public decimal Turnover3M { get { return GetTurnover(3); } }
		public List<Payment> LatePayments { get; private set; }
		public int MarketplaceSeniority { get; private set; }
		public Medal Medal { get { return this.m_oArguments.Medal; } }
		public MedalType MedalType { get { return this.m_oArguments.MedalType; } }
		public TurnoverType? TurnoverType { get { return this.m_oArguments.TurnoverType; } }
		public MetaData MetaData { get; private set; }
		public decimal ReservedFunds { get; private set; }
		public decimal SystemCalculatedAmount { get { return this.m_oArguments.SystemCalculatedAmount; } }

		[JsonIgnore]
		public Name CustomerName { get { return new Name(MetaData.FirstName, MetaData.LastName); } }

		[JsonIgnore]
		public NameForComparison CompanyName {
			get {
				if (this.m_bCompanyNameHasValue)
					return this.companyName;

				this.m_bCompanyNameHasValue = true;

				this.companyName = new NameForComparison(MetaData.ExperianCompanyName);
				if (this.companyName.AdjustedName == string.Empty)
					this.companyName = new NameForComparison(MetaData.EnteredCompanyName);

				return this.companyName;
			} // get
		} // CompanyName

		public ApprovalInputData() {
			Clean();
		} // constructor

		public void AddLatePayment(Payment oPayment) {
			LatePayments.Add(oPayment);
		} // AddLatePayment

		public void FullInit(ApprovalInputData aid) {
			Clean();

			PartialInit(
				aid.DataAsOf,
				aid.Configuration,
				aid.m_oArguments,
				aid.MetaData,
				aid.LatePayments,
				aid.DirectorNames,
				aid.HmrcBusinessNames
			);

			SetSeniority(aid.MarketplaceSeniority);
			SetAvailableFunds(aid.AvailableFunds, aid.ReservedFunds);
			SetTurnover(3, aid.GetTurnover(3));
			SetTurnover(12, aid.GetTurnover(12));
		} // FullInit

		public void FullInit(
			DateTime oDataAsOf,
			Configuration oCfg,
			Arguments oArgs,
			MetaData oMetaData,
			IEnumerable<Payment> oPayments,
			OriginationTime oOriginationTime,
			AutoApprovalTurnover oTurnover,
			AvailableFunds oFunds,
			List<Name> oDirectorNames,
			IEnumerable<NameForComparison> oHmrcBusinessNames
		) {
			PartialInit(oDataAsOf, oCfg, oArgs, oMetaData, oPayments, oDirectorNames, oHmrcBusinessNames);

			SetSeniority(oOriginationTime.Seniority);
			SetAvailableFunds(oFunds.Available, oFunds.Reserved);
			SetTurnoverData(oTurnover);
		} // FullInit

		public bool IsTurnoverGood() {
			return GetTurnover(3) * 4 > GetTurnover(12) * Configuration.TurnoverDropQuarterRatio;
		} // IsTurnoverGood

		public virtual string Serialize() {
			return JsonConvert.SerializeObject(new ApprovalInputDataSerializationModel(this), Formatting.Indented);
		} // Serialize

		public void SetArgs(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			decimal nAmount,
			Medal nMedal,
			MedalType medalType,
			TurnoverType? turnoverType
		) {
			this.m_oArguments = new Arguments(
				nCustomerID,
				cashRequestID,
				nlCashRequestID,
				nAmount,
				nMedal,
				medalType,
				turnoverType
			);
		} // SetArgs

		public void SetAvailableFunds(decimal nTotalAvailable, decimal nReserved) {
			AvailableFunds = nTotalAvailable - nReserved;
			ReservedFunds = nReserved;
		} // SetAvailableFunds

		public void SetConfiguration(Configuration oCfg) {
			Configuration = oCfg;
		} // SetConfiguration

		public void SetDataAsOf(DateTime v) {
			DataAsOf = v;
		} // SetDataAsOf

		public void SetDirectorNames(List<Name> oDirectorNames) {
			DirectorNames.Clear();

			if (oDirectorNames != null)
				DirectorNames.AddRange(oDirectorNames.Where(n => !n.IsEmpty));
		} // SetDirectorNames

		public void SetHmrcBusinessNames(IEnumerable<NameForComparison> oHmrcBusinessNames) {
			HmrcBusinessNames.Clear();

			if (oHmrcBusinessNames != null)
				HmrcBusinessNames.AddRange(oHmrcBusinessNames.Where(n => n.AdjustedName != string.Empty));
		} // SetHmrcBusinessNames

		public void SetMetaData(MetaData oMetaData) {
			MetaData = oMetaData;
		} // SetMetaData

		public void SetSeniority(int v) {
			MarketplaceSeniority = v;
		} // SetSeniority

		public void SetTurnoverData(AutoApprovalTurnover oTurnover) {
			SetTurnover(3, oTurnover[3]);
			SetTurnover(12, oTurnover[12]);
		} // SetTurnoverData

		private void PartialInit(
			DateTime oDataAsOf,
			Configuration oCfg,
			Arguments oArgs,
			MetaData oMetaData,
			IEnumerable<Payment> oPayments,
			List<Name> oDirectorNames,
			IEnumerable<NameForComparison> oHmrcBusinessNames
		) {
			SetDataAsOf(oDataAsOf);
			SetConfiguration(oCfg);
			this.m_oArguments = oArgs;
			SetMetaData(oMetaData);

			if (oPayments != null)
				LatePayments.AddRange(oPayments);

			SetDirectorNames(oDirectorNames);
			SetHmrcBusinessNames(oHmrcBusinessNames);
		} // PartialInit

		private decimal GetTurnover(int nMonthCount) {
			if (this.turnover == null)
				return 0;

			if (!this.turnover.ContainsKey(nMonthCount))
				return 0;

			return this.turnover[nMonthCount];
		} // GetTurnover

		private void Clean() {
			this.m_bCompanyNameHasValue = false;
			LatePayments = new List<Payment>();
			this.m_oArguments = new Arguments();
			DirectorNames = new List<Name>();
			HmrcBusinessNames = new List<NameForComparison>();

			DataAsOf = default(DateTime);
			Configuration = null;
			MetaData = null;

			MarketplaceSeniority = 0;

			AvailableFunds = 0;
			ReservedFunds = 0;

			this.turnover = null;
		} // Clean

		private void SetTurnover(int nMonthCount, decimal nTurnover) {
			if (this.turnover == null)
				this.turnover = new SortedDictionary<int, decimal>();

			this.turnover[nMonthCount] = nTurnover;
		} // SetTurnover

		private bool m_bCompanyNameHasValue;
		private Arguments m_oArguments;
		private SortedDictionary<int, decimal> turnover;
		private NameForComparison companyName;
	} // class ApprovalInputData
} // namespace
