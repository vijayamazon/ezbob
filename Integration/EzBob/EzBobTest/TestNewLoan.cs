namespace EzBobTest {
	using System;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan;
	using NUnit.Framework;

	[TestFixture]
	class TestNewLoan :BaseTestFixtue {


		[Test]
		public void TestLoader() {
			//NL_Loans l = new NL_Loans();
			//l.InterestRate = 3.5m;
			//l.IssuedTime = DateTime.UtcNow;
			//l.CreationTime = DateTime.UtcNow;
			//l.LoanSourceID = 1;
			//l.LoanStatusID = 2;
			//l.Position = 1;
			//var sss = l.ToString();
			//Console.WriteLine(sss);

			NL_LoanLegals loanLegals = new NL_LoanLegals {
				Amount = 20000m,
				RepaymentPeriod = 5,
				SignatureTime = DateTime.UtcNow,
				EUAgreementAgreed = true,
				COSMEAgreementAgreed = true,
				CreditActAgreementAgreed = true,
				PreContractAgreementAgreed = false,
				PrivateCompanyLoanAgreementAgreed = true,
				GuarantyAgreementAgreed = false,
				SignedName = "elina roytman",
				NotInBankruptcy = false
			};
			var s = loanLegals.ToString();
			Console.WriteLine(s);
		}


		[Test]
		public void TestRepaymentIntervalTypes() {
			Console.WriteLine(Enum.ToObject(typeof(RepaymentIntervalTypesId), 1));
		}

	    [Test]
	    public void TestLoanOptions1() {
            NL_LoanOptions NL_options = new NL_LoanOptions
            {
                LoanID = 123,
                AutoCharge = true,
                StopAutoChargeDate = DateTime.UtcNow,
                AutoLateFees = true,
                StopAutoLateFeesDate = DateTime.UtcNow,
                AutoInterest = true,
                StopAutoInterestDate = DateTime.UtcNow,
                ReductionFee = true,
                LatePaymentNotification = true,
                CaisAccountStatus = "asd",
                ManualCaisFlag = "qwe",
                EmailSendingAllowed = true,
                SmsSendingAllowed = true,
                MailSendingAllowed = true,
                UserID = 25,
                InsertDate = DateTime.UtcNow,
                IsActive = true,
                Notes = null
            };
            //var stra = new AddLoanOptions(NL_options, null);
            //stra.Execute();
	    }



	} // class TestNewLoan
} // namespace