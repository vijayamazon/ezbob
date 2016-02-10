namespace AutomationCalculator.AutoDecision.AutoRejection.Models {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.Common;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class LGRejectionInputData : RejectionInputData {
		public virtual int CompanyID { get; set; }
		public virtual TypeOfBusiness TypeOfBusiness { get; set; }
		public virtual bool CompanyIsRegulated { get; set; }
		public virtual int LoanCount { get; set; }
		public virtual CustomerOriginEnum? CustomerOrigin { get; set; }
		public virtual LoanSourceName? LoanSource { get; set; }

		public virtual Guid? RequestID { get; set; }
		public virtual long? ResponseID { get; set; }
		public virtual int? ResponseHttpStatus { get; set; }

		public virtual List<string> ResponseErrors {
			get {
				if (this.responseErrors == null)
					this.responseErrors = new List<string>();

				return this.responseErrors;
			} // get

			set {
				if (this.responseErrors == null)
					this.responseErrors = new List<string>();

				this.responseErrors.Clear();

				if ((value == null) || (value.Count < 1))
					return;

				this.responseErrors.AddRange(value);
			} // set
		} // ResponseErrors

		public virtual bool HardReject { get; set; }
		public virtual Bucket? Bucket { get; set; }
		public virtual decimal? Score { get; set; }

		public virtual MatchingGradeRanges MatchingGradeRanges {
			get {
				if (this.matchingGradeRanges == null)
					this.matchingGradeRanges = new MatchingGradeRanges();

				return this.matchingGradeRanges;
			} // get

			set {
				if (this.matchingGradeRanges == null)
					this.matchingGradeRanges = new MatchingGradeRanges();

				this.matchingGradeRanges.Clear();

				if ((value == null) || (value.Count < 1))
					return;

				this.matchingGradeRanges.AddRange(value);
			} // set
		} // MatchingGradeRanges

		public override void InitData(RejectionInputData data) {
			base.InitData(data);

			var lgData = data as LGRejectionInputData;

			if (lgData == null)
				return;

			CompanyID = lgData.CompanyID;
			TypeOfBusiness = lgData.TypeOfBusiness;
			CompanyIsRegulated = lgData.CompanyIsRegulated;
			LoanCount = lgData.LoanCount;
			CustomerOrigin = lgData.CustomerOrigin;
			LoanSource = lgData.LoanSource;

			RequestID = lgData.RequestID;
			ResponseID = lgData.ResponseID;
			ResponseHttpStatus = lgData.ResponseHttpStatus;
			ResponseErrors = lgData.ResponseErrors;
			HardReject = lgData.HardReject;
			Bucket = lgData.Bucket;
			Score = lgData.Score;
			MatchingGradeRanges = lgData.MatchingGradeRanges;
		} // InitData

		private MatchingGradeRanges matchingGradeRanges;
		private List<string> responseErrors;
	} // class LGRejectionInputData
} // namespace
