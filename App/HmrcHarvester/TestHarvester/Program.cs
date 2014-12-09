using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ezbob.HmrcHarvester;
using Ezbob.Logger;
namespace TestHarvester {
	using Integration.ChannelGrabberConfig;

	class Program {
		static void Main(string[] args) {
			var oLog = new LegacyLog(new ConsoleLog());

			FullTest(oLog);
		} // Main

		private static void FullTest(LegacyLog oLog) {
			var ad = new AccountData(Configuration.GetInstance(oLog.UnderlyingLog).GetVendorInfo("HMRC")) {
				Login = "829144784260",
				Password = "18june1974"
			};

			var harvester = new Harvester(ad, oLog.UnderlyingLog);

			if (harvester.Init()) {
				harvester.Run(true);
				harvester.Run(false);

				string sBaseDir = Path.Combine(Directory.GetCurrentDirectory(), "files");

				oLog.Info("{0} errors occured", harvester.Hopper.ErrorCount);

				if (harvester.Hopper.ErrorCount > 0) {
					oLog.Info("List of errors: begin");

					foreach (var pair in harvester.Hopper.Errors) {
						DataType nDataType = pair.Key;
						SortedDictionary<FileType, SortedDictionary<string, HarvesterError>> oDataTypeList = pair.Value;

						foreach (var ftPair in oDataTypeList) {
							FileType nFileType = ftPair.Key;

							foreach (KeyValuePair<string, HarvesterError> fileError in ftPair.Value) {
								string sFileName = fileError.Key;
								HarvesterError oError = fileError.Value;

								oLog.Warn("Data type: {0} File type: {1} File name: {2} Error code: {3} Error message: {4}",
									nDataType, nFileType, sFileName, oError.Code, oError.Message
								);
							} // for each file
						} // for each file type
					} // for each data type list

					oLog.Info("List of errors: end");
				} // if

				foreach (DataType nDataType in Enum.GetValues(typeof (DataType))) {
					foreach (FileType nFileType in Enum.GetValues(typeof (FileType))) {
						harvester.Hopper.ForEachFile(nDataType, nFileType, (dt, ft, sBaseFileName, oData) => {
							string sFilePath = Path.Combine(sBaseDir, string.Format("{0}-{1}.{2}",
								dt.ToString().ToLower(), sBaseFileName, ft.ToString().ToLower()
							));

							oLog.Info("Saving {0}...", sFilePath);

							var sw = new BinaryWriter(File.Open(sFilePath, FileMode.Create));

							sw.Write(oData, 0, oData.Length);

							sw.Close();

							oLog.Info("Saving {0} complete.", sFilePath);
						});
					} // foreach file type

					foreach (KeyValuePair<string, ISeeds> pair in harvester.Hopper.Seeds[nDataType]) {
						string sFileName = pair.Key;
						ISeeds oSeeds = pair.Value;

						switch (nDataType) {
						case DataType.VatReturn: {
								var oData = (VatReturnSeeds)oSeeds;

								oLog.Debug("Fetched file: {0} -- file content begin", sFileName);

								oLog.Debug("Registration #: {0}", oData.RegistrationNo);

								oLog.Debug("Period:\n\tName: {0}\n\tFrom: {1}\n\tTo: {1}\n\tDue: {2}",
									oData.Period, oData.DateFrom, oData.DateTo, oData.DateDue
								);

								oLog.Debug("Business name: {0}\nBusiness address:\n\t{1}",
									oData.BusinessName, string.Join("\n\t", oData.BusinessAddress)
								);

								var sb = new StringBuilder();

								foreach (KeyValuePair<string, Coin> rd in oData.ReturnDetails)
									sb.AppendFormat("\n\t{0}: {1} {2}", rd.Key, rd.Value.Amount, rd.Value.CurrencyCode);

								oLog.Debug("Return details:{0}", sb.ToString());

								oLog.Debug("Fetched file: {0} -- file content end", sFileName);
							} break;

						case DataType.PayeRtiTaxYears: {
								var oData = (RtiTaxYearSeeds)oSeeds;
								oLog.Debug("Fetched file: {0} -- file content begin", sFileName);

								foreach (RtiTaxMonthSeed rtms in oData.Months) {
									oLog.Debug("{0} - {1} paid {2} due {3}", rtms.DateStart, rtms.DateEnd, rtms.AmountPaid, rtms.AmountDue);
								} // for each month

								oLog.Debug("Fetched file: {0} -- file content end", sFileName);
							} break;
						} // switch
					} // for each seeds
				} // for each data type
			} // if init and run

			harvester.Done();
		} // FullTest

	} // class Program
} // namespace TestHarvester
