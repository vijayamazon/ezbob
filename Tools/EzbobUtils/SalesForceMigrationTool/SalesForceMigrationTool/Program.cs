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
        private static ILog Log = LogManager.GetLogger(typeof (Program));
        public static AConnection DB = new SqlConnection(new SafeILog(Log));
        //public static ISalesForceAppClient SfClient = new FakeApiClient("", "", "", "");
        static void Main(string[] args) {
            log4net.Config.XmlConfigurator.Configure();
            Log.Info("Begin SF Migration tool");
            
            //DB.ForEachRowSafe(MigrateContact, "SELECT d.id AS Id  FROM Director d INNER JOIN Customer c ON d.CustomerId=c.Id WHERE c.CollectionStatus <> 1 AND c.IsTest=0 AND c.WizardStep=4", CommandSpecies.Text);
            //MigrateLeadsFromCsv();
            MigrateLeadsFromDb();
            //MigrateContact(1189);
            //MigrateContact(1227);

			//SalesForceReruner sfReruner = new SalesForceReruner(SfClient, DB);
			//sfReruner.Rerun();

			//SalesForceAddMissingLeadsAccounts addMissingLeadsAccounts = new SalesForceAddMissingLeadsAccounts(SfClient, DB);
			//foreach (var customerID in SalesForceAddMissingLeadsAccounts.missingCustomers) {
			//	addMissingLeadsAccounts.AddLead(customerID);
			//	Thread.Sleep(200);
			//}

			//SalesReport salesReport = new SalesReport(DB, SfClient);
			//salesReport.Execute(new DateTime(2015, 07, 01), new DateTime(2015, 08, 01));
			//Log.Info("End SF Migration tool");
        }

        private static void MigrateLeadsFromDb() {
	        int[] customerIDs = {
		        43397,
		        42686,
		        42687,
		        31733,
		        42771,
		        43256,
		        43579,
		        17705,
		        42389,
		        42297,
		        43627,
		        37870,
		        42858,
		        15656,
		        42973,
		        28591,
		        43161,
		        42356,
		        25000,
		        42341,
		        42772,
		        34027,
		        42886,
		        42256,
		        17151,
		        3793,
		        41534,
		        40451,
		        42520,
		        26440,
		        42759,
		        40539,
		        30237,
		        42263,
		        22498,
		        43487,
		        41428,
		        43020,
		        4013,
		        43409,
		        21865,
		        43308,
		        13738,
		        43594,
		        43337,
		        42980,
		        32833,
		        35242,
		        42588,
		        7510,
		        42525,
		        21295,
		        43604,
		        43492,
		        42230,
		        42385,
		        42381,
		        40183,
		        42453,
		        43568,
		        43123,
		        42689,
		        24822,
		        42322,
		        37503,
		        28555,
		        22596,
		        19220,
		        43438,
		        43287,
		        33654,
		        42768,
		        43632,
		        25553,
		        42968,
		        39612,
		        43045,
		        42544,
		        42332,
		        32382,
		        43214,
		        37694,
		        42292,
		        19724,
		        30940,
		        42943,
		        42584,
		        5568,
		        28670,
		        43481,
		        38225,
		        43247,
		        43552,
		        38217,
		        35795,
		        43524,
		        22173,
		        42103,
		        42660,
		        34480,
		        43114,
		        40464,
		        10134,
		        42800,
		        43623,
		        42957,
		        41726,
		        43284,
		        42929,
		        20446,
		        25361,
		        5688,
		        42490,
		        43013,
		        23116,
		        43484,
		        42733,
		        24032,
		        42299,
		        43466,
		        42202,
		        32046,
		        38702,
		        42420,
		        42856,
		        24281,
		        42715,
		        28234,
		        42050,
		        42656,
		        43065,
		        32409,
		        43442,
		        40609,
		        24584,
		        42828,
		        33059,
		        42294,
		        391,
		        35933,
		        43499,
		        22993,
		        42441,
		        43347,
		        43529,
		        38075,
		        43573,
		        32680,
		        42421,
		        43540,
		        42945,
		        41779,
		        43162,
		        36061,
		        42577,
		        42988,
		        37394,
		        35288,
		        39032,
		        42435,
		        42839,
		        42965,
		        42461,
		        16303,
		        43271,
		        40479,
		        43316,
		        24179,
		        43598,
		        42324,
		        43167,
		        41132,
		        41875,
		        43603,
		        36642,
		        42802,
		        20550,
		        42889,
		        42779,
		        31973,
		        26417,
		        43372,
		        14142,
		        43191,
		        43355,
		        42862,
		        42857,
		        43076,
		        42679,
		        42250,
		        24714,
		        42440,
		        42327,
		        42585,
		        43491,
		        43242,
		        21609,
		        14991,
		        43085,
		        43577,
		        42278,
		        42657,
		        42418,
		        42966,
		        43429,
		        42725,
		        42757,
		        42482,
		        42328,
		        43171,
		        43111,
		        43009,
		        43587,
		        43564,
		        42935,
		        42762,
		        43582,
		        43323,
		        42300,
		        43493,
		        43317,
		        43160,
		        42767,
		        26679,
		        23373,
		        42878,
		        28709,
		        42434,
		        41531,
		        42611,
		        42872,
		        42566,
		        31347,
		        42429,
		        42432,
		        25749,
		        42721,
		        42557,
		        42363,
		        42362,
		        42780,
		        38165,
		        30702,
		        43363,
		        42909,
		        42400,
		        17481,
		        42653,
		        43322,
		        36028,
		        42847,
		        42672,
		        42783,
		        43184,
		        42954,
		        33197,
		        43610,
		        42342
	        };

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

	            try {
		            SfClient.CreateUpdateLeadAccount(model);
	            } catch (Exception ex) {
					Log.ErrorFormat("failed to rerun for customerID {0}, skipping \n {1}", customerID, ex);
	            }
	            Thread.Sleep(1000);
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
