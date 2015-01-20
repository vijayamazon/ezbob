﻿namespace Ezbob.Backend.Strategies.AutomationVerification {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;
	using PaymentServices.Calculators;

	public class MaamMedalAndPricing : AStrategy {
		public MaamMedalAndPricing(int topCount, int lastCheckedCashRequestID) {
			this.topCount = topCount;
			this.lastCheckedID = lastCheckedCashRequestID;
			this.data = new List<Datum>();
		} // constructor

		public override string Name {
			get { return "MaamMedalAndPricing"; }
		} // Name

		public override void Execute() {
			string top = (topCount > 0) ? "TOP " + topCount : string.Empty;

			string condition = (lastCheckedID > 0)
				? "AND r.Id < " + lastCheckedID
				: string.Empty;

			this.data.Clear();

			DB.ForEachRowSafe(
				ProcessRow,
				string.Format(QueryTemplate, top, condition),
				CommandSpecies.Text
			);

			var pc = new ProgressCounter("{0} cash requests processed.", Log, 100);

			foreach (Datum d in this.data) {
				var instance = new CalculateMedal(d.CustomerID, d.DecisionTime, true);
				instance.Execute();

				int amount = instance.Result.RoundOfferedAmount();
				d.Auto.Amount = amount;

				if (amount == 0) {
					d.Auto.InterestRate = 0;
					d.Auto.RepaymentPeriod = 0;
					d.Auto.SetupFee = 0;
				} else {
					var offerDualCalculator = new OfferDualCalculator(DB, Log);

					OfferResult offerResult = offerDualCalculator.CalculateOffer(
						d.CustomerID,
						d.DecisionTime,
						amount,
						d.LoanCount > 0,
						instance.Result.MedalClassification
					);

					d.Auto.InterestRate = offerResult.InterestRate;
					d.Auto.RepaymentPeriod = offerResult.Period;
					d.Auto.SetupFee = offerResult.SetupFee;
				} // if

				pc++;
			} // for

			pc.Log();

			Log.Debug("Output data - begin:");

			foreach (Datum d in this.data)
				Log.Debug("{0}", d);

			Log.Debug("Output data - end.");
		} // Execute

		private void ProcessRow(SafeReader sr) {
			Datum d = sr.Fill<Datum>();
			sr.Fill(d.Manual);
			sr.Fill(d.ManualCfg);

			d.ManualCfg.Calculate(d.Manual);
			d.LoadLoans(DB);

			this.data.Add(d);
		} // ProcessRow

		private class Datum {
			public Datum() {
				Manual = new MedalAndPricing();
				ManualCfg = new SetupFeeConfiguration();
				Auto = new MedalAndPricing();
			} // constructor

			public int CashRequestID { get; set; }
			public int CustomerID { get; set; }
			public int BrokerID { get; set; }

			[FieldName("UnderwriterDecisionDate")]
			public DateTime DecisionTime { get; set; }

			[FieldName("UnderwriterDecision")]
			public string Decision { get; set; }

			public MedalAndPricing Manual { get; private set; }
			public MedalAndPricing Auto { get; private set; }
			public SetupFeeConfiguration ManualCfg { get; private set; }

			public int LoanCount { get; private set; } // LoanCount

			public void LoadLoans(AConnection db) {
				LoanCount = db.ExecuteScalar<int>(
					"GetCustomerLoanCount",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", CustomerID),
					new QueryParameter("Now", DecisionTime)
				);
			} // LoadLoans

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"cash request: {0}, customer: {1}, broker: {2}, decision: {3} at {4}; auto: {5}; manual: {6}, cfg: {7}",
					CashRequestID,
					CustomerID,
					BrokerID,
					Decision,
					DecisionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					Auto,
					Manual,
					ManualCfg
				);
			} // ToString
		} // class Datum

		private class SetupFeeConfiguration {
			public int UseSetupFee { get; set; }
			public bool UseBrokerSetupFee { get; set; }

			[FieldName("ManualSetupFeePercent")]
			public decimal? Percent { get; set; }

			[FieldName("ManualSetupFeeAmount")]
			public decimal? Amount { get; set; }

			public void Calculate(MedalAndPricing map) {
				if (map == null)
					return;

				map.SetupFee = new SetupFeeCalculator(
					UseSetupFee == 1,
					UseBrokerSetupFee,
					(int)(Amount ?? 0),
					Percent
				).Calculate(map.Amount);
			} // Calculate

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"( use fee: {0}, broker fee: {1}; manual pct: {2}, amount {3} )",
					UseSetupFee,
					UseBrokerSetupFee,
					Percent.HasValue ? Percent.Value.ToString("P2", CultureInfo.InvariantCulture) : "--",
					Amount.HasValue ? Amount.Value.ToString("N2", CultureInfo.InvariantCulture) : "--"
				);
			} // ToString
		} // class SetupFeeConfiguration

		private class MedalAndPricing {
			public decimal Amount { get; set; }
			public decimal InterestRate { get; set; }
			public decimal SetupFee { get; set; }
			public int RepaymentPeriod { get; set; }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"{0} at {1} for {2} with fee {3}",
					Amount.ToString("N2", CultureInfo.InvariantCulture),
					InterestRate.ToString("P2", CultureInfo.InvariantCulture),
					Grammar.Number(RepaymentPeriod, "month"),
					SetupFee.ToString("N2", CultureInfo.InvariantCulture)
				);
			} // ToString
		} // class MedalAndPricing

		private readonly int topCount;
		private readonly int lastCheckedID;

		private readonly List<Datum> data; 

		private const string QueryTemplate = @"
SELECT {0}
	r.Id AS CashRequestID,
	r.IdCustomer AS CustomerID,
	c.BrokerID,
	r.UnderwriterDecisionDate,
	r.UnderwriterDecision,
	ISNULL(r.ManagerApprovedSum, 0) AS Amount,
	r.InterestRate,
	ISNULL(r.ApprovedRepaymentPeriod, r.RepaymentPeriod) AS RepaymentPeriod,
	r.UseSetupFee,
	r.UseBrokerSetupFee,
	r.ManualSetupFeePercent,
	r.ManualSetupFeeAmount
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN Security_User u ON r.IdUnderwriter = u.UserId
	INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
WHERE
	r.IdUnderwriter IS NOT NULL
	AND
	r.UnderwriterDecision IN ('Approved', 'Rejected')
	AND
	r.IdUnderwriter IS NOT NULL
	AND
	r.IdUnderwriter != 1
	{1}
ORDER BY
	r.Id DESC";
	} // class MaamMedalAndPricing
} // namespace

