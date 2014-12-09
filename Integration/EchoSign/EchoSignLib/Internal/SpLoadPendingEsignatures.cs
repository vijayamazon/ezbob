namespace EchoSignLib {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpLoadPendingEsignatures : AStoredProc {

		public SpLoadPendingEsignatures(int? nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Signatures = new SortedDictionary<int, Esignature>();

			CustomerID = nCustomerID;
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID == null) || (CustomerID > 0);
		} // HasValidParameters

		public int? CustomerID { get; set; }

		public void Load() {
			ForEachRowSafe((sr, bRowsetStart) => {
				int nEsignatureID = sr["EsignatureID"];

				if (!Signatures.ContainsKey(nEsignatureID)) {
					var oSignature = new Esignature(nEsignatureID);
					sr.Fill(oSignature);
					Signatures[oSignature.ID] = oSignature;
				} // if

				Esigner oSigner = sr.Fill<Esigner>();
				Signatures[nEsignatureID].Signers[oSigner.ID] = oSigner;

				return ActionResult.Continue;
			});
		} // Load

		public SortedDictionary<int, Esignature> Signatures { get; private set; }
	} // class SpLoadPendingEsignatures
} // namespace
