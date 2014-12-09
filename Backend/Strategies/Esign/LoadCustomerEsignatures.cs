namespace Ezbob.Backend.Strategies.Esign {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	internal class LoadCustomerEsignatures : AStoredProcedure {
		public LoadCustomerEsignatures(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

		public override bool HasValidParameters() {
			return !CustomerID.HasValue || (CustomerID > 0);
		} // HasValidParameters

		public SortedTable<int, long, Esignature> Load() {
			var oResult = new SortedTable<int, long, Esignature>();

			ForEachResult<ResultRow>(oRow => {
				if (!oResult.Contains(oRow.CustomerID)) {
					oResult[oRow.CustomerID, oRow.EsignatureID] = oRow.CreateEsignature();
					return ActionResult.Continue;
				} // if

				if (!oResult.Contains(oRow.CustomerID, oRow.EsignatureID)) {
					oResult[oRow.CustomerID, oRow.EsignatureID] = oRow.CreateEsignature();
					return ActionResult.Continue;
				} // if

				oResult[oRow.CustomerID, oRow.EsignatureID].Signers.Add(oRow.CreateEsigner());
				return ActionResult.Continue;
			});

			return oResult;
		} // Load

		public int? CustomerID { get; set; }

		public class ResultRow : AResultRow {
			public int CustomerID { get; set; }
			public long EsignatureID { get; set; }
			public DateTime SendDate { get; set; }
			public int EsignTemplateID { get; set; }
			public string DocumentName { get; set; }
			public int SignatureStatusID { get; set; }
			public string SignatureStatus { get; set; }
			public bool HasDocument { get; set; }
			public long EsignerID { get; set; }
			public int DirectorID { get; set; }
			public int ExperianDirectorID { get; set; }
			public string SignerEmail { get; set; }
			public string SignerFirstName { get; set; }
			public string SignerLastName { get; set; }
			public int SignerStatusID { get; set; }
			public string SignerStatus { get; set; }
			public DateTime? SignDate { get; set; }

			public Esignature CreateEsignature() {
				return new Esignature {
					CustomerID = CustomerID,
					ID = EsignatureID,
					HasDocument = HasDocument,
					SendDate = SendDate,
					TemplateID = EsignTemplateID,
					TemplateName = DocumentName,
					Status = SignatureStatus,
					StatusID = SignatureStatusID,
					Signers = new List<Esigner> { CreateEsigner(), },
				};
			} // CreateSignature

			public Esigner CreateEsigner() {
				return new Esigner {
					DirectorID = DirectorID,
					ID = EsignerID,
					SignDate = SignDate,
					Email = SignerEmail,
					FirstName = SignerFirstName,
					LastName = SignerLastName,
					Status = SignerStatus,
					StatusID = SignerStatusID,
				};
			} // CreateSigner
		} // ResultRow
	} // class LoadCustomerEsignatures
} // namespace
