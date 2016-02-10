namespace SalesForceMigrationTool {
	using System;
	using System.IO;
	using System.Threading;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;
	using SalesForceLib;
	using SalesForceLib.Models;

	class Program {
        public static ISalesForceAppClient SfClient = new SalesForceApiClient("techapi@ezbob.com", "Ezca$h123", "qCgy7jIz8PwQtIn3bwxuBv9h", "Production");
        protected static ILog Log = LogManager.GetLogger(typeof (Program));
        public static AConnection DB = new SqlConnection(new SafeILog(Log));
        //public static ISalesForceAppClient SfClient = new FakeApiClient("", "", "", "");
        static void Main(string[] args) {
            log4net.Config.XmlConfigurator.Configure();
            Log.Info("Begin SF Migration tool");
            
            //DB.ForEachRowSafe(MigrateContact, "SELECT d.id AS Id  FROM Director d INNER JOIN Customer c ON d.CustomerId=c.Id WHERE c.CollectionStatus <> 1 AND c.IsTest=0 AND c.WizardStep=4", CommandSpecies.Text);
            //MigrateLeadsFromCsv();
            //MigrateLeadsFromDb();
            //MigrateContact(1189);
            //MigrateContact(1227);
	        MigrateBrokerLeadsFromDb();
			//SalesForceReruner sfReruner = new SalesForceReruner(SfClient, DB);
			//sfReruner.Rerun();

			//SalesForceAddMissingLeadsAccounts addMissingLeadsAccounts = new SalesForceAddMissingLeadsAccounts(SfClient, DB);
			//foreach (var customerID in SalesForceAddMissingLeadsAccounts.missingCustomers) {
			//	addMissingLeadsAccounts.AddLead(customerID);
			//	Thread.Sleep(200);
			//}

			//SalesReport salesReport = new SalesReport(DB, SfClient);
			//salesReport.Execute(new DateTime(2015, 07, 01), new DateTime(2015, 08, 01));
			Log.Info("End SF Migration tool");
        }

        private static void MigrateLeadsFromDb()
        {
            int[] customerIDs = { 23188,23447,23641,23388,24081,16654,23630,23604,2940,12412,23457,23917 };

            foreach (var customerID in customerIDs)
            {
                Log.InfoFormat("migrating customer {0}", customerID);
                LeadAccountModel model = DB.FillFirst<LeadAccountModel>("SF_LoadAccountLead", CommandSpecies.StoredProcedure,
                new QueryParameter("@CustomerID", customerID),
                new QueryParameter("@Email"),
                new QueryParameter("@IsBrokerLead", false),
                new QueryParameter("@IsVipLead", false));

                if (string.IsNullOrEmpty(model.Email))
                {
                    Log.ErrorFormat("Email is null for customerID {0}, skipping", customerID);
                    continue;
                }

                SfClient.CreateUpdateLeadAccount(model);
                Thread.Sleep(3000);
            }
        }

		private static void MigrateBrokerLeadsFromDb() {
			string[] leadEmails = {
				"tallmpc@hotmail.com",
			};

			foreach (var leadEmail in leadEmails) {
				Log.InfoFormat("migrating lead {0}", leadEmail);
				LeadAccountModel model = DB.FillFirst<LeadAccountModel>("SF_LoadAccountLead", CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", null),
				new QueryParameter("@Email", leadEmail),
				new QueryParameter("@IsBrokerLead", true),
				new QueryParameter("@IsVipLead", false));

				if (string.IsNullOrEmpty(model.Email)) {
					Log.ErrorFormat("Email is null for lead {0}, skipping", leadEmail);
					continue;
				}
				try {
					SfClient.CreateUpdateLeadAccount(model);
					Thread.Sleep(1000);
				} catch (Exception ex) {
					Log.ErrorFormat("ERROR executing for lead {0}\n{1}", leadEmail, ex);
				}
			}
		}

        private static void MigrateLeadsFromCsv() {
            LeadsMigration migration = new LeadsMigration();
            string fileName = @"c:\ezbob\Tools\EzbobUtils\SalesForceMigrationTool\SalesForceMigrationTool\LeadsMigration.csv";
            byte[] dBytes = File.ReadAllBytes(fileName); ;

            var leads = migration.ParseCsv(dBytes, fileName);

            foreach (var lead in leads) {
                Log.InfoFormat("migrating lead {0} {1}", lead.FirstName, lead.Surname);
                SfClient.CreateUpdateLeadAccount(new LeadAccountModel {
                    Email = lead.Email,
                    CompanyName = "No Name",
                    Origin = "everline",
                    LeadSource = lead.GroupTag,
                    EzbobSource = lead.GroupTag,
                    Name = string.Format("{0} {1}", lead.FirstName, lead.Surname),
                    PhoneNumber = lead.MobilePhone,
                });
                Thread.Sleep(3000);
            }
        }

        private static ActionResult MigrateContact(SafeReader sf, bool arg2) {
            int directorID = sf["Id"];
            MigrateContact(directorID);
            return ActionResult.Continue;
        }

        private static void MigrateContact(int directorID)
        {
            Log.Debug(directorID);
            ContactModel model = DB.FillFirst<ContactModel>("SF_LoadContact", CommandSpecies.StoredProcedure,
                new QueryParameter("@CustomerID"),
                new QueryParameter("@DirectorID", directorID),
                new QueryParameter("@DirectorEmail"));

            if (string.IsNullOrEmpty(model.ContactEmail)) {
                model.ContactEmail = directorID.ToString() + "@noemail.com";
            }

            SfClient.CreateUpdateContact(model);
            Thread.Sleep(3000);
        }
    }
}
