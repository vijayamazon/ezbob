namespace EzBobTest.Calculator {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DbConstants;
    using Ezbob.Backend.CalculateLoan.LoanCalculator;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using NUnit.Framework;

    [TestFixture]
    public class TestLoanCalculator : BaseTestFixtue {

        private void DecorateNL_LoanNL_LoanHistory(NL_Model nlModel) {
            nlModel.Loan = new NL_Loans() {
                LoanID = 1,
                Histories = new List<NL_LoanHistory>()
            };
            nlModel.Loan.Histories.Add(new NL_LoanHistory() {
                LoanID = 1,
                LoanHistoryID = 1,
                EventTime = new DateTime(2015, 1, 15),
                InterestRate = 2,
                Amount = 999,
                RepaymentCount = 3
            });
        }

        public void DecorateNL_Offers(NL_Model nlModel, NLFeeTypes feeType, bool isBrokerSetUpFee = false) {
            nlModel.Offer = new NL_Offers() {
                OfferFees = new List<NL_OfferFees>() {
                    new NL_OfferFees() {
                        Percent = (decimal?)0.02,
                        LoanFeeTypeID = (int)feeType                        
                    }
                }
            };
            if (isBrokerSetUpFee)
                nlModel.Offer.BrokerSetupFeePercent = (decimal?)0.5;
        }

        private void DecorateHistory(NL_Model nlModel) {
            nlModel.Loan.Histories.Add(new NL_LoanHistory() {
                LoanID = 1,
                LoanHistoryID = 2,
                EventTime = new DateTime(2015, 2, 15),
                InterestRate = 3,
                Amount = 400,
                RepaymentCount = 4
            });
        }

        private void DecoratePayment(NL_Model nlModel) {
            NL_Payments nlPayments = new NL_Payments() {
                LoanID = 1,
                Amount = 333,
                CreationTime = new DateTime(2015, 2, 15),
                PaymentTime = new DateTime(2015, 2, 15),
                PaymentID = 1
            };
            nlModel.Loan.Payments.Add(nlPayments);
        }

        [Test]
        public void TestCreateScheduleSetupFee() {
            
            var nlModel = new NL_Model(1);
            DecorateNL_LoanNL_LoanHistory(nlModel);
            DecorateNL_Offers(nlModel, NLFeeTypes.SetupFee);
            
            ExcelCalculator excelCalculator = new ExcelCalculator(nlModel);
            excelCalculator.CreateSchedule();

            var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
            legacyLoanCalculator.CreateSchedule();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlModel.Loan.LastHistory().Schedule.Count == 3);

            var nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 1);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 999 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 2, 15));

            nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 2);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 666 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 3, 15));

            nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 3);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 333 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 4, 15));
        }

        [Test]
        public void TestCreateScheduleServicingFee() {

            var nlModel = new NL_Model(1);
            DecorateNL_LoanNL_LoanHistory(nlModel);
            DecorateNL_Offers(nlModel, NLFeeTypes.ServicingFee);

            var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
            legacyLoanCalculator.CreateSchedule();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlModel.Loan.LastHistory().Schedule.Count == 3);

            var nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 1);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 999 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 2, 15));

            nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 2);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 666 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 3, 15));

            nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 3);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 333 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 4, 15));
        }

        [Test]
        public void TestCreateScheduleSetupFeeBrokerSetupFee() {

            var nlModel = new NL_Model(1);
            DecorateNL_LoanNL_LoanHistory(nlModel);
            DecorateNL_Offers(nlModel, NLFeeTypes.ServicingFee, true);

            var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
            legacyLoanCalculator.CreateSchedule();

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlModel.Loan.LastHistory().Schedule.Count == 3);

            var nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 1);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 999 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 2, 15));

            nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 2);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 666 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 3, 15));

            nlLoanSchedule = nlModel.Loan.LastHistory().Schedule.FirstOrDefault(x => x.Position == 3);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(nlLoanSchedule != null &&
                          nlLoanSchedule.Principal == 333 &&
                          nlLoanSchedule.Balance == 333 &&
                          nlLoanSchedule.InterestRate == 2 &&
                          nlLoanSchedule.PlannedDate == new DateTime(2015, 4, 15));
        }

        [Test]
        public void GetStateHistoriesEvents() {
            var nlModel = new NL_Model(1);
            DecorateNL_LoanNL_LoanHistory(nlModel);
            DecorateHistory(nlModel);

            var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);

            legacyLoanCalculator.GetState();
        }

        [Test]
        public void GetStateAddPayment() {
            var nlModel = new NL_Model(1);
            DecorateNL_LoanNL_LoanHistory(nlModel);
            DecorateNL_Offers(nlModel, NLFeeTypes.ServicingFee);
            DecoratePayment(nlModel);

            var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
            legacyLoanCalculator.GetState();
        }

        [Test]
        public void GetStateAcceptedRolloverEvents() {
            var nlModel = new NL_Model(1);
            DecorateNL_LoanNL_LoanHistory(nlModel);
            DecorateNL_Offers(nlModel, NLFeeTypes.ServicingFee);
            DecoratePayment(nlModel);

            var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
            //legacyLoanCalculator.
            //legacyLoanCalculator.GetState();
        }


        //[Test]
        //public void CalculateApr() {
        //}
        //[Test]
        //public void GetStateFeesEvents() {
        //    var nlModel = new NL_Model(1);
        //    DecorateNL_LoanNL_LoanHistory(nlModel);
        //    var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
        //    legacyLoanCalculator.GetState();


        //}

        //[Test]
        //public void GetStatePaymentsEvents() {
        //    var nlModel = new NL_Model(1);
        //    DecorateNL_LoanNL_LoanHistory(nlModel);
        //    var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
        //    legacyLoanCalculator.GetState();


        //}

        //[Test]
        //public void GetStateSchedulesEvents() {
        //    var nlModel = new NL_Model(1);
        //    DecorateNL_LoanNL_LoanHistory(nlModel);
        //    var legacyLoanCalculator = new LegacyLoanCalculator(nlModel, DateTime.UtcNow);
        //    legacyLoanCalculator.GetState();


        //}


    }
}
