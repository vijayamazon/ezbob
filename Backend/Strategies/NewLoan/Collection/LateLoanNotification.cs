namespace Ezbob.Backend.Strategies.NewLoan.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using ConfigManager;
    using DbConstants;
    using Ezbob.Backend.CalculateLoan.LoanCalculator;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Backend.Strategies.MailStrategies;
    using Ezbob.Backend.Strategies.Misc;
    using Ezbob.Database;
    using EZBob.DatabaseLib.Model.Database.Loans;
    using IMailLib;
    using PaymentServices.Calculators;
    using StructureMap;

    /// <summary>
    /// Late Loan Notification
    /// </summary>
    public class LateLoanNotification : AStrategy
    {
        public LateLoanNotification()
        {
            this.now = DateTime.UtcNow;
            this.collectionIMailer = new CollectionMail(
                ConfigManager.CurrentValues.Instance.ImailUserName,
                ConfigManager.CurrentValues.Instance.IMailPassword,
                ConfigManager.CurrentValues.Instance.IMailDebugModeEnabled,
                ConfigManager.CurrentValues.Instance.IMailDebugModeEmail,
                ConfigManager.CurrentValues.Instance.IMailSavePath);
        } // constructor

        private readonly DateTime now;

        public override string Name { get { return "Late Loan Notification"; } }
        private readonly IMailLib.CollectionMail collectionIMailer;
        private List<CollectionSmsTemplate> smsTemplates;
        private const string CollectionDay8to14EmailTemplate = "Mandrill - Last warning - Debt recovery";
        private const string CollectionDay7EmailTemplate = "Mandrill - 20p late fee";
        private const string CollectionDay31EmailTemplate = "Mandrill - legal process starting";
        private const string CollectionDay1to6EmailTemplate = "Mandrill - missed payment";
        private const string CollectionDay15EmailTemplate = "Mandrill - Warning notice- 40p late fee";
        private const string CollectionDay0EmailTemplate = "Mandrill - you missed your payment";

        public override void Execute()
        {
            try {
                LoadSmsTemplates();
                LoadImailTemplates();
                DB.ForEachRowSafe((sr, bRowsetStart) =>{
                    try{
                        HandleCollectionLogic(sr);
                    }
                    catch (Exception ex){
                        Log.Error(ex, "Failed to handle collection for customer {0}", sr["CustomerID"]);
                    }

                    return ActionResult.Continue;
                }, "NL_LateLoansNotificationGet",
                    CommandSpecies.StoredProcedure, new QueryParameter("Now", this.now));
            } catch (Exception ex) {
                NL_AddLog(LogType.Error, "Strategy Faild", null, null, ex.ToString(), ex.StackTrace);
            }
            
        } //Execute
        private void LoadSmsTemplates()
        {
            this.smsTemplates = DB.Fill<CollectionSmsTemplate>("LoadCollectionSmsTemplates", CommandSpecies.StoredProcedure);
        } //LoadSmsTemplates        
        private void LoadImailTemplates()
        {
            List<CollectionSnailMailTemplate> templates = DB.Fill<CollectionSnailMailTemplate>("LoadCollectionSnailMailTemplates", CommandSpecies.StoredProcedure);
            this.collectionIMailer.SetTemplates(templates.Select(x => new SnailMailTemplate
            {
                ID = x.CollectionSnailMailTemplateID,
                Type = x.Type,
                OriginID = x.OriginID,
                Template = x.Template,
                IsActive = x.IsActive,
                TemplateName = x.TemplateName,
                FileName = x.FileName,
                IsLimited = x.IsLimited
            }));
        } //LoadImailTemplates
        private void HandleCollectionLogic(SafeReader sr)
        {
            DateTime scheduleDate = sr["ScheduleDate"];
            int loanId = sr["LoanID"];
            string dayPhone = sr["DaytimePhone"];
            string mobilePhone = sr["MobilePhone"];
            int lateDays = (int)(this.now - scheduleDate).TotalDays;

            var model = new CollectionDataModel{
                CustomerID = sr["CustomerID"],
                OriginID = sr["OriginID"],
                LoanID = loanId,
                LoanHistoryID = sr["LoanHistoryID"],
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
            HandleCollection(model, lateDays);
            
            //TODO those fields are not included in NL scheme need to provide alternative for BI
            //UpdateLoanStats(loanId, lateDays, model.AmountDue);
        
        } //HandleCollectionLogic
        private void HandleCollection(CollectionDataModel model, int lateDays)
        {
            CollectionType collectionType = GetCollectionType(lateDays);
            CollectionStatusNames collectionStatusName = GetCollectionStatusName(collectionType);
            string emailTemplate = GetCollectionEmailTemplateName(collectionType);

            bool isSendEmail = IsSendEmail(collectionType);
            bool isSendSMS = IsSendSMS(collectionType);
            bool isSendImail = IsSendImail(collectionType);

            if (collectionStatusName != CollectionStatusNames.Default){
                var collectionChangeStatus = new LateCustomerStatusChange(model.CustomerID, model.LoanID, collectionStatusName, collectionType,this.now);
                collectionChangeStatus.Execute();
                new AddCollectionLog(new CollectionLog(){
                    LoanID = model.LoanID,
                    TimeStamp = this.now,
                    Type = collectionChangeStatus.Type.ToString(),
                    CustomerID = model.CustomerID,
                    LoanHistoryID = model.LoanHistoryID,
                    Comments = string.Empty,
                    Method = CollectionMethod.ChangeStatus.ToString()
                }).Execute();
            }
            if (isSendEmail){
                SendCollectionEmail(emailTemplate, model, collectionType);
            }
            if (isSendSMS){
                SendCollectionSms(model, collectionType);
            }
            if (isSendImail){
                SendCollectionImail(model, collectionType);
            }
        } //HandleCollection
        private string GetCollectionEmailTemplateName(CollectionType collectionType){
            switch (collectionType){
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
        } //GetCollectionEmailTemplateName
        private CollectionStatusNames GetCollectionStatusName(CollectionType collectionType){
            switch (collectionType){
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
        private CollectionType GetCollectionType(int lateDays){
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
        } //GetCollectionType
        private bool IsSendEmail(CollectionType collectionType)
        {
            return new[] {
                CollectionType.CollectionDay0,
                CollectionType.CollectionDay1to6,
                CollectionType.CollectionDay7,
                CollectionType.CollectionDay8to14,
                CollectionType.CollectionDay15,
                CollectionType.CollectionDay31
            }.Contains(collectionType);
        } //IsSendEmail
        private bool IsSendSMS(CollectionType collectionType)
        {
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
        } //IsSendSMS
        private bool IsSendImail(CollectionType collectionType)
        {
            return new[] {
                CollectionType.CollectionDay7,
                CollectionType.CollectionDay15,
                CollectionType.CollectionDay31
            }.Contains(collectionType);       
        } //IsSendImail
        private void SaveCollectionSnailMailMetadata(int collectionLogID, FileMetadata fileMetadata)
        {
            if (fileMetadata == null)
            {
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
        } //SaveCollectionSnailMailMetadata     
        private void SendCollectionEmail(string emailTemplateName, CollectionDataModel model, CollectionType type)
        {
            if (model.EmailSendingAllowed){
                var variables = new Dictionary<string, string> {
                    {
                        "FirstName", model.FirstName
                    }, {
                        "RefNum", model.LoanRefNum
                    }, {
                        "FeeAmount", model.FeeAmount.ToString(CultureInfo.InvariantCulture)
                    }, {
                        "AmountCharged", model.AmountDue.ToString(CultureInfo.InvariantCulture)
                    }, {
                        "ScheduledAmount", model.AmountDue.ToString(CultureInfo.InvariantCulture)
                    },
                };
                CollectionMails collectionMails = new CollectionMails(model.CustomerID, emailTemplateName, variables);
                collectionMails.Execute();

                new AddCollectionLog(new CollectionLog(){
                    LoanID = model.LoanID,
                    TimeStamp = this.now,
                    Type = type.ToString(),
                    CustomerID = model.CustomerID,
                    LoanHistoryID = model.LoanHistoryID,
                    Comments = string.Empty,
                    Method = CollectionMethod.Email.ToString()
                }).Execute();
            }
            else{
                Log.Info("Collection sending email is not allowed, email is not sent to customer {0}\n email template {1}", model.CustomerID, emailTemplateName);
            }
        } //SendCollectionEmail
        private void SendCollectionImail(CollectionDataModel model, CollectionType type)
        {
            if (model.ImailSendingAllowed)
            {
                try {
                    IMailLib.CollectionMailModel mailModel = GetCollectionMailModel(model);
                    if (mailModel != null) {
                        AddCollectionLog collectionLog;
                        switch (type) {
                        case CollectionType.CollectionDay7:
                            if (mailModel.IsLimited) {
                                Log.Info("Sending imail {0} to customer {1}", model.CustomerID, type);
                                FileMetadata personal;
                                FileMetadata business;
                                this.collectionIMailer.SendDefaultTemplateComm7(mailModel, out personal, out business);

                                collectionLog = new AddCollectionLog(new CollectionLog() {
                                    LoanID = model.LoanID,
                                    TimeStamp = this.now,
                                    Type = type.ToString(),
                                    CustomerID = model.CustomerID,
                                    LoanHistoryID = model.LoanHistoryID,
                                    Comments = string.Empty,
                                    Method = CollectionMethod.Mail.ToString()
                                });
                                collectionLog.Execute();
                                int collection7LogID = collectionLog.CollectionLog.CollectionLogID;

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

                            collectionLog = new AddCollectionLog(new CollectionLog() {
                                LoanID = model.LoanID,
                                TimeStamp = this.now,
                                Type = type.ToString(),
                                CustomerID = model.CustomerID,
                                LoanHistoryID = model.LoanHistoryID,
                                Comments = string.Empty,
                                Method = CollectionMethod.Mail.ToString()
                            });
                            collectionLog.Execute();
                            int collection15LogID = collectionLog.CollectionLog.CollectionLogID;

                            SaveCollectionSnailMailMetadata(collection15LogID, day15Metadata);
                            break;
                        case CollectionType.CollectionDay31:
                            if (!mailModel.IsLimited) {
                                Log.Info("Sending imail {0} to customer {1}", model.CustomerID, type);
                                FileMetadata consumer = this.collectionIMailer.SendDefaultTemplateConsumer31(mailModel);

                                collectionLog = new AddCollectionLog(new CollectionLog() {
                                    LoanID = model.LoanID,
                                    TimeStamp = this.now,
                                    Type = type.ToString(),
                                    CustomerID = model.CustomerID,
                                    LoanHistoryID = model.LoanHistoryID,
                                    Comments = string.Empty,
                                    Method = CollectionMethod.Mail.ToString()
                                });
                                collectionLog.Execute();
                                int collection31LogID = collectionLog.CollectionLog.CollectionLogID;

                                SaveCollectionSnailMailMetadata(collection31LogID, consumer);
                            }
                            break;
                        }
                    } else {
                        Log.Error(null, "Sending Imail failed for customer {0} mail data is missing", model.CustomerID);
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex, "Sending Imail failed for customer {0}", model.CustomerID);
                }
            }
            else
            {
                Log.Info("Collection sending mail is not allowed, mail is not sent to customer {0}\n template {1}", model.CustomerID, type);
            }
        } //SendCollectionImail
        private CollectionMailModel GetCollectionMailModel(CollectionDataModel model)
        {
            SafeReader sr = DB.GetFirst("NL_LateLoanMailDataGet", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", model.CustomerID), new QueryParameter("LoanID", model.LoanID));

			//NL_Model nlModel = new NL_Model(model.CustomerID){Loan = new NL_Loans()};

			GetLoanState strategy = new GetLoanState(model.CustomerID, model.LoanID, DateTime.UtcNow);
            strategy.Execute();

			//nlModel = strategy.Result;

			//ALoanCalculator calc = new LegacyLoanCalculator(nlModel);
			//calc.GetState();

			var outstandingPrincipal = strategy.Result.Principal;

			var firstMissedSchedule = strategy.Result.Loan.LastHistory().Schedule.FirstOrDefault(x => x.LoanScheduleStatusID == (int)NLScheduleStatuses.Late);

            if (firstMissedSchedule == null)
                return null;

			var secondMissedSchedule = strategy.Result.Loan.LastHistory().Schedule.Where(x => x.LoanScheduleStatusID == (int)NLScheduleStatuses.Late).Skip(1).First();


            var mailModel = new CollectionMailModel{                

                CustomerName = model.FullName,
                CustomerAddress = new Address{
                    Line1 = sr["CAddress1"],
                    Line2 = sr["CAddress2"],
                    Line3 = sr["CAddress3"],
                    Line4 = sr["CAddress4"],
                    Postcode = sr["CPostcode"]
                },
                CompanyAddress = new Address{
                    Line1 = sr["BAddress1"],
                    Line2 = sr["BAddress2"],
                    Line3 = sr["BAddress3"],
                    Line4 = sr["BAddress4"],
                    Postcode = sr["BPostcode"],
                },
                GuarantorAddress = new Address{ //TODO implement
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
                OutstandingPrincipal = outstandingPrincipal,
                CustomerId = model.CustomerID,
                OriginId = model.OriginID,

            };


            mailModel.MissedPayment = new MissedPaymentModel {
                AmountDue = firstMissedSchedule.AmountDue,
                DateDue = firstMissedSchedule.PlannedDate,
                Fees = firstMissedSchedule.Fees,
                RepaidAmount = firstMissedSchedule.OpenPrincipal,
                RepaidDate = null //TODO retrieve the real value
            };

            if (secondMissedSchedule != null) {
                mailModel.PreviousMissedPayment = new MissedPaymentModel {
                    AmountDue = secondMissedSchedule.AmountDue,
                    DateDue = secondMissedSchedule.PlannedDate,
                    Fees = secondMissedSchedule.Fees,
                    RepaidAmount = secondMissedSchedule.OpenPrincipal,
                    RepaidDate = null //TODO retrieve the real value
                };
            }

            var loanRepository = ObjectFactory.GetInstance<LoanRepository>();
            Loan loan = loanRepository.Get(model.LoanID);
            var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan, this.now, CurrentValues.Instance.AmountToChargeFrom);
            var balance = payEarlyCalc.TotalEarlyPayment();
            mailModel.OutstandingBalance = balance;

            return mailModel;
        } //GetCollectionMailModel
        private void SendCollectionSms(CollectionDataModel model, CollectionType type)
        {
            var smsModel = this.smsTemplates.FirstOrDefault(x => x.IsActive && x.OriginID == model.OriginID && x.Type == type.ToString());
            if (smsModel == null){
                Log.Info("Collection not sending sms, sms template is not found. customer {0} origin {1} type {2}",model.CustomerID, model.OriginID, type);
                return;
            }

            var smsTemplate = string.Format(smsModel.Template,
                model.FirstName,
                FormattingUtils.NumericFormats(model.AmountDue),
                FormattingUtils.FormatDateToString(model.DueDate));

            if (model.SmsSendingAllowed && !ConfigManager.CurrentValues.Instance.SmsTestModeEnabled){

                Log.Info("Collection sending sms to customer {0} phone number {1}\n content {2}",model.CustomerID, model.PhoneNumber, smsTemplate);

                new SendSms(model.CustomerID, 1, model.PhoneNumber, smsTemplate).Execute();

                var collectionLog = new AddCollectionLog(new CollectionLog(){
                    LoanID = model.LoanID,
                    TimeStamp = this.now,
                    Type = type.ToString() ,
                    CustomerID = model.CustomerID,
                    LoanHistoryID = model.LoanHistoryID,
                    Comments = string.Empty,
                    Method = CollectionMethod.Sms.ToString()
                });
                collectionLog.Execute();
            } else if (model.SmsSendingAllowed){
                Log.Info("Collection sending sms is in test mode, sms is not sent to customer {0} phone number {1}\n content {2}", model.CustomerID, model.PhoneNumber, smsTemplate);

                var collectionLog = new AddCollectionLog(new CollectionLog(){
                    LoanID = model.LoanID,
                    TimeStamp = this.now,
                    Type =type.ToString(),
                    CustomerID = model.CustomerID,
                    LoanHistoryID = model.LoanHistoryID,
                    Comments = string.Empty,
                    Method =  CollectionMethod.Sms.ToString()
                });
                collectionLog.Execute();
            }
            else{
                Log.Info("Collection sending sms is not allowed, sms is not sent to customer {0} phone number {1}\n content {2}",
                    model.CustomerID, model.PhoneNumber, smsTemplate);
            }
        } //SendCollectionSms

        //private void UpdateLoanStats(int loanId, int daysBetween, decimal amountDue)
        //{
        //    //TODO This is wrong logic, there was a bug that corrupted all the values in loan table need to be backfilled and fixed
        //    var model = new LoanStatsModel();
        //    if (daysBetween > 0 && daysBetween <= 30)
        //    {
        //        model.Late30Num++;
        //        model.Late30 += amountDue;
        //    }
        //    else if (daysBetween > 30 && daysBetween <= 60)
        //    {
        //        model.Late60Num++;
        //        model.Late60 += amountDue;
        //    }
        //    else if (daysBetween > 60 && daysBetween <= 90)
        //    {
        //        model.Late90Num++;
        //        model.Late90 += amountDue;
        //    }
        //    else
        //    { // daysBetween > 90
        //        model.Late90PlusNum++;
        //        model.Late90Plus += amountDue;
        //    } // if

        //    model.PastDues += amountDue;
        //    model.PastDuesNum++;

        //    DB.ExecuteNonQuery(
        //        "UpdateCollection",
        //        CommandSpecies.StoredProcedure,
        //        new QueryParameter("LoanId", loanId),
        //        new QueryParameter("Late30", model.Late30),
        //        new QueryParameter("Late30Num", model.Late30Num),
        //        new QueryParameter("Late60", model.Late60),
        //        new QueryParameter("Late60Num", model.Late60Num),
        //        new QueryParameter("Late90", model.Late90),
        //        new QueryParameter("Late90Num", model.Late90Num),
        //        new QueryParameter("PastDues", model.PastDues),
        //        new QueryParameter("PastDuesNum", model.PastDuesNum),
        //        new QueryParameter("IsDefaulted", 0),
        //        new QueryParameter("Late90Plus", model.Late90Plus),
        //        new QueryParameter("Late90PlusNum", model.Late90PlusNum)
        //        );

        //    //TODO save loan statistics to new table
        //    Log.Info("add new late fee and mark loan as late for loanId {0}", loanId);
        //} // UpdateLoanStats
    }
} // namespace
