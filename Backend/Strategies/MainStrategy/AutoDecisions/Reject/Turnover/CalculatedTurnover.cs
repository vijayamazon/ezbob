namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CalculatedTurnover {

		public CalculatedTurnover() {
			m_nAnnual = null;
			m_nQuarter = null;

			m_oEcomm = new GroupTurnover<EcommPeriodValue>();
			m_oAccounting = new GroupTurnover<AccountingPeriodValue>();
			m_oHmrc = new GroupTurnover<HmrcPeriodValue>();
			m_oYodlee = new GroupTurnover<YodleePeriodValue>();
		} // constructor

		public void Add(SafeReader sr, ASafeLog oLog) {
			Row r = sr.Fill<Row>();

			r.WriteToLog(oLog);

			if (!r.IsTotal)
				return;

			m_oEcomm.Add(r);
			m_oAccounting.Add(r);
			m_oHmrc.Add(r);
			m_oYodlee.Add(r);

			m_nAnnual = null;
			m_nQuarter = null;
		} // Add

		public decimal Annual {
			get {
				if (m_nAnnual != null)
					return m_nAnnual.Value;

				m_nAnnual = Max(
					m_oEcomm[12],      m_oEcomm[6]      * 2, m_oEcomm[3]      * 4, m_oEcomm[1]      * 12,
					m_oAccounting[12], m_oAccounting[6] * 2, m_oAccounting[3] * 4, m_oAccounting[1] * 12,
					m_oHmrc[12],       m_oHmrc[6]       * 2, m_oHmrc[3]       * 4, m_oHmrc[1]       * 12,
					m_oYodlee[12],     m_oYodlee[6]     * 2, m_oYodlee[3]     * 4, m_oYodlee[1]     * 12
				);

				return m_nAnnual.Value;
			} // get
		} // Annual

		private decimal? m_nAnnual;

		public decimal Quarter {
			get {
				if (m_nQuarter != null)
					return m_nQuarter.Value;

				m_nQuarter = Max(
					m_oEcomm[3],      m_oEcomm[1],
					m_oAccounting[3], m_oAccounting[1],
					m_oHmrc[3],       m_oHmrc[1],
					m_oYodlee[3],     m_oYodlee[1]
				);

				return m_nQuarter.Value;
			} // get
		} // Quarter

		private decimal? m_nQuarter;

		private readonly GroupTurnover<EcommPeriodValue> m_oEcomm;
		private readonly GroupTurnover<AccountingPeriodValue> m_oAccounting;
		private readonly GroupTurnover<HmrcPeriodValue> m_oHmrc;
		private readonly GroupTurnover<YodleePeriodValue> m_oYodlee;

		private static decimal Max(params decimal[] args) {
			if (args.Length < 1)
				return 0;

			decimal nMax = args[0];

			for (int i = 1; i < args.Length; i++)
				nMax = Math.Max(nMax, args[i]);

			return nMax;
		} // Max

	} // class CalculatedTurnover
} // namespace
