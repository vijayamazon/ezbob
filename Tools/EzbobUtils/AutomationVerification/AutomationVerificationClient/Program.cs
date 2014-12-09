namespace AutomationVerification
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using AutomationCalculator.AutoDecision;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html.Tags;
	using Reports;

	public class Program
	{
		public static ASafeLog Log;

		public static void Main()
		{
			try
			{
				var from = DateTime.Today.AddDays(-1);// new DateTime(2013, 10, 01);//todo change
				var to = DateTime.Today;
				Log = new FileLog("AutomationVerification", bUtcTimeInName:true, bAppend:true, sPath: @"C:\EzUtils\AutomationVerification\");
				Log.Debug("Running Begin");
				var systemDecisions = GetAutomaticDecisions(from, to);
				var verificitaionDecisions = GetVerificationDecisions(systemDecisions);
				var report = GetComparisonReport(systemDecisions, verificitaionDecisions);
				//SendReport(report);
				SendReport(to, GetTable(report), Log);
				Log.Debug("Running End");
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		//private static void SendReport(IEnumerable<VerificationReport> report)
		//{
		//	foreach (var verificationReport in report)
		//	{
		//		Console.WriteLine("{0} {1} {2} {3} {4} {5}", verificationReport.CashRequestId, verificationReport.CustomerId, verificationReport.SystemDecision, verificationReport.SystemComment, verificationReport.VerificationDecision, verificationReport.VerificationComment);
		//	}
		//}

		private static void SendReport(DateTime oDate, DataTable data, ASafeLog oLog)
		{
			oLog.Debug("Generating automation verification report...");

			var oDb = new SqlConnection();

			oLog.Debug("Loading Automation verification report metadata from db...");

			var reportMetaData = new Report(oDb, "RPT_AUTOMATION_VERIFICATION");

			var rh = new BaseReportHandler(oDb, oLog);

			var sender = new ReportDispatcher(oDb, oLog);

			var email = new ReportEmail();

			email.ReportBody.Append(new H2().Append(new Text(reportMetaData.GetTitle(oDate))));

			email.ReportBody.Append(
				rh.TableReport(new ReportQuery(reportMetaData, oDate, oDate), data)
			);

			var sTo = new StringBuilder();

			sTo.Append(reportMetaData.ToEmail);

			oLog.Debug("Sending report...");

			sender.Dispatch(
				"Automation Verification " + oDate.ToString("MMMM d yyyy", CultureInfo.InvariantCulture),
				oDate,
				email.HtmlBody,
				null,
				sTo.ToString()
			);

			oLog.Debug("Automation verification report generation complete.");
		}

		private static DataTable GetTable(IEnumerable<VerificationReport> reportRows)
		{
				var dt = new DataTable();

				dt.Columns.Add("CashRequestId", typeof (int));
				dt.Columns.Add("CustomerId", typeof (int));
				dt.Columns.Add("SystemDecision", typeof (string));
				dt.Columns.Add("SystemComment", typeof (string));
				dt.Columns.Add("VerificationDecision", typeof (string));
				dt.Columns.Add("VerificationComment", typeof (string));
				dt.Columns.Add("SystemCalculatedSum", typeof (int));
				dt.Columns.Add("SystemApprovedSum", typeof(int));
				dt.Columns.Add("VerificationApprovedSum", typeof (int));
				dt.Columns.Add("Css", typeof (string));

				foreach (var vr in reportRows)
				{
					dt.Rows.Add(vr.CashRequestId, vr.CustomerId, vr.SystemDecision, vr.SystemComment, vr.VerificationDecision,
						vr.VerificationComment, vr.SystemCalculatedSum, vr.SystemApprovedSum, vr.VerificationApprovedSum, vr.IsMatch ? "Successful" : "Failed unmatched");
				}

				return dt;
		}

		private static IEnumerable<VerificationReport> GetComparisonReport(Dictionary<int, AutoDecision> systemDecisions, Dictionary<int, Dictionary<DecisionType, AutoDecision>> verificitaionDecisions)
		{
			var verificationReportList = new List<VerificationReport>();
			foreach (int cashRequest in systemDecisions.Keys)
			{
				if (systemDecisions[cashRequest].SystemDecision == Decision.Reject &&
					systemDecisions[cashRequest].Comment.Contains("Auto Re-Reject"))
				{
					var verificationDesicion = verificitaionDecisions[cashRequest][DecisionType.AutoReReject];
					Compare(verificationReportList, systemDecisions[cashRequest], verificationDesicion, cashRequest);
				}

				else if (systemDecisions[cashRequest].SystemDecision == Decision.Reject &&
				         systemDecisions[cashRequest].Comment.Contains("AutoReject"))
				{
					var verificationDesicion = verificitaionDecisions[cashRequest][DecisionType.AutoReject];
					Compare(verificationReportList, systemDecisions[cashRequest], verificationDesicion, cashRequest);
				}else if (systemDecisions[cashRequest].SystemDecision == Decision.Approve &&
				         systemDecisions[cashRequest].Comment.Contains("Auto Re-Approval"))
				{
					var verificationDesicion = verificitaionDecisions[cashRequest][DecisionType.AutoReApprove];
					Compare(verificationReportList, systemDecisions[cashRequest], verificationDesicion, cashRequest);
				}else if(systemDecisions[cashRequest].SystemDecision == Decision.Manual)
				{
					var areEqual = true;
					var sb = new StringBuilder();
					foreach (var verificationDecision in verificitaionDecisions[cashRequest])
					{
						if (verificationDecision.Value.SystemDecision != Decision.Manual)
						{
							areEqual = false;
							Compare(verificationReportList, systemDecisions[cashRequest], verificationDecision.Value, cashRequest);
							break;
						}
						else
						{
							sb.AppendLine(verificationDecision.Value.Comment + "<br>");
						}
					}
					if (areEqual)
					{
						verificationReportList.Add(new VerificationReport
						{
							CashRequestId = cashRequest,
							CustomerId = systemDecisions[cashRequest].CustomerId,
							SystemDecision = systemDecisions[cashRequest].SystemDecision,
							SystemComment = systemDecisions[cashRequest].Comment,
							VerificationDecision = Decision.Manual,
							VerificationComment = sb.ToString(),
							SystemCalculatedSum = systemDecisions[cashRequest].SystemCalculatedSum,
							SystemApprovedSum = systemDecisions[cashRequest].SystemApprovedSum,
							VerificationApprovedSum = 0,
							IsMatch = true
						});
					}
				}
				else
				{
					foreach (var verificationDecision in verificitaionDecisions[cashRequest])
					{
						Compare(verificationReportList, systemDecisions[cashRequest], verificationDecision.Value, cashRequest);
					}
				}
			}

			return verificationReportList;
		}

		private static void Compare(List<VerificationReport> verificationReportList, AutoDecision systemDecision, AutoDecision verificationDesicion, int cashRequest)
		{
			//if (systemDecision.SystemDecision != verificationDesicion.SystemDecision)
			//{
				verificationReportList.Add(new VerificationReport
				{
					CashRequestId = cashRequest,
					CustomerId = systemDecision.CustomerId,
					SystemDecision = systemDecision.SystemDecision,
					SystemComment = systemDecision.Comment,
					VerificationDecision = verificationDesicion.SystemDecision,
					VerificationComment = verificationDesicion.Comment,
					SystemCalculatedSum = systemDecision.SystemCalculatedSum,
					SystemApprovedSum = systemDecision.SystemApprovedSum,
					VerificationApprovedSum = verificationDesicion.SystemCalculatedSum,
					IsMatch = systemDecision.SystemDecision == verificationDesicion.SystemDecision
				});
			//}
		}

		private static Dictionary<int, Dictionary<DecisionType,AutoDecision>> GetVerificationDecisions(Dictionary<int, AutoDecision> decisions)
		{
			var verificationDecisions = new Dictionary<int, Dictionary<DecisionType,AutoDecision>>();
			var c = new SqlConnection(Log);
			var db = new DbHelper(c, Log);
			var rejectionConstants = db.GetRejectionConfigs();
			var aj = new AutoRejectionCalculator(c, Log, rejectionConstants);
			var arr = new AutoReRejectionCalculator(c, Log);
			var ara = new AutoReApprovalCalculator(c, Log);
			var aa = new AutoApprovalCalculator(c, Log);

			foreach (var autoDecision in decisions.Values)
			{
				var autoDecisionsDict = new Dictionary<DecisionType, AutoDecision>();
				string reason;
				if (db.IsOffline(autoDecision.CustomerId))
				{
					autoDecisionsDict.Add(DecisionType.IsOffline, new AutoDecision
					{
						CashRequestId = autoDecision.CashRequestId,
						CustomerId = autoDecision.CustomerId,
						SystemDecision = Decision.Manual,
						Comment = "Offline Customer, no auto rules"
					});
					verificationDecisions.Add(autoDecision.CashRequestId, autoDecisionsDict);
					continue;
				}

				bool isAutoRejected = aj.IsAutoRejected(autoDecision.CustomerId, out reason);
				autoDecisionsDict.Add(DecisionType.AutoReject, new AutoDecision
					{
						CashRequestId = autoDecision.CashRequestId,
						CustomerId = autoDecision.CustomerId,
						SystemDecision = isAutoRejected ? Decision.Reject : Decision.Manual,
						Comment = reason
					});

				bool isAutoReRejected = arr.IsAutoReRejected(autoDecision.CustomerId, autoDecision.CashRequestId, out reason);
				autoDecisionsDict.Add(DecisionType.AutoReReject, new AutoDecision
				{
					CashRequestId = autoDecision.CashRequestId,
					CustomerId = autoDecision.CustomerId,
					SystemDecision = isAutoReRejected ? Decision.Reject : Decision.Manual,
					Comment = reason
				});

				int amount = 0;

				bool isAutoReApproved = ara.IsAutoReApproved(autoDecision.CustomerId, autoDecision.CashRequestId, out reason, out amount);
				autoDecisionsDict.Add(DecisionType.AutoReApprove, new AutoDecision
				{
					CashRequestId = autoDecision.CashRequestId,
					CustomerId = autoDecision.CustomerId,
					SystemDecision = isAutoReApproved ? Decision.Approve : Decision.Manual,
					SystemCalculatedSum = isAutoReApproved ? amount : 0,
					Comment = reason
				});

				bool isAutoApproved = aa.IsAutoApproved(autoDecision.CustomerId, out reason, out amount);
				autoDecisionsDict.Add(DecisionType.AutoApprove, new AutoDecision
				{
					CashRequestId = autoDecision.CashRequestId,
					CustomerId = autoDecision.CustomerId,
					SystemDecision = isAutoApproved ? Decision.Approve : Decision.Manual,
					SystemCalculatedSum = amount,
					Comment = reason
				});

				verificationDecisions.Add(autoDecision.CashRequestId, autoDecisionsDict);
			}

			return verificationDecisions;
		}

		private static Dictionary<int, AutoDecision> GetAutomaticDecisions(DateTime from, DateTime to)
		{
			var db = new DbHelper(new SqlConnection(Log), Log);
			var decisionsTable = db.GetAutoDecisions(from, to);
			return decisionsTable.ToDictionary<SafeReader, int, AutoDecision>(row => row["CashRequestId"],
				row => new AutoDecision {
					CashRequestId = row["CashRequestId"], 
					CustomerId = row["CustomerId"], 
					SystemDecision = (Decision) Enum.Parse(typeof (Decision), row["SystemDecision"].ToString()), 
					SystemDecisionDate = row["SystemDecisionDate"], 
					SystemCalculatedSum = row["SystemCalculatedSum"], 
					SystemApprovedSum = (row["ManagerApprovedSum"] == null || string.IsNullOrEmpty(row["ManagerApprovedSum"].ToString())) ? 0 : row["ManagerApprovedSum"], 
					MedalType = (Medal) Enum.Parse(typeof (Medal), row["MedalType"].ToString()), 
					RepaymentPeriod = row["RepaymentPeriod"], 
					ScorePoints = row["ScorePoints"], 
					ExpirianRating = row["ExpirianRating"], 
					AnualTurnover = row["AnualTurnover"], 
					InterestRate = row["InterestRate"], 
					HasLoans = row["HasLoans"], 
					Comment = row["Comment"],
			});
		}
	}
}
