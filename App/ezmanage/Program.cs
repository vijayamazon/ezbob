using System;
using System.Configuration;
using System.Globalization;
using NDesk.Options;
using Scorto.DBCommon;
using StructureMap;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFileExtension = "log4net", Watch = true)]

namespace ezmanage
{
	internal class Program
    {

        public static ILog _log = LogManager.GetLogger("ezmanage");

        private static bool loan;
        private static bool create;
        private static bool recalculate;
        private static int id;
        private static decimal amount = 0;
        private static DateTime date = DateTime.UtcNow;

        private static void Main(string[] args)
        {
            //for debug
            //args = new[]{"-lc","-id=2318","-amount=1000","-date=27/07/2013"};
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

			var lConnectionString = ConfigurationManager.AppSettings["DBConnection"]; //DBConnectionLocal
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

            Console.Write("\nPress any key to exit . . .");
            Console.Read();
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
                        };

            p.Parse(args);
        }
    }
}
