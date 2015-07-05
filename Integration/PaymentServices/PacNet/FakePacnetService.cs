namespace PaymentServices.PacNet {
	using System;
	using Ezbob.Logger;

	public class FakePacnetService : IPacnetService {
		public virtual PacnetReturnData SendMoney(
			int userId,
			decimal amount,
			string bankNumber,
			string accountNumber,
			string accountName,
			string fileName = null,
			string currencyCode = "GBP",
			string description = null
		) {
			log.Msg(
				"Fake SendMoney userId {0} amount {1} bankNumber {2} accountNumber {3} accountName {4} fileName {5}" +
				"currencyCode {6} description {7}",
				userId,
				amount,
				bankNumber,
				accountNumber,
				accountName,
				fileName,
				currencyCode,
				description
			);

			return new PacnetReturnData {
				Status = "Submitted",
				TrackingNumber = new Random().Next(111111111, 999999999).ToString()
			};
		} // SendMoney

		public virtual PacnetReturnData CheckStatus(int userId, string trackingNumber) {
			int r = new Random(DateTime.Now.Millisecond).Next(0, 2);

			var pacnetReturnData = new PacnetReturnData() {
				Status = statuses[r],
				TrackingNumber = trackingNumber,
				Error = "Fake " + statuses[r]
			};

			log.Msg("Fake CheckStatus trackingNumber {0} Status {1}", trackingNumber, pacnetReturnData.Status);

			return pacnetReturnData;
		} // CheckStatus

		public virtual PacnetReturnData CloseFile(int userId, string fileName) {
			return new PacnetReturnData();
		} // CloseFile

		private static readonly string[] statuses = { "inprogress", "submitted", "error" };
		private static readonly ASafeLog log = new SafeILog(typeof(FakePacnetService));
	} // class FakePacnetService

	public class FailingPacnetService : FakePacnetService {
		public override PacnetReturnData SendMoney(
			int customerId,
			decimal amount,
			string bankNumber,
			string accountNumber,
			string accountName,
			string fileName = null,
			string currencyCode = "GBP",
			string description = null
		) {
			throw new PacnetException("SendMoney Failed");
		} // SendMoney
	} // class FailingPacnetService
} // namespace
