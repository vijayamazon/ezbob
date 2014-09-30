namespace Reports {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;

	public class ReportQuery {
		public ReportQuery() : this(null) {}

		public ReportQuery(Report rpt) {
			Report = rpt;

			m_oValues = new SortedDictionary<string, dynamic>();

			m_oArgMatch = new SortedDictionary<string, string>();

			m_oArgMatch[DateStartArg] = Report.DateRangeArg;
			m_oArgMatch[DateEndArg] = Report.DateRangeArg;
			m_oArgMatch[ShowNonCashArg] = Report.ShowNonCashArg;
			m_oArgMatch[CustomerIDArg] = Report.CustomerArg;
			m_oArgMatch[CustomerNameOrEmailArg] = Report.CustomerArg;

			if (rpt != null) {
				StoredProcedure = rpt.StoredProcedure;
				Columns = rpt.Columns;
			} // if
		} // constructor

		public ReportQuery(Report rpt, DateTime dateStart, DateTime dateEnd) : this(rpt) {
			DateStart = dateStart;
			DateEnd = dateEnd;
		} // constructor

		public ReportQuery(Report rpt, DateTime dateStart, DateTime dateEnd, string customer, bool? nonCash)
			: this(rpt)
		{
			DateStart = dateStart;
			DateEnd = dateEnd;

			string sUserKey = customer.Trim();

			if (sUserKey != string.Empty)
			{
				int nUserID = 0;

				if (int.TryParse(sUserKey, out nUserID))
					UserID = nUserID;
				else
					UserID = null;

				UserNameOrEmail = sUserKey;
			} // if sUserKey is not empty

			if (nonCash.HasValue)
			{
				ShowNonCashTransactions = nonCash.Value ? 1 : 0;
			}
		} // constructor

		public string StoredProcedure { get; set; }

		public ColumnInfo[] Columns { get; set; }

		public int? UserID {
			get { return this[CustomerIDArg]; }
			set { this[CustomerIDArg] = value; }
		} // UserID

		public string UserNameOrEmail {
			get { return this[CustomerNameOrEmailArg]; }
			set { this[CustomerNameOrEmailArg] = value; }
		} // UserNameOrEmail

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

		public void Execute(AConnection oDB, Func<SafeReader, bool, ActionResult> oFunc) {
			oDB.ForEachRowSafe(oFunc, StoredProcedure, CreateParameters());
		} // Execute

		private QueryParameter[] CreateParameters() {
			var oParams = new List<QueryParameter>();

			foreach (KeyValuePair<string, dynamic> pair in m_oValues) {
				if (Report.Arguments.ContainsKey(m_oArgMatch[pair.Key]))
					oParams.Add(new QueryParameter(pair.Key, pair.Value ?? DBNull.Value));
			} // for each

			return oParams.ToArray();
		} // CreateParameters

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

		private readonly SortedDictionary<string, string> m_oArgMatch;

		private const string ShowNonCashArg = "@ShowNonCashTransactions";
		private const string DateStartArg = "@DateStart";
		private const string DateEndArg = "@DateEnd";
		private const string CustomerNameOrEmailArg = "@CustomerNameOrEmail";
		private const string CustomerIDArg = "@CustomerID";
	} // class ReportQuery
} // namespace Reports
