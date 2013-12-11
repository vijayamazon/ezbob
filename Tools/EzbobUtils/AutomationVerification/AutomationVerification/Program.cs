using System;


namespace AutomationVerification
{
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using AutomationCalculator;
	using CommonLib;

	class Program
	{
		static void Main(string[] args)
		{
			var from = new DateTime(2013,10,01);//todo change
			var to = DateTime.Today;
			var systemDecisions = GetAutomaticDecisions(from, to);
			var verificitaionDecisions = GetVerificationDecisions(systemDecisions);
			var report = GetComparisonReport(systemDecisions, verificitaionDecisions);
			SendReport(report);
		}

		private static void SendReport(IEnumerable<VerificationReport> report)
		{
			foreach (var verificationReport in report)
			{
				Console.WriteLine("{0} {1} {2} {3} {4} {5}", verificationReport.CashRequestId, verificationReport.CustomerId, verificationReport.SystemDecision, verificationReport.SystemComment, verificationReport.VerificationDecision, verificationReport.VerificationComment);
			}
		}

		private static List<VerificationReport> GetComparisonReport(Dictionary<int, AutoDecision> systemDecisions, Dictionary<int, AutoDecision> verificitaionDecisions)
		{
			var verificationReportList = new List<VerificationReport>();
			foreach (int cashRequest in systemDecisions.Keys)
			{
				if (systemDecisions[cashRequest].SystemDecision != verificitaionDecisions[cashRequest].SystemDecision)
				{
					verificationReportList.Add(new VerificationReport
						{
							CashRequestId = cashRequest,
							CustomerId = systemDecisions[cashRequest].CustomerId,
							SystemDecision = systemDecisions[cashRequest].SystemDecision,
							SystemComment = systemDecisions[cashRequest].Comment,
							VerificationDecision = verificitaionDecisions[cashRequest].SystemDecision,
							VerificationComment = verificitaionDecisions[cashRequest].Comment
						});
				}
			}
			return verificationReportList;
		}

		private static Dictionary<int, AutoDecision> GetVerificationDecisions(Dictionary<int, AutoDecision> decisions)
		{
			var verificationDecisions = new Dictionary<int, AutoDecision>();
			var aj = new AutoRejectionCalculator();
			foreach (var autoDecision in decisions.Values)
			{
				string reason;
				bool isAutoRejected = aj.IsAutoRejected(autoDecision.CustomerId, out reason);
				verificationDecisions.Add(autoDecision.CashRequestId, new AutoDecision
					{
						CashRequestId = autoDecision.CashRequestId,
						CustomerId = autoDecision.CustomerId,
						SystemDecision = isAutoRejected ? Decision.Reject : Decision.Manual,
						Comment = reason
					});
			}

			return verificationDecisions;
		}

		private static Dictionary<int, AutoDecision> GetAutomaticDecisions(DateTime from, DateTime to)
		{
			var db = new DbHelper();
			var decisionsTable = db.GetAutoDecisions(from, to);
			var decisionsDict = new Dictionary<int, AutoDecision>();
			foreach (DataRow row in decisionsTable.Rows)
			{
				decisionsDict.Add(int.Parse(row["CashRequestId"].ToString()), new AutoDecision
					{
						CashRequestId = int.Parse(row["CashRequestId"].ToString()),
						CustomerId = int.Parse(row["CustomerId"].ToString()),
						SystemDecision = (Decision) Enum.Parse(typeof (Decision), row["SystemDecision"].ToString()),
						SystemDecisionDate = DateTime.Parse(row["SystemDecisionDate"].ToString()),
						SystemCalculatedSum = int.Parse(row["SystemCalculatedSum"].ToString()),
						MedalType = (Medal) Enum.Parse(typeof (Medal), row["MedalType"].ToString()),
						RepaymentPeriod = int.Parse(row["RepaymentPeriod"].ToString()),
						ScorePoints = double.Parse(row["ScorePoints"].ToString()),
						ExpirianRating = int.Parse(row["ExpirianRating"].ToString()),
						AnualTurnover = int.Parse(row["AnualTurnover"].ToString()),
						InterestRate = double.Parse(row["InterestRate"].ToString()),
						HasLoans = Boolean.Parse(row["HasLoans"].ToString()),
						Comment = row["Comment"].ToString(),
					});
			}
			return decisionsDict;
		}
	}
}
