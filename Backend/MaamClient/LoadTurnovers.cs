namespace MaamClient {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.Turnover;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
	using JetBrains.Annotations;
	using MailApi;

	internal static class SomeExt {
		public static string Stringify(this DateTime t) {
			return t.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // Stringify

		public static string Stringify(this int x) {
			if (x == 0)
				return "&nbsp;";

			return x.ToString("N0", CultureInfo.InvariantCulture);
		} // Stringify

		public static string Stringify(this long x) {
			if (x == 0)
				return "&nbsp;";

			return x.ToString("N0", CultureInfo.InvariantCulture);
		} // Stringify

		public static string Stringify(this decimal x) {
			if (x == 0)
				return "&nbsp;";

			return x.ToString("N2", CultureInfo.InvariantCulture);
		} // Stringify
	} // class SomeExt

	internal class LoadTurnovers {
		public LoadTurnovers(Args args, AConnection db, ASafeLog log) {
			Log = log ?? new SafeLog();
			DB = db;
			Args = args;
		} // constructor

		public void Run() {
			Load();
			Generate();
			Send();
		} // Run

		private SortedDictionary<long, ACalculatedTurnoverBase> Result { get; set; }

		private SortedDictionary<long, CrCuTi> Input { get; set; }

		private ATag Email { get; set; }

		private ASafeLog Log { get; set; }

		private AConnection DB { get; set; }

		private Args Args { get; set; }

		private class CrCuTi {
			public long CashRequestID {
				get;
				[UsedImplicitly]
				set;
			}

			public int CustomerID {
				get;
				[UsedImplicitly]
				set;
			}

			public DateTime DecisionTime {
				get;
				[UsedImplicitly]
				set;
			}

			public string UnderwriterDecision {
				get;
				[UsedImplicitly]
				set;
			}
		}

		// class CrCuTi
		private class DisplayTurnoverTotals {
			public int AllCount { get; private set; }
			public int GoodCount { get; private set; }
			public int BadCount { get; private set; }

			public decimal ActualTurnoverMedian {
				get {
					if (this.actualTurnoverMedian.HasValue)
						return this.actualTurnoverMedian.Value;

					this.actualTurnoverMedian = 0;
					int half = this.actualTurnovers.Sum(pair => pair.Value) / 2;

					int count = 0;

					foreach (KeyValuePair<decimal, int> pair in this.actualTurnovers) {
						count += pair.Value;
						this.actualTurnoverMedian = pair.Key;

						if (count >= half)
							break;
					} // for each

					return this.actualTurnoverMedian.Value;
				} // get
			} // ActualTurnoverMedian

			public DisplayTurnoverTotals() {
				AllCount = 0;
				GoodCount = 0;
				BadCount = 0;

				this.actualTurnovers = new SortedDictionary<decimal, int>();
			} // constructor

			public void Add(DisplayTurnover dt, bool hasOther) {
				if (!dt.HasValue && hasOther) {
					AllCount++;
					GoodCount++;
					return;
				} // if

				AllCount++;

				if (dt.HasValue && dt.IsGood)
					GoodCount++;
				else
					BadCount++;

				if (this.actualTurnovers.ContainsKey(dt.ActualRatio))
					this.actualTurnovers[dt.ActualRatio]++;
				else
					this.actualTurnovers[dt.ActualRatio] = 1;

				this.actualTurnoverMedian = null;
			} // Add

			public void LogRawActualTurnovers(ASafeLog log, string title) {
				log.Debug("{0} actual turnover data ({1}) - begin", title, this.actualTurnovers.Sum(pair => pair.Value));
				log.Debug("\n\t{0}", string.Join("\n\t", this.actualTurnovers.Select(pair => pair.Key + ": " + pair.Value)));
				log.Debug("{0} actual turnover data - end", title);
			} // LogRawActualTurnovers

			private readonly SortedDictionary<decimal, int> actualTurnovers;
			private decimal? actualTurnoverMedian;
		}

		private class DisplayTurnover {
			public bool HasValue { get; private set; }
			public decimal RequestedRatio { get; private set; }
			public decimal PeriodTurnover { get; private set; }
			public decimal AnnualizedTurnover { get; private set; }
			public decimal WaterMark { get; private set; }
			public decimal ActualRatio { get; private set; }

			public string AnnualizedTurnoverStyle { get; private set; }
			public string ActualRatioStyle { get; private set; }
			public decimal YearTurnover { get; private set; }

			public bool IsGood {
				get { return ActualRatio >= RequestedRatio; }
			} // IsGood

			public DisplayTurnover(bool hasValue, int periodLen, decimal periodTurnover, decimal yearTurnover, decimal ratio) {
				HasValue = hasValue;

				PeriodLength = periodLen;
				PeriodTurnover = periodTurnover;
				YearTurnover = yearTurnover;
				RequestedRatio = ratio;

				WaterMark = YearTurnover * RequestedRatio;

				AnnualizedTurnover = PeriodTurnover / PeriodLength * 12;

				ActualRatio = HasValue
					? (YearTurnover == 0 ? 0 : AnnualizedTurnover / YearTurnover)
					: 0;

				AnnualizedTurnoverStyle = AnnualizedTurnover < WaterMark ? red : null;

				ActualRatioStyle = ActualRatio < RequestedRatio ? red : null;
			} // constructor

			private int PeriodLength { get; set; }
			private const string red = "color:red;";
		}

		private class CellValue {
			public string ValueStr {
				get {
					if (this.storedValue is int)
						return ((int)this.storedValue).Stringify();

					if (this.storedValue is long)
						return ((long)this.storedValue).Stringify();

					if (this.storedValue is decimal)
						return ((decimal)this.storedValue).Stringify();

					if (this.storedValue is DateTime)
						return ((DateTime)this.storedValue).Stringify();

					return this.storedValue == null ? "&nbsp;" : this.storedValue.ToString();
				}
			} // ValueStr

			public string Style { get; private set; }

			public bool AlignRight { get; private set; }

			public int Colspan { get; private set; }

			public CellValue(object v, string s = null, int colspan = 1) {
				this.storedValue = v;
				Style = s;
				Colspan = colspan;

				AlignRight = (v is int) || (v is long) || (v is decimal) || (v is DateTime);
			}

			private readonly object storedValue;
			// constructor
		}

		private void Generate() {
			const string header = "background-color:blue;color:yellow;";
			const string blueish = "background-color:#D6F8FF;";
			const string orangeish = "background-color:#ffddd6;color:green;font-weight:bold;";

			var tbl = new Table().Add<Ezbob.Utils.Html.Attributes.Style>("border-collapse:collapse;");

			Thead thead = new Thead();
			tbl.Append(thead);

			thead.Append(AddRow<Th>(
				new CellValue("Cash request ID", header),
				new CellValue("Customer ID", header),
				new CellValue("Manual decision", header),
				new CellValue("Decision time", header),
				new CellValue("Online data time", header),
				new CellValue("Online 1Y turnover", header),
				new CellValue("Online 1M requested ratio", header),
				new CellValue("Online 1M turnover", header),
				new CellValue("Online annualized 1M turnover", header),
				new CellValue("Online 1M watermark value", header),
				new CellValue("Online 1M actual ratio", header),
				new CellValue("Online 3M requested ratio", header),
				new CellValue("Online 3M turnover", header),
				new CellValue("Online annualized 3M turnover", header),
				new CellValue("Online 3M watermark value", header),
				new CellValue("Online 3M actual ratio", header),
				new CellValue("HMRC data time", header),
				new CellValue("HMRC 1Y turnover", header),
				new CellValue("HMRC 3M requested ratio", header),
				new CellValue("HMRC 3M turnover", header),
				new CellValue("HMRC annualized 3M turnover", header),
				new CellValue("HMRC 3M watermark value", header),
				new CellValue("HMRC 3M actual ratio", header),
				new CellValue("HMRC 6M requested ratio", header),
				new CellValue("HMRC 6M turnover", header),
				new CellValue("HMRC annualized 6M turnover", header),
				new CellValue("HMRC 6M watermark value", header),
				new CellValue("HMRC 6M actual ratio", header)
				));

			var online1Total = new DisplayTurnoverTotals();
			var online3Total = new DisplayTurnoverTotals();
			var hmrc3Total = new DisplayTurnoverTotals();
			var hmrc6Total = new DisplayTurnoverTotals();

			int hasBothCount = 0;

			Tbody totalsBody = new Tbody();
			tbl.Append(totalsBody);

			Tbody tbody = new Tbody();
			tbl.Append(tbody);
			/*
			foreach (KeyValuePair<long, CalculatedTurnover> pair in Result) {
				long cashRequestID = pair.Key;
				CalculatedTurnover turnover = pair.Value;
				CrCuTi crcuti = Input[cashRequestID];

				decimal online12 = turnover.GetOnline(12);
				decimal hmrc12 = turnover.GetHmrc(12);

				if (turnover.OnlineUpdateTime.HasValue && turnover.HmrcUpdateTime.HasValue)
					hasBothCount++;

				DisplayTurnover online1 = new DisplayTurnover(turnover.OnlineUpdateTime.HasValue, 1, turnover.GetOnline(1), online12, CurrentValues.Instance.AutoApproveOnlineTurnoverDropMonthRatio);
				online1Total.Add(online1, turnover.HmrcUpdateTime.HasValue);

				DisplayTurnover online3 = new DisplayTurnover(turnover.OnlineUpdateTime.HasValue, 3, turnover.GetOnline(3), online12, CurrentValues.Instance.AutoApproveOnlineTurnoverDropQuarterRatio);
				online3Total.Add(online3, turnover.HmrcUpdateTime.HasValue);

				DisplayTurnover hmrc3 = new DisplayTurnover(turnover.HmrcUpdateTime.HasValue, 3, turnover.GetHmrc(3), hmrc12, CurrentValues.Instance.AutoApproveHmrcTurnoverDropQuarterRatio);
				hmrc3Total.Add(hmrc3, turnover.OnlineUpdateTime.HasValue);

				DisplayTurnover hmrc6 = new DisplayTurnover(turnover.HmrcUpdateTime.HasValue, 6, turnover.GetHmrc(6), hmrc12, CurrentValues.Instance.AutoApproveHmrcTurnoverDropHalfYearRatio);
				hmrc6Total.Add(hmrc6, turnover.OnlineUpdateTime.HasValue);

				tbody.Append(AddRow<Td>(
					new CellValue(cashRequestID),
					new CellValue(crcuti.CustomerID),
					new CellValue(crcuti.UnderwriterDecision, crcuti.UnderwriterDecision == "Approved" ? "color:green;" : "color:red;"),
					new CellValue(crcuti.DecisionTime),

					new CellValue(turnover.OnlineUpdateTime, blueish),
					new CellValue(online12, blueish),

					new CellValue(online1.RequestedRatio, orangeish),
					new CellValue(online1.PeriodTurnover),
					new CellValue(online1.AnnualizedTurnover, online1.AnnualizedTurnoverStyle),
					new CellValue(online1.WaterMark),
					new CellValue(online1.ActualRatio, online1.ActualRatioStyle),

					new CellValue(online3.RequestedRatio, orangeish),
					new CellValue(online3.PeriodTurnover),
					new CellValue(online3.AnnualizedTurnover, online3.AnnualizedTurnoverStyle),
					new CellValue(online3.WaterMark),
					new CellValue(online3.ActualRatio, online3.ActualRatioStyle),

					new CellValue(turnover.HmrcUpdateTime, blueish),
					new CellValue(hmrc12, blueish),

					new CellValue(hmrc3.RequestedRatio, orangeish),
					new CellValue(hmrc3.PeriodTurnover),
					new CellValue(hmrc3.AnnualizedTurnover, hmrc3.AnnualizedTurnoverStyle),
					new CellValue(hmrc3.WaterMark),
					new CellValue(hmrc3.ActualRatio, hmrc3.ActualRatioStyle),

					new CellValue(hmrc6.RequestedRatio, orangeish),
					new CellValue(hmrc6.PeriodTurnover),
					new CellValue(hmrc6.AnnualizedTurnover, hmrc6.AnnualizedTurnoverStyle),
					new CellValue(hmrc6.WaterMark),
					new CellValue(hmrc6.ActualRatio, hmrc6.ActualRatioStyle)
				));
			} // for each pair
			*/
			totalsBody.Append(AddTotalRow<Td>(
				new CellValue("Total cash requests", colspan: 3),
				new CellValue(Result.Count),
				new CellValue(null, colspan: 2),
				new CellValue("Total count", colspan: 4),
				new CellValue(online1Total.AllCount),
				new CellValue("Total count", colspan: 4),
				new CellValue(online3Total.AllCount),
				new CellValue(null, colspan: 2),
				new CellValue("Total count", colspan: 4),
				new CellValue(hmrc3Total.AllCount),
				new CellValue("Total count", colspan: 4),
				new CellValue(hmrc6Total.AllCount)
				));

			totalsBody.Append(AddTotalRow<Td>(
				new CellValue("Both online and HMRC", colspan: 3),
				new CellValue(hasBothCount),
				new CellValue(null, colspan: 2),
				new CellValue("Good count", colspan: 4),
				new CellValue(online1Total.GoodCount),
				new CellValue("Good count", colspan: 4),
				new CellValue(online3Total.GoodCount),
				new CellValue(null, colspan: 2),
				new CellValue("Good count", colspan: 4),
				new CellValue(hmrc3Total.GoodCount),
				new CellValue("Good count", colspan: 4),
				new CellValue(hmrc6Total.GoodCount)
				));

			totalsBody.Append(AddTotalRow<Td>(
				new CellValue(null, colspan: 6),
				new CellValue("Bad count", colspan: 4),
				new CellValue(online1Total.BadCount),
				new CellValue("Bad count", colspan: 4),
				new CellValue(online3Total.BadCount),
				new CellValue(null, colspan: 2),
				new CellValue("Bad count", colspan: 4),
				new CellValue(hmrc3Total.BadCount),
				new CellValue("Bad count", colspan: 4),
				new CellValue(hmrc6Total.BadCount)
				));

			totalsBody.Append(AddTotalRow<Td>(
				new CellValue(null, colspan: 6),
				new CellValue("Actual turnover median", colspan: 4),
				new CellValue(online1Total.ActualTurnoverMedian),
				new CellValue("Actual turnover median", colspan: 4),
				new CellValue(online3Total.ActualTurnoverMedian),
				new CellValue(null, colspan: 2),
				new CellValue("Actual turnover median", colspan: 4),
				new CellValue(hmrc3Total.ActualTurnoverMedian),
				new CellValue("Actual turnover median", colspan: 4),
				new CellValue(hmrc6Total.ActualTurnoverMedian)
				));

			Email = new Body().Append(tbl);

			online1Total.LogRawActualTurnovers(Log, "Online 1M");
			online3Total.LogRawActualTurnovers(Log, "Online 3M");
			hmrc3Total.LogRawActualTurnovers(Log, "HMRC 3M");
			hmrc6Total.LogRawActualTurnovers(Log, "HMRC 6M");
		} // Generate

		private void Send() {
			string sEmail = CurrentValues.Instance.MaamEmailReceiver;

			if (string.IsNullOrWhiteSpace(sEmail))
				Log.Debug("Not sending:\n{0}", Email);
			else {
				new Mail().Send(
					sEmail,
					null,
					Email.ToString(),
					CurrentValues.Instance.MailSenderEmail,
					CurrentValues.Instance.MailSenderName,
					"Man Against A Machine - Turnover data"
					);
			} // if
		} // Send

		private Tr AddTotalRow<T>(params CellValue[] cells) where T : ATableCell, new() {
			Tr tr = AddRow<T>(cells);
			tr.Add<Ezbob.Utils.Html.Attributes.Style>("background-color:#720278;color:white;font-weight:bold;");
			return tr;
		} // AddTotalRow

		private Tr AddRow<T>(params CellValue[] cells) where T : ATableCell, new() {
			var tr = new Tr();

			foreach (CellValue cv in cells) {
				var cell = new T();

				if (cv.Colspan > 1)
					cell.Add<Colspan>(cv.Colspan.ToString(CultureInfo.InvariantCulture));

				cell.Append(new Text(cv.ValueStr));

				if ((typeof(T) == typeof(Td)) && cv.AlignRight)
					cell.Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;");

				cell.Add<Ezbob.Utils.Html.Attributes.Style>("border:1px solid black;padding:5px;");

				if (!string.IsNullOrWhiteSpace(cv.Style))
					cell.Add<Ezbob.Utils.Html.Attributes.Style>(cv.Style);

				tr.Append(cell);
			} // for each

			return tr;
		} // AddRow

		private void Load() {
			Result = new SortedDictionary<long, ACalculatedTurnoverBase>();
			Input = new SortedDictionary<long, CrCuTi>();

			/*
		Args.Query = QueryFormat;
		Args.Condition = "AND t.CashRequestID > {0}";

		List<CrCuTi> lst = DB.Fill<CrCuTi>(Args.Query, CommandSpecies.Text);

		var pc = new ProgressCounter("{0} of " + lst.Count + " cash requests processed.", this.Log, 10);

		foreach (CrCuTi crcuti in lst) {
			Input[crcuti.CashRequestID] = crcuti;
			var turnover = new CalculatedTurnover();
			turnover.Init();

			Result[crcuti.CashRequestID] = turnover;

			DB.ForEachRowSafe(
				sr => turnover.Add(sr, Log),
				"GetCustomerTurnoverDataForAutoApprove",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", crcuti.CustomerID),
				new QueryParameter("Now", crcuti.DecisionTime)
			);

			pc++;
		} // for each

		pc.Log();
			*/
		} // Load

		private const string QueryFormat = @"
SELECT DISTINCT {0}
	t.CashRequestID,
	r.IdCustomer AS CustomerID,
	r.UnderwriterDecisionDate AS DecisionTime,
	r.UnderwriterDecision
FROM
	DecisionTrace tc
	INNER JOIN DecisionTrail t
		ON tc.TrailID = t.TrailID
		AND t.Tag = '2014-12-10_13-52-40_bdeb9337ecc4436ab52d4c8f78bb5699'
	INNER JOIN CashRequests r ON t.CashRequestID = r.Id
WHERE
	tc.Name IN (
		'AutomationCalculator.ProcessHistory.AutoApproval.OnlineOneMonthTurnover',
		'AutomationCalculator.ProcessHistory.AutoApproval.OnlineThreeMonthsTurnover',
		'AutomationCalculator.ProcessHistory.AutoApproval.HmrcThreeMonthsTurnover',
		'AutomationCalculator.ProcessHistory.AutoApproval.HalfYearTurnover'
	)
	-- AND tc.DecisionStatusID != 1
	{1}
ORDER BY
	t.CashRequestID DESC";
	} // class LoadTurnovers
} // namespace
