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
			ElapsedTimeInfo = new ElapsedTimeInfo();

			m_oSp = new SpSaveVatReturnSummary(DB, Log) {
				CustomerMarketplaceID = nCustomerMarketplaceID,
				HistoryRecordID = nHistoryRecordID,
				SourceID = nSourceID,
				VatReturnRecords = oVatReturn,
				RtiTaxMonthRawData = oRtiMonths,
			};
			m_oSp.InitEntries((VatReturnSourceType)nSourceID);

			m_oRaw = new LoadVatReturnRawData(nCustomerMarketplaceID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Save VAT return data"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oRaw.Execute();

			foreach (var oOld in m_oRaw.VatReturnRawData)
				foreach (var oNew in m_oSp.VatReturnRecords)
					if (oOld.Overlaps(oNew))
						m_oSp.AddHistoryItem(oOld, oNew);

			Log.Debug(m_oSp);

			// TODO: fill elapsed time
			// TODO: m_oSp.ExecuteNonQuery();
			// TODO: new CalculateVatReturnSummary(m_oSp.CustomerMarketplaceID, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#region property ElapsedTimeInfo

		public ElapsedTimeInfo ElapsedTimeInfo { get; private set; } // ElapsedTimeInfo

		#endregion property ElapsedTimeInfo

		#endregion public

		#region private

		private readonly SpSaveVatReturnSummary m_oSp;
		private readonly LoadVatReturnRawData m_oRaw;

		#region class SpSaveVatReturnSummary
		// ReSharper disable ValueParameterNotUsed

		private class SpSaveVatReturnSummary : AStoredProc {
			#region enum DeleteReasons

			public enum DeleteReasons {
				UploadedEqual = 1,
				UploadedNotEqual = 2,
				OverriddenByUploaded = 3,
				ManualUpdated = 4,
				ManualDeleted = 5,
				ManualRejectedByUploaded = 6,
				ManualRejectedByLinked = 7,
				UploadedRejectedByLinked = 8,
				LinkedUpdated = 9,
			} // enum DeleteReasons

			#endregion enum DeleteReasons

			#region constructor

			public SpSaveVatReturnSummary(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				m_oHistoryItems = new List<HistoryItem>();
			} // constructor

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

			#region method AddHistoryItem

			public void AddHistoryItem(VatReturnRawData oDeletedItem) {
				oDeletedItem.IsDeleted = true;

				m_oHistoryItems.Add(new HistoryItem {
					DeleteRecordInternalID = oDeletedItem.InternalID,
					ReasonID = (int)DeleteReasons.ManualDeleted,
				});
			} // AddHistoryItem

			public void AddHistoryItem(VatReturnRawData oOld, VatReturnRawData oNew) {
				// At this point oOld and oNew have overlapping date intervals and same registration # and oNew is not deleted.

				DeleteReasons nReason;
				VatReturnRawData oDeletedItem;
				VatReturnRawData oReasonItem;

				switch (oOld.SourceType) {
				case VatReturnSourceType.Linked:
					if (oNew.SourceType == VatReturnSourceType.Linked) {
						oDeletedItem = oOld;
						oReasonItem = oNew;
						nReason = DeleteReasons.LinkedUpdated;
					}
					else {
						oDeletedItem = oNew;
						oReasonItem = oOld;
						nReason = oNew.SourceType == VatReturnSourceType.Manual ? DeleteReasons.ManualRejectedByLinked : DeleteReasons.UploadedRejectedByLinked;
					}
					break;

				case VatReturnSourceType.Uploaded:
					switch (oNew.SourceType) {
					case VatReturnSourceType.Linked:
						oDeletedItem = oOld;
						oReasonItem = oNew;
						nReason = DeleteReasons.UploadedRejectedByLinked;
						break;

					case VatReturnSourceType.Uploaded:
						oDeletedItem = oNew;
						oReasonItem = oOld;
						nReason = oOld.SameAs(oNew) ? DeleteReasons.UploadedEqual : DeleteReasons.UploadedNotEqual;
						break;

					default: // oNew.SourceType is Manual
						oDeletedItem = oNew;
						oReasonItem = oOld;
						nReason = DeleteReasons.ManualRejectedByUploaded;
						break;
					} // switch

					break;

				default: // oOld.SourceType is Manual
					oDeletedItem = oOld;
					oReasonItem = oNew;

					switch (oNew.SourceType) {
					case VatReturnSourceType.Linked:
						nReason = DeleteReasons.ManualRejectedByLinked;
						break;

					case VatReturnSourceType.Uploaded:
						nReason = DeleteReasons.OverriddenByUploaded;
						break;

					default:
						nReason = DeleteReasons.ManualUpdated;
						break;
					} // switch

					break;
				} // switch

				oDeletedItem.IsDeleted = true;

				m_oHistoryItems.Add(new HistoryItem {
					DeleteRecordInternalID = oDeletedItem.InternalID,
					ReasonRecordID = oReasonItem.RecordID,
					ReasonID = (int)nReason,
				});
			} // AddHistoryItem

			#endregion method AddHistoryItem

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
			public Entry[] VatReturnEntries { get; set; } // VatReturnEntries

			[UsedImplicitly]
			public RtiTaxMonthRawData[] RtiTaxMonthRawData { get; set; } // RtiTaxMonthRawData

			[UsedImplicitly]
			public HistoryItem[] HistoryItems {
				get { return m_oHistoryItems.ToArray(); }
				set { }
			} // HistoryItems

			private readonly List<HistoryItem> m_oHistoryItems;

			#endregion DB arguments

			#region method InitEntries

			public void InitEntries(VatReturnSourceType nSourceType) {
				int nRecordID = 1;
				var lst = new List<Entry>();

				foreach (VatReturnRawData r in VatReturnRecords) {
					r.SourceType = nSourceType;
					r.RecordID = nRecordID;

					if (r.IsDeleted)
						AddHistoryItem(r);

					lst.AddRange(r.Data.Select(e => new Entry {
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

					os.AppendFormat("End of {0}.\n", o.Item1);
				} // for each

				return os.ToString();
			} // ToSring

			#endregion method ToString

			#region class Entry

			public class Entry : ITraversable {
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
			} // class Entry

			#endregion class Entry

			#region class HistoryItem

			public class HistoryItem : ITraversable {
				[UsedImplicitly]
				public Guid DeleteRecordInternalID { get; set; }

				[UsedImplicitly]
				public int? ReasonRecordID { get; set; }

				[UsedImplicitly]
				public int ReasonID { get; set; }

				[UsedImplicitly]
				public DateTime DeleteTime {
					get { return DateTime.UtcNow; }
					set { }
				} // DeleteTime
			} // HistoryItem

			#endregion class HistoryItem
		} // class SpSaveVatReturnSummary

		// ReSharper restore ValueParameterNotUsed
		#endregion class SpSaveVatReturnSummary

		#endregion private
	} // class SaveVatReturnData
} // namespace EzBob.Backend.Strategies.VatReturn
