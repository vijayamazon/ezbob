namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class SaveVatReturnData : AVatReturnStrategy {
		public SaveVatReturnData(
			int nCustomerMarketplaceID,
			int nHistoryRecordID,
			int nSourceID,
			IEnumerable<VatReturnRawData> oVatReturn,
			IEnumerable<RtiTaxMonthRawData> oRtiMonths
		) {
			m_oStopper = new Stopper();

			m_oSp = new SpSaveVatReturnData(this, DB, Log) {
				CustomerMarketplaceID = nCustomerMarketplaceID,
				HistoryRecordID = nHistoryRecordID,
				SourceID = nSourceID,
				VatReturnRecords = new List<VatReturnRawData>(oVatReturn),
				RtiTaxMonthRawData = new List<RtiTaxMonthRawData>(oRtiMonths),
			};
			m_oSp.InitEntries((VatReturnSourceType)nSourceID);

			m_oRaw = new LoadVatReturnRawData(nCustomerMarketplaceID);
		} // constructor

		public override string Name {
			get { return "Save VAT return data"; }
		} // Name

		public override void Execute() {
			if (m_oSp.IsEmptyInput())
				return;

			m_oStopper.Execute(ElapsedDataMemberType.RetrieveDataFromDatabase, () => {
				m_oRaw.Execute();

				foreach (var oOld in m_oRaw.VatReturnRawData)
					foreach (var oNew in m_oSp.VatReturnRecords)
						if (oOld.Overlaps(oNew))
							m_oSp.AddHistoryItem(oOld, oNew);
			});

			Log.Debug(m_oSp);

			var os = new StringBuilder();

			os.AppendLine("SaveVatReturnData output - begin:");

			int nRowNum = 0;

			m_oStopper.Execute(ElapsedDataMemberType.StoreDataToDatabase, () => {
				m_oSp.ForEachRow((oReader, bRowsetStart) => {
					var vals = new object[oReader.FieldCount];

					int nRead = oReader.GetValues(vals);

					os.AppendFormat("\nRow {0}{3}: {1} fields, {2} read.\n", nRowNum, oReader.FieldCount, nRead, bRowsetStart ? " NEW ROWSET" : string.Empty);

					for (int i = 0; i < nRead; i++)
						os.AppendFormat("\t{2} - {0}: {1}\n", oReader.GetName(i), vals[i], i);

					nRowNum++;
					return ActionResult.Continue;
				});
			});

			os.AppendLine("SaveVatReturnData output - end.");

			Log.Debug("\n{0}\n", os);

			var oSummary = new CalculateVatReturnSummary(m_oSp.CustomerMarketplaceID);
			oSummary.Execute();

			ElapsedTimeInfo.MergeData(oSummary.Stopper.ElapsedTimeInfo);
		} // Execute

		public ElapsedTimeInfo ElapsedTimeInfo { get { return m_oStopper.ElapsedTimeInfo; } } // ElapsedTimeInfo

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class SpSaveVatReturnData : AStoredProc {
			public SpSaveVatReturnData(AStrategy oStrategy, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				HistoryItems = new List<HistoryItem>();
				m_oOldDeletedItems = new SortedSet<int>();

				m_oStrategy = oStrategy;
			} // constructor

			public override bool HasValidParameters() {
				return
					(CustomerMarketplaceID > 0) &&
					(HistoryRecordID > 0) &&
					(SourceID > 0) &&
					!IsEmptyInput();
			} // HasValidParameters

			public bool IsEmptyInput() {
				return (VatReturnRecords.Count < 1) && (RtiTaxMonthRawData.Count < 1);
			} // IsEmptyInput

			public void AddHistoryItem(VatReturnRawData oOld, VatReturnRawData oNew) {
				// At this point oOld and oNew have overlapping date intervals and same registration # and oNew is not deleted.

				DeleteReasons nReason;
				VatReturnRawData oDeletedItem;
				VatReturnRawData oReasonItem;

				switch (oOld.SourceType) {
				case VatReturnSourceType.Linked:
					if (oNew.SourceType == VatReturnSourceType.Linked) {
						oDeletedItem = Delete(oOld);
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
						oDeletedItem = Delete(oOld);
						oReasonItem = oNew;
						nReason = DeleteReasons.UploadedRejectedByLinked;
						break;

					case VatReturnSourceType.Uploaded:
						oDeletedItem = oNew;
						oReasonItem = oOld;
						nReason = oOld.SameAs(oNew) ? DeleteReasons.UploadedEqual : DeleteReasons.UploadedNotEqual;
						break;

					case VatReturnSourceType.Manual:
						oDeletedItem = oNew;
						oReasonItem = oOld;
						nReason = DeleteReasons.ManualRejectedByUploaded;
						break;

					default:
						throw new StrategyAlert(m_oStrategy, "Non implemented VAT return source type: " + oNew.SourceType.ToString());
					} // switch

					break;

				case VatReturnSourceType.Manual:
					oDeletedItem = Delete(oOld);
					oReasonItem = oNew;

					switch (oNew.SourceType) {
					case VatReturnSourceType.Linked:
						nReason = DeleteReasons.ManualRejectedByLinked;
						break;

					case VatReturnSourceType.Uploaded:
						nReason = DeleteReasons.OverriddenByUploaded;
						break;

					case VatReturnSourceType.Manual:
						nReason = DeleteReasons.ManualUpdated;
						break;

					default:
						throw new StrategyAlert(m_oStrategy, "Non implemented VAT return source type: " + oNew.SourceType.ToString());
					} // switch

					break;

				default:
					throw new StrategyAlert(m_oStrategy, "Non implemented VAT return source type: " + oNew.SourceType.ToString());
				} // switch

				oDeletedItem.IsDeleted = true;

				HistoryItems.Add(new HistoryItem {
					DeleteRecordInternalID = oDeletedItem.InternalID,
					ReasonRecordInternalID = oReasonItem.InternalID,
					ReasonID = (int)nReason,
				});
			} // AddHistoryItem

			public int CustomerMarketplaceID { get; set; } // CustomerMarketplaceID

			public int HistoryRecordID { get; set; } // HistoryRecordID

			public int SourceID { get; set; } // SourceID

			public DateTime Now {
				get { return DateTime.UtcNow; } // get
				set { } // set, !!! DO NOT REMOVE, DO NOT MAKE PRIVATE !!!
			} // Now

			public List<VatReturnRawData> VatReturnRecords { get; set; } // VatReturnRecords

			public List<Entry> VatReturnEntries { get; set; } // VatReturnEntries

			public List<RtiTaxMonthRawData> RtiTaxMonthRawData { get; set; } // RtiTaxMonthRawData

			public List<HistoryItem> HistoryItems { get; set; } // HistoryItems

			public List<int> OldDeletedItems {
				get { return m_oOldDeletedItems.ToList(); }
				set { }
			} // HistoryItems

			public override string ToString() {
				var os = new StringBuilder();

				os.AppendFormat("MP ID: {0}, History ID: {1}, Source ID: {2}\n", CustomerMarketplaceID, HistoryRecordID, SourceID);

				var sd = new List<Tuple<string, IEnumerable>> {
					new Tuple<string, IEnumerable>("records", VatReturnRecords),
					new Tuple<string, IEnumerable>("entries", VatReturnEntries),
					new Tuple<string, IEnumerable>("RTI months", RtiTaxMonthRawData),
					new Tuple<string, IEnumerable>("history items", HistoryItems),
					new Tuple<string, IEnumerable>("old deleted items", OldDeletedItems),
				};

				foreach (var o in sd) {
					os.AppendFormat("Start of {0}:\n", o.Item1);

					foreach (var p in o.Item2)
						os.AppendLine(p.ToString());

					os.AppendFormat("End of {0}.\n", o.Item1);
				} // for each

				return os.ToString();
			} // ToSring

			public void InitEntries(VatReturnSourceType nSourceType) {
				var lst = new List<Entry>();

				foreach (VatReturnRawData r in VatReturnRecords) {
					r.SourceType = nSourceType;

					if (r.IsDeleted)
						AddHistoryItem(r);

					lst.AddRange(r.Data.Select(e => new Entry {
						Amount = e.Value.Amount,
						BoxName = e.Key,
						CurrencyCode = e.Value.CurrencyCode,
						RecordInternalID = r.InternalID,
					}));
				} // for each record

				VatReturnEntries = lst.ToList();
			} // InitEntries

			public class Entry {
				public Guid RecordInternalID { get; set; }

				public string BoxName { get; set; }

				public decimal Amount { get; set; }

				public string CurrencyCode { get; set; }

				public override string ToString() {
					return string.Format("{0}: {1} = {2} {3}", RecordInternalID, BoxName, Amount, CurrencyCode);
				} // ToString
			} // class Entry

			public class HistoryItem {
				public Guid DeleteRecordInternalID { get; set; }

				public Guid? ReasonRecordInternalID { get; set; }

				public int ReasonID { get; set; }

				public override string ToString() {
					return string.Format(
						"For reason {0} deleted record {1} because of {2}.",
						ReasonID,
						DeleteRecordInternalID,
						ReasonRecordInternalID.HasValue ? "record " + ReasonRecordInternalID.Value.ToString() : "no record"
					);
				} // ToString
			} // HistoryItem

			private VatReturnRawData Delete(VatReturnRawData oItem) {
				m_oOldDeletedItems.Add(oItem.RecordID);
				return oItem;
			} // Delete

			private void AddHistoryItem(VatReturnRawData oDeletedItem) {
				oDeletedItem.IsDeleted = true;

				HistoryItems.Add(new HistoryItem {
					DeleteRecordInternalID = oDeletedItem.InternalID,
					ReasonID = (int)DeleteReasons.ManualDeleted,
				});
			} // AddHistoryItem

			private readonly AStrategy m_oStrategy;
			private readonly SortedSet<int> m_oOldDeletedItems; 
		} // class SpSaveVatReturnData

		private readonly Stopper m_oStopper;
		private readonly SpSaveVatReturnData m_oSp;
		private readonly LoadVatReturnRawData m_oRaw;
	} // class SaveVatReturnData
} // namespace Ezbob.Backend.Strategies.VatReturn
