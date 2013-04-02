using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Controllers;
using EzBob.Web.Areas.Customer.Controllers.Exceptions;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Infrastructure;
using Moq;
using NUnit.Framework;
using PaymentServices.PacNet;
using ZohoCRM;

namespace EzBob.Tests
{
    [TestFixture]
    [Ignore]
    public class GetCashFixture
    {
        private Customer _customer;
        private GetCashController _controller;
        private Mock<IPacnetService> _pacnet;
        private Mock<ICustomerNameValidator> _validator;
        private Mock<IAppCreator> _appCreator;
        private Mock<ILoanHistoryRepository> _loanHistoryMock;
        private Mock<IZohoFacade> _crm;

        [SetUp]
        public void SetUp()
        {
            _customer = new Customer()
                {
                    Status = Status.Approved,
                    OfferStart = DateTime.UtcNow.AddDays(-1),
                    OfferValidUntil = DateTime.UtcNow.AddDays(1),
                    IsSuccessfullyRegistered = true,
                    BankAccount = new BankAccount()
                        {
                            AccountNumber = "12345678",
                            SortCode = "12-34-56"
                        },
                    PersonalInfo = new PersonalInfo
                        {
                            FirstName = "John",
                            Surname = "Black"
                        }
                };
            _customer.CashRequests.Add(new CashRequest(){InterestRate = 0.06M, RepaymentPeriod = 3});

            var context = new Mock<IEzbobWorkplaceContext>();
            context.Setup(x => x.Customer).Returns(_customer);

            _pacnet = new Mock<IPacnetService>();
            _pacnet
                .Setup(x => x.SendMoney(It.IsAny<int>(),It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new PacnetReturnData());

            var paypoint = new Mock<IPayPointFacade>();
            paypoint.Setup(x => x.CheckHash(It.IsAny<string>(), It.IsAny<Uri>())).Returns(true);

            var log = new Mock<IPacnetPaypointServiceLogRepository>();

            _appCreator = new Mock<IAppCreator>();

            _validator = new Mock<ICustomerNameValidator>();
            _validator
                .Setup(x => x.CheckCustomerName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var customers = new Mock<ICustomerRepository>();

            _loanHistoryMock = new Mock<ILoanHistoryRepository>();

            var agreementsGenerator = new Mock<IAgreementsGenerator>();

            _crm = new Mock<IZohoFacade>();

            var creator = new Mock<ILoanCreator>();

            _controller = new GetCashController(
                context.Object, 
                paypoint.Object, 
                _appCreator.Object, 
                _validator.Object, 
                log.Object, 
                customers.Object, 
                creator.Object,
                _crm.Object);

            var request = new Mock<HttpRequestBase>(MockBehavior.Strict);
            request.SetupGet(x => x.ApplicationPath).Returns("/");
            request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/a", UriKind.Absolute));

            var controllerContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            controllerContext.SetupGet(x => x.Request).Returns(request.Object);

            _controller.ControllerContext = new ControllerContext(controllerContext.Object, new RouteData(), _controller);            
        }

        [Test]
        public void throws_exception_when_customer_is_not_aproved()
        {
            _customer.Status = Status.Rejected;
            Assert.Throws<CustomerIsNotApprovedException>(() =>
                {
                    var result = _controller.PayPointCallback(true, "1", "A", "1", 100, "0.0.0.0", "true", "hash", "OK", 100M, "4444333322221111", "", "");
                });            
        }

        [Test]
        public void substract_setup_fee_when_sending_money()
        {
            _pacnet
                .Setup(x => x.SendMoney(It.IsAny<int>(), 70, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new PacnetReturnData(){Status = "Ok"})
                .Verifiable("SendMoney was not called with amount 70");

            var result = _controller.PayPointCallback(true, "1", "A", "1", 100, "0.0.0.0", "true", "hash", "OK", 100M, "4444333322221111", "", "");

            Assert.That(_customer.LastCashRequest.UseSetupFee, Is.False);

            Assert.That(_customer.SetupFee, Is.EqualTo(30));
        }

        [Test]
        public void create_history_record_for_loan_when_get_cash()
        {
            _loanHistoryMock.Setup(x => x.Save(It.IsAny<LoanHistory>())).Verifiable("Loan History was not saved");

            var result = _controller.PayPointCallback(true, "1", "A", "1", 100, "0.0.0.0", "true", "hash", "OK", 100M, "4444333322221111", "", "");

            _loanHistoryMock.VerifyAll();
        }

        [Test]
        public void invalid_name_creates_application()
        {
            _pacnet
                .Setup(x => x.SendMoney(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new PacnetReturnData() { Status = "Ok" })
                .Verifiable("SendMoney was not called with amount 70");

            _validator
                .Setup(x => x.CheckCustomerName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var result = _controller.PayPointCallback(true, "1", "A", "1", 100, "0.0.0.0", "true", "hash", "OK", 100M, "4444333322221111", "", "");

            _appCreator.Verify(x => x.PayPointNameValidationFailed(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<Customer>()));
        }

        [Test]
        public void do_not_substract_setup_fee_when_sending_money()
        {
            _customer.SetupFee = 30;
            _pacnet
                .Setup(x => x.SendMoney(It.IsAny<int>(), 100, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new PacnetReturnData(){Status = "Ok"})
                .Verifiable("SendMoney was not called with amount 70");

            var result = _controller.PayPointCallback(true, "1", "A", "1", 100, "0.0.0.0", "true", "hash", "OK", 100M, "4444333322221111", "", "");

            Assert.That(_customer.SetupFee, Is.EqualTo(30));

        }
    }
}