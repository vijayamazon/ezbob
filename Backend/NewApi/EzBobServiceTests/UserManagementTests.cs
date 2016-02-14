using System;

namespace EzBobServiceTests {
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Security.Cryptography;
    using EzBobModels;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence;
    using EzBobPersistence.Customer;
    using EzBobService;
    using EzBobService.Customer;
    using Moq;
    using NUnit.Framework;
    using StructureMap;

    [TestFixture]
    public class UserManagementTests : TestBase {

        private class TestUspsertQuery : QueryBase {

            private class Model {
                public int Id { get; set; }
                public string Name { get; set; }
                public string LastName { get; set; }
            }

            public TestUspsertQuery(string connectionString)
                : base(connectionString) {}

            public string Test() {
                SqlConnection connection = new SqlConnection();
                Model m = new Model {
                    Id = 1,
                    Name = "TestModel",
                    LastName = "aaaa"
                };

                var pair1 = new KeyValuePair<string, object>("Id", 20);
                var pair2 = new KeyValuePair<string, object>("Name", "lala");

                var cmd = GetUpsertCommand(m, connection, "Models", new KeyValuePair<string, object>[] {
                    pair1, pair2
                });

                if (!cmd.HasValue) {
                    return string.Empty;
                }

                return cmd.GetValue()
                    .CommandText;
            }

        }

        [Test]
        public void TestUpsertQueryGenerator() {
            TestUspsertQuery upsert = new TestUspsertQuery("lalala");

            string test = upsert.Test().Replace("\r\n", "").Replace(" ", "");
            string expected = "MERGE Models AS TARGET\r\nUSING (VALUES(@Id,@Name,@LastName))\r\n AS SOURCE (Id,Name,LastName)\r\n ON (TARGET.Id = @Id AND TARGET.Name = @Name)\r\nWHEN MATCHED THEN\r\nUPDATE\r\n  SET \r\n   Id = SOURCE.Id,\r\n   Name = SOURCE.Name,\r\n   LastName = SOURCE.LastName\r\nWHEN NOT MATCHED THEN INSERT (LastName,Id,Name)\r\nVALUES ( SOURCE.LastName,@Id,@Name);";
            expected = expected.Replace("\r\n", "").Replace(" ", "");
            Assert.AreEqual(test, expected);
        }

        [Test]
        public void TestSaveCustomer() {
            IContainer container = InitContainer(typeof(CustomerProcessor));

            container.Configure(c => c.ForSingletonOf<CustomerProcessor>()
                .Use<CustomerProcessor>());
            var customerQueries = container.GetInstance<ICustomerQueries>();

            Customer customer = new Customer() {
                Name = GetRandomEmailAddress(),
                Id = GetRundomInteger(2000, int.MaxValue),
                Status = CustomerStatus.Registered.ToString(),
                RefNumber = GetRandomPassword(),
                WizardStep = (int)WizardStepType.SignUp,
                CollectionStatus = (int)CollectionStatusNames.Enabled,
                IsTest = true,
                IsOffline = null,
                PromoCode = "lalala",
                PersonalInfo = {
                    MobilePhone = "123456789",
                    MobilePhoneVerified = false,
                    FirstName = "aaaaaa",
                    Surname = "bbbbb"
                },
                TrustPilotStatusID = (int)TrustPilotStauses.Neither,
                GreetingMailSentDate = DateTime.UtcNow,
                Vip = false,
                WhiteLabelId = null,
                BrokerID = null,
                GoogleCookie = "kind of google cookie",
                ReferenceSource = "lalalalala",
                AlibabaId = null,
                IsAlibaba = false,
                OriginID = -1,
            };

            var res = customerQueries.UpsertCustomer(customer);
            Assert.True(res.HasValue, "failed to save customer");


            Customer dbCustomer = customerQueries.GetCustomerById(customer.Id);
            bool areEqual = AreEqualByWritableReadableProperties(customer, dbCustomer);
            Assert.True(areEqual, "expected to get equal objects");
        }

        [Test]
        public void TestGetUserIdByUserName()
        {
            IContainer container = InitContainer(typeof(CustomerProcessor));

            container.Configure(c => c.ForSingletonOf<CustomerProcessor>()
                .Use<CustomerProcessor>());

            var customerSignUp = container.GetInstance<CustomerProcessor>();
            var resultInfo = customerSignUp.GetUserIdByUserName("admin");
            int kk = 0;
        }

        [Test]
        [Ignore("Should be updated to the changed flow")]
        public void TestFullSignUp() {
            IContainer container = InitContainer(typeof(CustomerProcessor));

            var customerSignUp = container.GetInstance<CustomerProcessor>();


            string passwordString = GetRandomPassword();
            string emailAddress = GetRandomEmailAddress();

            var loginInfo = new LoginInfo() {
                Password = new Password(passwordString, passwordString),
                Email = emailAddress,
                PasswordAnswer = "bbbb",
                RemoteIp = "111.111.111.111",
                PasswordQuestionId = 1
            };

            Customer customer = new Customer {
                Name = emailAddress,
                Id = GetRundomInteger(2000, int.MaxValue),
                Status = CustomerStatus.Registered.ToString(),
                RefNumber = GetRandomPassword(),
                WizardStep = (int)WizardStepType.SignUp,
                CollectionStatus = (int)CollectionStatusNames.Enabled,
                IsTest = true,
                IsOffline = null,
                PromoCode = "lalala",
                PersonalInfo = {
                    MobilePhone = "123456789",
                    MobilePhoneVerified = false,
                    FirstName = GenerateName(),
                    Surname = GenerateName(),
                    MaritalStatus = EzBobModels.Enums.MaritalStatus.Married,
                    Gender = Gender.M,
                    DateOfBirth = DateTime.Today
                },
                TrustPilotStatusID = (int)TrustPilotStauses.Neither,
                GreetingMailSentDate = DateTime.UtcNow,
                Vip = false,
                WhiteLabelId = null,
                BrokerID = null,
                GoogleCookie = "kind of google cookie",
                ReferenceSource = "lalalalala",
                AlibabaId = null,
                IsAlibaba = false,
                OriginID = 1
            };

            CampaignSourceRef campaignSourceRef = new CampaignSourceRef() { 
                RSource = "Direct",
                RDate = DateTime.UtcNow
            };

            decimal requestedLoan = 10000;
            //TODO: update test for customer address and customer phone (two last nulls)
            var infoAccumulator = customerSignUp.UpdateCustomer(customer, requestedLoan, customer.ReferenceSource, GenerateVisitTimesString(), campaignSourceRef, null, null);
            Assert.False(infoAccumulator.HasErrors, "expected no errors");


            CustomerQueries realCustomerQueries = new CustomerQueries("Server=localhost;Database=ezbob;User Id=ezbobuser;Password=ezbobuser;MultipleActiveResultSets=true");

            var customerQueriesMock = new Mock<ICustomerQueries>();
//            customerQueriesMock.SetupAllProperties();

            Func<string, string, int, string, string, SecurityUser> createUser = (s1, s2, i, s3, s4) => realCustomerQueries.CreateSecurityUser(s1, s2, i, s3, s4);

            customerQueriesMock.Setup(o => o.CreateSecurityUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns(createUser);

            customerQueriesMock.Setup(o => o.UpsertCustomer(It.IsAny<Customer>()))
                .Returns(() => null);

            container.Configure(r => r.ForSingletonOf<ICustomerQueries>()
                .Use(() => customerQueriesMock.Object));

            container.EjectAllInstancesOf<CustomerProcessor>();

            customerSignUp = container.GetInstance<CustomerProcessor>();

            //TODO: update test for customer address and customer phone (two last nulls)
            infoAccumulator = customerSignUp.UpdateCustomer(customer, requestedLoan, customer.ReferenceSource, GenerateVisitTimesString(), campaignSourceRef, null, null);
            Assert.True(infoAccumulator.HasErrors, "expected to have errors");
        }

        private IEnumerable<Tuple<string, string, int, string, string>> GenerateInvalidValues() {
            string[] a1 = {
                "", null
            };
            string[] a2 = {
                "", null
            };
            int[] a3 = {
                -1, -2
            };
            string[] a4 = {
                "", null
            };
            string[] a5 = {
                "", null
            };

            for (int i = 0; i < 2; ++i) {
                for (int j = 0; j < 2; ++j) {
                    for (int k = 0; k < 2; ++k) {
                        for (int l = 0; l < 2; ++l) {
                            for (int m = 0; m < 2; ++m) {
                                yield return new Tuple<string, string, int, string, string>(a1[i], a2[j], a3[k], a4[l], a5[m]);
                            }
                        }
                    }
                }
            }
        }
    }
}
