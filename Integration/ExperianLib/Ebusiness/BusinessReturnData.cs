namespace ExperianLib.Ebusiness {
	using System;

	public abstract class BusinessReturnData {
		#region public

		public virtual bool IsError {
			get { return !string.IsNullOrEmpty(Error); }
		} // IsError

		#region property Error

		public virtual string Error {
			get { return m_sError; }
			protected set { m_sError = value; }
		} // Error

		private string m_sError;

		#endregion property Error

		#region property LastCheckDate

		public virtual DateTime? LastCheckDate {
			get { return m_oLastCheckDate; }
			protected set { m_oLastCheckDate = value; }
		} // LastCheckDate

		private DateTime? m_oLastCheckDate;

		#endregion property LastCheckDate

		public virtual bool IsDataExpired { get; set; }
		public virtual long ServiceLogID { get; private set; }

		#region property BureauScore

		public virtual decimal BureauScore {
			get { return m_nBureauScore; }
			protected set { m_nBureauScore = value; }
		} // BureauScore

		private decimal m_nBureauScore;

		#endregion property BureauScore

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

		#region property CacheHit

		public virtual bool CacheHit {
			get { return m_bCacheHit; }
			set { m_bCacheHit = value; }
		} // CacheHit

		private bool m_bCacheHit;

		#endregion property CacheHit

		public abstract bool IsLimited { get; }

		#endregion public

		#region protected

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

		#endregion constructors
	} // class BusinessReturnData
} // namespace
