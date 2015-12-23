namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MailStrategies;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Collection;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using IMailLib;
	using PaymentServices.Calculators;
	using PaymentServices.PayPoint;
	using StructureMap;

	/// <summary>
	/// Retrieves all loans that are late - updates customer, loan, loan schedule statuses,
	/// sends email, sms and mail according to collection rules
	/// </summary>
	public class SetLateLoanStatus : AStrategy {
		public SetLateLoanStatus() {
			this.collectionIMailer = new CollectionMail(
				ConfigManager.CurrentValues.Instance.ImailUserName,
				ConfigManager.CurrentValues.Instance.IMailPassword,
				ConfigManager.CurrentValues.Instance.IMailDebugModeEnabled,
				ConfigManager.CurrentValues.Instance.IMailDebugModeEmail,
				ConfigManager.CurrentValues.Instance.IMailSavePath);
		} // constructor

		public override string Name { get { return "SetLateLoanStatus"; } }

		public override void Execute() {

			this.now = DateTime.UtcNow;

			//-----------Select relevan loans----------------------------------------------------
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				//-----------Mark Loans as Late----------------------------------------------------
				MarkLoanAsLate(sr);
				return ActionResult.Continue;
			}, "GetLoansToCollect",
			CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));

			//-----------Send collection mails sms imails and change status --------------------
			LoadSmsTemplates();
			LoadImailTemplates();
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				try {
					HandleCollectionLogic(sr);
				} catch (Exception ex) {
					Log.Error(ex, "Failed to handle collection for customer {0}", sr["CustomerID"]);
				}

				return ActionResult.Continue;
			}, "GetLateForCollection",
			CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));

			//-----------Change status to enabled for cured loans--------------------------------
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				int customerID = sr["CustomerID"];
				int loanID = sr["LoanID"];
				try {
					HandleCuredLoan(customerID, loanID);
				} catch (Exception ex) {
					Log.Error(ex, "Failed to handle cured loan for customer {0}", customerID);
				}
				return ActionResult.Continue;
			}, "GetCuredLoansForCollection", CommandSpecies.StoredProcedure);


			// run NL "SetLateLoanStatus" job.
			try {
				LateLoanJob nlLateJob = new LateLoanJob(this.now);
				nlLateJob.Execute();
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Debug(ex.Message);
				NL_AddLog(LogType.Error, "Strategy failed", this.now, null, ex.Message, ex.StackTrace);
			}

		}//Execute

		private void LoadImailTemplates() {
			List<CollectionSnailMailTemplate> templates = this.DB.Fill<CollectionSnailMailTemplate>("LoadCollectionSnailMailTemplates", CommandSpecies.StoredProcedure);
			this.collectionIMailer.SetTemplates(templates.Select(x => new SnailMailTemplate {
				ID = x.CollectionSnailMailTemplateID,
				Type = x.Type,
				OriginID = x.OriginID,
				Template = x.Template,
				IsActive = x.IsActive,
				TemplateName = x.TemplateName,
				FileName = x.FileName,
				IsLimited = x.IsLimited
			}));
		}

		private void LoadSmsTemplates() {
			this.smsTemplates = DB.Fill<CollectionSmsTemplate>("LoadCollectionSmsTemplates", CommandSpecies.StoredProcedure);
		}//LoadSmsTemplates

		private void HandleCuredLoan(int customerID, int loanID) {
			ChangeStatus(customerID, loanID, CollectionStatusNames.Enabled, CollectionType.Cured);
		}//HandleCuredLoan

		private int AddCollectionLog(int customerID, int loanID, CollectionType type, CollectionMethod method) {
			Log.Info("Adding collection log to customer {0} loan {1} type {2} method {3}", customerID, loanID, type, method);
			return DB.ExecuteScalar<int>("AddCollectionLog",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("LoanID", loanID),
				new QueryParameter("Type", type.ToString()),
				new QueryParameter("Method", method.ToString()),
				new QueryParameter("Now", this.now));
		}//AddCollectionLog

		private void SaveCollectionSnailMailMetadata(int collectionLogID, FileMetadata fileMetadata) {
			if (fileMetadata == null) {
				return;
			}

			Log.Info("Adding collection snail mail metadata collection log id {0} file {1}", collectionLogID, fileMetadata.Name);
			DB.ExecuteNonQuery("AddCollectionSnailMailMetadata",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CollectionLogID", collectionLogID),
				new QueryParameter("Name", fileMetadata.Name),
				new QueryParameter("ContentType", fileMetadata.ContentType),
				new QueryParameter("Path", fileMetadata.Path),
				new QueryParameter("Now", this.now),
				new QueryParameter("CollectionSnailMailTemplateID", fileMetadata.TemplateID));
		}//SaveCollectionSnailMailMetadata

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
			}//if
		}//CalculateFee

		private void ChangeStatus(int customerID, int loanID, CollectionStatusNames status, CollectionType type) {
			Log.Info("Changing collection status to customer {0} loan {1} type {2} status {3}", customerID, loanID, type, status);
			bool wasChanged = DB.ExecuteScalar<bool>("UpdateCollectionStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID),
				new QueryParameter("CollectionStatus", (int)status),
				new QueryParameter("Now", this.now));
			if (!wasChanged) {
				Log.Info("ChangeStatus to customer {0} loan {1} status {2} was not changed - customer already in this status", customerID, loanID, status);
			}

			AddCollectionLog(customerID, loanID, type, CollectionMethod.ChangeStatus);

			//TODO update loan collection status if want to be on loan level and not on customer level
			Log.Info("update loan collection status if want to be on loan level and not on customer level for customer {0}, loan {1}", customerID, loanID);
		}//ChangeStatus

		private void CollectionDay0(CollectionDataModel model, CollectionType type) {
			//send email Default Template0
			SendCollectionEmail(CollectionDay0EmailTemplate, model, type);

			//send sms Default SMS0
			SendCollectionSms(model, type);
		}//CollectionDay0

		private void CollectionDay3(CollectionDataModel model, CollectionType type) {
			//send sms Default SMS3
			SendCollectionSms(model, type);
		}//CollectionDay3

		private void CollectionDay10(CollectionDataModel model, CollectionType type) {
			//send sms Default SMS10
			SendCollectionSms(model, type);
		}//CollectionDay10

		private void CollectionDay13(CollectionDataModel model, CollectionType type) {
			//send sms Default SMS13
			SendCollectionSms(model, type);
		}//CollectionDay13

		private void CollectionDay15(CollectionDataModel model, CollectionType type) {
			//change status to 15-30 days missed
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed15To30, type);

			//send email Default Template14-29
			SendCollectionEmail(CollectionDay15EmailTemplate, model, type);

			//send imail DefaulttemplateConsumer14
			SendCollectionImail(model, type);
		}//CollectionDay15

		private void CollectionDay1to6(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed1To14, type);

			//send email Default Template1-6
			SendCollectionEmail(CollectionDay1to6EmailTemplate, model, type);
		}//CollectionDay1to6

		private void CollectionDay21(CollectionDataModel model, CollectionType type) {
			//send sms Default SMS21
			SendCollectionSms(model, type);
		}//CollectionDay21

		private void CollectionDay31(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed31To45, type);
			//send email Default Template30
			SendCollectionEmail(CollectionDay31EmailTemplate, model, type);

			//send imail DefaulttemplateConsumer31
			SendCollectionImail(model, type);

			//send sms Default SMS31
			SendCollectionSms(model, type);
		}//CollectionDay31

		private void CollectionDay46(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed46To60, type);
		}//CollectionDay46

		private void CollectionDay60(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed61To90, type);
		}//CollectionDay60

		private void CollectionDay7(CollectionDataModel model, CollectionType type) {
			//send email Default Template7
			SendCollectionEmail(CollectionDay7EmailTemplate, model, type);

			//send imail DefaulttemplateComm7 for limited loan
			SendCollectionImail(model, type);

			//send sms Default SMS7
			SendCollectionSms(model, type);
		}//CollectionDay7

		private void CollectionDay8to14(CollectionDataModel model, CollectionType type) {
			//send email Default Template8-13
			SendCollectionEmail(CollectionDay8to14EmailTemplate, model, type);
		}//CollectionDay8to14

		private void CollectionDay90(CollectionDataModel model, CollectionType type) {
			ChangeStatus(model.CustomerID, model.LoanID, CollectionStatusNames.DaysMissed90Plus, type);
		}//CollectionDay90

		private CollectionMailModel GetCollectionMailModel(CollectionDataModel model) {
			SafeReader sr = DB.GetFirst("GetDataForCollectionMail", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("LoanID", model.LoanID));
			CollectionMailModel mailModel = new CollectionMailModel {
				CustomerName = model.FullName,
				CustomerAddress = new Address {
					Line1 = sr["CAddress1"],
					Line2 = sr["CAddress2"],
					Line3 = sr["CAddress3"],
					Line4 = sr["CAddress4"],
					Postcode = sr["CPostcode"],
				},
				CompanyAddress = new Address {
					Line1 = sr["BAddress1"],
					Line2 = sr["BAddress2"],
					Line3 = sr["BAddress3"],
					Line4 = sr["BAddress4"],
					Postcode = sr["BPostcode"],
				},
				GuarantorAddress = new Address { //TODO implement
					Line1 = sr["GAddress1"],
					Line2 = sr["GAddress2"],
					Line3 = sr["GAddress3"],
					Line4 = sr["GAddress4"],
					Postcode = sr["GPostcode"],
				},
				GuarantorName = sr["GuarantorName"], //TODO implement
				IsLimited = sr["IsLimited"],
				CompanyName = sr["CompanyName"],
				Date = this.now,

				LoanAmount = sr["LoanAmount"],
				LoanRef = sr["LoanRef"],
				LoanDate = sr["LoanDate"],
				//OutstandingBalance = sr["OutstandingBalance"],
				OutstandingPrincipal = sr["OutstandingPrincipal"],
				CustomerId = model.CustomerID,
				OriginId = model.OriginID,
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

			var loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			Loan loan = loanRepository.Get(model.LoanID);
			var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, this.now, CurrentValues.Instance.AmountToChargeFrom);
			var balance = payEarlyCalc.TotalEarlyPayment();
			mailModel.OutstandingBalance = balance;

			try {
				GetLoanIDByOldID s1 = new GetLoanIDByOldID(loan.Id);
				s1.Execute();
				long nlLoanId = s1.LoanID;
				if (nlLoanId > 0) {
					GetLoanState nlState = new GetLoanState(model.CustomerID, nlLoanId, this.now, 1);
					nlState.Execute();
					var nlModel = nlState.Result;
					Log.Info("<<< NL_Compare: {0}\n===============loan: {1}  >>>", nlModel, loan);
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Log.Info("<<< NL_Compare fail at: {0}, err: {1}", Environment.StackTrace, ex.Message);
			}

			return mailModel;
		}//GetCollectionMailModel

		/// <summary>
		/// For the first late schedule 
		/// </summary>
		private void HandleCollectionLogic(SafeReader sr) {
			DateTime scheduleDate = sr["ScheduleDate"];
			int loanId = sr["LoanID"];
			string dayPhone = sr["DaytimePhone"];
			string mobilePhone = sr["MobilePhone"];
			int lateDays = (int)(this.now - scheduleDate).TotalDays;

			var model = new CollectionDataModel {
				CustomerID = sr["CustomerID"],
				OriginID = sr["OriginID"],
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

			if (lateDays == 0) { CollectionDay0(model, CollectionType.CollectionDay0); }

			if (lateDays >= 1 && lateDays <= 6) { CollectionDay1to6(model, CollectionType.CollectionDay1to6); }

			if (lateDays == 3) { CollectionDay3(model, CollectionType.CollectionDay3); }

			if (lateDays == 7) { CollectionDay7(model, CollectionType.CollectionDay7); }

			if (lateDays >= 8 && lateDays <= 14) { CollectionDay8to14(model, CollectionType.CollectionDay8to14); }

			if (lateDays == 10) { CollectionDay10(model, CollectionType.CollectionDay10); }

			if (lateDays == 13) { CollectionDay13(model, CollectionType.CollectionDay13); }

			if (lateDays == 15) { CollectionDay15(model, CollectionType.CollectionDay15); }

			if (lateDays == 21) { CollectionDay21(model, CollectionType.CollectionDay21); }

			if (lateDays == 31) { CollectionDay31(model, CollectionType.CollectionDay31); }

			if (lateDays == 46) { CollectionDay46(model, CollectionType.CollectionDay46); }

			if (lateDays == 60) { CollectionDay60(model, CollectionType.CollectionDay60); }

			if (lateDays == 90) { CollectionDay90(model, CollectionType.CollectionDay90); }

			UpdateLoanStats(loanId, lateDays, model.AmountDue);
		}//HandleCollectionLogic

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
				DB.ExecuteNonQuery("UpdateLoanScheduleStatus", CommandSpecies.StoredProcedure, new QueryParameter("Id", id), new QueryParameter("Status", "Late"));
			}

			int daysBetween = (int)(this.now - scheduleDate).TotalDays;
			int feeAmount, feeType;
			CalculateFee(daysBetween, interest, out feeAmount, out feeType);

			bool appliedLateCharge = false;
			if (feeType != 0) {
				var papi = new PayPointApi();
				appliedLateCharge = papi.ApplyLateCharge(feeAmount, loanId, feeType);
			} // if

			//TODO add late fees to new table
			Log.Info("add new late fee and mark loan as late for customer {0}", customerId);

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
				CollectionMails collectionMails = new CollectionMails(model.CustomerID, emailTemplateName, variables);
				collectionMails.Execute();
				AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Email);
			} else {
				Log.Info("Collection sending email is not allowed, email is not sent to customer {0}\n email template {1}", model.CustomerID, emailTemplateName);
			}
		}//SendCollectionEmail

		private void SendCollectionImail(CollectionDataModel model, CollectionType type) {
			if (model.ImailSendingAllowed) {
				try {
					IMailLib.CollectionMailModel mailModel = GetCollectionMailModel(model);
					switch (type) {
					case CollectionType.CollectionDay7:
						if (mailModel.IsLimited) {
							Log.Info("Sending imail {0} to customer {1}", model.CustomerID, type);
							FileMetadata personal;
							FileMetadata business;
							this.collectionIMailer.SendDefaultTemplateComm7(mailModel, out personal, out business);
							int collection7LogID = AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Mail);
							SaveCollectionSnailMailMetadata(collection7LogID, personal);
							SaveCollectionSnailMailMetadata(collection7LogID, business);
						}
						break;
					case CollectionType.CollectionDay15:
						FileMetadata day15Metadata;
						if (mailModel.IsLimited) {
							Log.Info("Sending imail {0} to customer {1}", model.CustomerID, type);
							day15Metadata = this.collectionIMailer.SendDefaultNoticeComm14Borrower(mailModel);
							//TODO uncomment when guarantor is implemented: 
							//collectionIMailer.SendDefaultWarningComm7Guarantor(mailModel);
						} else {
							Log.Info("Sending imail {0} to customer {1}", model.CustomerID, type);
							day15Metadata = this.collectionIMailer.SendDefaultTemplateConsumer14(mailModel);
						}
						int collection15LogID = AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Mail);
						SaveCollectionSnailMailMetadata(collection15LogID, day15Metadata);
						break;
					case CollectionType.CollectionDay31:
						if (!mailModel.IsLimited) {
							Log.Info("Sending imail {0} to customer {1}", model.CustomerID, type);
							FileMetadata consumer = this.collectionIMailer.SendDefaultTemplateConsumer31(mailModel);
							int collection31LogID = AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Mail);
							SaveCollectionSnailMailMetadata(collection31LogID, consumer);
						}
						break;
					}
				} catch (Exception ex) {
					Log.Error(ex, "Sending Imail failed for customer {0}", model.CustomerID);
				}
			} else {
				Log.Info("Collection sending mail is not allowed, mail is not sent to customer {0}\n template {1}", model.CustomerID, type);
			}
		}//SendCollectionImail

		private void SendCollectionSms(CollectionDataModel model, CollectionType type) {
			var smsModel = this.smsTemplates.FirstOrDefault(x => x.IsActive && x.OriginID == model.OriginID && x.Type == type.ToString());
			if (smsModel == null) {
				Log.Info("Collection not sending sms, sms template is not found. customer {0} origin {1} type {2}",
					model.CustomerID, model.OriginID, type);
				return;
			}

			var smsTemplate = string.Format(smsModel.Template, model.FirstName);

			if (model.SmsSendingAllowed && !ConfigManager.CurrentValues.Instance.SmsTestModeEnabled) {
				Log.Info("Collection sending sms to customer {0} phone number {1}\n content {2}",
					model.CustomerID, model.PhoneNumber, smsTemplate);
				new SendSms(model.CustomerID, 1, model.PhoneNumber, smsTemplate).Execute();
				AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Sms);
			} else if (model.SmsSendingAllowed) {
				Log.Info("Collection sending sms is in test mode, sms is not sent to customer {0} phone number {1}\n content {2}",
					model.CustomerID, model.PhoneNumber, smsTemplate);
				AddCollectionLog(model.CustomerID, model.LoanID, type, CollectionMethod.Sms);
			} else {
				Log.Info("Collection sending sms is not allowed, sms is not sent to customer {0} phone number {1}\n content {2}",
					model.CustomerID, model.PhoneNumber, smsTemplate);
			}
		}//SendCollectionSms

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

			//TODO save loan statistics to new table
			Log.Info("add new late fee and mark loan as late for loanId {0}", loanId);
		}// UpdateLoanStats

		private const string CollectionDay8to14EmailTemplate = "Mandrill - Last warning - Debt recovery";
		private const string CollectionDay7EmailTemplate     = "Mandrill - 20p late fee";
		private const string CollectionDay31EmailTemplate    = "Mandrill - legal process starting";
		private const string CollectionDay1to6EmailTemplate  = "Mandrill - missed payment";
		private const string CollectionDay15EmailTemplate    = "Mandrill - Warning notice- 40p late fee";
		private const string CollectionDay0EmailTemplate     = "Mandrill - you missed your payment";

		private readonly IMailLib.CollectionMail collectionIMailer;
		private DateTime now;
		private List<CollectionSmsTemplate> smsTemplates;


	}// class SetLateLoanStatus
} // namespace
