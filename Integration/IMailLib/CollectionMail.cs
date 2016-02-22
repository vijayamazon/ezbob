namespace IMailLib {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using IMailLib.Helpers;
	using log4net;

	public class CollectionMail {
		public CollectionMail(string userName, string password, bool isDebugMode = false, string debugModeEmail = null, string savePath = null) {
			this.api = new IMailApi();
			this.userName = userName;
			this.password = password;
			this.isDebugMode = isDebugMode;
			this.debugModeEmail = debugModeEmail;
			this.savePath = savePath;
		}

		public void SetTemplates(IEnumerable<SnailMailTemplate> snailMailTemplates) {
			this.templates = snailMailTemplates;
		}

		public FileMetadata SendDefaultNoticeComm14Borrower(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{ "CustomerName", model.CustomerName }, 
				{ "CompanyName", model.CompanyName }, 
				{ "GuarantorName", model.GuarantorName },
				{ "Date", model.Date.ToLongDateWithDayOfWeek() }, 
				{ "LoanRef", model.LoanRef }, 
				{ "LoanDate", model.LoanDate.ToLongDate() }, 
				{ "LoanAmount", model.LoanAmount.ToNumericNoDecimals() }, 
				{ "SchedDate", model.MissedPayment.DateDue.ToLongDate() }, 
				{ "AmountDue", model.MissedPayment.AmountDue.ToNumeric2Decimals() }, 
				{ "AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals() }, 
				{ "AmountTotal", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals() }, 
				{ "OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals() },
			};

			SetAddress(model.CustomerAddress, ref variables);

			var templateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaultnoticeComm14BorrowerTemplateName && model.OriginId == x.OriginID && x.IsActive);
			if (templateModel == null) {
				Log.Warn("template " + DefaultnoticeComm14BorrowerTemplateName + " was not found for origin" + model.OriginId);
				return null;
			}
			Stream template = PrepareMail.ByteArrayToStream(templateModel.Template);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			return SendMail(pdfData, model.CustomerId, DefaultnoticeComm14BorrowerTemplateName, templateModel.ID);
		}

		public void SendDefaultTemplateComm7(CollectionMailModel model, out FileMetadata personalFileMetadata, out FileMetadata businessFileMetadata) {
			var variables = new Dictionary<string, string> {
				{ "CustomerName", model.CustomerName }, 
				{ "CompanyName", model.CompanyName }, 
				{ "Date", model.Date.ToLongDateWithDayOfWeek() },
				{ "LoanRef", model.LoanRef }, 
				{ "LoanDate", model.LoanDate.ToLongDate() },
				{ "LoanAmount", model.LoanAmount.ToNumericNoDecimals() }, 
				{ "SchedDate", model.MissedPayment.DateDue.ToLongDate() },
				{ "AmountDue", model.MissedPayment.AmountDue.ToNumeric2Decimals() }, 
				{ "AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals() }, 
				{ "AmountTotal", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals() }, 
				{ "OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals() },
			};

			personalFileMetadata = SendDefaultTemplateComm7Personal(model.CustomerId, variables, model.CustomerAddress, model.OriginId);
			businessFileMetadata = SendDefaultTemplateComm7Business(model.CustomerId, variables, model.CompanyAddress, model.OriginId);
		}

		public FileMetadata SendDefaultTemplateConsumer14(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{ "CustomerName", model.CustomerName }, 
				{ "Date", model.Date.ToLongDateWithDayOfWeek() }, 
				{ "LoanRef", model.LoanRef }, 
				{ "LoanDate", model.LoanDate.ToLongDate() }, 
				{ "LoanAmount", model.LoanAmount.ToNumericNoDecimals() }, 
				{ "SchedDate", model.MissedPayment.DateDue.ToLongDate() }, 
				{ "AmoDueNoFees", (model.MissedPayment.AmountDue - model.MissedPayment.Fees).ToNumeric2Decimals() }, 
				{ "AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals() }, 
				{ "TotalNoFees", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount - model.MissedPayment.Fees).ToNumeric2Decimals() }, 
				{ "OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals() }, 
				{ "FeeAmount", model.MissedPayment.Fees.ToNumericNoDecimals() }, 
				{ "Date10", model.Date.AddDays(10).ToLongDate() }
			};
			SetAddress(model.CustomerAddress, ref variables);

			var templateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaulttemplateConsumer14TemplateName && model.OriginId == x.OriginID && x.IsActive);
			if (templateModel == null) {
				Log.Warn("template " + DefaulttemplateConsumer14TemplateName + " was not found for origin" + model.OriginId);
				return null;
			}

			Stream template = PrepareMail.ByteArrayToStream(templateModel.Template);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			byte[] concatinatedMail = pdfData;

			var attachmentTemplateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaulttemplateConsumer14Attachment && model.OriginId == x.OriginID && x.IsActive);
			if (attachmentTemplateModel != null) {
				concatinatedMail = PrepareMail.ConcatinatePdfFiles(new List<byte[]> {
					pdfData,
					attachmentTemplateModel.Template
				});
			} else {
				Log.Warn("template " + DefaulttemplateConsumer14Attachment + " was not found for origin" + model.OriginId);
			}

			return SendMail(concatinatedMail, model.CustomerId, DefaulttemplateConsumer14TemplateName, templateModel.ID);
		}

		public FileMetadata SendDefaultTemplateConsumer31(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{ "CustomerName", model.CustomerName }, 
				{ "Date", model.Date.ToLongDateWithDayOfWeek() }, 
				{ "LoanRef", model.LoanRef }, 
				{ "LoanDate", model.LoanDate.ToLongDate() }, 
				{ "TotalBalance", (model.MissedInterest + model.OutstandingPrincipal).ToNumeric2Decimals() }, 
				{ "AmountDue1", (model.PreviousMissedPayment.AmountDue).ToNumeric2Decimals() }, 
				{ "DateDue1", (model.PreviousMissedPayment.DateDue).ToLongDate() }, 
				{ "PartialPaid1", (model.PreviousMissedPayment.RepaidAmount).ToNumeric2Decimals() }, 
				{ "RepaidDate1", (model.PreviousMissedPayment.RepaidDate).ToLongDate() }, 
				{ "Total1", (model.PreviousMissedPayment.AmountDue - model.PreviousMissedPayment.RepaidAmount).ToNumeric2Decimals() }, 
				{ "AmountDue2", (model.MissedPayment.AmountDue).ToNumeric2Decimals() },  
				{ "DateDue2", (model.MissedPayment.DateDue).ToLongDate() }, 
				{ "PartialPaid2", (model.MissedPayment.RepaidAmount).ToNumeric2Decimals() },  
				{ "RepaidDate2", (model.MissedPayment.RepaidDate).ToLongDate() }, 
				{ "Total2", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals() }, 
				{ "Total", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount + model.PreviousMissedPayment.AmountDue - model.PreviousMissedPayment.RepaidAmount).ToNumeric2Decimals() },
			};

			SetAddress(model.CustomerAddress, ref variables);

			var templateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaulttemplateConsumer31TemplateName && model.OriginId == x.OriginID && x.IsActive);
			if (templateModel == null) {
				Log.Warn("template " + DefaulttemplateConsumer31TemplateName + " was not found for origin" + model.OriginId);
				return null;
			}

			Stream template = PrepareMail.ByteArrayToStream(templateModel.Template);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			byte[] concatinatedMail = pdfData;

			var attachmentTemplateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaulttemplateConsumer31Attachment && model.OriginId == x.OriginID && x.IsActive);
			if (attachmentTemplateModel != null) {
				concatinatedMail = PrepareMail.ConcatinatePdfFiles(new List<byte[]> {
					pdfData,
					attachmentTemplateModel.Template
				});
			} else {
				Log.Warn("template " + DefaulttemplateConsumer31Attachment + " was not found for origin" + model.OriginId);
			}

			return SendMail(concatinatedMail, model.CustomerId, DefaulttemplateConsumer31TemplateName, templateModel.ID);
		}

		public FileMetadata SendDefaultWarningComm7Guarantor(CollectionMailModel model) {
			var variables = new Dictionary<string, string> {
				{ "CustomerName", model.CustomerName },
				{ "CompanyName", model.CompanyName }, 
				{ "GuarantorName", model.GuarantorName },  
				{ "Date", model.Date.ToLongDateWithDayOfWeek() }, 
				{ "LoanRef", model.LoanRef }, 
				{ "LoanDate", model.LoanDate.ToLongDate() }, 
				{ "LoanAmount", model.LoanAmount.ToNumericNoDecimals() }, 
				{ "SchedDate", model.MissedPayment.DateDue.ToLongDate() }, 
				{ "AmountDue", model.MissedPayment.AmountDue.ToNumeric2Decimals() }, 
				{ "AmountPaid", model.MissedPayment.RepaidAmount.ToNumeric2Decimals() }, 
				{ "AmountTotal", (model.MissedPayment.AmountDue - model.MissedPayment.RepaidAmount).ToNumeric2Decimals() }, 
				{ "OutstandingBalance", model.OutstandingBalance.ToNumeric2Decimals() },
			};

			SetAddress(model.GuarantorAddress, ref variables);

			var templateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaultwarningComm7GuarantorTemplateName && model.OriginId == x.OriginID && x.IsActive);
			if (templateModel == null) {
				Log.Warn("template " + DefaultwarningComm7GuarantorTemplateName + " was not found for origin" + model.OriginId);
				return null;
			}
			Stream template = PrepareMail.ByteArrayToStream(templateModel.Template);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			return SendMail(pdfData, model.CustomerId, DefaultwarningComm7GuarantorTemplateName, templateModel.ID);
		}

		public FileMetadata SendAnual77ANotification(int customerID, SnailMailTemplate template, Address address, Dictionary<string,string> variables, TableModel schedule, string scheduleNode) {
			SetAddress(address, ref variables);
			Stream templateStream = PrepareMail.ByteArrayToStream(template.Template);
			var templateDoc = PrepareMail.GetDocumentFromTemplate(templateStream);
			var scheduleTableDoc = PrepareMail.CreateTable(schedule);
			var templateDocWithSchedule = PrepareMail.ReplaceNodeByAnotherDocument(templateDoc, scheduleNode, scheduleTableDoc);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(templateDocWithSchedule, variables);
			return SendMail(pdfData, customerID, template.TemplateName, template.ID);
		}

		private FileMetadata SendDefaultTemplateComm7Business(int customerID, Dictionary<string, string> variables, Address companyAddress, int originId) {
			SetAddress(companyAddress, ref variables);
			var templateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaulttemplateComm7BusinessTemplateName && originId == x.OriginID && x.IsActive);
			if (templateModel == null) {
				Log.Warn("template " + DefaulttemplateComm7BusinessTemplateName + " was not found for origin" + originId);
				return null;
			}
			Stream template = PrepareMail.ByteArrayToStream(templateModel.Template);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			return SendMail(pdfData, customerID, DefaulttemplateComm7BusinessTemplateName, templateModel.ID);
		}

		private FileMetadata SendDefaultTemplateComm7Personal(int customerID, Dictionary<string, string> variables, Address customerAddress, int originId) {
			SetAddress(customerAddress, ref variables);
			var templateModel = this.templates.FirstOrDefault(x => x.TemplateName == DefaulttemplateComm7PersonalTemplateName && originId == x.OriginID && x.IsActive);
			if (templateModel == null) {
				Log.Warn("template " + DefaulttemplateComm7PersonalTemplateName + " was not found for origin" + originId);
				return null;
			}
			Stream template = PrepareMail.ByteArrayToStream(templateModel.Template);
			byte[] pdfData = PrepareMail.ReplaceParametersAndConvertToPdf(template, variables);
			return SendMail(pdfData, customerID, DefaulttemplateComm7PersonalTemplateName, templateModel.ID);
		}

		private FileMetadata SendMail(byte[] pdfData, int customerID, string templateName, int templateID) {
			Log.InfoFormat("Sending mail to customer {0} template {1}", customerID, templateName);
			bool success = false;
			success = this.api.Authenticate(this.userName, this.password);
			if (!success) {
				Log.ErrorFormat("Imail authentication failed\n{0}", this.api.GetErrorMessage());
				return null;
			}
			if (this.isDebugMode) {
				Log.InfoFormat("Sending mail to customer {0} template {1} in debug mode to email {2}", customerID, templateName, this.debugModeEmail);
				if (!string.IsNullOrEmpty(this.debugModeEmail)) {
					success = this.api.SetEmailPreview(this.debugModeEmail);
					if (!success) {
						Log.ErrorFormat("Imail authentication failed\n{0}", this.api.GetErrorMessage());
						return null;
					}
				} else {
					Log.ErrorFormat("Imail Debug mode and email is not provided");
					return null;
				}
			}

			success = this.api.ProcessPrintReadyPDF(pdfData, null, false);
			if (!success) {
				Log.ErrorFormat("Imail ProcessPrintReadyPDF failed\n{0}", this.api.GetErrorMessage());
				return null;
			}
			if (!string.IsNullOrEmpty(this.savePath)) {
				try {
					return PrepareMail.SaveFile(pdfData, this.savePath, customerID, templateName, templateID);
				} catch(Exception ex) {
					Log.WarnFormat("Failed to save mail copy for {0} for customer {1}\n{2}", templateName, customerID, ex);
				}
			}

			return null;
		}

		private void SetAddress(Address address, ref Dictionary<string, string> variables) {
			variables["Address1"] = address.Line1;
			variables["Address2"] = address.Line2;
			variables["Address3"] = address.Line3;
			variables["Address4"] = address.Line4;
			variables["Postcode"] = address.Postcode;
		}


		private const string DefaulttemplateComm7BusinessTemplateName = "DefaulttemplateComm7BusinessTemplateName";
		private const string DefaulttemplateComm7PersonalTemplateName = "DefaulttemplateComm7PersonalTemplateName";

		private const string DefaultnoticeComm14BorrowerTemplateName = "DefaultnoticeComm14BorrowerTemplateName";
		private const string DefaulttemplateConsumer14Attachment = "DefaulttemplateConsumer14Attachment";
		private const string DefaulttemplateConsumer14TemplateName = "DefaulttemplateConsumer14TemplateName";

		private const string DefaulttemplateConsumer31Attachment = "DefaulttemplateConsumer31Attachment";
		private const string DefaulttemplateConsumer31TemplateName = "DefaulttemplateConsumer31TemplateName";
		
		//TODO implement the guarantor snail mail sending logic
		private const string DefaultwarningComm7GuarantorTemplateName = "DefaultwarningComm7GuarantorTemplateName";
		
		private readonly IMailApi api;
		private readonly string debugModeEmail;
		private readonly bool isDebugMode;
		private readonly string password;
		private readonly string userName;
		private readonly string savePath;
		protected static readonly ILog Log = LogManager.GetLogger(typeof (CollectionMail));
		private IEnumerable<SnailMailTemplate> templates;
	}
}
