namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using IMailLib;
	using MailStrategies.API;
	using PaymentServices.PayPoint;

	/// <summary>
	/// Retrieves all loans that are late - updates customer, loan, loan schedule statuses,
	/// sends email, sms and mail according to collection rules
	/// </summary>
	public class SetLateLoanStatus : AStrategy {
		public SetLateLoanStatus() {
			mailer = new StrategiesMailer();
			collectionIMailer = new CollectionMail(
				ConfigManager.CurrentValues.Instance.ImailUserName,
				ConfigManager.CurrentValues.Instance.IMailPassword,
				ConfigManager.CurrentValues.Instance.IMailDebugModeEnabled,
				ConfigManager.CurrentValues.Instance.IMailDebugModeEmail,
				ConfigManager.CurrentValues.Instance.IMailSavePath);
		} // constructor

		public override string Name {
			get {
				return "Set Late Loan Status";
			}
		}

		public override void Execute() {
			now = DateTime.UtcNow;

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				MarkLoanAsLate(sr);
				return ActionResult.Continue;
			}, "GetLoansToCollect",
			CommandSpecies.StoredProcedure, new QueryParameter("Now", now));


			DB.ForEachRowSafe((sr, bRowsetStart) => {
				try {
					HandleCollectionLogic(sr);
				} catch (Exception ex) {
					Log.Error(ex, "Failed to handle collection for customer {0}", sr["CustomerID"]);
				}
				
				return ActionResult.Continue;
			}, "GetLateForCollection", 
			CommandSpecies.StoredProcedure, new QueryParameter("Now", now));
		}//Execute

		private void AddCollectionLog(int customerID, int loanID, CollectionType type, CollectionMethod method) {
			DB.ExecuteNonQuery("AddCollectionLog",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("LoanID", loanID),
				new QueryParameter("Type", type.ToString()),
				new QueryParameter("Method", method.ToString()),
				new QueryParameter("Now", now));
		}

		private void CalculateFee(int daysBetween, decimal interest, out int feeAmount, out int feeType) {
			feeAmount = 0;
			feeType = 0;

			if (daysBetween >= CurrentValues.Instance.CollectionPeriod1 && daysBetween < CurrentValues.Instance.CollectionPeriod2) {
				feeAmount = CurrentValues.Instance.LatePaymentCharge;
				feeType = CurrentValues.Instance.LatePaymentCharge.ID;
			} else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest > 0) {
				feeAmount = CurrentValues.Instance.AdministrationCharge;
				feeType = CurrentValues.Instance.AdministrationCharge.ID;
			} else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest <= 0) {
				feeAmount = CurrentValues.Instance.PartialPaymentCharge;
				feeType = CurrentValues.Instance.PartialPaymentCharge.ID;
			}
		}

		private void ChangeStatus(int customerID, int loanID, CollectionStatusNames status, CollectionType type) {
			DB.ExecuteNonQuery("UpdateCollectionStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("CollectionStatus", (int)status),
				new QueryParameter("Now", now));

			AddCollectionLog(customerID, loanID, type, CollectionMethod.ChangeStatus);
		}

		private void CollectionDay0(CollectionDataModel model, CollectionType type) {
			//send email Default Template0
			SendCollectionEmail(CollectionDay0EmailTemplate, model, type);

			//send sms Default SMS0
			string smsTemplate = string.Format("{0} This is a courtesy message to remind you that a payment of {1} is overdue with ezbob. Please call Emma at 02033711842 to arrange your payment ASAP.",
				model.FirstName, model.AmountDue);
			SendCollectionSms(smsTemplate, model, type);
		}

		private void CollectionDay15(CollectionDataModel model, CollectionType type) {
			//change status to 15-30 days missed
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed15To30, type);

			//send email Default Template14-29
			SendCollectionEmail(CollectionDay15EmailTemplate, model, type);

			//send imail DefaulttemplateConsumer14
			SendCollectionImail(model, type);

			//send sms Default SMS14
			string smsTemplate = string.Format("{0} final reminder that your account in the amount of {1} on your account was due on {2}. If we do not receive the payment in full ezbob will submit your action to our legal department.",
				model.FirstName, model.AmountDue, model.DueDate.ToString("dd/MM/yyyy"));
			SendCollectionSms(smsTemplate, model, type);
		}

		private void CollectionDay1to6(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed1To14, type);

			//send email Default Template1-6
			SendCollectionEmail(CollectionDay1to6EmailTemplate, model, type);

			//send sms Default SMS1-6
			string smsTemplate = string.Format("{0} the outstanding balance of {1} with ezbob has not been settled. Failure to settle the account will result in additional late payment fees being added to your account balance. Emma 02033711842",
				model.FirstName, model.AmountDue);
			SendCollectionSms(smsTemplate, model, type);
		}

		private void CollectionDay21(CollectionDataModel model, CollectionType type) {
			//send sms Default SMS21
			string smsTemplate = string.Format("{0} you have failed to settle your payment. Ezbob has submitted your account for legal proceedings. Emma 02033711842",
				model.FirstName);
			SendCollectionSms(smsTemplate, model, type);
		}

		private void CollectionDay31(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed31To45, type);
			//send email Default Template30
			SendCollectionEmail(CollectionDay31EmailTemplate, model, type);

			//send imail DefaulttemplateConsumer31
			SendCollectionImail(model, type);

			//send sms Default SMS31
			string smsTemplate = string.Format("{0} you have failed to settle your payment. Ezbob has submitted your account for legal proceedings. Emma 02033711842",
				model.FirstName);
			SendCollectionSms(smsTemplate, model, type);
		}

		private void CollectionDay46(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed46To60, type);
		}

		//CollectionDay31
		private void CollectionDay60(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed61To90, type);
		}

		private void CollectionDay7(CollectionDataModel model, CollectionType type) {
			//send email Default Template7
			SendCollectionEmail(CollectionDay7EmailTemplate, model, type);

			//send imail DefaulttemplateComm7 for limited loan
			SendCollectionImail(model, type);

			//send sms Default SMS7
			string smsTemplate = string.Format("{0}, the outstanding balance of {1}, including a late payment of £20 on your account with ezbob has not been settled. Failure to settle the account within 2 days will result in debt collection proceedings being taken to retrieve the debt. Emma 02033711842",
				model.FirstName, model.AmountDue);
			SendCollectionSms(smsTemplate, model, type);
		}

		private void CollectionDay8to14(CollectionDataModel model, CollectionType type) {
			//send email Default Template8-13
			SendCollectionEmail(CollectionDay8to14EmailTemplate, model, type);

			//send sms Default SMS8-13
			string smsTemplate = string.Format("Warning – {0} late fees and daily interest has been added to your ezbob account. If you don’t do something quickly, ezbob can take actions against you. Please call Emma 02033711842 to arrange your payment ASAP.", model.FirstName);
			SendCollectionSms(smsTemplate, model, type);
		}

		//CollectionDay21
		private void CollectionDay90(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed90Plus, type);
		}

		private CollectionMailModel GetCollectionMailModel(CollectionDataModel model) {
			SafeReader sr = DB.GetFirst("GetDataForCollectionMail", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("LoanID", model.LoanID));
			CollectionMailModel mailModel = new CollectionMailModel {
				CustomerName = model.FullName,
				CustomerAddress = new Address {
					Line1 = sr["CAddress1"],
					Line2 = sr["CAddress2"],
					Line3 = sr["CAddress3"],
					Line4 = sr["CAddress4"],
					Line5 = sr["CAddress5"],
					Postcode = sr["CPostcode"],
				},
				CompanyAddress = new Address {
					Line1 = sr["BAddress1"],
					Line2 = sr["BAddress2"],
					Line3 = sr["BAddress3"],
					Line4 = sr["BAddress4"],
					Line5 = sr["BAddress5"],
					Postcode = sr["BPostcode"],
				},
				GuarantorAddress = new Address { //TODO implement
					Line1 = sr["GAddress1"],
					Line2 = sr["GAddress2"],
					Line3 = sr["GAddress3"],
					Line4 = sr["GAddress4"],
					Line5 = sr["GAddress5"],
					Postcode = sr["GPostcode"],
				},
				GuarantorName = sr["GuarantorName"], //TODO implement
				IsLimited = sr["IsLimited"],
				CompanyName = sr["CompanyName"],
				Date = now,

				LoanAmount = sr["LoanAmount"],
				LoanRef = sr["LoanRef"],
				LoanDate = sr["LoanDate"],
				OutstandingBalance = sr["OutstandingBalance"],
				OutstandingPrincipal = sr["OutstandingPrincipal"],
				CustomerId = model.CustomerID,
				MissedPayment = new MissedPaymentModel {
					AmountDue = sr["AmountDue"],
					DateDue = sr["SchedDate"],
					Fees = sr["Fees"],
					RepaidAmount = sr["RepaidAmount"],
					RepaidDate = sr["RepaidDate"]
				},
				PreviousMissedPayment = new MissedPaymentModel {
					AmountDue = sr["PreviousAmountDue"],
					DateDue = sr["PreviousSchedDate"],
					Fees = sr["PreviousFees"],
					RepaidAmount = sr["PreviousRepaidAmount"],
					RepaidDate = sr["PreviousRepaidDate"]
				},
			};

			return mailModel;
		}

		/// <summary>
		/// For the first late schedule 
		/// </summary>
		private void HandleCollectionLogic(SafeReader sr) {
			DateTime scheduleDate = sr["ScheduleDate"];
			int loanId = sr["LoanID"];
			string dayPhone = sr["DaytimePhone"];
			string mobilePhone = sr["MobilePhone"];
			int lateDays = (int)(now - scheduleDate).TotalDays;

			var model = new CollectionDataModel {
				CustomerID = sr["CustomerID"],
				LoanID = loanId,
				ScheduleID = sr["ScheduleID"],
				LoanRefNum = sr["LoanRefNum"],
				FirstName = sr["FirstName"],
				FullName = sr["FullName"],
				AmountDue = sr["AmountDue"],
				Interest = sr["Interest"],
				FeeAmount = sr["Fees"],
				Email = sr["email"],
				DueDate = scheduleDate,
				LateDays = lateDays,
				PhoneNumber = string.IsNullOrEmpty(mobilePhone) ? dayPhone : mobilePhone,
				SmsSendingAllowed = sr["SmsSendingAllowed"],
				EmailSendingAllowed = sr["EmailSendingAllowed"],
				ImailSendingAllowed = sr["MailSendingAllowed"],
			};

			Log.Info(model.ToString());



			if (lateDays == 0) {
				CollectionDay0(model, CollectionType.CollectionDay0);
			}

			if (lateDays >= 1 && lateDays <= 6) {
				CollectionDay1to6(model, CollectionType.CollectionDay1to6);
			}

			if (lateDays == 7) {
				CollectionDay7(model, CollectionType.CollectionDay7);
			}

			if (lateDays >= 8 && lateDays <= 14) {
				CollectionDay8to14(model, CollectionType.CollectionDay8to14);
			}

			if (lateDays == 15) {
				CollectionDay15(model, CollectionType.CollectionDay15);
			}

			if (lateDays == 21) {
				CollectionDay21(model, CollectionType.CollectionDay21);
			}

			if (lateDays == 31) {
				CollectionDay31(model, CollectionType.CollectionDay31);
			}

			if (lateDays == 46) {
				CollectionDay46(model, CollectionType.CollectionDay46);
			}

			if (lateDays == 60) {
				CollectionDay60(model, CollectionType.CollectionDay60);
			}

			if (lateDays == 90) {
				CollectionDay90(model, CollectionType.CollectionDay90);
			}

			UpdateLoanStats(loanId, lateDays, model.AmountDue);
		}

		/// <summary>
		/// For each loan schedule marks it as late, it's loan as late, applies fee if needed
		/// </summary>
		/// <param name="sr"></param>
		private void MarkLoanAsLate(SafeReader sr) {
			int id = sr["id"];
			int loanId = sr["LoanId"];
			int customerId = sr["CustomerId"];
			DateTime scheduleDate = sr["ScheduleDate"];
			string loanStatus = sr["LoanStatus"];
			string scheduleStatus = sr["ScheduleStatus"];
			decimal interest = sr["Interest"];

			//Some strange logic to set custom installment date was used for fake loan generation logic --not reproduced
			//if (!isLastInstallment) {
			//	decimal amountDue = new PayPointApi().GetAmountToPay(id);

			//	int daysBetweenCustom = customInstallmentDate.HasValue ? (int)(customInstallmentDate.Value - DateTime.UtcNow).TotalDays : 0;

			//	if (!(amountDue > ConfigManager.CurrentValues.Instance.AmountToChargeFrom && daysBetweenCustom > 1)) {
			//		DB.ExecuteNonQuery(
			//			"UpdateLoanScheduleCustomDate",
			//			CommandSpecies.StoredProcedure,
			//			new QueryParameter("Id", id)
			//			);
			//		return;
			//	}
			//} // if
			if (loanStatus != "Late") {
				DB.ExecuteNonQuery(
					"UpdateLoanStatusToLate", CommandSpecies.StoredProcedure,
					new QueryParameter("LoanId", loanId),
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("PaymentStatus", "Late"),
					new QueryParameter("LoanStatus", "Late")
					);
			}

			if (scheduleStatus != "Late") {
				DB.ExecuteNonQuery("UpdateLoanScheduleStatus", CommandSpecies.StoredProcedure,
					new QueryParameter("Id", id),
					new QueryParameter("Status", "Late")
					);
			}

			int daysBetween = (int)(now - scheduleDate).TotalDays;
			int feeAmount, feeType;
			CalculateFee(daysBetween, interest, out feeAmount, out feeType);

			bool appliedLateCharge = false;
			if (feeType != 0) {
				var papi = new PayPointApi();
				appliedLateCharge = papi.ApplyLateCharge(feeAmount, loanId, feeType);
			} // if

			Log.Info("Applied late charge for customer {0} loan {1} : {2}", customerId, loanId, appliedLateCharge);

		} // MarkLoansAsLate

		private void SendCollectionEmail(string emailTemplateName, CollectionDataModel model, CollectionType type) {
			if (model.EmailSendingAllowed) {
				var variables = new Dictionary<string, string> {
					{"FirstName", model.FirstName}, 
					{"RefNum", model.LoanRefNum}, 
					{"FeeAmount", model.FeeAmount.ToString(CultureInfo.InvariantCulture)},
					{"AmountCharged", model.AmountDue.ToString(CultureInfo.InvariantCulture)},
					{"ScheduledAmount", model.AmountDue.ToString(CultureInfo.InvariantCulture)},
				};
				mailer.Send(emailTemplateName, variables, new Addressee(model.Email));
				AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Email);
			} else {
				Log.Info("Collection sending email is not allowed, email is not sent to customer {0}\n email template {1}", model.CustomerID, emailTemplateName);
			}
		}

		private void SendCollectionImail(CollectionDataModel model, CollectionType type) {
			if (model.ImailSendingAllowed) {
				try {
					Log.Info("Sending imail {0} to customer {1}", model.CustomerID, type);
					IMailLib.CollectionMailModel mailModel = GetCollectionMailModel(model);
					switch (type) {
					case CollectionType.CollectionDay7:
						if (mailModel.IsLimited) {
							collectionIMailer.SendDefaultTemplateComm7(mailModel);
							AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Mail);
						}
						break;
					case CollectionType.CollectionDay15:
						if (mailModel.IsLimited) {
							collectionIMailer.SendDefaultNoticeComm7Borrower(mailModel);
							//TODO uncomment when guarantor is implemented: 
							//collectionIMailer.SendDefaultWarningComm7Guarantor(mailModel);
						} else {
							collectionIMailer.SendDefaultTemplateConsumer14(mailModel);
						}
						AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Mail);
						break;
					case CollectionType.CollectionDay31:
						collectionIMailer.SendDefaultTemplateConsumer31(mailModel);
						AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Mail);
						break;
					}

					
				} catch (Exception ex) {
					Log.Error(ex, "Sending Imail failed for customer {0}", model.CustomerID);
				}
			} else {
				Log.Info("Collection sending mail is not allowed, mail is not sent to customer {0}\n template {1}", model.CustomerID, type);
			}

		}

		private void SendCollectionSms(string smsTemplate, CollectionDataModel model, CollectionType type) {
			if (model.SmsSendingAllowed && !ConfigManager.CurrentValues.Instance.SmsTestModeEnabled) {
				new SendSms(model.CustomerID, 1, model.PhoneNumber, smsTemplate).Execute();
			} else if (model.SmsSendingAllowed) {
				Log.Info("Collection sending sms is in test mode, sms is not sent to customer {0} phone number {1}\n content {2}",
					model.CustomerID, model.PhoneNumber, smsTemplate);
				AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Sms);
			} else {
				Log.Info("Collection sending sms is not allowed, sms is not sent to customer {0} phone number {1}\n content {2}",
					model.CustomerID, model.PhoneNumber, smsTemplate);
			}
		}

		/// <summary>
		/// TODO This is wrong logic, there was a bug that corrupted all the values in loan table need to be backfilled and fixed
		/// </summary>
		private void UpdateLoanStats(int loanId, int daysBetween, decimal amountDue) {
			var model = new LoanStatsModel();
			if (daysBetween > 0 && daysBetween <= 30) {
				model.Late30Num++;
				model.Late30 += amountDue;
			} else if (daysBetween > 30 && daysBetween <= 60) {
				model.Late60Num++;
				model.Late60 += amountDue;
			} else if (daysBetween > 60 && daysBetween <= 90) {
				model.Late90Num++;
				model.Late90 += amountDue;
			} else { // daysBetween > 90
				model.Late90PlusNum++;
				model.Late90Plus += amountDue;
			} // if

			model.PastDues += amountDue;
			model.PastDuesNum++;

			DB.ExecuteNonQuery(
				"UpdateCollection",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LoanId", loanId),
				new QueryParameter("Late30", model.Late30),
				new QueryParameter("Late30Num", model.Late30Num),
				new QueryParameter("Late60", model.Late60),
				new QueryParameter("Late60Num", model.Late60Num),
				new QueryParameter("Late90", model.Late90),
				new QueryParameter("Late90Num", model.Late90Num),
				new QueryParameter("PastDues", model.PastDues),
				new QueryParameter("PastDuesNum", model.PastDuesNum),
				new QueryParameter("IsDefaulted", 0),
				new QueryParameter("Late90Plus", model.Late90Plus),
				new QueryParameter("Late90PlusNum", model.Late90PlusNum)
				);
		}// UpdateLoanStats


		private const string CollectionDay8to14EmailTemplate = "Mandrill - ezbob - Last warning - Debt recovery";
		private const string CollectionDay7EmailTemplate     = "Mandrill - ezbob - 20p late fee";
		private const string CollectionDay31EmailTemplate    = "Mandrill - ezbob - legal process starting";
		private const string CollectionDay1to6EmailTemplate  = "Mandrill - ezbob - missed payment";
		private const string CollectionDay15EmailTemplate    = "Mandrill - Warning notice - ezbob - 40p late fee";
		private const string CollectionDay0EmailTemplate     = "Mandrill - ezbob - you missed your payment";

		private readonly IMailLib.CollectionMail collectionIMailer;
		private readonly StrategiesMailer mailer;
		private DateTime now;

		public enum CollectionMethod {
			Email,
			Mail,
			Sms,
			ChangeStatus
		}

		public enum CollectionType {
			CollectionDay0,
			CollectionDay1to6,
			CollectionDay7,
			CollectionDay8to14,
			CollectionDay15,
			CollectionDay21,
			CollectionDay31,
			CollectionDay46,
			CollectionDay60,
			CollectionDay90
		}

		public class CollectionDataModel {
			public decimal AmountDue { get; set; }
			public int CustomerID { get; set; }
			public DateTime DueDate { get; set; }
			public string Email { get; set; }
			public bool EmailSendingAllowed { get; set; }
			public decimal FeeAmount { get; set; }
			public string FirstName { get; set; }
			public string FullName { get; set; }
			public bool ImailSendingAllowed { get; set; }
			public decimal Interest { get; set; }
			public int LateDays { get; set; }
			public int LoanID { get; set; }
			public string LoanRefNum { get; set; }
			public string PhoneNumber { get; set; }
			public int ScheduleID { get; set; }
			public bool SmsSendingAllowed { get; set; }
			public override string ToString() {
				return string.Format(@"Collection model for 
CustomerID:{0}
LoanID:{1},LoanRefNum:{11},
ScheduleID:{2},
Email:{6},FirstName:{3}{4},FullName:{5}, 
PhoneNumber:{7}
AmountDue:{8},FeeAmount:{9},Interest:{10}, 
DueDate:{12},
LateDays: {13}
EmailSendingAllowed:{14}, SmsSendingAllowed: {15}, ImailSendingAllowed: {16}",
					CustomerID, LoanID, ScheduleID,
					FirstName, "", FullName, Email, PhoneNumber, AmountDue, FeeAmount, Interest, LoanRefNum, DueDate, LateDays,
					EmailSendingAllowed, SmsSendingAllowed, ImailSendingAllowed);
			}
		}

		public class LoanStatsModel {
			public decimal Late30 { get; set; }
			public int Late30Num { get; set; }
			public decimal Late60 { get; set; }
			public int Late60Num { get; set; }
			public decimal Late90 { get; set; }
			public int Late90Num { get; set; }
			public decimal Late90Plus { get; set; }
			public int Late90PlusNum { get; set; }
			public decimal PastDues { get; set; }
			public int PastDuesNum { get; set; }
		}
	}// class 
} // namespace
