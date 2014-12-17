namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using MailStrategies.API;
	using PaymentServices.PayPoint;

	/// <summary>
	/// Retrieves all loans that are late - updates customer, loan, loan schedule statuses,
	/// sends email, sms and mail according to collection rules
	/// </summary>
	public class SetLateLoanStatus : AStrategy {

		public SetLateLoanStatus() {
			mailer = new StrategiesMailer();
		} // constructor

		public override string Name {
			get {
				return "Set Late Loan Status";
			}
		}// Name

		public override void Execute() {
			now = DateTime.UtcNow;
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				MarkLoanAsLate(sr, now);
				return ActionResult.Continue;
			}, "GetLoansToCollect", CommandSpecies.StoredProcedure, new QueryParameter("Now", now));


			DB.ForEachRowSafe((sr, bRowsetStart) => {
				HandleCollectionLogic(sr);
				return ActionResult.Continue;
			}, "GetLateForCollection", CommandSpecies.StoredProcedure, new QueryParameter("Now", now));
		}//Execute

		/// <summary>
		/// For each loan schedule marks it as late, it's loan as late, applies fee if needed
		/// </summary>
		/// <param name="sr"></param>
		private void MarkLoanAsLate(SafeReader sr, DateTime now) {
			int id = sr["id"];
			int loanId = sr["LoanId"];
			int customerId = sr["CustomerId"];
			DateTime scheduleDate = sr["ScheduleDate"];
			string loanStatus = sr["LoanStatus"];
			string scheduleStatus = sr["ScheduleStatus"];
			decimal interest = sr["Interest"];
			//Some strange logic to set custom installment date was used for fake loan generation logic fix not reproduced

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

		} // MarkLoansAsLate

		/// <summary>
		/// For the first late schedule 
		/// </summary>
		private void HandleCollectionLogic(SafeReader sr) {
			DateTime scheduleDate = sr["ScheduleDate"];

			int loanId = sr["LoanID"];
			string dayPhone = sr["DaytimePhone"];
			string mobilePhone = sr["MobilePhone"];

			var model = new CollectionDataModel {
				CustomerID = sr["CustomerID"],
				LoanID = loanId,
				ScheduleID = sr["ScheduleID"],
				LoanRefNum = sr["LoanRefNum"],
				FirstName = sr["FirstName"],
				Surname = sr["Surname"],
				FullName = sr["FullName"],
				AmountDue = sr["AmountDue"],
				Interest = sr["Interest"],
				FeeAmount = sr["Fees"],
				Email = sr["email"],
				DueDate = scheduleDate,
				PhoneNumber = string.IsNullOrEmpty(dayPhone) ? mobilePhone : dayPhone,
				BusinessAddress = "", //todo
				PersonalAddress = "", //todo
				SmsSendingAllowed = sr["EmailSendingAllowed"],
				EmailSendingAllowed = sr["EmailSendingAllowed"],
				ImailSendingAllowed = sr["EmailSendingAllowed"],
			};

			Log.Info(model.ToString());

			int daysBetween = (int)(now - scheduleDate).TotalDays;

			if (daysBetween == 0) {
				CollectionDay0(model);
			}

			if (daysBetween >= 1 && daysBetween <= 6) {
				CollectionDay1to6(model);
			}

			if (daysBetween == 7) {
				CollectionDay7(model);
			}

			if (daysBetween >= 8 && daysBetween <= 14) {
				CollectionDay8to14(model);
			}

			if (daysBetween == 15) {
				CollectionDay15(model);
			}

			if (daysBetween == 21) {
				CollectionDay21(model);
			}

			if (daysBetween == 31) {
				CollectionDay31(model);
			}

			if (daysBetween == 46) {
				CollectionDay46(model);
			}

			if (daysBetween == 60) {
				CollectionDay60(model);
			}

			if (daysBetween == 90) {
				CollectionDay90(model);
			}

			UpdateLoanStats(loanId, daysBetween, model.AmountDue);
		}// HandleCollectionLogic

		/// <summary>
		/// TODO This is wrong logic, there was a bug that corrupted all the values in loan table need to be backfilled and fixed
		/// </summary>
		private void UpdateLoanStats(int loanId, int daysBetween, decimal amountDue) {
			var model = new LoanStatsModel();
			if (daysBetween > 0 && daysBetween <= 30) {
				model.Late30Num++;
				model.Late30 += amountDue;
			}
			else if (daysBetween > 30 && daysBetween <= 60) {
				model.Late60Num++;
				model.Late60 += amountDue;
			}
			else if (daysBetween > 60 && daysBetween <= 90) {
				model.Late90Num++;
				model.Late90 += amountDue;
			}
			else { // daysBetween > 90
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

		private void CalculateFee(int daysBetween, decimal interest, out int feeAmount, out int feeType) {
			feeAmount = 0;
			feeType = 0;

			if (daysBetween >= CurrentValues.Instance.CollectionPeriod1 && daysBetween < CurrentValues.Instance.CollectionPeriod2) {
				feeAmount = CurrentValues.Instance.LatePaymentCharge;
				feeType = CurrentValues.Instance.LatePaymentCharge.ID;
			}
			else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest > 0) {
				feeAmount = CurrentValues.Instance.AdministrationCharge;
				feeType = CurrentValues.Instance.AdministrationCharge.ID;
			}
			else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest <= 0) {
				feeAmount = CurrentValues.Instance.PartialPaymentCharge;
				feeType = CurrentValues.Instance.PartialPaymentCharge.ID;
			}
		} // CalculateFee


		private void CollectionDay0(CollectionDataModel model) {
			//send email Default Template0
			const string templateName = "Mandrill - ezbob - you missed your payment";
			SendCollectionEmail(templateName, model);
			//send sms Default SMS0
			string smsTemplate = string.Format("{0} This is a courtesy message to remind you that a payment of {1} is overdue with ezbob. Please call Emma at 02033711842 to arrange your payment ASAP.",
				model.FirstName, model.AmountDue);
			SendCollectionSms(smsTemplate, model);
		}//CollectionDay0

		private void CollectionDay1to6(CollectionDataModel model) {
			ChangeStatus(model.CustomerID, CollectionStatusNames.DaysMissed1To14);

			//send email Default Template1-6
			const string templateName = "Mandrill - ezbob - missed payment";
			SendCollectionEmail(templateName, model);
			//send sms Default SMS1-6
			string smsTemplate = string.Format("{0} the outstanding balance of {1} with ezbob has not been settled. Failure to settle the account will result in additional late payment fees being added to your account balance. Emma 02033711842",
				model.FirstName, model.AmountDue);
			SendCollectionSms(smsTemplate, model);
		}//CollectionDay1to6

		private void CollectionDay7(CollectionDataModel model) {
			//send email Default Template7
			const string templateName = "Mandrill - ezbob - £20 late fee was added to your account";
			SendCollectionEmail(templateName, model);
			//send imail DefaulttemplateComm7 for limited loan
			SendCollectionImail("DefaulttemplateComm7", model);
			//send sms Default SMS7
			string smsTemplate = string.Format("{0}, the outstanding balance of {1}, including a late payment of £20 on your account with ezbob has not been settled. Failure to settle the account within 2 days will result in debt collection proceedings being taken to retrieve the debt. Emma 02033711842",
				model.FirstName, model.AmountDue);
			SendCollectionSms(smsTemplate, model);
		}//CollectionDay7

		private void CollectionDay8to14(CollectionDataModel model) {
			//send email Default Template8-13
			const string templateName = "Mandrill - ezbob - Last warning - Debt recovery agency";
			SendCollectionEmail(templateName, model);
			//send sms Default SMS8-13
			string smsTemplate = string.Format("Warning – {0} late fees and daily interest has been added to your ezbob account. If you don’t do something quickly, ezbob can take actions against you. Please call Emma 02033711842 to arrange your payment ASAP.", model.FirstName);
			SendCollectionSms(smsTemplate, model);
		}//CollectionDay8to14

		private void CollectionDay15(CollectionDataModel model) {
			//change status to 15-30 days missed
			ChangeStatus(model.CustomerID, CollectionStatusNames.DaysMissed15To30);
			//send email Default Template14-29
			const string templateName = "Mandrill - Warning notice - ezbob - £40 late fee";
			SendCollectionEmail(templateName, model);
			//send imail DefaulttemplateConsumer14
			SendCollectionImail("DefaulttemplateConsumer14", model);
			//send sms Default SMS14
			string smsTemplate = string.Format("{0} final reminder that your account in the amount of {1} on your account was due on {2}. If we do not receive the payment in full ezbob will submit your action to our legal department.",
				model.FirstName, model.AmountDue, model.DueDate.ToString("dd/MM/yyyy"));
			SendCollectionSms(smsTemplate, model);
		}//CollectionDay15

		private void CollectionDay21(CollectionDataModel model) {
			//send sms Default SMS21
			string smsTemplate = string.Format("{0} you have failed to settle your payment. Ezbob has submitted your account for legal proceedings. Emma 02033711842",
				model.FirstName);
			SendCollectionSms(smsTemplate, model);
		}//CollectionDay21

		private void CollectionDay31(CollectionDataModel model) {
			ChangeStatus(model.CustomerID, CollectionStatusNames.DaysMissed31To45);
			//send email Default Template30
			const string templateName = "Mandrill - ezbob - legal process starting";
			SendCollectionEmail(templateName, model);
			//send imail DefaulttemplateConsumer31
			SendCollectionImail("DefaulttemplateConsumer31", model);
			//send sms Default SMS31
			string smsTemplate = string.Format("{0} you have failed to settle your payment. Ezbob has submitted your account for legal proceedings. Emma 02033711842",
				model.FirstName);
			SendCollectionSms(smsTemplate, model);
		}//CollectionDay31

		private void CollectionDay46(CollectionDataModel model) {
			ChangeStatus(model.CustomerID, CollectionStatusNames.DaysMissed46To60);
		}

		private void CollectionDay60(CollectionDataModel model) {
			ChangeStatus(model.CustomerID, CollectionStatusNames.DaysMissed61To90);
		}

		private void CollectionDay90(CollectionDataModel model) {
			ChangeStatus(model.CustomerID, CollectionStatusNames.DaysMissed90Plus);
		}

		private void SendCollectionEmail(string emailTemplateName, CollectionDataModel model) {
			if (model.EmailSendingAllowed) {
				var variables = new Dictionary<string, string> {
					{"FirstName", model.FirstName}, 
					{"RefNum", model.LoanRefNum}, 
					{"FeeAmount", model.FeeAmount.ToString(CultureInfo.InvariantCulture)},
					{"AmountCharged", model.AmountDue.ToString(CultureInfo.InvariantCulture)},
					{"ScheduledAmount", model.AmountDue.ToString(CultureInfo.InvariantCulture)},
				};
				mailer.Send(emailTemplateName, variables, new Addressee(model.Email));
			}
			else {
				Log.Debug("Collection sending email is not allowed, email is not sent to customer {0}\n email template {1}", model.CustomerID, emailTemplateName);
			}
		}//SendCollectionEmail

		private void SendCollectionSms(string smsTemplate, CollectionDataModel model) {
			if (model.SmsSendingAllowed) {
				new SendSms(model.CustomerID, 1, model.PhoneNumber, smsTemplate).Execute();
			}
			else {
				Log.Debug("Collection sending sms is not allowed, sms is not sent to customer {0}\n content {1}", model.CustomerID, smsTemplate);
			}
		}//SendCollectionSms

		private void SendCollectionImail(string mailTemplate, CollectionDataModel model) {
			if (model.ImailSendingAllowed) {
				//TODO implement
			}
			else {
				Log.Debug("Collection sending mail is not allowed, mail is not sent to customer {0}\n template {1}", model.CustomerID, mailTemplate);
			}
		} //SendCollectionImail

		private void ChangeStatus(int customerID, CollectionStatusNames status) {
			DB.ExecuteNonQuery("UpdateCollectionStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("CollectionStatus", (int)status));
		} //ChangeStatus

		private readonly StrategiesMailer mailer;
		private DateTime now;

		public class LoanStatsModel {
			public decimal Late30 { get; set; }
			public decimal Late60 { get; set; }
			public decimal Late90 { get; set; }
			public decimal Late90Plus { get; set; }
			public decimal PastDues { get; set; }
			public int Late30Num { get; set; }
			public int Late60Num { get; set; }
			public int Late90Num { get; set; }
			public int Late90PlusNum { get; set; }
			public int PastDuesNum { get; set; }
		}//class

		public class CollectionDataModel {
			public int CustomerID { get; set; }
			public int LoanID { get; set; }
			public int ScheduleID { get; set; }
			public string FirstName { get; set; }
			public string Surname { get; set; }
			public string FullName { get; set; }
			public string Email { get; set; }
			public string PersonalAddress { get; set; }
			public string BusinessAddress { get; set; }
			public string PhoneNumber { get; set; }
			public decimal AmountDue { get; set; }
			public decimal FeeAmount { get; set; }
			public decimal Interest { get; set; }
			public string LoanRefNum { get; set; }
			public DateTime DueDate { get; set; }
			public bool EmailSendingAllowed { get; set; }
			public bool SmsSendingAllowed { get; set; }
			public bool ImailSendingAllowed { get; set; }

			public override string ToString() {
				return string.Format(@"Collection model for CustomerID:{0}
LoanID:{1},ScheduleID:{2},LoanRefNum:{11}
FirstName:{3}, Surname:{4}, FullName:{5}, Email:{6}, PhoneNumber:{7}
AmountDue:{8}, FeeAmount:{9}, Interest:{10}, DueDate:{12}
EmailSendingAllowed:{13}, SmsSendingAllowed: {14}, ImailSendingAllowed: {15}",
					CustomerID, LoanID, ScheduleID,
					FirstName, Surname, FullName, Email, PhoneNumber, AmountDue, FeeAmount, Interest, LoanRefNum, DueDate,
					EmailSendingAllowed, SmsSendingAllowed, ImailSendingAllowed);
			}
		}//class
	}// class 
} // namespace
