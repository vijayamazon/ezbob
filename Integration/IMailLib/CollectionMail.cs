namespace IMailLib {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using IMailLib.Helpers;
	using log4net;

	public class CollectionMail {
		public CollectionMail(string userName, string password, bool isDebugMode = false, string debugModeEmail = null, string savePath = null) {
			api = new IMailApi();
			this.userName = userName;
			this.password = password;
			this.isDebugMode = isDebugMode;
			this.debugModeEmail = debugModeEmail;
			this.savePath = savePath;
		}

		public void SendDefaultNoticeComm7Borrower(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{
					"CustomerName", model.CustomerName
				}, {
					"CompanyName", model.CompanyName
				}, {
					"GuarantorName", model.GuarantorName
				}, {
					"Date", model.Date.ToLongDateWithDayOfWeek()
				}, {
					"LoanRef", model.LoanRef
				}, {
					"LoanDate", model.LoanDate.ToLongDate()
				}, {
					"LoanAmount", model.LoanAmount.ToNumericNoDecimals()
				}, {
					"SchedDate", model.MissedPayment.DateDue.ToLongDate()
				}, {
					"AmountDue", model.MissedPayment.AmountDue.ToNumeric2Decimals()
				}, {
					"AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals()
				}, {
					"AmountTotal", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals()
				}, {
					"OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals()
				},
			};

			SetAddress(model.CustomerAddress, ref variables);
			Stream template = PrepareMail.ExtractResourceAsStream(DefaultnoticeComm7BorrowerTemplateName);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			SendMail(pdfData, model.CustomerId, DefaultnoticeComm7BorrowerTemplateName);
		}

		public void SendDefaultTemplateComm7(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{
					"CustomerName", model.CustomerName
				}, {
					"CompanyName", model.CompanyName
				}, {
					"Date", model.Date.ToLongDateWithDayOfWeek()
				}, {
					"LoanRef", model.LoanRef
				}, {
					"LoanDate", model.LoanDate.ToLongDate()
				}, {
					"LoanAmount", model.LoanAmount.ToNumericNoDecimals()
				}, {
					"SchedDate", model.MissedPayment.DateDue.ToLongDate()
				}, {
					"AmountDue", model.MissedPayment.AmountDue.ToNumeric2Decimals()
				}, {
					"AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals()
				}, {
					"AmountTotal", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals()
				}, {
					"OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals()
				},
			};

			SendDefaultTemplateComm7Personal(model.CustomerId, variables, model.CustomerAddress);
			SendDefaultTemplateComm7Business(model.CustomerId, variables, model.CompanyAddress);
		}

		public void SendDefaultTemplateConsumer14(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{
					"CustomerName", model.CustomerName
				}, {
					"Date", model.Date.ToLongDateWithDayOfWeek()
				}, {
					"LoanRef", model.LoanRef
				}, {
					"LoanDate", model.LoanDate.ToLongDate()
				}, {
					"LoanAmount", model.LoanAmount.ToNumericNoDecimals()
				}, {
					"SchedDate", model.MissedPayment.DateDue.ToLongDate()
				}, {
					"AmoDueNoFees", (model.MissedPayment.AmountDue - model.MissedPayment.Fees).ToNumeric2Decimals()
				}, {
					"AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals()
				}, {
					"TotalNoFees", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount - model.MissedPayment.Fees).ToNumeric2Decimals()
				}, {
					"OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals()
				}, {
					"FeeAmount", model.MissedPayment.Fees.ToNumericNoDecimals()
				}, {
					"Date10", model.Date.AddDays(10)
						.ToLongDate()
				}
			};
			SetAddress(model.CustomerAddress, ref variables);

			Stream template = PrepareMail.ExtractResourceAsStream(DefaulttemplateConsumer14TemplateName);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			byte[] pdfAttachment = PrepareMail.ExtractResource(DefaulttemplateConsumer14Attachment);
			byte[] concatinatedMail = PrepareMail.ConcatinatePdfFiles(new List<byte[]> {
				pdfData,
				pdfAttachment
			});

			SendMail(concatinatedMail, model.CustomerId, DefaulttemplateConsumer14TemplateName);
		}

		public void SendDefaultTemplateConsumer31(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{
					"CustomerName", model.CustomerName
				}, {
					"Date", model.Date.ToLongDateWithDayOfWeek()
				}, {
					"LoanRef", model.LoanRef
				}, {
					"LoanDate", model.LoanDate.ToLongDate()
				}, {
					"TotalBalance", (model.MissedInterest + model.OutstandingPrincipal).ToNumeric2Decimals()
				}, {
					"AmountDue1", (model.PreviousMissedPayment.AmountDue).ToNumeric2Decimals()
				}, {
					"DateDue1", (model.PreviousMissedPayment.DateDue).ToLongDate()
				}, {
					"PartialPaid1", (model.PreviousMissedPayment.RepaidAmount).ToNumeric2Decimals()
				}, {
					"RepaidDate1", (model.PreviousMissedPayment.RepaidDate).ToLongDate()
				}, {
					"Total1", (model.PreviousMissedPayment.AmountDue - model.PreviousMissedPayment.RepaidAmount).ToNumeric2Decimals()
				}, {
					"AmountDue2", (model.MissedPayment.AmountDue).ToNumeric2Decimals()
				}, {
					"DateDue2", (model.MissedPayment.DateDue).ToLongDate()
				}, {
					"PartialPaid2", (model.MissedPayment.RepaidAmount).ToNumeric2Decimals()
				}, {
					"RepaidDate2", (model.MissedPayment.RepaidDate).ToLongDate()
				}, {
					"Total2", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals()
				}, {
					"Total", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount + model.PreviousMissedPayment.AmountDue - model.PreviousMissedPayment.RepaidAmount).ToNumeric2Decimals()
				},
			};

			SetAddress(model.CustomerAddress, ref variables);

			Stream template = PrepareMail.ExtractResourceAsStream(DefaulttemplateConsumer31TemplateName);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			byte[] pdfAttachment = PrepareMail.ExtractResource(DefaulttemplateConsumer31Attachment);
			byte[] concatinatedMail = PrepareMail.ConcatinatePdfFiles(new List<byte[]> {
				pdfData,
				pdfAttachment
			});

			SendMail(concatinatedMail, model.CustomerId, DefaulttemplateConsumer31TemplateName);
		}

		public void SendDefaultWarningComm7Guarantor(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{
					"CustomerName", model.CustomerName
				}, {
					"CompanyName", model.CompanyName
				}, {
					"GuarantorName", model.GuarantorName
				}, {
					"Date", model.Date.ToLongDateWithDayOfWeek()
				}, {
					"LoanRef", model.LoanRef
				}, {
					"LoanDate", model.LoanDate.ToLongDate()
				}, {
					"LoanAmount", model.LoanAmount.ToNumericNoDecimals()
				}, {
					"SchedDate", model.MissedPayment.DateDue.ToLongDate()
				}, {
					"AmountDue", model.MissedPayment.AmountDue.ToNumeric2Decimals()
				}, {
					"AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals()
				}, {
					"AmountTotal", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals()
				}, {
					"OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals()
				},
			};

			SetAddress(model.GuarantorAddress, ref variables);
			Stream template = PrepareMail.ExtractResourceAsStream(DefaultwarningComm7GuarantorTemplateName);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			SendMail(pdfData, model.CustomerId, DefaultwarningComm7GuarantorTemplateName);
		}

		private void SendDefaultTemplateComm7Business(int customerID, Dictionary<string, string> variables, Address companyAddress) {
			SetAddress(companyAddress, ref variables);
			Stream template = PrepareMail.ExtractResourceAsStream(DefaulttemplateComm7BusinessTemplateName);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			SendMail(pdfData, customerID, DefaulttemplateComm7BusinessTemplateName);
		}

		private void SendDefaultTemplateComm7Personal(int customerID, Dictionary<string, string> variables, Address customerAddress) {
			SetAddress(customerAddress, ref variables);
			Stream template = PrepareMail.ExtractResourceAsStream(DefaulttemplateComm7PersonalTemplateName);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			SendMail(pdfData, customerID, DefaulttemplateComm7PersonalTemplateName);
		}

		private void SendMail(byte[] pdfData, int customerID, string templateName) {
			Log.InfoFormat("Sending mail to customer {0} template {1}", customerID, templateName);
			bool success = false;
			success = api.Authenticate(userName, password);
			if (!success)
				throw new Exception(api.GetErrorMessage());
			if (isDebugMode) {
				if (!string.IsNullOrEmpty(debugModeEmail)) {
					success = api.SetEmailPreview(debugModeEmail);
					if (!success)
						throw new Exception(api.GetErrorMessage());
				} else
					throw new Exception("Debug mode and email is not provided");
			}

			success = api.ProcessPrintReadyPDF(pdfData, null, false);
			if (!success)
				throw new Exception(api.GetErrorMessage());

			if (!string.IsNullOrEmpty(savePath)) {
				try {
					PrepareMail.SaveFile(pdfData, savePath, customerID, templateName);
				} catch(Exception ex) {
					Log.WarnFormat("Failed to save mail copy for {0} for customer {1}\n{2}", templateName, customerID, ex);
				}
			}
		}

		private void SetAddress(Address address, ref Dictionary<string, string> variables) {
			variables["Address1"] = address.Line1;
			variables["Address2"] = address.Line2;
			variables["Address3"] = address.Line3;
			variables["Address4"] = address.Line4;
			variables["Address5"] = address.Line5;
			variables["Postcode"] = address.Postcode;
		}

		private const string DefaultnoticeComm7BorrowerTemplateName = "IMailLib.CollectionTemplates.default-notice-to-borrowers.docx";
		private const string DefaulttemplateComm7BusinessTemplateName = "IMailLib.CollectionTemplates.notice-of-default-to-business.docx";
		private const string DefaulttemplateComm7PersonalTemplateName = "IMailLib.CollectionTemplates.notice-to-guarantor.docx";
		private const string DefaulttemplateConsumer14Attachment = "IMailLib.CollectionTemplates.information-sheet-default.pdf";
		private const string DefaulttemplateConsumer14TemplateName = "IMailLib.CollectionTemplates.default-notice.docx";
		private const string DefaulttemplateConsumer31Attachment = "IMailLib.CollectionTemplates.information-sheet-arrears.pdf";
		private const string DefaulttemplateConsumer31TemplateName = "IMailLib.CollectionTemplates.sums-of-arrears.docx";
		private const string DefaultwarningComm7GuarantorTemplateName = "IMailLib.CollectionTemplates.warning-letter-to-guarantors.docx";
		private readonly IMailApi api;
		private readonly string debugModeEmail;
		private readonly bool isDebugMode;
		private readonly string password;
		private readonly string userName;
		private readonly string savePath;
		private readonly ILog Log = LogManager.GetLogger(typeof (CollectionMail));
	}
}
