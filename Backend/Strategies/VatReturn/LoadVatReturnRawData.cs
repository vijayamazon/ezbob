namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LoadVatReturnRawData : AStrategy {
		public LoadVatReturnRawData(int nCustomerMarketplaceID) {
			m_oSp = new SpLoadVatReturnRawData(nCustomerMarketplaceID, DB, Log);

			VatReturnRawData = new VatReturnRawData[0];
			RtiTaxMonthRawData = new RtiTaxMonthRawData[0];
		} // constructor

		public override string Name {
			get { return "Load VAT return raw data"; }
		} // Name

		public override void Execute() {
			var oVat = new SortedDictionary<int, VatReturnRawData>();
			var oRti = new List<RtiTaxMonthRawData>();

			m_oSp.ForEachRowSafe((sr, bRowsetStart) => {
				string sRowType = sr["RowType"];

				switch (sRowType) {
				case "record":
					var r = sr.Fill<VatReturnRawData>();
					oVat[r.RecordID] = r;
					break;

				case "entry":
					oVat[sr["RecordID"]].Data[sr["Name"]] = new Coin(sr["Amount"], sr["CurrencyCode"]);
					break;

				case "rti":
					oRti.Add(sr.Fill<RtiTaxMonthRawData>());
					break;
				} // switch

				return ActionResult.Continue;
			});

			if (oVat.Count > 0) {
				VatReturnRawData = new VatReturnRawData[oVat.Count];
				oVat.Values.CopyTo(VatReturnRawData, 0);
				Array.Sort(VatReturnRawData);
			} // if

			if (oRti.Count > 0) {
				RtiTaxMonthRawData = oRti.ToArray();
				Array.Sort(RtiTaxMonthRawData);
			} // if
		} // Execute

		public VatReturnRawData[] VatReturnRawData { get; private set; }

		public RtiTaxMonthRawData[] RtiTaxMonthRawData { get; private set; }

		private readonly SpLoadVatReturnRawData m_oSp;

		private class SpLoadVatReturnRawData : AStoredProc {
			public SpLoadVatReturnRawData(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerMarketplaceID = nCustomerMarketplaceID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }
		} // class SpLoadVatReturnRawData

	} // class LoadVatReturnRawData
} // namespace Ezbob.Backend.Strategies.VatReturn
