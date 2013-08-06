using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ezbob.Database;

namespace Reports {
	public class ReportQuery {
		public ReportQuery() : this(null) {}

		public ReportQuery(Report rpt) {
			Report = rpt;

			m_oValues = new SortedDictionary<string, dynamic>();

			m_oArgMatch = new SortedDictionary<string, string>();

			m_oArgMatch[DateStartArg] = Report.DateRangeArg;
			m_oArgMatch[DateEndArg] = Report.DateRangeArg;
			m_oArgMatch[ShowNonCashArg] = Report.ShowNonCashArg;

			if (rpt != null) {
				StoredProcedure = rpt.StoredProcedure;
				Columns = rpt.Columns;
			} // if
		} // constructor

		public ReportQuery(Report rpt, DateTime dateStart, DateTime dateEnd) : this(rpt) {
			DateStart = dateStart;
			DateEnd = dateEnd;
		} // constructor

		public string StoredProcedure { get; set; }

		public ColumnInfo[] Columns { get; set; }

		public int? ShowNonCashTransactions {
			get { return this[ShowNonCashArg]; }
			set { this[ShowNonCashArg] = value; }
		} // ShowNonCashTransactions

		public DateTime? DateStart {
			get { return this[DateStartArg]; }
			set { this[DateStartArg] = value; }
		} // DateStart

		public DateTime? DateEnd {
			get { return this[DateEndArg]; }
			set { this[DateEndArg] = value; }
		} // DateEnd

		public DataTable Execute(AConnection oDB) {
			return oDB.ExecuteReader(StoredProcedure, (
				from pair in m_oValues
				where Report.Arguments.ContainsKey(m_oArgMatch[pair.Key])
				select new QueryParameter(pair.Key, pair.Value)
			).ToArray());
		} // Execute

		private dynamic this[string sArgName] {
			get {
				if (m_oValues.ContainsKey(sArgName))
					return m_oValues[sArgName];

				return null;
			} // get
			set {
				if (value == null)
					if (m_oValues.ContainsKey(sArgName))
						m_oValues.Remove(sArgName);

				m_oValues[sArgName] = value;
			} // set
		} // indexer

		private readonly SortedDictionary<string, dynamic> m_oValues;

		public Report Report { get; set; }

		private SortedDictionary<string, string> m_oArgMatch;

		private const string ShowNonCashArg = "@ShowNonCashTransactions";
		private const string DateStartArg = "@DateStart";
		private const string DateEndArg = "@DateEnd";
	} // class ReportQuery
} // namespace Reports
