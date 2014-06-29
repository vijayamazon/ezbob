using System;
using System.Collections.Generic;
using System.Globalization;
using Ezbob.Logger;
using iTextSharp.text.pdf;

namespace Ezbob.HmrcHarvester {
	#region class VatReturnPdfThrasher

	public class VatReturnPdfThrasher : AThrasher {
		#region public

		#region constructor

		public VatReturnPdfThrasher(bool bVerboseLogging = false, ASafeLog oLog = null) : base(bVerboseLogging, oLog) {
		} // constructor

		#endregion constructor

		#region method Run

		public override ISeeds Run(SheafMetaData oFileID, byte[] oFile) {
			Info("Parsing {0}...", oFileID);

			m_oReader = new PdfReader(oFile);

			m_oSeeds = new VatReturnSeeds(this);

			if (!VatPeriod())
				return null;

			if (!BusinessDetails())
				return null;

			if (!ReturnDetails())
				return null;

			Info("Parsing {0} complete.", oFileID);

			return m_oSeeds;
		} // Run

		#endregion method Run

		#endregion public

		#region private

		#region method VatPeriod

		private bool VatPeriod() {
			m_oSeeds.Set(VatReturnSeeds.Field.Period, Read("returnDetailsDto.vatPeriod.formattedAvailablePeriod"), this);

			var kv = new Dictionary<string, VatReturnSeeds.Field>();
			kv["returnDetailsDto.vatPeriod.formattedDateFrom"] = VatReturnSeeds.Field.DateFrom;
			kv["returnDetailsDto.vatPeriod.formattedDateTo"] = VatReturnSeeds.Field.DateTo;
			kv["returnDetailsDto.vatPeriod.formattedDueDate"] = VatReturnSeeds.Field.DateDue;

			foreach (KeyValuePair<string, VatReturnSeeds.Field> pair in kv) {
				DateTime d;

				string sDate = Read(pair.Key);

				if (DateTime.TryParseExact(sDate, "dd MMM yyyy", Culture, DateTimeStyles.None, out d))
					m_oSeeds.Set(pair.Value, d, this);
			} // for each

			return m_oSeeds.IsPeriodValid();
		} // VatPeriod

		#endregion method VatPeriod

		#region method BusinessDetails

		private bool BusinessDetails() {
			string sNum = Read("returnDetailsDto.traderDetails.vrn").Replace(" ", "");

			long nNum = 0;

			if (long.TryParse(sNum, out nNum))
				m_oSeeds.Set(VatReturnSeeds.Field.RegistrationNo, nNum, this);

			m_oSeeds.Set(VatReturnSeeds.Field.BusinessName, Read("returnDetailsDto.traderDetails.tradersName"), this);

			var aryAddress = new List<string>();

			for (int i = 1; i <= 6; i++)
				aryAddress.Add(Read("returnDetailsDto.traderDetails." + (i == 6 ? "postcode" : "address" + i)));

			m_oSeeds.Set(VatReturnSeeds.Field.BusinessAddress, aryAddress.ToArray(), this);

			return m_oSeeds.AreBusinessDetailsValid();
		} // BusinessDetails

		#endregion method BusinessDetails

		#region method ReturnDetails

		private bool ReturnDetails() {
			for (int i = 1; i <= 9; i++) {
				string sValueFieldName = string.Format("returnDetailsDto.vatReturnForm.box{0}.display{1}", i, i <= 5 ? "Amount" : "Pounds");

				decimal nAmount = AThrasher.ParseGBP(Read(sValueFieldName));

				string sFieldName = string.Format("{0} (Box {1})", Read("vatBox" + i + "Pdf"), i);

				m_oSeeds.ReturnDetails[sFieldName] = new Coin(nAmount, "GBP");

				Debug("VatReturnSeeds.ReturnDetails[{0}] = {1}", sFieldName, nAmount);
			} // foreach

			return true;
		} // ReturnDetails

		#endregion method ReturnDetails

		#region method Read

		private string Read(string sFieldName) {
			return m_oReader.AcroFields.GetField(sFieldName);
		} // Read

		#endregion method Read

		#region fields

		private PdfReader m_oReader;
		private VatReturnSeeds m_oSeeds;

		#endregion fields

		#endregion private
	} // class VatReturnPdfThrasher

	#endregion class VatReturnPdfThrasher
} // namespace Ezbob.HmrcHarvester
