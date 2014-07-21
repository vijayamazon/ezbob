namespace ExperianLib.Ebusiness {
	using System;

	public abstract class BusinessReturnData {
		#region public

		public virtual bool IsError {
			get { return !string.IsNullOrEmpty(Error); }
		} // IsError

		public virtual string Error { get; protected set; }
		public virtual DateTime? LastCheckDate { get; protected set; }
		public virtual bool IsDataExpired { get; set; }
		public virtual string OutputXml { get; private set; }
		public virtual long ServiceLogID { get; private set; }

		public virtual decimal BureauScore { get; protected set; }

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
		public virtual bool CacheHit { get; set; }

		public abstract bool IsLimited { get; }

		#endregion public

		#region protected

		protected BusinessReturnData(string sError, decimal nBureauScore) {
			LastCheckDate = DateTime.UtcNow;
			Error = sError;
			BureauScore = nBureauScore;
		} // constructor

		protected BusinessReturnData(Exception ex) {
			Error = ex.Message;
		} // constructor

		protected BusinessReturnData(long nServiceLogID, string outputXml, DateTime lastCheckDate) { 
			LastCheckDate = lastCheckDate;
			OutputXml = outputXml;
			ServiceLogID = nServiceLogID;
		} // constructor

		#endregion constructors
	} // class BusinessReturnData
} // namespace
