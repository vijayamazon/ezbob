﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobTest.OpenPlatformTests.Investor {
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.Strategies.Investor;
	using Ezbob.Backend.Strategies.Misc;
	using EzBobTest.OpenPlatformTests.Core;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using NUnit.Framework;
	using SalesForceLib;
	using StructureMap;

	[TestFixture]
	public class TestOpenPlatformStrategies : BaseTestFixtue {
		[SetUp]
		public new void Init() {
			base.Init();

			ObjectFactory.Configure(x => {
				x.For<IEzServiceAccessor>()
					.Use<EzServiceAccessorShort>();
				x.For<ILoanRepository>()
					.Use<LoanRepository>();
				x.For<ILoanScheduleRepository>()
					.Use<LoanScheduleRepository>();
				x.For<ILoanHistoryRepository>()
					.Use<LoanHistoryRepository>();
				x.For<ISalesForceAppClient>()
					.Use<FakeApiClient>();
				x.For<ILoanTransactionMethodRepository>()
					.Use<LoanTransactionMethodRepository>();

			});

			Ezbob.Backend.Strategies.Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
		} // Init

		public void TestCreateInvestor() {
			DateTime now = DateTime.UtcNow;

			var stra = new CreateInvestor(
				new InvestorModel {
					Name = "TestInvestor",
					IsActive = true,
					Timestamp = now,
					InvestorType = new InvestorTypeModel { InvestorTypeID = 1 }
				},
				new List<InvestorContactModel> {
					new InvestorContactModel {
						Timestamp = now, 
						IsActive = true, 
						Email = "stasd+investor1@ezbob.com",
						Comment = "comment", 
						IsPrimary = true,
						LastName = "last", 
						PersonalName = "personal", 
						Mobile = "011111111111", 
						OfficePhone = "02222222222", 
						Role = "Role"
					}
				},
				new List<InvestorBankAccountModel> {
					new InvestorBankAccountModel{
						IsActive = true,
						Timestamp = now,
						BankAccountName = "aname",
						BankAccountNumber = "123",
						BankBranchName = "bname",
						BankBranchNumber = "456",
						BankCode = "789",
						BankCountryID = "UK",
						BankName = "bank",
						AccountType = new InvestorAccountTypeModel{ InvestorAccountTypeID = 1 },
						InvestorBankAccountID = 1,
						RepaymentKey = "key"
					},
					new InvestorBankAccountModel{
						IsActive = true,
						Timestamp = now,
						BankAccountName = "aname",
						BankAccountNumber = "123",
						BankBranchName = "bname",
						BankBranchNumber = "456",
						BankCode = "789",
						BankCountryID = "UK",
						BankName = "bank",
						AccountType = new InvestorAccountTypeModel{ InvestorAccountTypeID = 1},
						InvestorBankAccountID = 1,
						RepaymentKey = "key"
					}
				});
			stra.Execute();

			Assert.IsTrue(stra.Result);
		}

		[Test]
		public void TestLoadInvestor() {
			var stra = new LoadInvestor(25);

			stra.Execute();
			Assert.IsNotNull(stra.Result);
			Assert.Greater(stra.Result.Contacts.Count, 0);
		}

		[Test]
		public void TestManageInvestor() {
			var stra = new ManageInvestorBankAccount(new InvestorBankAccountModel {
				IsActive = true,
				InvestorID = 10,
				Timestamp = DateTime.Today,
				InvestorBankAccountID = 4,
				AccountType = new InvestorAccountTypeModel { InvestorAccountTypeID = 2 },
				BankCode = "435",
				BankAccountNumber = "2345",
				BankAccountName = "sdggfds"
			});

			stra.Execute();
			Assert.IsTrue(stra.Result);
		}

		[Test]
		public void TestFindInvestor() {
			var stra = new FindInvestorForOffer(3449, 42814);
			stra.Execute();
			Assert.IsTrue(stra.IsFound);
		}

		[Test]
		public void TestLoadDecisionHistory() {
			var stra = new LoadDecisionHistory(3477);
			stra.Execute();
			Assert.Greater(stra.Result.Count(), 0);
		}

	}
}
