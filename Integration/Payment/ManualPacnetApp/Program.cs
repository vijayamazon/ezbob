
namespace ManualPacnetApp
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate;
	using PaymentServices.PacNet;
	using StandaloneInitializer;
	using StructureMap;

	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("a");
			StandaloneApp.Execute<App>(args);
		}

		public class App : StandaloneApp
		{
			public override void Run(string[] args)
			{
				Console.WriteLine("b");
				if (args.Length < 1)
				{
					Usage("not enough arguments");
				}
				
				try
				{
					
					string fileName = "ezbob";
					string currencyCode = "GBP";
					string description = "EZBOB";
					if (args[0] == "send")
					{
						int customerId = 0;
						decimal amount;
						
						if (!int.TryParse(args[1], out customerId))
						{
							Usage("wrong customerId");
						}
						var session = ObjectFactory.GetInstance<ISession>();
						Customer cus = session.Get<Customer>(customerId);
						if (!decimal.TryParse(args[2], out amount))
						{
							Usage("wrong amount");
						}
						if (!cus.HasBankAccount)
						{
							throw new Exception("Customer don't have bank account");
						}
						string bankNumber = cus.BankAccount.SortCode;
						string accountNumber = cus.BankAccount.AccountNumber;
						string accountName = GetCustomerNameForPacNet(cus);
						if (args.Length >= 4) fileName = args[3];
						if (args.Length >= 5) currencyCode = args[4];
						if (args.Length >= 6) description = args[5];

						Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}", args[0],
						                  customerId, amount, bankNumber, accountNumber, accountName,
						                  fileName, currencyCode, description);

						var service = new PacnetService();
						//service.SendMoney(customerId, amount, bankNumber, accountNumber, accountName, fileName, currencyCode, description);
					}
					else if (args[0] == "close")
					{
						if (args.Length >= 2) fileName = args[1];
						Console.WriteLine("{0} {1}", args[0], fileName);
						var service = new PacnetService();
						//service.CloseFile(fileName: fileName);
					}
					else
					{
						Usage("wrong params");
					}
				}
				catch (Exception ex)
				{
					Usage(ex.Message);
				}
			}

			private static void Usage(string message = "")
			{
				Console.WriteLine(message);
				Console.WriteLine(
					@"usage in order to add transfer to file: \n ManualPacnetApp.exe <'send'> <customerId> <amount> [fileName = EZBOB] [currencyCode = GBP] [description = ezbob]");
				Console.WriteLine(
					@"usage in order to close file and begin proccessing:\n ManualPacnetApp.exe <close> [fileName = EZBOB]");
			}

			private static string GetCustomerNameForPacNet(Customer customer)
			{
				string name = string.Format("{0} {1}", customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname);

				if (name.Length > 18)
				{
					name = customer.PersonalInfo.Surname;
				}

				if (name.Length > 18)
				{
					name = name.Substring(0, 17);
				}

				return name;
			}
		}

		public static void Init()
		{
			Bootstrap.Init();
		}
	}
}
