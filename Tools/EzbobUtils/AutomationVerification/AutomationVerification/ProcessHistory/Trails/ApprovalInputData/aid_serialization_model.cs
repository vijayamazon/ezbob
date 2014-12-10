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
			public DateTime DataAsOf { get; set; }

			[UsedImplicitly]
			public Configuration Configuration { get; set; }

			[UsedImplicitly]
			public MetaData MetaData { get; set; }

			[UsedImplicitly]
			public List<string> MetaDataValidationErrors { get; set; }

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public decimal SystemCalculatedAmount { get; set; }

			[UsedImplicitly]
			[JsonConverter(typeof(StringEnumConverter))]
			public Medal Medal { get; set; }

			[UsedImplicitly]
			public List<string> WorstStatusList { get; set; }

			[UsedImplicitly]
			public int MarketplaceSeniority { get; set; }

			[UsedImplicitly]
			public List<Payment> LatePayments { get; set; }

			[UsedImplicitly]
			public decimal OnlineTurnover1M { get; set; }
			[UsedImplicitly]
			public decimal OnlineTurnover3M { get; set; }
			[UsedImplicitly]
			public decimal OnlineTurnover1Y { get; set; }

			[UsedImplicitly]
			public DateTime? OnlineUpdateTime { get; set; }
			[UsedImplicitly]
			public bool HasOnline { get; set; }

			[UsedImplicitly]
			public bool HasHmrc { get; set; }
			[UsedImplicitly]
			public DateTime? HmrcUpdateTime { get; set; }

			[UsedImplicitly]
			public decimal HmrcTurnover3M { get; set; }
			[UsedImplicitly]
			public decimal HmrcTurnover6M { get; set; }
			[UsedImplicitly]
			public decimal HmrcTurnover1Y { get; set; }

			[UsedImplicitly]
			public decimal AvailableFunds { get; set; }
			[UsedImplicitly]
			public decimal ReservedFunds { get; set; }

			[UsedImplicitly]
			public List<Tuple<string, string>> DirectorNames { get; set; }

			[UsedImplicitly]
			public List<string> HmrcBusinessNames { get; set; }

			public SerializationModel InitFrom(ApprovalInputData src) {
				DataAsOf = src.DataAsOf;

				MetaData = src.MetaData;

				MetaDataValidationErrors = new List<string>();
				if ((src.MetaData != null) && (src.MetaData.ValidationErrors != null))
					MetaDataValidationErrors.AddRange(src.MetaData.ValidationErrors);

				Configuration = src.Configuration;
				CustomerID = src.CustomerID;
				SystemCalculatedAmount = src.SystemCalculatedAmount;
				Medal = src.Medal;

				WorstStatusList = new List<string>();
				if (src.WorstStatusList != null)
					WorstStatusList.AddRange(src.WorstStatusList);

				MarketplaceSeniority = src.MarketplaceSeniority;

				LatePayments = new List<Payment>();
				if (src.LatePayments != null)
					LatePayments.AddRange(src.LatePayments);

				OnlineTurnover1M = src.OnlineTurnover1M;
				OnlineTurnover3M = src.OnlineTurnover3M;
				OnlineTurnover1Y = src.OnlineTurnover1Y;

				OnlineUpdateTime = src.OnlineUpdateTime;
				HasOnline = src.HasOnline;

				HasHmrc = src.HasHmrc;
				HmrcUpdateTime = src.HmrcUpdateTime;

				HmrcTurnover3M = src.HmrcTurnover3M;
				HmrcTurnover6M = src.HmrcTurnover6M;
				HmrcTurnover1Y = src.HmrcTurnover1Y;

				AvailableFunds = src.AvailableFunds;
				ReservedFunds = src.ReservedFunds;

				DirectorNames = new List<Tuple<string, string>>();
				if (src.DirectorNames != null)
					DirectorNames.AddRange(src.DirectorNames.Select(n => new Tuple<string, string>(n.FirstName, n.LastName)));

				HmrcBusinessNames = new List<string>();
				if (src.HmrcBusinessNames != null)
					HmrcBusinessNames.AddRange(src.HmrcBusinessNames);

				return this;
			} // InitFrom

			public void FlushTo(ApprovalInputData dst) {
				dst.Clean();

				dst.SetDataAsOf(DataAsOf);
				dst.SetConfiguration(Configuration);
				dst.SetMetaData(MetaData);
				dst.MetaData.RestoreValidationErrors(MetaDataValidationErrors);
				dst.SetArgs(CustomerID, SystemCalculatedAmount, Medal);
				dst.SetWorstStatuses(WorstStatusList);
				dst.SetSeniority(MarketplaceSeniority);
				dst.LatePayments = new List<Payment>(LatePayments);

				dst.SetOnlineTurnover(1, OnlineTurnover1M);
				dst.SetOnlineTurnover(3, OnlineTurnover3M);
				dst.SetOnlineTurnover(12, OnlineTurnover1Y);

				dst.OnlineUpdateTime = OnlineUpdateTime;
				dst.HasOnline = HasOnline;

				dst.HasHmrc = HasHmrc;
				dst.HmrcUpdateTime = HmrcUpdateTime;

				dst.SetHmrcTurnover(3, HmrcTurnover3M);
				dst.SetHmrcTurnover(6, HmrcTurnover6M);
				dst.SetHmrcTurnover(12, HmrcTurnover1Y);

				dst.AvailableFunds = AvailableFunds;
				dst.ReservedFunds = ReservedFunds;

				dst.DirectorNames.Clear();
				dst.DirectorNames.AddRange(DirectorNames.Select(t => new Name(t.Item1, t.Item2)));

				dst.SetHmrcBusinessNames(HmrcBusinessNames);
			} // FlushTo
		} // class SerializationModel
	} // class ApprovalInputData
} // namespace
