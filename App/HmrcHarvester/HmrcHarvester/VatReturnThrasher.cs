using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Ezbob.Logger;
using HtmlAgilityPack;

namespace Ezbob.HmrcHarvester {

	public class VatReturnThrasher : AThrasher {

		public VatReturnThrasher(bool bVerboseLogging = false, ASafeLog oLog = null) : base(bVerboseLogging, oLog) {
			Seeds = new VatReturnSeeds(this);
		} // constructor

		public override ISeeds Seeds { get; protected set; }

		public override ISeeds Run(SheafMetaData oFileID, byte[] oFile) {
			Info("Parsing {0}...", oFileID);

			var doc = new HtmlDocument();
			doc.LoadHtml(Encoding.UTF8.GetString(oFile));

			// Actual XPath expression is //*[@id="VAT0012"]/div[2]/form/div
			// However HtmlAgilityPack parses file wrong and DIVs that should be
			// children of the FORM become siblings of the FORM.

			HtmlNodeCollection oDivs = doc.DocumentNode.SelectNodes("//*[@id=\"VAT0012\"]/div[2]/div");

			if ((oDivs == null) || (oDivs.Count != 4)) {
				Warn("Data sections not found in {0}.", oFileID);
				return null;
			} // if

			var seeds = (VatReturnSeeds)Seeds;

			if (!VatPeriod(oDivs[0], seeds))
				return null;

			if (!BusinessDetails(oDivs[1], seeds))
				return null;

			if (!ReturnDetails(oDivs[2], seeds))
				return null;

			Info("Parsing {0} complete.", oFileID);

			return seeds;
		} // Run

		private bool VatPeriod(HtmlNode oNode, VatReturnSeeds seeds) {
			HtmlNode oDL = oNode.SelectSingleNode("dl");

			if (oDL == null) {
				Warn("VAT period dates not found.");
				return false;
			} // if

			var dlp = new DLParser(oDL, true, VerboseLogging, this);

			if (!dlp.Success) {
				Info("DL parser failed, not setting VAT period fields.");
				return false;
			} // if

			if (dlp.Data.ContainsKey("Period:"))
				seeds.Set(VatReturnSeeds.Field.Period, dlp.Data["Period:"], this);

			var kv = new Dictionary<string, VatReturnSeeds.Field>();
			kv["Date from:"] = VatReturnSeeds.Field.DateFrom;
			kv["Date to:"] = VatReturnSeeds.Field.DateTo;
			kv["Due date:"] = VatReturnSeeds.Field.DateDue;

			foreach (KeyValuePair<string, VatReturnSeeds.Field> pair in kv) {
				if (!dlp.Data.ContainsKey(pair.Key))
					continue;

				DateTime d;

				if (DateTime.TryParseExact(dlp.Data[pair.Key], "dd MMM yyyy", Culture, DateTimeStyles.None, out d))
					seeds.Set(pair.Value, d, this);
			} // for each

			return seeds.IsPeriodValid();
		} // VatPeriod

		private bool BusinessDetails(HtmlNode oNode, VatReturnSeeds seeds) {
			HtmlNode oDL = oNode.SelectSingleNode("dl");

			if (oDL == null) {
				Warn("Business details not found.");
				return false;
			} // if

			var dlp = new DLParser(oDL, false, VerboseLogging, this);

			if (!dlp.Success) {
				Info("DL parser failed, not setting business details fields.");
				return false;
			} // if

			if (dlp.Data.ContainsKey("VAT Registration Number:")) {
				string sNum = dlp.Data["VAT Registration Number:"].Replace(" ", "");

				long nNum;

				if (long.TryParse(sNum, out nNum))
					seeds.Set(VatReturnSeeds.Field.RegistrationNo, nNum, this);
			} // if

			if (dlp.Data.ContainsKey("Business name:"))
				seeds.Set(VatReturnSeeds.Field.BusinessName, dlp.Data["Business name:"], this);

			if (dlp.Data.ContainsKey("Business address:")) {
				seeds.Set(
					VatReturnSeeds.Field.BusinessAddress,
					dlp.Data["Business address:"].Split(new string[] { Environment.NewLine }, StringSplitOptions.None),
					this
				);
			} // if

			return seeds.AreBusinessDetailsValid();
		} // BusinessDetails

		private bool ReturnDetails(HtmlNode oNode, VatReturnSeeds seeds) {
			HtmlNode oDL = oNode.SelectSingleNode("dl");

			if (oDL == null) {
				Warn("Return details not found.");
				return false;
			} // if

			var dlp = new DLParser(oDL, true, VerboseLogging, this);

			if (!dlp.Success) {
				Info("DL parser failed, not setting return details fields.");
				return false;
			} // if

			foreach (KeyValuePair<string, string> pair in dlp.Data) {
				if (pair.Key.Length > 1) {
					decimal nAmount = AThrasher.ParseGBP(pair.Value);
					seeds.ReturnDetails[pair.Key.Substring(0, pair.Key.Length - 1)] = new Coin(nAmount, "GBP");
					Debug("VatReturnSeeds.ReturnDetails[{0}] = {1}", pair.Key, nAmount);
				} // if
			} // foreach

			return true;
		} // ReturnDetails

	} // class VatReturnThrasher

} // namespace Ezbob.HmrcHarvester
