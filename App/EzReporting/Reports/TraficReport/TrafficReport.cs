namespace Reports.TraficReport
{
	using System;
	using System.Data;
	using Ezbob.Database;
	using System.Collections.Generic;
	using Ezbob.Logger;
	using System.Linq;

	public class TrafficReport : SafeLog
	{
		public TrafficReport(AConnection oDB, ASafeLog oLog = null)
			: base(oLog)
		{
			m_oDB = oDB;
		}

		public KeyValuePair<ReportQuery, DataTable> CreateTrafficReport(Report report, DateTime from, DateTime to)
		{
			var traficReportDict = new Dictionary<Source, TraficReportRow>();

			var spCustomers = new CustomersData(m_oDB, this) { DateStart = from, DateEnd = to };
			spCustomers.ForEachResult<CustomersData.ResultRow>(row =>
				{
					Source source = SourceRefMapper.GetSourceBySourceRef(row.ReferenceSource, row.GoogleCookie);
					if (traficReportDict.ContainsKey(source))
					{
						traficReportDict[source].LoanAmount += row.LoanAmount;
						traficReportDict[source].Loans += row.NumOfLoans;
						traficReportDict[source].Registrations += row.Customers;
						traficReportDict[source].Applications += row.Applications;
						traficReportDict[source].NumOfApproved += row.NumOfApproved;
						traficReportDict[source].NumOfRejected += row.NumOfRejected;

					}
					else
					{
						traficReportDict[source] = new TraficReportRow
							{
								LoanAmount = row.LoanAmount,
								Loans = row.NumOfLoans,
								Registrations = row.Customers,
								Applications = row.Applications,
								NumOfApproved = row.NumOfApproved,
								NumOfRejected = row.NumOfRejected,
								Channel = source,
							};
					}
					return ActionResult.Continue;
				});

			var spAnalytics = new AnalyticsData(m_oDB, this) { DateStart = from, DateEnd = to };
			spAnalytics.ForEachResult<AnalyticsData.ResultRow>(row =>
			{
				Source source = SourceRefMapper.GetSourceByAnalytics(row.Source);

				//exclude all ezbob analytics as irrelevant
				if (source == Source.Ezbob)
				{
					return ActionResult.Continue;
				}

				if (traficReportDict.ContainsKey(source))
				{
					traficReportDict[source].Visitors += row.Visitors;
					traficReportDict[source].Visits += row.Visits;
				}
				else
				{
					traficReportDict[source] = new TraficReportRow
					{
						Visitors = row.Visitors,
						Visits = row.Visits,
						Channel = source
					};
				}
				return ActionResult.Continue;
			});

			var table = ToTable(traficReportDict);
			var reprortQuery = new ReportQuery(report)
			{
				DateStart = from,
				DateEnd = to
			};
			return new KeyValuePair<ReportQuery, DataTable>(reprortQuery, table);
		}

		private DataTable ToTable(Dictionary<Source, TraficReportRow> traficReportDict)
		{
			var oOutput = new DataTable();

			oOutput.Columns.Add("Channel", typeof(string));
			oOutput.Columns.Add("Visits", typeof(int));
			oOutput.Columns.Add("Visitors", typeof(int));
			oOutput.Columns.Add("Registrations", typeof(int));
			oOutput.Columns.Add("PercentOfRegistrations", typeof(decimal));
			oOutput.Columns.Add("Applications", typeof(int));
			oOutput.Columns.Add("NumOfApproved", typeof(int));
			oOutput.Columns.Add("NumOfRejected", typeof(int));
			oOutput.Columns.Add("Loans", typeof(int));
			oOutput.Columns.Add("LoanAmount", typeof(int));
			oOutput.Columns.Add("Cost", typeof(int));
			oOutput.Columns.Add("NewCustomerCost", typeof(int));
			oOutput.Columns.Add("ROI", typeof(int));
			oOutput.Columns.Add("Css", typeof (string));

			var rows = traficReportDict.Values.ToList().OrderByDescending(x => x.LoanAmount);

			var totalRow = new TraficReportRow { Channel = Source.Total, Css = "total" };
			foreach (var row in rows)
			{
				row.PercentOfRegistrations = row.Visitors == 0 ? 0M : (decimal)row.Registrations/row.Visitors;
				ToRow(oOutput, row);
				totalRow.Visits += row.Visits;
				totalRow.Visitors += row.Visitors;
				totalRow.Registrations += row.Registrations;
				totalRow.Applications += row.Applications;
				totalRow.NumOfRejected += row.NumOfApproved;
				totalRow.Loans += row.Loans;
				totalRow.LoanAmount += row.LoanAmount;
			}

			totalRow.PercentOfRegistrations = totalRow.Visitors == 0 ? 0M : (decimal)totalRow.Registrations / totalRow.Visitors;
			ToRow(oOutput, totalRow);

			return oOutput;
		}

		private void ToRow(DataTable tbl, TraficReportRow row)
		{
			tbl.Rows.Add(
					row.Channel.ToString(), row.Visits, row.Visitors, row.Registrations, row.PercentOfRegistrations, 
					row.Applications, row.NumOfApproved, row.NumOfRejected,
					row.Loans, row.LoanAmount, row.Cost, row.NewCustomerCost, row.ROI, row.Css);
		}

		private readonly AConnection m_oDB;

	}
}
