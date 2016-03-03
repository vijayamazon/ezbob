namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

	/// <summary>
	/// Late Loan Notification
	/// </summary>
	public class LateLoanNotification : Misc.SetLateLoanStatus {

		public LateLoanNotification(DateTime? runTime) {
			if (runTime != null)
				now = (DateTime)runTime;
			else
				now = DateTime.UtcNow;

			base.now = now;

			nlNotificationsList = new List<CollectionDataModel>();
			sendNotificationForNewLoan = CurrentValues.Instance.SendCollectionMailOnNewLoan;
		} // constructor

		public new DateTime now { get; private set; }
		public override string Name { get { return "LateLoanNotification"; } }
		public List<CollectionDataModel> nlNotificationsList { get; private set; }
		public bool sendNotificationForNewLoan { get; private set; }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			try {

				DB.ForEachRowSafe((sr, bRowsetStart) => {
					BuildCollectionDataModel(sr);
					return ActionResult.Continue;
				}, "NL_LateLoansNotificationGet", CommandSpecies.StoredProcedure, new QueryParameter("Now", now));

				if (nlNotificationsList.Count > 0) {
					LoadSmsTemplates();
					LoadImailTemplates();
					foreach (CollectionDataModel model in nlNotificationsList) {
						NLSendNotifications(model);
					}
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy failed", null, null, ex.ToString(), ex.StackTrace);
			}

		} //Execute


		private void BuildCollectionDataModel(SafeReader sr) {
			string mobilePhone = sr["MobilePhone"];
			var model = new CollectionDataModel {
				CustomerID = sr["CustomerID"],
				OriginID = sr["OriginID"],
				LoanRefNum = sr["LoanRefNum"],
				FirstName = sr["FirstName"],
				FullName = sr["FullName"],
				Email = sr["email"],
				PhoneNumber = string.IsNullOrEmpty(mobilePhone) ? sr["DaytimePhone"] : mobilePhone,
				DueDate = sr["ScheduleDate"],
				LoanID = sr["OldLoanID"],
				ScheduleID = 0,
				NLLoanID = sr["LoanID"],
				LoanHistoryID = sr["LoanHistoryID"],
				NLScheduleID = sr["ScheduleID"],
				SmsSendingAllowed = sr["SmsSendingAllowed"],
				EmailSendingAllowed = sr["EmailSendingAllowed"],
				ImailSendingAllowed = sr["MailSendingAllowed"]
			};

			model.LateDays = (int)(now - model.DueDate).TotalDays;

			GetLoanState loanState = new GetLoanState(model.CustomerID, model.NLLoanID, now);
			loanState.Execute();
			List<NL_LoanSchedules> schedules = new List<NL_LoanSchedules>();
			loanState.Result.Loan.Histories.ForEach(h => schedules.AddRange(h.Schedule));
			var scheduleItem = schedules.FirstOrDefault(s => s.LoanScheduleID == model.NLScheduleID);
			model.Interest = loanState.Result.Interest;

			if (scheduleItem != null) {
				model.FeeAmount = scheduleItem.Fees;
				model.AmountDue = scheduleItem.AmountDue;
			}
			nlNotificationsList.Add(model);

			Log.Info(model.ToString());
			NL_AddLog(LogType.Info, "CollectionDataModel", now, model, null, null);
		}

		private void NLSendNotifications(CollectionDataModel model) {
			// prevent for sending notification twice - for old and new loan to sane customer 
			if (!sendNotificationForNewLoan) {
				model.SmsSendingAllowed = false;
				model.ImailSendingAllowed = false;
				model.EmailSendingAllowed = false;
				model.UpdateCustomerAllowed = false;
			}

			CollectionType collectionType = GetCollectionType(model.LateDays);
			CollectionStatusNames collectionStatusName = GetCollectionStatusName(collectionType);
			string emailTemplate = GetCollectionEmailTemplateName(collectionType);

			bool isSendEmail = IsSendEmail(collectionType);
			bool isSendSMS = IsSendSMS(collectionType);
			bool isSendImail = IsSendImail(collectionType);
			bool isChangeStatusCall = IsChangeStatusCall(collectionType);

			NL_AddLog(LogType.Info, "NLSendNotifications", new object[] {
				model, collectionType, collectionStatusName, emailTemplate,isSendEmail, isSendImail, isSendImail, isChangeStatusCall
			}, null, null, null);

			if (isChangeStatusCall) {
				ChangeStatus(model.CustomerID, model.LoanID, collectionStatusName, collectionType, model);
			}
			if (isSendEmail) {
				SendCollectionEmail(emailTemplate, model, collectionType);
			}
			if (isSendSMS) {
				SendCollectionSms(model, collectionType);
			}
			if (isSendImail) {
				SendCollectionImail(model, collectionType);
			}
		}

		private string GetCollectionEmailTemplateName(CollectionType collectionType) {
			switch (collectionType) {
			case CollectionType.CollectionDay0:
				return CollectionDay0EmailTemplate;
			case CollectionType.CollectionDay15:
				return CollectionDay15EmailTemplate;
			case CollectionType.CollectionDay1to6:
				return CollectionDay1to6EmailTemplate;
			case CollectionType.CollectionDay31:
				return CollectionDay31EmailTemplate;
			case CollectionType.CollectionDay7:
				return CollectionDay7EmailTemplate;
			case CollectionType.CollectionDay8to14:
				return CollectionDay8to14EmailTemplate;
			}
			return String.Empty;
		}

		private CollectionStatusNames GetCollectionStatusName(CollectionType collectionType) {
			switch (collectionType) {
			case CollectionType.CollectionDay1to6:
				return CollectionStatusNames.DaysMissed1To14;
			case CollectionType.CollectionDay15:
				return CollectionStatusNames.DaysMissed15To30;
			case CollectionType.CollectionDay31:
				return CollectionStatusNames.DaysMissed31To45;
			case CollectionType.CollectionDay46:
				return CollectionStatusNames.DaysMissed46To60;
			case CollectionType.CollectionDay60:
				return CollectionStatusNames.DaysMissed61To90;
			case CollectionType.CollectionDay90:
				return CollectionStatusNames.DaysMissed90Plus;
			}
			return CollectionStatusNames.Default;
		}

		private CollectionType GetCollectionType(int lateDays) {
			if (lateDays == 0)
				return CollectionType.CollectionDay0;
			if (lateDays >= 1 && lateDays <= 6)
				return CollectionType.CollectionDay1to6;
			if (lateDays == 7)
				return CollectionType.CollectionDay7;
			if (lateDays >= 8 && lateDays <= 14)
				return CollectionType.CollectionDay8to14;
			if (lateDays == 15)
				return CollectionType.CollectionDay15;
			if (lateDays == 21)
				return CollectionType.CollectionDay21;
			if (lateDays == 31)
				return CollectionType.CollectionDay31;
			if (lateDays == 46)
				return CollectionType.CollectionDay46;
			if (lateDays == 60)
				return CollectionType.CollectionDay60;
			if (lateDays == 90)
				return CollectionType.CollectionDay90;
			return CollectionType.Cured;
		}

		private bool IsSendEmail(CollectionType collectionType) {
			return new[] {
                CollectionType.CollectionDay0,
                CollectionType.CollectionDay1to6,
                CollectionType.CollectionDay7,
                CollectionType.CollectionDay8to14,
                CollectionType.CollectionDay15,
                CollectionType.CollectionDay31
            }.Contains(collectionType);
		}

		private bool IsSendSMS(CollectionType collectionType) {
			return new[] {
                CollectionType.CollectionDay0,
                CollectionType.CollectionDay1to6,
                CollectionType.CollectionDay7,
                CollectionType.CollectionDay8to14,
                CollectionType.CollectionDay15,
                CollectionType.CollectionDay21,
                CollectionType.CollectionDay31,
                CollectionType.CollectionDay46
            }.Contains(collectionType);
		}

		private bool IsSendImail(CollectionType collectionType) {
			return new[] {
                CollectionType.CollectionDay7,
                CollectionType.CollectionDay15,
                CollectionType.CollectionDay31
            }.Contains(collectionType);
		}

		private bool IsChangeStatusCall(CollectionType collectionType) {
			return new[] {
				CollectionType.CollectionDay15,
				CollectionType.CollectionDay1to6,
				CollectionType.CollectionDay31,
				CollectionType.CollectionDay46,
				CollectionType.CollectionDay60,
				CollectionType.CollectionDay90,
				CollectionType.Cured
            }.Contains(collectionType);
		}
	}
} // namespace
