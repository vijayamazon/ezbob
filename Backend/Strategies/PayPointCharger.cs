namespace EzBob.Backend.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using PaymentServices.PayPoint;
	using Web.Code;
	using log4net;
	using DbConnection;

	public class PayPointCharger
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PayPointCharger));
		private readonly StrategiesMailer mailer = new StrategiesMailer();

		private readonly int customerId;
		private string Subject;

		public PayPointCharger(int customerId)
		{
			this.customerId = customerId;
			ReadConfigs();
		}

		private decimal SCRIPT_AmountDueNew;
		private string EmailSubject;
		private int collectionPeriod1;
		private int collectionPeriod1Id;
		private int collectionPeriod2;
		private int collectionPeriod2Id;
		private int collectionPeriod3;
		private int collectionPeriod3Id;
		private int latePaymentCharge;
		private int latePaymentChargeId;
		private int partialPaymentCharge;
		private int partialPaymentChargeId;
		private int administrationCharge;
		private int administrationChargeId;
		private int amountToChargeFrom;


		private void ReadConfigs()
		{
			DataTable configsDataTable = DbConnection.ExecuteSpReader("SetLateLoanStatusGetConfigs"); // TODO: rename file\sp name\ here and in set late loan strat
			DataRow configsResult = configsDataTable.Rows[0];

			collectionPeriod1 = int.Parse(configsResult["CollectionPeriod1"].ToString());
			collectionPeriod1Id = int.Parse(configsResult["CollectionPeriod1Id"].ToString());
			collectionPeriod2 = int.Parse(configsResult["CollectionPeriod2"].ToString());
			collectionPeriod2Id = int.Parse(configsResult["CollectionPeriod2Id"].ToString());
			collectionPeriod3 = int.Parse(configsResult["CollectionPeriod3"].ToString());
			collectionPeriod3Id = int.Parse(configsResult["CollectionPeriod3Id"].ToString());
			latePaymentCharge = int.Parse(configsResult["LatePaymentCharge"].ToString());
			latePaymentChargeId = int.Parse(configsResult["LatePaymentChargeId"].ToString());
			partialPaymentCharge = int.Parse(configsResult["PartialPaymentCharge"].ToString());
			partialPaymentChargeId = int.Parse(configsResult["PartialPaymentChargeId"].ToString());
			administrationCharge = int.Parse(configsResult["AdministrationCharge"].ToString());
			administrationChargeId = int.Parse(configsResult["AdministrationChargeId"].ToString());
			amountToChargeFrom = int.Parse(configsResult["AmountToChargeFrom"].ToString());
		}

		public void Execute()
		{
			EmailSubject = "Get PayPoint strategy error";

			DataTable dt = DbConnection.ExecuteSpReader("GetCustomersForPayPoint");
			foreach (DataRow row in dt.Rows)
			{
				HandleOneCustomer(row);
			}
		}

		private void HandleOneCustomer(DataRow row)
		{
			decimal amountDue = decimal.Parse(row["AmountDue"].ToString());
			int id = int.Parse(row["id"].ToString());
			string payPointTransactionId = row["PayPointTransactionId"].ToString();
			int loanId = int.Parse(row["LoanId"].ToString());
			decimal repaymentAmount = decimal.Parse(row["RepaymentAmount"].ToString());
			string firstName = row["FirstName"].ToString();
			string email = row["Email"].ToString();
			DateTime scheduledDate = DateTime.Parse(row["Date"].ToString());
			bool reductionFee = bool.Parse(row["ReductionFee"].ToString());
			int delinquency = int.Parse(row["Delinquency"].ToString());
			decimal interest = decimal.Parse(row["Interest"].ToString());
			decimal lateCharges = decimal.Parse(row["LateCharges"].ToString());
			bool? latePaymentNotification = null;
			bool tmp;
			if (bool.TryParse(row["LatePaymentNotification"].ToString(), out tmp))
			{
				latePaymentNotification = tmp;
			}
			bool stopSendingEmails = bool.Parse(row["StopSendingEmails"].ToString());
			string refNum = row["RefNum"].ToString();
			bool lastInstallment = bool.Parse(row["LastInstallment"].ToString());

			bool stopSendingEmails1 = stopSendingEmails;
			bool? latePaymentNotification1 = latePaymentNotification;


			var papi = new PayPointApi();
			decimal result = papi.GetAmountToPay(id);
			decimal SCRIPT_AmountDue = result;

			if (!(lastInstallment && SCRIPT_AmountDue > 0) && SCRIPT_AmountDue < amountToChargeFrom)
			{
				return;
			}

			decimal SCRIPT_AmountDue_Initial = SCRIPT_AmountDue;
			int Counter = 0;

			/*string ResultError;
			bool HasError;
			string Var2;
			string Var3;
			string Code = null;
			string CodeDescription;
			string NEW_PaypointId;
			DateTime NEW_PostDate;*/
			PayPointReturnData rr;

			int afterNode = 0; // 1= mail settings 
							   // 2= is first late period
							   // 3= Is Late?
			bool shouldGoToGetLoanStatus = false;
			bool shouldGoToMulti = false;
			string ScheduledDateScalarStr = FormattingUtils.FormatDateToString(scheduledDate);

			while (Counter <= 3) // can be only in 3 nodes after - remember which...
			{
				if (MakeAutoPayment(id, SCRIPT_AmountDue, out rr))
				{
					if (rr.Code == "P:A") // Not enough money
					{
						if (!reductionFee)
						{
							afterNode = 1;
							break;
						}

						Counter++;

						if (Counter > 3)
						{
							afterNode = 1;
							break;
						}

						if (Counter == 1)
						{
							SCRIPT_AmountDue = Math.Round((SCRIPT_AmountDue_Initial*(decimal) 0.75), 2);
						}
						else if (Counter == 2)
						{
							SCRIPT_AmountDue = Math.Round((SCRIPT_AmountDue_Initial*(decimal) 0.5), 2);
						}
						else if (Counter == 3)
						{
							SCRIPT_AmountDue = Math.Round((SCRIPT_AmountDue_Initial*(decimal) 0.25), 2);
						}
					}
					else if (rr.Code == "A")
					{
						afterNode = 3;
						break;
					}
					else
					{
						if ((DateTime.UtcNow - scheduledDate).TotalDays >= 1)
						{
							afterNode = 2;
							break;
						}
						
						afterNode = 1;
						break;
					}
				}
				else
				{
					// TODO: remove from sp all that is not used
					DataTable dt1 = DbConnection.ExecuteSpReader("GetPersonalDataForCollection",
					                                             DbConnection.CreateParam("LoanId", loanId));
					DataRow result1 = dt1.Rows[0];

					int App_CustomerId = int.Parse(result1["CustomerId"].ToString());
					string App_Email = result1["Email"].ToString();
					string App_Fullname = result1["Fullname"].ToString();

					var variables = new Dictionary<string, string>
						{
							{"UserID", App_CustomerId.ToString(CultureInfo.InvariantCulture)},
							{"Email", App_Email},
							{"FullName", App_Fullname},
							{"Amount", SCRIPT_AmountDue_Initial.ToString(CultureInfo.InvariantCulture)}
						};
					mailer.SendToEzbob(variables, "Mandrill - PayPoint Script Exception", "Script node crash");
					break;
				}
			}

			if (afterNode == 1)
			{
				var variables = new Dictionary<string, string>
						{
							{"FirstName", firstName},
							{"AmountOwed", SCRIPT_AmountDue_Initial.ToString(CultureInfo.InvariantCulture)},
							{"DueDate", FormattingUtils.FormatDateToString(DateTime.UtcNow)}
						};
				mailer.SendToCustomerAndEzbob(variables, email, "Mandrill - Automatic Re-Payment has Failed",
											  "Your Automatic Re-Payment has Failed");
			}
			else if (afterNode == 2)
			{
				if ((scheduledDate <= DateTime.UtcNow &&
				     scheduledDate.AddDays(collectionPeriod1) >= DateTime.UtcNow &&
				     (
						 !latePaymentNotification.HasValue || 
						 latePaymentNotification.Value
					 )
					) ||
					(scheduledDate.AddDays(collectionPeriod1 + 1) <= DateTime.UtcNow && 
					 scheduledDate.AddDays(collectionPeriod2) >= DateTime.UtcNow &&
					 (
						 !latePaymentNotification.HasValue ||
						 latePaymentNotification.Value
					 ))
					)
				{
					var variables = new Dictionary<string, string>
						{
							{"AmountCharged", SCRIPT_AmountDue_Initial.ToString(CultureInfo.InvariantCulture)},
							{"RefNum", refNum},
							{"FirstName", firstName}
						};
					mailer.SendToCustomerAndEzbob(variables, email, "Mandrill - Loan repayment late (1D late)", "Your re-payment is late");
				}
				else
				{
					Subject = "Customer is late for more then 14 days and would not receive this email";
				}

				// go to loan
				shouldGoToGetLoanStatus = true;
			}
			else if (afterNode == 3)
			{
				if ((DateTime.UtcNow - scheduledDate).TotalDays >= 1)
				{
					DataTable dt1 = DbConnection.ExecuteSpReader("GetScheduleInterest",
																 DbConnection.CreateParam("LoanSchedule", id));
					DataRow result1 = dt1.Rows[0];

					interest = decimal.Parse(result1["Interest"].ToString());

					try
					{
						var papi2 = new PayPointApi();
						var result2 = papi2.GetAmountToPay(id);
						SCRIPT_AmountDueNew = result2;
					}
					catch (Exception ex)
					{
						return;
					}

					if ((scheduledDate <= DateTime.UtcNow && 
						scheduledDate.AddDays(collectionPeriod1) >= DateTime.UtcNow && 
						interest != 0 && 
							(
								!latePaymentNotification.HasValue ||
								latePaymentNotification.Value
							)
						)
						||
						(scheduledDate.AddDays(collectionPeriod1 + 1) <= DateTime.UtcNow && 
						scheduledDate.AddDays(collectionPeriod2) >= DateTime.UtcNow && 
						interest != 0 &&
							(
								!latePaymentNotification.HasValue ||
								latePaymentNotification.Value
							)
						))
					{
						Subject = "Reminder : Your ezbob monthly re-payment is overdue";
						var variables = new Dictionary<string, string>
						{
							{"FirstName", firstName},
							{"AmountToCharge", SCRIPT_AmountDue_Initial.ToString(CultureInfo.InvariantCulture)},
							{"ChargeDate", ScheduledDateScalarStr}
						};
						mailer.SendToCustomerAndEzbob(variables, email, "Mandrill - Payment reminder", Subject);
						shouldGoToGetLoanStatus = true;
					}
					else if (
						(scheduledDate <= DateTime.UtcNow && 
						scheduledDate.AddDays(collectionPeriod1) >= DateTime.UtcNow && 
						SCRIPT_AmountDueNew != 0 &&
						interest == 0 &&
							(
								!latePaymentNotification.HasValue ||
								latePaymentNotification.Value
							)
						)
						|| 
						(scheduledDate.AddDays(collectionPeriod1 + 1) <= DateTime.UtcNow && 
						scheduledDate.AddDays(collectionPeriod2) >= DateTime.UtcNow && 
						SCRIPT_AmountDueNew != 0 && 
						interest == 0 &&
							(
								!latePaymentNotification.HasValue ||
								latePaymentNotification.Value
							)
						))
					{
						Subject = "ezbob received a partial re-payment from you";
						var variables = new Dictionary<string, string>
						{
							{"FirstName", firstName},
							{"AmountCharged", SCRIPT_AmountDue.ToString(CultureInfo.InvariantCulture)},
							{"ChargeDate", ScheduledDateScalarStr},
							{"InitialAmountDue", SCRIPT_AmountDue_Initial.ToString(CultureInfo.InvariantCulture)}
						};
						mailer.SendToCustomerAndEzbob(variables, email, "Mandrill - Partial repayment", Subject);
						shouldGoToGetLoanStatus = true;
					}
					else if (
						(scheduledDate <= DateTime.UtcNow && 
						scheduledDate.AddDays(collectionPeriod1) >= DateTime.UtcNow && 
						SCRIPT_AmountDueNew == 0 &&
						interest == 0 &&
							(
								!latePaymentNotification.HasValue ||
								latePaymentNotification.Value
							)
						)
						||
						(scheduledDate.AddDays(collectionPeriod1 + 1) <= DateTime.UtcNow && 
						scheduledDate.AddDays(collectionPeriod2) >= DateTime.UtcNow && 
						SCRIPT_AmountDueNew == 0 && 
						interest == 0 &&
							(
								!latePaymentNotification.HasValue ||
								latePaymentNotification.Value
							)
						))
					{
						Subject = "We received your re-payment in full";
						var variables = new Dictionary<string, string>
						{
							{"FirstName", firstName},
							{"AmountCharged", SCRIPT_AmountDue.ToString(CultureInfo.InvariantCulture)},
							{"ChargeDate", ScheduledDateScalarStr}
						};
						mailer.SendToCustomerAndEzbob(variables, email, "Mandrill - Received Full Repayment", Subject);
						shouldGoToGetLoanStatus = true;
					}
					else
					{
						shouldGoToMulti = true;
					}
				}
				else
				{
					shouldGoToMulti = true;
				}
			}

			if (shouldGoToMulti)
			{
				EmailSubject = "Dear " + firstName + ", your payment of £" + SCRIPT_AmountDue +
				               " has been credited to your ezbob account.";
				var variables = new Dictionary<string, string>
					{
						{"AMOUNT", SCRIPT_AmountDue.ToString(CultureInfo.InvariantCulture)},
						{"FirstName", firstName},
						{"DATE", FormattingUtils.FormatDateToString(DateTime.UtcNow)},
						{"RefNum", refNum}
					};
				mailer.SendToCustomerAndEzbob(variables, email, "Mandrill - Repayment confirmation", EmailSubject);
				shouldGoToGetLoanStatus = true;
			}

			if (shouldGoToGetLoanStatus)
			{
				DataTable dt1 = DbConnection.ExecuteSpReader("GetLoanStatus",
																DbConnection.CreateParam("LoanId", loanId));
				DataRow result1 = dt1.Rows[0];

				string LoanStatus = result1["Status"].ToString();
				if (LoanStatus == "PaidOff")
				{
					Subject = "You have paid your loan off in full.  Benefit from a lower interest cost on your next loan.";
					var variables = new Dictionary<string, string>
					{
						{"FirstName", firstName},
						{"RefNum", refNum}
					};
					mailer.SendToCustomerAndEzbob(variables, email, "Mandrill - Loan paid in full", Subject);
				}
			}
		}

		private bool MakeAutoPayment(int id, decimal amountDue, out PayPointReturnData result)
		{
			try
			{
				var papi = new PayPointApi();
				result = papi.MakeAutomaticPayment(id, amountDue);
				return true;
			}
			catch (Exception ex)
			{
				result = null;
				return false;
			}
		}
	}
}
