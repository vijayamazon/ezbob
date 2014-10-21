namespace EzBob.Backend.Strategies.Misc 
{
	using MailStrategies.API;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.PayPoint;

	public class AutoPaymentResult 
	{
		public decimal ActualAmountCharged { get; set; }
		public bool PaymentFailed { get; set; }
		public bool PaymentCollectedSuccessfully { get; set; }
		public bool IsException { get; set; }
	}

	public class PayPointCharger : AStrategy
	{
		private readonly int amountToChargeFrom;
		private readonly StrategiesMailer mailer;
		private readonly PayPointApi payPointApi = new PayPointApi();

		public PayPointCharger(AConnection db, ASafeLog oLog) : base(db, oLog)
		{
			mailer = new StrategiesMailer(DB, Log);

			SafeReader sr = DB.GetFirst("PayPointChargerGetConfigs", CommandSpecies.StoredProcedure);
			amountToChargeFrom = sr["AmountToChargeFrom"];
		}

		public override void Execute()
		{
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => 
				{
					HandleOnePayment(sr);
					return ActionResult.Continue;
				},
				"GetCustomersForPayPoint",
				CommandSpecies.StoredProcedure
			);
		}

		public override string Name { get { return "PayPoint Charger"; } }

		private void HandleOnePayment(SafeReader sr)
		{
			int loanScheduleId = sr["LoanScheduleId"];
			int loanId = sr["LoanId"];
			string firstName = sr["FirstName"];
			int customerId = sr["CustomerId"];
			string customerMail = sr["Email"];
			string fullname = sr["Fullname"];
			bool reductionFee = sr["ReductionFee"];
			string refNum = sr["RefNum"];
			bool lastInstallment = sr["LastInstallment"];

			decimal amountDue = payPointApi.GetAmountToPay(loanScheduleId);

			if (!ShouldCharge(lastInstallment, amountDue))
			{
				Log.Info("Will not charge loan schedule id {0} (amount {1}): the minimal amount for collection is {2}.",
				         loanScheduleId, amountDue, amountToChargeFrom);
				return;
			}

			decimal initialAmountDue = amountDue;

			AutoPaymentResult autoPaymentResult = TryToMakeAutoPayment(
				loanScheduleId,
				initialAmountDue,
				customerId,
				customerMail,
				fullname,
				reductionFee
				);

			if (autoPaymentResult.IsException)
			{
				return;
			}

			if (autoPaymentResult.PaymentFailed)
			{
				SendFailureMail(firstName, initialAmountDue, customerMail);
				return;
			} // if

			if (autoPaymentResult.PaymentCollectedSuccessfully)
			{
				SendConfirmationMail(firstName, amountDue, refNum, customerMail);
				SendLoanStatusMail(loanId, firstName, refNum, customerMail);
			}
		}

		private AutoPaymentResult TryToMakeAutoPayment(int loanScheduleId, decimal initialAmountDue, int customerId, string customerMail, string fullname, bool reductionFee)
		{
			var result = new AutoPaymentResult();
			decimal actualAmountCharged = initialAmountDue;

			int counter = 0;

			while (counter <= 3)
			{
				PayPointReturnData payPointReturnData;

				if (MakeAutoPayment(loanScheduleId, actualAmountCharged, out payPointReturnData))
				{
					if (IsNotEnoughMoney(payPointReturnData))
					{
						if (!reductionFee)
						{
							result.PaymentFailed = true;
							return result;
						}

						counter++;

						if (counter > 3)
						{
							result.PaymentFailed = true;
							return result;
						}

						if (counter == 1)
						{
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.75), 2);
							Log.Info("Trying to charge 75% (Attempt #{0}). Customer:{1} Original amount:{2} Calculated amount:{3}", counter + 1, customerId, initialAmountDue, actualAmountCharged);
						}
						else if (counter == 2)
						{
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.5), 2);
							Log.Info("Trying to charge 50% (Attempt #{0}). Customer:{1} Original amount:{2} Calculated amount:{3}", counter + 1, customerId, initialAmountDue, actualAmountCharged);
						}
						else if (counter == 3)
						{
							actualAmountCharged = Math.Round((initialAmountDue * (decimal)0.25), 2);
							Log.Info("Trying to charge 25% (Attempt #{0}). Customer:{1} Original amount:{2} Calculated amount:{3}", counter + 1, customerId, initialAmountDue, actualAmountCharged);
						}
					}
					else if (IsCollectionSuccessful(payPointReturnData))
					{
						result.PaymentCollectedSuccessfully = true;
						result.ActualAmountCharged = actualAmountCharged;
						return result;
					}
					else
					{
						result.PaymentFailed = true;
						return result;
					}
				}
				else
				{
					SendExceptionMail(initialAmountDue, customerId, customerMail, fullname);
					result.IsException = true;
					return result;
				}
			}

			return result;
		}

		private void SendConfirmationMail(string firstName, decimal amountDue, string refNum, string customerMail)
		{
			var variables = new Dictionary<string, string>
			{
				{"AMOUNT", amountDue.ToString(CultureInfo.InvariantCulture)},
				{"FirstName", firstName},
				{"DATE", FormattingUtils.FormatDateToString(DateTime.UtcNow)},
				{"RefNum", refNum}
			};

			mailer.Send("Mandrill - Repayment confirmation", variables, new Addressee(customerMail));
		}

		private void SendLoanStatusMail(int loanId, string firstName, string refNum, string customerMail)
		{
			SafeReader sr = DB.GetFirst(
				"GetLoanStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("LoanId", loanId)
			);

			string loanStatus = sr["Status"];

			if (loanStatus == "PaidOff")
			{
				var variables = new Dictionary<string, string>
					{
						{"FirstName", firstName},
						{"RefNum", refNum}
					};

				mailer.Send("Mandrill - Loan paid in full", variables, new Addressee(customerMail));
			}
		}

		private void SendFailureMail(string firstName, decimal initialAmountDue, string customerMail)
		{
			var variables = new Dictionary<string, string>
			{
				{"FirstName", firstName},
				{"AmountOwed", initialAmountDue.ToString(CultureInfo.InvariantCulture)},
				{"DueDate", FormattingUtils.FormatDateToString(DateTime.UtcNow)}
			};

			mailer.Send("Mandrill - Automatic Re-Payment has Failed", variables, new Addressee(customerMail));
		}

		private void SendExceptionMail(decimal initialAmountDue, int customerId, string customerMail, string fullName)
		{
			mailer.Send("Mandrill - PayPoint Script Exception", new Dictionary<string, string> {
				{"UserID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Email", customerMail},
				{"FullName", fullName},
				{"Amount", initialAmountDue.ToString(CultureInfo.InvariantCulture)}
			});
		}

		private bool ShouldCharge(bool lastInstallment, decimal amountDue)
		{
			return (lastInstallment && amountDue > 0) || amountDue >= amountToChargeFrom;
		}

		private bool MakeAutoPayment(int loanScheduleId, decimal amountDue, out PayPointReturnData result)
		{
			try
			{
				result = payPointApi.MakeAutomaticPayment(loanScheduleId, amountDue);
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Failed making auto payment for loan schedule id:{0} exception:{1}", loanScheduleId, ex);
				result = null;
				return false;
			}
		}

		private static bool IsCollectionSuccessful(PayPointReturnData payPointReturnData)
		{
			return payPointReturnData.Code == "A";
		}

		private static bool IsNotEnoughMoney(PayPointReturnData payPointReturnData)
		{
			return payPointReturnData.Code == "N" && payPointReturnData.Message == "INSUFF FUNDS";
		}
	}
}
