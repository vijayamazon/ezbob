namespace ExperianLib.Ebusiness {
	using System;

	public abstract class BusinessReturnData {

		public virtual bool IsError {
			get { return !string.IsNullOrEmpty(Error); }
		} // IsError

		public virtual string Error {
			get { return m_sError; }
			protected set { m_sError = value; }
		} // Error

		private string m_sError;

		public virtual DateTime? LastCheckDate {
			get { return m_oLastCheckDate; }
			protected set { m_oLastCheckDate = value; }
		} // LastCheckDate

		private DateTime? m_oLastCheckDate;

		public virtual bool IsDataExpired { get; set; }
		public virtual long ServiceLogID { get; private set; }

		public virtual decimal BureauScore {
			get { return m_nBureauScore; }
			protected set { m_nBureauScore = value; }
		} // BureauScore

		private decimal m_nBureauScore;

		public virtual decimal MaxBureauScore { get; set; }
		public virtual decimal CreditLimit { get; set; }

		public virtual string CompanyName { get; set; }
		public virtual string AddressLine1 { get; set; }
		public virtual string AddressLine2 { get; set; }
		public virtual string AddressLine3 { get; set; }
		public virtual string AddressLine4 { get; set; }
		public virtual string AddressLine5 { get; set; }
		public virtual string PostCode { get; set; }

		public virtual DateTime? IncorporationDate { get; set; }

		public virtual bool CacheHit {
			get { return m_bCacheHit; }
			set { m_bCacheHit = value; }
		} // CacheHit

		private bool m_bCacheHit;

		public abstract bool IsLimited { get; }

		protected BusinessReturnData(string sError, decimal nBureauScore) {
			m_oLastCheckDate = DateTime.UtcNow;
			m_sError = sError;
			m_nBureauScore = nBureauScore;
		} // constructor

		protected BusinessReturnData(Exception ex) {
			m_sError = ex.Message;
		} // constructor

		protected BusinessReturnData(long nServiceLogID, DateTime lastCheckDate, bool bCacheHit) {
			m_sError = string.Empty;
			ServiceLogID = nServiceLogID;
			m_oLastCheckDate = lastCheckDate;
			m_bCacheHit = bCacheHit;
		} // constructor

	} // class BusinessReturnData
} // namespace
