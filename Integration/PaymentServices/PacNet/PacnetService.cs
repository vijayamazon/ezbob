﻿using System;
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

		public PacnetReturnData SendMoney(int customerId, decimal amount, string bankNumber, string accountNumber, string accountName, string fileName = "ezbob", string currencyCode = "GBP", string description = "EZBOB")
		{
			Log.DebugFormat("PacnetService SendMoney customerId {0} amount {1} bankNumber {2} accountNumber {3} accountName {4} fileName {5} currencyCode {6} description {7}", customerId, amount, bankNumber, accountNumber, accountName, fileName, currencyCode, description);

			try
			{
				var request = new RavenRequest("submit");

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
					Log.DebugFormat("SendMoney result: " + wr);
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
		public PacnetReturnData CheckStatus(int customerId, string trackingNumber)
		{
			Log.DebugFormat("CheckStatus: trackingNumber={0}", trackingNumber);
			try
			{
				var request = new RavenRequest("status");
				request.Set("PaymentRoutingNumber", RoutingNumber);
				request.Set("TrackingNumber", trackingNumber);
				request.Set("PymtType", PaymentType);

				RavenResponse response = request.Send();
				using (var wr = new StringWriter())
				{
					Serializer.Serialize(wr, response);
					Log.DebugFormat("Result: " + wr);
				}
				Log.DebugFormat("CheckStatus completed successfully");
				var pacnetResponse = new PacnetReturnData(response);
				Log.DebugFormat("CheckStatus customerId {0} trackingNumber {1} Status {2}", customerId, trackingNumber, pacnetResponse.Status);
				return pacnetResponse;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return new PacnetReturnData { Error = ex.Message };
			}
		}

		//-----------------------------------------------------------------------------------
		public PacnetReturnData CloseFile(int customerId = 0, string fileName = "ezbob")
		{
			Log.DebugFormat("CloseFile: trackingNumber={0}", fileName);
			try
			{
				var request = new RavenRequest("closefile");
				request.Set("Filename", fileName);

				RavenResponse response = request.Send();
				using (var wr = new StringWriter())
				{
					Serializer.Serialize(wr, response);
					Log.DebugFormat("Result: " + wr);
				}
				Log.DebugFormat("CloseFile completed successfully");
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

				var request = new RavenRequest("payments");
				request.Set("ReportFormat", "RavenPaymentFile_v1.0");
				request.Set("StartTime", timestampProvider.FormatTimestamp(startTime));
				request.Set("EndTime", timestampProvider.FormatTimestamp(endTime));
				request.Set("ResultFields", "PRN PymtType Amount Currency CardNumber Description FileName Status");

				RavenResponse response = request.Send();

				OutputReport(response);

				using (var wr = new StringWriter())
				{
					Serializer.Serialize(wr, response);
					Log.DebugFormat("Result: " + wr);
				}
				Log.DebugFormat("GetReport completed successfully");
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
