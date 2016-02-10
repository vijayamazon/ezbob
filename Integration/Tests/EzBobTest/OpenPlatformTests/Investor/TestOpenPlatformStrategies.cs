namespace EzBobTest.OpenPlatformTests.Investor {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Common;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Backend.Strategies.Investor;
	using Ezbob.Backend.Strategies.LegalDocs;
	using Ezbob.Backend.Strategies.LogicalGlue;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using EZBob.DatabaseLib.Model.Database;
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

        [Test]
		public void TestCreateInvestor() {
			DateTime now = DateTime.UtcNow;

			var stra = new CreateInvestor(1,
				new InvestorModel {
					Name = "TestInvestor",
					IsActive = true,
					Timestamp = now,
					InvestorType = new InvestorTypeModel { InvestorTypeID = (int)I_InvestorTypeEnum.Private }
				},
				new List<InvestorContactModel> {
					new InvestorContactModel {
						Timestamp = now, 
						IsActive = true, 
						Email = "stasd+investor16@ezbob.com",
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
						AccountType = new InvestorAccountTypeModel{ InvestorAccountTypeID = (int)I_InvestorAccountTypeEnum.Funding },
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
						AccountType = new InvestorAccountTypeModel{ InvestorAccountTypeID = (int)I_InvestorAccountTypeEnum.Repayments },
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
		/*	var stra = new ManageInvestorBankAccount(new InvestorBankAccountModel {
				IsActive = true,
				InvestorID = 10,
				Timestamp = DateTime.Today,
				InvestorBankAccountID = 4,
				AccountType = new InvestorAccountTypeModel { InvestorAccountTypeID = (int)I_InvestorAccountTypeEnum.Repayments },
				BankCode = "435",
				BankAccountNumber = "2345",
				BankAccountName = "sdggfds"
			});
*/
			/*stra.Execute();
			Assert.IsTrue(stra.Result);*/
		}

		[Test]
		public void TestFindInvestor() {
			var stra = new FindInvestorForOffer(3406, 42828);
			stra.Execute();
			Assert.IsTrue(stra.IsFound);
		}

        [Test]
        public void TestGetLegalDocs() {
            var stra = new GetLegalDocs(1,true,1);
            stra.Execute();
            Assert.IsTrue(stra.LoanAgreementTemplate.Count > 0);
        }

		[Test]
		public void TestLoadDecisionHistory() {
			var stra = new LoadDecisionHistory(3477);
			stra.Execute();
			Assert.Greater(stra.Result.Count(), 0);
		}

		[Test]
		public void TestNotifyRiskPendingInvestorOffer() {
			var stra = new NotifyRiskPendingInvestorOffer(154, 1000, DateTime.UtcNow.AddDays(1));
			stra.Execute();
		}

		[Test]
		public void TestNotifyInvestorUtilizedFunds() {
			var stra = new NotifyInvestorUtilizedFunds(2);
			stra.Execute();
		}

		[Test]
		public void TestLinkOfferToInvestor() {
			var linkOfferToInvestor = new LinkOfferToInvestor(3406, 42824, false, null, 1);
			linkOfferToInvestor.Execute();
		}

		[Test]
		public void TestLoadAccountingInvestor() {
			var loadAccountingData = new LoadAccountingData();
			loadAccountingData.Execute();
		}


        [Test]
        public void TestManualLegalDocsSyncTemplatesFiles() {
			var stra = new ManualLegalDocsSyncTemplatesFiles(@"C:\ezbob\App\PluginWeb\EzBob.Web\Areas\Customer\Views\Agreement");
            stra.Execute();
        }

		[Test]
		public void TestLinkLoanRepaymentToInvestor() {
			var linkOfferToInvestor = new LinkRepaymentToInvestor(1062, 1845, 32, DateTime.UtcNow, 1);
			linkOfferToInvestor.Execute();
		}

		[Test]
		public void TestLogicalGlueCache() {
			GetLatestKnownInference inference = new GetLatestKnownInference(3406, DateTime.UtcNow, false);
			inference.Execute();
			if (inference.Inference != null) {
				var bucket = inference.Inference.Bucket;
				Assert.IsNotNull(bucket);

			}
		}
	}
}
