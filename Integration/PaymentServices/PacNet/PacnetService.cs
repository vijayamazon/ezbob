using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using Raven.API;
using log4net;
using Raven.API.Support;

namespace PaymentServices.PacNet
{
	public class PacnetService : IPacnetService
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PacnetService));
		private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(RavenResponse));

		private const string RoutingNumber = "857151";
		private const string PaymentType = "fp_credit";

		public PacnetReturnData SendMoney(int userId, decimal amount, string bankNumber, string accountNumber, string accountName, string fileName = "ezbob", string currencyCode = "GBP", string description = "EZBOB")
		{
			Log.InfoFormat("PacnetService SendMoney userId {0} amount {1} bankNumber {2} accountNumber {3} accountName {4} fileName {5} currencyCode {6} description {7}", userId, amount, bankNumber, accountNumber, accountName, fileName, currencyCode, description);

			try
			{
                var request = new RavenRequest(RavenOperationType.SUBMIT.ToString());

				request.Set("PaymentRoutingNumber", RoutingNumber);
				request.Set("PaymentType", PaymentType);
				request.Set("Amount", ((int)(amount * 100)).ToString(CultureInfo.InvariantCulture));
				request.Set("CurrencyCode", currencyCode);
				request.Set("BankNumber", bankNumber);
				request.Set("AccountNumber", accountNumber);
				request.Set("AccountName", accountName);
				request.Set("Description", description);
				if (!String.IsNullOrEmpty(fileName)) request.Set("Filename", fileName);

				var response = request.Send();
				using (var wr = new StringWriter())
				{
					Serializer.Serialize(wr, response);
					Log.InfoFormat("SendMoney result: " + wr);
				}

				var pacnetReturnData = new PacnetReturnData(response);

				if (pacnetReturnData.HasError)
				{
					//transaction failed;
					Log.Error("SendMoney failed");
					throw new PacnetException(pacnetReturnData.Error);
				}

				if (pacnetReturnData.Status.Contains("Invalid"))
				{
					Log.ErrorFormat("SendMoney failed: {0}", pacnetReturnData.Status);
					throw new PacnetException(pacnetReturnData.Status);
				}

				Log.InfoFormat("SendMoney completed successfully");

				return pacnetReturnData;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				throw;
			}
		}

		//-----------------------------------------------------------------------------------
		public PacnetReturnData CheckStatus(int userId, string trackingNumber)
		{
			Log.InfoFormat("CheckStatus: trackingNumber={0}", trackingNumber);
			try
			{
                var request = new RavenRequest(RavenOperationType.STATUS.ToString());
				request.Set("PaymentRoutingNumber", RoutingNumber);
				request.Set("TrackingNumber", trackingNumber);
				request.Set("PymtType", PaymentType);

				RavenResponse response = request.Send();
				using (var wr = new StringWriter())
				{
					Serializer.Serialize(wr, response);
					Log.DebugFormat("Result: " + wr);
				}
                Log.InfoFormat("CheckStatus completed successfully");
				var pacnetResponse = new PacnetReturnData(response);
                Log.InfoFormat("CheckStatus trackingNumber {0} Status {1}", trackingNumber, pacnetResponse.Status);
				return pacnetResponse;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return new PacnetReturnData { Error = ex.Message };
			}
		}

		//-----------------------------------------------------------------------------------
		public PacnetReturnData CloseFile(int userId, string fileName = "ezbob")
		{
            Log.InfoFormat("CloseFile: fileName={0}", fileName);
			try
			{
				var request = new RavenRequest(RavenOperationType.CLOSEFILE.ToString());
				request.Set("Filename", fileName);

				RavenResponse response = request.Send();
				using (var wr = new StringWriter())
				{
					Serializer.Serialize(wr, response);
					Log.InfoFormat("Result: " + wr);
				}
                Log.InfoFormat("CloseFile completed successfully");
				return new PacnetReturnData(response);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return new PacnetReturnData { Error = ex.Message };
			}
		}

		//-----------------------------------------------------------------------------------
		public PacnetReturnData CloseTodayAndYesterdayFiles(int customerId)
		{
			var currentDate = DateTime.Now;
			string currentFile = "orangemoney.wf-RT" + currentDate.ToString("yyyy-MM-dd");
			string currentFilePrev = "orangemoney.wf-RT" + currentDate.AddDays(-1).ToString("yyyy-MM-dd");
			var result = CloseFile(customerId, currentFile);
			if (!result.HasError)
			{
				result = CloseFile(customerId, currentFilePrev);
			}
			return result;
		}

		//-----------------------------------------------------------------------------------

		public PacnetReturnData GetReport(DateTime endTime, DateTime startTime)
		{
			try
			{

				var timestampProvider = new TimestampProvider();

                var request = new RavenRequest(RavenOperationType.PAYMENTS.ToString());
				request.Set("ReportFormat", "RavenPaymentFile_v1.0");
				request.Set("StartTime", timestampProvider.FormatTimestamp(startTime));
				request.Set("EndTime", timestampProvider.FormatTimestamp(endTime));
				request.Set("ResultFields", "PRN PymtType Amount Currency CardNumber Description FileName Status");

				RavenResponse response = request.Send();

				OutputReport(response);

				using (var wr = new StringWriter())
				{
					Serializer.Serialize(wr, response);
                    Log.InfoFormat("Result: " + wr);
				}
				Log.InfoFormat("GetReport completed successfully");
				return new PacnetReturnData(response);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return new PacnetReturnData { Error = ex.Message };
			}
		}

		protected void OutputReport(RavenResponse response)
		{
			bool reportToConsole = true;
			var reportGenerator = new ReportGenerator();
			if (reportToConsole)
			{
				reportGenerator.PrintReport(response.Get("Report"));
			}
			else
			{
				reportGenerator.SaveReport(response.Get("Report"), "pacnetReport.txt");
			}
		}
	}
}
