using System;
using System.Threading;
using Ezbob.Database;
using Ezbob.Logger;
using SalesForceLib;
using SalesForceLib.Models;

namespace SalesForceMigrationTool
{
    class Program
    {
        public static ASafeLog Log = new ConsoleLog();
        public static AConnection DB = new SqlConnection(Log);

        //todo fill the credentials
        //public static ISalesForceAppClient SfClient = new SalesForceApiClient("", "", "", "");

        public static ISalesForceAppClient SfClient = new FakeApiClient("", "", "", "");
        static void Main(string[] args)
        {
            DB.ForEachRowSafe(MigrateContact, "SELECT Id FROM Director", CommandSpecies.Text);
        }

        private static ActionResult MigrateContact(SafeReader sf, bool arg2)
        {
            int directorId = sf["Id"];
            Console.WriteLine(directorId);
            ContactModel model = DB.FillFirst<ContactModel>("SF_LoadContact", CommandSpecies.StoredProcedure,
                new QueryParameter("@CustomerID"),
                new QueryParameter("@DirectorID", directorId),
                new QueryParameter("@DirectorEmail"));
            SfClient.CreateUpdateContact(model);
            Thread.Sleep(1000);
            return ActionResult.Continue;
        }
    }
}
