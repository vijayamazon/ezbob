namespace StrategiesTestTool
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Strategies.ScoreCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;

	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Not enough parameter");
				Usage();
				return;
			}

			AConnection db = new SqlConnection(new Ezbob.Context.Environment(), new ConsoleLog());

			switch (args[0])
			{
				case "medal":
					try
					{
						decimal annualTurnover = decimal.Parse(args[1]);
						int experianScore = int.Parse(args[2]);
						decimal mpSeniorityYears = decimal.Parse(args[3]);
						decimal positiveFeedbackCount = decimal.Parse(args[4]);
						var maritalStatus = (MaritalStatus) int.Parse(args[5]);
						var gender = (Gender) int.Parse(args[6]);
						int numberOfStores = int.Parse(args[7]);
						bool firstRepaymentDatePassed = bool.Parse(args[8]);
						decimal ezbobSeniority = decimal.Parse(args[9]);
						int ezbobNumOfLoans = int.Parse(args[10]);
						int ezbobNumOfLateRepayments = int.Parse(args[11]);
						int ezbobNumOfEarlyReayments = int.Parse(args[12]);
						
						var msc = new MedalScoreCalculator(db, new ConsoleLog());
						var response = msc.CalculateMedalScore(annualTurnover,
						                                       experianScore,
						                                       mpSeniorityYears,
						                                       positiveFeedbackCount,
						                                       maritalStatus,
						                                       gender,
						                                       numberOfStores,
						                                       firstRepaymentDatePassed,
						                                       ezbobSeniority,
						                                       ezbobNumOfLoans,
						                                       ezbobNumOfLateRepayments,
						                                       ezbobNumOfEarlyReayments);

					}
					catch (Exception ex)
					{
						Console.WriteLine("Error in parsing the parameters. \n {0}", ex.Message);
						Usage();
					}
					break;
				default:
					Console.WriteLine("unknown strategy name");
					Usage();
					break;
			}
		}

		private static void Usage()
		{
			Console.WriteLine("Usage: StrategiesTestTool.exe <strategy name> [parameters...]");
			Console.WriteLine("Strategy Score Medal Calculation: name: medal, parameters: annualTurnover experianScore mpSeniorityYears positiveFeedbackCount maritalStatus gender numberOfStores firstRepaymentDatePassed ezbobSeniority ezbobNumOfLoans ezbobNumOfLateRepayments ezbobNumOfEarlyReayments");
			Console.WriteLine("maritalStatus: Married=0,Single=1,Divorced=2,Widowed=3,LivingTogether=4,Separated=5,Other=6 \n gender: M=0,F=1, firstRepaymentDatePassed: true/false");
			Console.WriteLine("Example: medal 125000 740 8 10000 0 0 0 false 1.2 0 0 0");
		}
	}
}
