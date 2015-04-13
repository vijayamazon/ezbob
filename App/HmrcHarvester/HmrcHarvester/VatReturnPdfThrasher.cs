using System;
using System.Collections.Generic;
using System.Globalization;
using Ezbob.Logger;
using iTextSharp.text.pdf;

namespace Ezbob.HmrcHarvester {
	public class VatReturnPdfThrasher : AThrasher {
		public VatReturnPdfThrasher(bool bVerboseLogging = false, ASafeLog oLog = null) : base(bVerboseLogging, oLog) {
		} // constructor

		public override ISeeds Run(SheafMetaData oFileID, byte[] oFile) {
			Info("Parsing {0}...", oFileID);

			m_oReader = new PdfReader(oFile);

			Seeds = new VatReturnSeeds(this);

			if (!VatPeriod())
				return null;

			if (!BusinessDetails())
				return null;

			if (!ReturnDetails())
				return null;

			Info("Parsing {0} complete.", oFileID);

			return Seeds;
		} // Run

		public override ISeeds Seeds { get; protected set; }

		private bool VatPeriod() {
			var oSeeds = (VatReturnSeeds)Seeds;

			oSeeds.Set(VatReturnSeeds.Field.Period, Read("returnDetailsDto.vatPeriod.formattedAvailablePeriod"), this);

			var kv = new Dictionary<string, VatReturnSeeds.Field>();
			kv["returnDetailsDto.vatPeriod.formattedDateFrom"] = VatReturnSeeds.Field.DateFrom;
			kv["returnDetailsDto.vatPeriod.formattedDateTo"] = VatReturnSeeds.Field.DateTo;
			kv["returnDetailsDto.vatPeriod.formattedDueDate"] = VatReturnSeeds.Field.DateDue;

			foreach (KeyValuePair<string, VatReturnSeeds.Field> pair in kv) {
				DateTime d;

				string sDate = Read(pair.Key);

				if (DateTime.TryParseExact(sDate, "dd MMM yyyy", Culture, DateTimeStyles.None, out d))
					oSeeds.Set(pair.Value, d, this);
			} // for each

			return oSeeds.IsPeriodValid();
		} // VatPeriod

		private bool BusinessDetails() {
			var oSeeds = (VatReturnSeeds)Seeds;

			string sNum = Read("returnDetailsDto.traderDetails.vrn").Replace(" ", "");

			long nNum;

			if (long.TryParse(sNum, out nNum))
				oSeeds.Set(VatReturnSeeds.Field.RegistrationNo, nNum, this);

			oSeeds.Set(VatReturnSeeds.Field.BusinessName, Read("returnDetailsDto.traderDetails.tradersName"), this);

			var aryAddress = new List<string>();

			for (int i = 1; i <= 6; i++)
				aryAddress.Add(Read("returnDetailsDto.traderDetails." + (i == 6 ? "postcode" : "address" + i)));

			oSeeds.Set(VatReturnSeeds.Field.BusinessAddress, aryAddress.ToArray(), this);

			return oSeeds.AreBusinessDetailsValid();
		} // BusinessDetails

		private bool ReturnDetails() {
			var oSeeds = (VatReturnSeeds)Seeds;

			for (int i = 1; i <= 9; i++) {
				string sValueFieldName = string.Format("returnDetailsDto.vatReturnForm.box{0}.display{1}", i, i <= 5 ? "Amount" : "Pounds");

				decimal nAmount = AThrasher.ParseGBP(Read(sValueFieldName));

				string sFieldName = string.Format("{0} (Box {1})", Read("vatBox" + i + "Pdf"), i);

				oSeeds.ReturnDetails[sFieldName] = new Coin(nAmount, "GBP");

				Debug("VatReturnSeeds.ReturnDetails[{0}] = {1}", sFieldName, nAmount);
			} // foreach

			return true;
		} // ReturnDetails

		private string Read(string sFieldName) {
			return m_oReader.AcroFields.GetField(sFieldName);
		} // Read

		private PdfReader m_oReader;
	} // class VatReturnPdfThrasher
} // namespace Ezbob.HmrcHarvester
