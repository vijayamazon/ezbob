namespace Reports.TraficReport
{
	using System;
	using System.Data;
	using Ezbob.Database;
	using System.Collections.Generic;
	using Ezbob.Logger;

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
			
			var spCustomers = new CustomersData(m_oDB, this) {DateStart = from, DateEnd = to};
			spCustomers.ForEachResult<CustomersData.ResultRow>(row =>
				{
					Source source = SourceRefMapper.GetSourceBySourceRef(row.ReferenceSource, row.GoogleCookie);
					if (traficReportDict.ContainsKey(source))
					{
						traficReportDict[source].LoanAmount += row.LoanAmount;
						traficReportDict[source].Loans += row.NumOfLoans;
						traficReportDict[source].Registrations += row.Customers;
					}
					else
					{
						traficReportDict[source] = new TraficReportRow
							{
								LoanAmount = row.LoanAmount,
								Loans = row.NumOfLoans,
								Registrations = row.Customers,
								Channel = source
							};
					}
					return ActionResult.Continue;
				});

			var spAnalytics = new AnalyticsData(m_oDB, this) { DateStart = from, DateEnd = to };
			spAnalytics.ForEachResult<AnalyticsData.ResultRow>(row =>
			{
				Source source = SourceRefMapper.GetSourceByAnalytics(row.Source);
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
			var reprortQuery = new ReportQuery(report) {
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
			oOutput.Columns.Add("Applications", typeof(int));
			oOutput.Columns.Add("Loans", typeof(int));
			oOutput.Columns.Add("LoanAmount", typeof(int));
			oOutput.Columns.Add("Cost", typeof(int));
			oOutput.Columns.Add("NewCustomerCost", typeof(int));
			oOutput.Columns.Add("ROI", typeof(int));

			foreach (var row in traficReportDict.Values)
			{
				ToRow(oOutput, row);
			}
			return oOutput;
		}

		private void ToRow(DataTable tbl, TraficReportRow row)
		{
			tbl.Rows.Add(
					row.Channel.ToString(), row.Visits, row.Visitors, row.Registrations, row.Applications,
					row.Loans, row.LoanAmount, row.Cost, row.NewCustomerCost,row.ROI);
		}

		private readonly AConnection m_oDB;

	}
}
