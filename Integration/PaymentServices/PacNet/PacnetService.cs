namespace PaymentServices.PacNet {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Xml.Serialization;
	using Ezbob.Logger;
	using Raven.API;
	using Raven.API.Support;

	public class PacnetService : IPacnetService {
		public PacnetReturnData SendMoney(
			int userId,
			decimal amount,
			string bankNumber,
			string accountNumber,
			string accountName,
			string fileName = "ezbob",
			string currencyCode = "GBP",
			string description = "EZBOB"
		) {
			var logNotify = new Action<Exception, Severity, string>((ex, severity, note) => {
				if (ex == null) {
					log.Say(
						severity,
						"{8}Pacnet send money(userId {0}, amount {1}, bankNumber {2}, accountNumber {3}, accountName {4}," +
						"fileName {5}, currencyCode {6}, description {7})",
						userId,
						amount,
						bankNumber,
						accountNumber,
						accountName,
						fileName,
						currencyCode,
						description,
						note
					);
				} else {
					log.Alert(
						ex,
						"{8}Pacnet send money(userId {0}, amount {1}, bankNumber {2}, accountNumber {3}, accountName {4}," +
						"fileName {5}, currencyCode {6}, description {7})",
						userId,
						amount,
						bankNumber,
						accountNumber,
						accountName,
						fileName,
						currencyCode,
						description,
						note
					);
				} // if
			});

			logNotify(null, Severity.Msg, string.Empty);

			try {
				var request = new RavenRequest(RavenOperationType.SUBMIT.ToString());

				request.Set("PaymentRoutingNumber", RoutingNumber);
				request.Set("PaymentType", PaymentType);
				request.Set("Amount", ((int)(amount * 100)).ToString(CultureInfo.InvariantCulture));
				request.Set("CurrencyCode", currencyCode);
				request.Set("BankNumber", bankNumber);
				request.Set("AccountNumber", accountNumber);
				request.Set("AccountName", accountName);
				request.Set("Description", description);

				if (!string.IsNullOrEmpty(fileName))
					request.Set("Filename", fileName);

				var response = request.Send();

				using (var wr = new StringWriter()) {
					serializer.Serialize(wr, response);
					log.Debug("SendMoney result: {0}", wr);
				} // using

				var pacnetReturnData = new PacnetReturnData(response);

				if (pacnetReturnData.HasError) { // transaction failed;
					logNotify(null, Severity.Alert, string.Format("Failed with error '{0}' at ", pacnetReturnData.Error));
					throw new PacnetException(pacnetReturnData.Error);
				} // if

				if (pacnetReturnData.Status.Contains("Invalid")) {
					logNotify(null, Severity.Alert, string.Format("Invalid status '{0}' at ", pacnetReturnData.Status));
					throw new PacnetException(pacnetReturnData.Status);
				} // if

				logNotify(null, Severity.Msg, "Completed successfully: ");

				return pacnetReturnData;
			} catch (Exception ex) {
				logNotify(ex, Severity.Alert, "Error while: ");
				throw;
			} // try
		} // SendMoney

		public PacnetReturnData CheckStatus(int userId, string trackingNumber) {
			log.Msg("Pacnet check status(user id: {0}, tracking #: '{1}').", userId, trackingNumber);

			try {
				var request = new RavenRequest(RavenOperationType.STATUS.ToString());

				request.Set("PaymentRoutingNumber", RoutingNumber);
				request.Set("TrackingNumber", trackingNumber);
				request.Set("PymtType", PaymentType);

				RavenResponse response = request.Send();

				using (var wr = new StringWriter()) {
					serializer.Serialize(wr, response);
					log.Debug("Result: {0}", wr);
				} // using

				log.Msg(
					"Completed successfully: Pacnet check status(user id: {0}, tracking #: '{1}').",
					userId,
					trackingNumber
				);

				var pacnetResponse = new PacnetReturnData(response);

				log.Msg(
					"Status = '{2}' at Pacnet check status(user id: {0}, tracking #: '{1}').",
					userId,
					trackingNumber,
					pacnetResponse.Status
				);

				return pacnetResponse;
			} catch (Exception ex) {
				log.Alert(
					ex,
					"Error while: Pacnet check status(user id: {0}, tracking #: '{1}').",
					userId,
					trackingNumber
				);

				return new PacnetReturnData { Error = ex.Message };
			} // try
		} // CheckStatus

		public PacnetReturnData CloseFile(int userId, string fileName = "ezbob") {
			log.Msg("Pacnet close file(user id: {0}, fileName: '{1}'", userId, fileName);

			try {
				var request = new RavenRequest(RavenOperationType.CLOSEFILE.ToString());

				request.Set("Filename", fileName);

				RavenResponse response = request.Send();

				using (var wr = new StringWriter()) {
					serializer.Serialize(wr, response);
					log.Debug("Result: {0}", wr);
				} // using

				log.Msg("Completed successfully: Pacnet close file(user id: {0}, fileName: '{1}'", userId, fileName);

				return new PacnetReturnData(response);
			} catch (Exception ex) {
				log.Alert(ex, "Error while: Pacnet close file(user id: {0}, fileName: '{1}'", userId, fileName);
				return new PacnetReturnData { Error = ex.Message };
			} // try
		} // CloseFile

		public PacnetReturnData CloseTodayAndYesterdayFiles(int customerId) {
			var currentDate = DateTime.Now;

			string currentFile = "orangemoney.wf-RT" + currentDate.ToString("yyyy-MM-dd");

			string currentFilePrev = "orangemoney.wf-RT" + currentDate.AddDays(-1).ToString("yyyy-MM-dd");

			var result = CloseFile(customerId, currentFile);

			if (!result.HasError)
				result = CloseFile(customerId, currentFilePrev);

			return result;
		} // CloseTodayAndYesterdayFiles

		public PacnetReturnData GetReport(DateTime endTime, DateTime startTime) {
			try {
				var timestampProvider = new TimestampProvider();

				var request = new RavenRequest(RavenOperationType.PAYMENTS.ToString());

				request.Set("ReportFormat", "RavenPaymentFile_v1.0");
				request.Set("StartTime", timestampProvider.FormatTimestamp(startTime));
				request.Set("EndTime", timestampProvider.FormatTimestamp(endTime));
				request.Set("ResultFields", "PRN PymtType Amount Currency CardNumber Description FileName Status");

				RavenResponse response = request.Send();

				OutputReport(response);

				using (var wr = new StringWriter()) {
					serializer.Serialize(wr, response);
					log.Debug("Result: " + wr);
				} // using

				log.Msg("GetReport completed successfully");
				return new PacnetReturnData(response);
			} catch (Exception ex) {
				log.Alert(ex, "GetReport failed.");
				return new PacnetReturnData { Error = ex.Message };
			} // try
		} // GetReport

		protected void OutputReport(RavenResponse response) {
			bool reportToConsole = true;

			var reportGenerator = new ReportGenerator();

			if (reportToConsole)
				reportGenerator.PrintReport(response.Get("Report"));
			else
				reportGenerator.SaveReport(response.Get("Report"), "pacnetReport.txt");
		} // OutputReport

		private static readonly ASafeLog log = new SafeILog(typeof(PacnetService));
		private static readonly XmlSerializer serializer = new XmlSerializer(typeof(RavenResponse));

		private const string RoutingNumber = "857151";
		private const string PaymentType = "fp_credit";
	} // class PacnetService
} // namespace
