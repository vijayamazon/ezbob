using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Code.MpUniq;
using Moq;
using NUnit.Framework;

namespace EzBob.Tests.MPUniqCheckerTests
{
    [TestFixture]
    public class MPUniqCheckerFixture
    {
        private Mock<ICustomerMarketPlaceRepository> _cmr;
        private MPUniqChecker _checker;
        private Customer _customer;
        private Guid _marketplaceType;
        private Mock<IMP_WhiteListRepository> _whiteList;

        [SetUp]
        public void SetUp()
        {
            _cmr = new Mock<ICustomerMarketPlaceRepository>();
            _whiteList = new Mock<IMP_WhiteListRepository>();
            _checker = new MPUniqChecker(_cmr.Object, _whiteList.Object);
            _customer = new Customer();
            _marketplaceType = new Guid();
        }

        [Test]
        public void cannot_add_marketplace_twice_for_the_same_customer()
        {
            Assert.Throws<MarketPlaceAddedByThisCustomerException>(()=>
                {
                    _whiteList.Setup(x => x.IsMarketPlaceInWhiteList(_marketplaceType, "mp1")).Returns(false);
                    _cmr.Setup(x => x.Exists(_marketplaceType, "mp1")).Returns(true);
                    _cmr.Setup(x => x.Exists(_marketplaceType, _customer, "mp1")).Returns(true);
                    _checker.Check(_marketplaceType, _customer, "mp1");
                    
                });
        }

        [Test]
        public void cannot_add_marketplace_twice_for_different_customers()
        {
            Assert.Throws<MarketPlaceIsAlreadyAddedException>(() =>
                {
                    _whiteList.Setup(x => x.IsMarketPlaceInWhiteList(_marketplaceType, "mp1")).Returns(false);
                    _cmr.Setup(x => x.Exists(_marketplaceType, "mp1")).Returns(true);
                    _cmr.Setup(x => x.Exists(_marketplaceType, _customer, "mp1")).Returns(false);
                    _checker.Check(_marketplaceType, _customer, "mp1");

                });
        }

        [Test]
        public void white_list_allows_to_add_multiple_marketplaces()
        {
            Assert.DoesNotThrow(() =>
                {
                    _whiteList.Setup(x => x.IsMarketPlaceInWhiteList(_marketplaceType, "mp1")).Returns(true);
                    _cmr.Setup(x => x.Exists(_marketplaceType, "mp1")).Returns(true);
                    _cmr.Setup(x => x.Exists(_marketplaceType, _customer, "mp1")).Returns(true);
                    _checker.Check(_marketplaceType, _customer, "mp1");
                });
        }
    }
}