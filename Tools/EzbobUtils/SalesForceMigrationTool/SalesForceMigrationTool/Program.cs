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

        public static ISalesForceAppClient SfClient = new SalesForceApiClient("techapi@ezbob.com", "Ezca$h123", "qCgy7jIz8PwQtIn3bwxuBv9h", "Production");

        //public static ISalesForceAppClient SfClient = new FakeApiClient("", "", "", "");
        static void Main(string[] args)
        {
            DB.ForEachRowSafe(MigrateContact, "SELECT d.id AS Id  FROM Director d INNER JOIN Customer c ON d.CustomerId=c.Id WHERE c.CollectionStatus <> 1 AND c.IsTest=0 AND c.WizardStep=4", CommandSpecies.Text);
        }

        private static ActionResult MigrateContact(SafeReader sf, bool arg2)
        {
            int directorId = sf["Id"];
            Console.WriteLine(directorId);
            ContactModel model = DB.FillFirst<ContactModel>("SF_LoadContact", CommandSpecies.StoredProcedure,
                new QueryParameter("@CustomerID"),
                new QueryParameter("@DirectorID", directorId),
                new QueryParameter("@DirectorEmail"));

            if (string.IsNullOrEmpty(model.ContactEmail))
            {
                model.ContactEmail = directorId.ToString() + "@noemail.com";
            }

            SfClient.CreateUpdateContact(model);
            Thread.Sleep(3000);
            return ActionResult.Continue;
        }
    }
}
