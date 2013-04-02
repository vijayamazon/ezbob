using System;
using EZBob.DatabaseLib.Model.Database;
using NUnit.Framework;
using ZohoCRM;

namespace EzBob.Tests.ZohoCRMTests
{
    [TestFixture]
    public class RegisterLead
    {
        [Test]
        [Ignore]
        public void can_register_lead_from_customer()
        {
            var crm = new ZohoCRM.ZohoFacade(new ZohoConfigTest(), null, null, null);
            var customer = new Customer()
                               {
                                   Name = "EthanHall@teleworm.co.uk1"
                               };
            crm.RegisterLead(customer);
        }

        [Test]
        [Ignore]
        public void can_register_lead_and_convert_it()
        {
            var crm = new ZohoFacade(new ZohoConfigTest(), null, null, null);
            var customer = new Customer()
                               {
                                   Name = "LucilleGBlair@dayrep.com3",
                                   PersonalInfo = new PersonalInfo()
                                                      {
                                                          DateOfBirth = new DateTime(1980, 5, 3),
                                                          FirstName = "Lucille ",
                                                          Surname = "Blair",
                                                          Gender = Gender.F,
                                                          MobilePhone = "+770-751-7478",
                                                          DaytimePhone = "770-751-7478",
                                                          OverallTurnOver = 234000,
                                                          WebSiteTurnOver = 2100
                                                      }
                               };
            crm.RegisterLead(customer);

            crm.ConvertLead(customer);
        }
    }
}