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
			this.stopper = new Stopper();

			this.saveVatReturnData = new SpSaveVatReturnData(this, DB, Log) {
				CustomerMarketplaceID = nCustomerMarketplaceID,
				HistoryRecordID = nHistoryRecordID,
				SourceID = nSourceID,
				VatReturnRecords = new List<VatReturnRawData>(oVatReturn),
				RtiTaxMonthRawData = new List<RtiTaxMonthRawData>(oRtiMonths),
			};
			this.saveVatReturnData.InitEntries((VatReturnSourceType)nSourceID);

			this.loadVatReturnRawData = new LoadVatReturnRawData(nCustomerMarketplaceID);
		} // constructor

		public override string Name {
			get { return "Save VAT return data"; }
		} // Name

		public override void Execute() {
			if (this.saveVatReturnData.IsEmptyInput())
				return;

			this.stopper.Execute(ElapsedDataMemberType.RetrieveDataFromDatabase, () => {
				this.loadVatReturnRawData.Execute();

				foreach (var oOld in this.loadVatReturnRawData.VatReturnRawData)
					foreach (var oNew in this.saveVatReturnData.VatReturnRecords)
						if (oOld.Overlaps(oNew))
							this.saveVatReturnData.AddHistoryItem(oOld, oNew);
			});

			Log.Debug(this.saveVatReturnData);

			var os = new StringBuilder();

			os.AppendLine("SaveVatReturnData output - begin:");

			int nRowNum = 0;

			this.stopper.Execute(ElapsedDataMemberType.StoreDataToDatabase, () => {
				this.saveVatReturnData.ForEachRow((oReader, bRowsetStart) => {
					var vals = new object[oReader.FieldCount];

					int nRead = oReader.GetValues(vals);

					os.AppendFormat(
						"\nRow {0}{3}: {1} fields, {2} read.\n",
						nRowNum,
						oReader.FieldCount,
						nRead,
						bRowsetStart ? " NEW ROWSET" : string.Empty
					);

					for (int i = 0; i < nRead; i++)
						os.AppendFormat("\t{2} - {0}: {1}\n", oReader.GetName(i), vals[i], i);

					nRowNum++;
					return ActionResult.Continue;
				});
			});

			os.AppendLine("SaveVatReturnData output - end.");

			Log.Debug("\n{0}\n", os);

			var oSummary = new CalculateVatReturnSummary(this.saveVatReturnData.CustomerMarketplaceID)
				.SetHistoryRecordID(this.saveVatReturnData.HistoryRecordID);
			oSummary.Execute();

			DB.ExecuteNonQuery(
				"UpdateMpTotalsHmrc",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@HistoryID", this.saveVatReturnData.HistoryRecordID)
			);

			ElapsedTimeInfo.MergeData(oSummary.Stopper.ElapsedTimeInfo);
		} // Execute

		public ElapsedTimeInfo ElapsedTimeInfo { get { return this.stopper.ElapsedTimeInfo; } } // ElapsedTimeInfo

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class SpSaveVatReturnData : AStoredProc {
			public SpSaveVatReturnData(AStrategy oStrategy, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				HistoryItems = new List<HistoryItem>();
				this.m_oOldDeletedItems = new SortedSet<int>();

				this.m_oStrategy = oStrategy;
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
				// At this point oOld and oNew have overlapping date intervals
				// and same registration # and oNew is not deleted.

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
						nReason = oNew.SourceType == VatReturnSourceType.Manual
							? DeleteReasons.ManualRejectedByLinked
							: DeleteReasons.UploadedRejectedByLinked;
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
						throw new StrategyAlert(this.m_oStrategy, "Non implemented VAT return source type: " + oNew.SourceType);
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
						throw new StrategyAlert(this.m_oStrategy, "Non implemented VAT return source type: " + oNew.SourceType);
					} // switch

					break;

				default:
					throw new StrategyAlert(this.m_oStrategy, "Non implemented VAT return source type: " + oNew.SourceType);
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
				get { return this.m_oOldDeletedItems.ToList(); }
				set { }
			} // HistoryItems

			public override string ToString() {
				var os = new StringBuilder();

				os.AppendFormat(
					"MP ID: {0}, History ID: {1}, Source ID: {2}\n",
					CustomerMarketplaceID,
					HistoryRecordID,
					SourceID
				);

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
				this.m_oOldDeletedItems.Add(oItem.RecordID);
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

		private readonly Stopper stopper;
		private readonly SpSaveVatReturnData saveVatReturnData;
		private readonly LoadVatReturnRawData loadVatReturnRawData;
	} // class SaveVatReturnData
} // namespace Ezbob.Backend.Strategies.VatReturn
