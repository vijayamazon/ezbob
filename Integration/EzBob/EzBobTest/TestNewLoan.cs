namespace EzBobTest {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan;
	using NHibernate.Linq;
	using NUnit.Framework;

	[TestFixture]
	class TestNewLoan : BaseTestFixtue {


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
			NL_LoanOptions NL_options = new NL_LoanOptions {
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

		[Test]
		public void TestNL_CalculateLoanSchedule() {

			List<NL_LoanHistory> histories = new List<NL_LoanHistory>();
			histories.Add(new NL_LoanHistory() { EventTime = DateTime.UtcNow });

			NL_Model model = new NL_Model(47) {
				//CustomerID = 47,
				UserID = 25852,
				CalculatorImplementation = typeof(BankLikeLoanCalculator).AssemblyQualifiedName,
				Histories = histories
			};

			CalculateLoanSchedule strategy = new CalculateLoanSchedule(model);
			strategy.Context.UserID = model.UserID;
			try {

				strategy.Execute();
				NL_Model result = strategy.Result;

				this.m_oLog.Debug(result.Error);

				result.Schedule.OfType<NL_LoanSchedules>().ForEach(s => this.m_oLog.Debug(s));
				result.Fees.OfType<NL_LoanFees>().ForEach(s => this.m_oLog.Debug(s));
				Console.WriteLine(result.APR);

			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}


		[Test]
		public void AddLoan() {

			NL_Model model = new NL_Model(47) {
				UserID = 25852,
				CalculatorImplementation = typeof(BankLikeLoanCalculator).AssemblyQualifiedName,
				Loan = new NL_Loans() { OldLoanID = 44, Refnum = "111111" }
			};

			model.Agreements.Add(new NLAgreementItem() {
				TemplateModel = new TemplateModel() { Template = "aa{A}bb{B}" },
				Agreement = new NL_LoanAgreements() {
					LoanAgreementTemplateID = (int)NLLoanAgreementTemplateTypes.PreContractAgreement,
					FilePath = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, "preContract/")
				}
			});

			model.Agreements.Add(new NLAgreementItem() {
				TemplateModel = new TemplateModel() { Template = "xx{X}yy{Y}" },
				Agreement = new NL_LoanAgreements() {
					LoanAgreementTemplateID = (int)NLLoanAgreementTemplateTypes.GuarantyAgreement,
					FilePath = Path.Combine(CurrentValues.Instance.AgreementPdfLoanPath1, "guarantyAgreement/")
				},
			});

			model.Histories.Add(new NL_LoanHistory() { EventTime = DateTime.UtcNow });

			AddLoan strategy = new AddLoan(model);
			strategy.Context.UserID = model.UserID;
			try {
				strategy.Execute();
				this.m_oLog.Debug(strategy.Error);
				this.m_oLog.Debug(strategy.LoanID);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}


	} // class TestNewLoan
} // namespace