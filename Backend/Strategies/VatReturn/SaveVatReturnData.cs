namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using JetBrains.Annotations;

	public class SaveVatReturnData : AStrategy {
		#region public

		#region constructor

		public SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			VatReturnRawData[] oVatReturn,
			RtiTaxMonthRawData[] oRtiMonths,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oSp = new SpSaveVatReturnSummary(DB, Log) {
				CustomerMarketplaceID = nCustomerMarketplaceID,
				HistoryRecordID = nHistoryRecordID,
				SourceID = nSourceID,
				VatReturnRecords = oVatReturn,
				RtiTaxMonthRawData = oRtiMonths,
			};
			m_oSp.InitEntries();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Save VAT return data"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Debug(m_oSp);

			// TODO: m_oSp.ExecuteNonQuery();
			// TODO: new CalculateVatReturnSummary(m_oSp.CustomerMarketplaceID, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpSaveVatReturnSummary m_oSp;

		#region class VatReturnRawEntry

		private class VatReturnRawEntry : ITraversable {
			[UsedImplicitly]
			public int RecordID { get; set; }

			[UsedImplicitly]
			public string BoxName { get; set; }

			[UsedImplicitly]
			public decimal Amount { get; set; }

			[UsedImplicitly]
			public string CurrencyCode { get; set; }

			public override string ToString() {
				return string.Format("{0}: {1} = {2} {3}", RecordID, BoxName, Amount, CurrencyCode);
			} // ToString
		} // class VatReturnRawEntry

		#endregion class VatReturnRawEntry

		#region class SpSaveVatReturnSummary
		// ReSharper disable ValueParameterNotUsed

		private class SpSaveVatReturnSummary : AStoredProc {
			#region constructor

			public SpSaveVatReturnSummary(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return
					(CustomerMarketplaceID > 0) &&
					(HistoryRecordID > 0) &&
					(SourceID > 0) &&
					(
						(VatReturnRecords.Length > 0) ||
						(RtiTaxMonthRawData.Length > 0)
					);
			} // HasValidParameters

			#endregion method HasValidParameters

			#region DB arguments

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; } // CustomerMarketplaceID

			[UsedImplicitly]
			public int HistoryRecordID { get; set; } // HistoryRecordID

			[UsedImplicitly]
			public int SourceID { get; set; } // SourceID

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; } // get
				set { } // set, !!! DO NOT REMOVE, DO NOT MAKE PRIVATE !!!
			} // Now

			[UsedImplicitly]
			public VatReturnRawData[] VatReturnRecords { get; set; } // VatReturnRecords

			[UsedImplicitly]
			public VatReturnRawEntry[] VatReturnEntries { get; set; } // VatReturnEntries

			[UsedImplicitly]
			public RtiTaxMonthRawData[] RtiTaxMonthRawData { get; set; } // RtiTaxMonthRawData

			#endregion DB arguments

			#region method InitEntries

			public void InitEntries() {
				int nRecordID = 1;
				var lst = new List<VatReturnRawEntry>();

				foreach (VatReturnRawData r in VatReturnRecords) {
					r.RecordID = nRecordID;
					r.IsDeleted = false;

					lst.AddRange(r.Data.Select(e => new VatReturnRawEntry {
						Amount = e.Value.Amount,
						BoxName = e.Key,
						CurrencyCode = e.Value.CurrencyCode,
						RecordID = nRecordID,
					}));

					nRecordID++;
				} // for each record

				VatReturnEntries = lst.ToArray();
			} // InitEntries

			#endregion method InitEntries

			#region method ToString

			public override string ToString() {
				var os = new StringBuilder();

				os.AppendFormat("MP ID: {0}, History ID: {1}, Source ID: {2}\n", CustomerMarketplaceID, HistoryRecordID, SourceID);

				var sd = new List<Tuple<string, IEnumerable>> {
					new Tuple<string, IEnumerable>("records", VatReturnRecords),
					new Tuple<string, IEnumerable>("entries", VatReturnEntries),
					new Tuple<string, IEnumerable>("RTI months", RtiTaxMonthRawData),
				};

				foreach (var o in sd) {
					os.AppendFormat("Start of {0}:\n", o.Item1);

					foreach (var p in o.Item2)
						os.AppendLine(p.ToString());

					os.AppendFormat("End of {0}:\n", o.Item1);
				} // for each

				return os.ToString();
			} // ToSring

			#endregion method ToString
		} // class SpSaveVatReturnSummary

		// ReSharper restore ValueParameterNotUsed
		#endregion class SpSaveVatReturnSummary

		#endregion private
	} // class SaveVatReturnData
} // namespace EzBob.Backend.Strategies.VatReturn
