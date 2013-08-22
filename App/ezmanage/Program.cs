using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using ApplicationMng.Signal;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Signals.ZohoCRM;
using NDesk.Options;
using Scorto.DBCommon;
using StructureMap;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFileExtension = "log4net", Watch = true)]

namespace ezmanage
{
	using EZBob.DatabaseLib.Model.Database;

	internal class Program
    {

        public static ILog _log = LogManager.GetLogger("ezmanage");

        private static bool loan;
        private static bool _updateCrm;
        private static bool create;
        private static bool recalculate;
        private static int id;
        private static decimal amount = 0;
        private static DateTime date = DateTime.UtcNow;

        private static void Main(string[] args)
        {
            try
            {
                _log.Info("Starting ezmanage");
                //Scanner.Register();

                Init();

                ParseOptions(args);

                //loan actions
                LoanActions();
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw;
            }
        }

        private static void Init()
        {
            ObjectFactory.Initialize(x =>
                                         {
                                             x.AddRegistry<EzRegistry>();
                                             x.AddRegistry<ApplicationMng.Repository.RepositoryRegistry>();
                                         });

            var lConnectionString = ConfigurationManager.AppSettings["DBConnection"];
            DbCommon.Init("DBSQLServer.dll", lConnectionString);

        }

        private static void LoanActions()
        {
            if (loan)
            {
                //create loan
                if (create)
                {
                    CreateLoan();
                } else if (recalculate)
                {
                    RecalculateLoans();
                }
            }
            if (_updateCrm)
            {
                UpdateAllCustomersForCrm();
            }

            Console.Write("\nPress any key to exit . . .");
            Console.Read();
        }

        private static void UpdateAllCustomersForCrm()
        {
            Console.WriteLine("Update ZOHO CRM was started");
            var customerIdsForUpdate =
                ObjectFactory.GetInstance<CustomerRepository>()
                             .GetAll()
                             .Where(x => x.WizardStep == WizardStepType.AllStep && !x.IsTest)
                             .Select(x=>x.Id);

            foreach (var customerId in customerIdsForUpdate)
            {
                _log.InfoFormat("Update for Customer Id={0} was started", customerId);
                new ZohoSignalClient(customerId).Execute();
            }
        }

        private static void RecalculateLoans()
        {
            var lc = ObjectFactory.GetInstance<LoanRefresher>();
            lc.Refresh(DateTime.UtcNow);
        }

        private static void CreateLoan()
        {
            if (amount <= 0.0M)
            {
                //amount cannot be less than 0
                _log.Error("Amount is too small");
                return;
            }
            if (id == 0)
            {
                //id cannot be equal to 0
                _log.Error("Customer should be specified");
                return;
            }

            _log.InfoFormat("Creating loan for customer {0}, amount {1}", id, amount);

            var lc = ObjectFactory.GetInstance<LoanCreatorNoChecks>();
            lc.CreateLoan(id, amount, date);
        }

        private static void ParseOptions(string[] args)
        {
            var p = new OptionSet()
                        {
                            {
                                "l", "loan actions",
                                v => loan = true
                            },
                            {
                                "c", "create",
                                v => create = true
                            },
                            {
                                "r", "recalculate",
                                v => recalculate = true
                            },
                            {
                                "id=",
                                "id of entity\n" +
                                "this must be an integer.",
                                (int v) => id = v
                            },
                            {
                                "amount=",
                                "loan amount\n" +
                                "this must be an integer.",
                                (decimal v) => amount = v
                            },
                            {
                                "date=",
                                "action date\n" +
                                "this must be an date in format dd/MM/yyyy.",
                                (string v) => date = DateTime.ParseExact(v, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                            },
                            {
                                "u", "update all customer in Zoho CRM",
                                v => _updateCrm = true
                            },
                        };

            p.Parse(args);
        }
    }
}
