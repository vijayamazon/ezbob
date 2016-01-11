namespace AutomationCalculator.ProcessHistory.Trails {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using JetBrains.Annotations;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public partial class ApprovalInputData : ITrailInputData {
		private class SerializationModel {
			[UsedImplicitly]
			public decimal AvailableFunds { get; set; }

			[UsedImplicitly]
			public Configuration Configuration { get; set; }

			[UsedImplicitly]
			public List<string> EnabledTraces { get; set; }

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public long CashRequestID { get; set; }

			[UsedImplicitly]
			public long NlCashRequestID { get; set; }

			[UsedImplicitly]
			public DateTime DataAsOf { get; set; }

			[UsedImplicitly]
			public List<Tuple<string, string>> DirectorNames { get; set; }

			[UsedImplicitly]
			public List<string> HmrcBusinessNames { get; set; }

			[UsedImplicitly]
			public decimal Turnover1Y { get; set; }

			[UsedImplicitly]
			public decimal Turnover3M { get; set; }

			[UsedImplicitly]
			public List<Payment> LatePayments { get; set; }

			[UsedImplicitly]
			public int MarketplaceSeniority { get; set; }

			[UsedImplicitly]
			[JsonConverter(typeof(StringEnumConverter))]
			public Medal Medal { get; set; }

			[UsedImplicitly]
			[JsonConverter(typeof(StringEnumConverter))]
			public MedalType MedalType { get; set; }

			[UsedImplicitly]
			public string TurnoverTypeStr { get; set; }

			[UsedImplicitly]
			public MetaData MetaData { get; set; }

			[UsedImplicitly]
			public List<string> MetaDataValidationErrors { get; set; }

			[UsedImplicitly]
			public decimal ReservedFunds { get; set; }

			[UsedImplicitly]
			public decimal SystemCalculatedAmount { get; set; }

			public void FlushTo(ApprovalInputData dst) {
				Configuration.EnabledTraces.Clear();

				foreach (string traceName in EnabledTraces)
					Configuration.EnabledTraces.Add(traceName);

				dst.Clean();

				dst.SetDataAsOf(DataAsOf);
				dst.SetConfiguration(Configuration);
				dst.SetMetaData(MetaData);
				dst.MetaData.RestoreValidationErrors(MetaDataValidationErrors);

				AutomationCalculator.Common.TurnoverType turnoverType;

				bool turnoverTypeParsed = Enum.TryParse(TurnoverTypeStr, out turnoverType);

				dst.SetArgs(
					CustomerID,
					CashRequestID,
					NlCashRequestID,
					SystemCalculatedAmount,
					Medal,
					MedalType,
					turnoverTypeParsed ? turnoverType : (AutomationCalculator.Common.TurnoverType?)null
				);

				dst.SetSeniority(MarketplaceSeniority);
				dst.LatePayments = new List<Payment>(LatePayments);

				dst.SetTurnover(3, Turnover3M);
				dst.SetTurnover(12, Turnover1Y);

				dst.AvailableFunds = AvailableFunds;
				dst.ReservedFunds = ReservedFunds;

				dst.DirectorNames.Clear();
				dst.DirectorNames.AddRange(DirectorNames.Select(t => new Name(t.Item1, t.Item2)));

				dst.SetHmrcBusinessNames(HmrcBusinessNames.Select(s => new NameForComparison(s)));
			} // FlushTo

			public SerializationModel InitFrom(ApprovalInputData src) {
				DataAsOf = src.DataAsOf;

				MetaData = src.MetaData;

				MetaDataValidationErrors = new List<string>();
				if ((src.MetaData != null) && (src.MetaData.ValidationErrors != null))
					MetaDataValidationErrors.AddRange(src.MetaData.ValidationErrors);

				Configuration = src.Configuration;
				EnabledTraces = new List<string>(src.Configuration.EnabledTraces);

				CustomerID = src.CustomerID;
				SystemCalculatedAmount = src.SystemCalculatedAmount;
				Medal = src.Medal;
				MedalType = src.MedalType;
				TurnoverTypeStr = src.TurnoverType == null ? string.Empty : src.TurnoverType.ToString();

				MarketplaceSeniority = src.MarketplaceSeniority;

				LatePayments = new List<Payment>();
				if (src.LatePayments != null)
					LatePayments.AddRange(src.LatePayments);

				Turnover1Y = src.Turnover1Y;
				Turnover3M = src.Turnover3M;

				AvailableFunds = src.AvailableFunds;
				ReservedFunds = src.ReservedFunds;

				DirectorNames = new List<Tuple<string, string>>();
				if (src.DirectorNames != null) {
					DirectorNames.AddRange(src.DirectorNames.Select(
						n => new Tuple<string, string>(n.FirstName, n.LastName)
					));
				} // if

				HmrcBusinessNames = new List<string>();
				if (src.HmrcBusinessNames != null)
					HmrcBusinessNames.AddRange(src.HmrcBusinessNames.Select(n => n.RawName));

				return this;
			} // InitFrom
		} // class SerializationModel
	} // class ApprovalInputData
} // namespace
