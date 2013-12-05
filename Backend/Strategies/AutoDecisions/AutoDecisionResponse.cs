namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;

	public class AutoDecisionResponse
	{
		public AutoDecisionResponse(AutoDecisionRequest request)
		{
			IsReRejected = request.IsReRejected;
			AutoRejectReason = request.AutoRejectReason;
			CreditResult = request.CreditResult;
			UserStatus = request.UserStatus;
			SystemDecision = request.SystemDecision;
			ModelLoanOffer = request.ModelLoanOffer;
			LoanOffer_UnderwriterComment = request.LoanOffer_UnderwriterComment;
			LoanOffer_OfferValidDays = request.LoanOffer_OfferValidDays;
			App_ApplyForLoan = request.App_ApplyForLoan;
			App_ValidFor = request.App_ValidFor;
			LoanOffer_EmailSendingBanned_new = request.LoanOffer_EmailSendingBanned_new;
			IsAutoApproval = request.IsAutoApproval;
		}

		public bool IsReRejected { get; set; }
		public string AutoRejectReason { get; set; }
		public string CreditResult { get; set; }
		public string UserStatus { get; set; }
		public string SystemDecision { get; set; }
		public int ModelLoanOffer { get; set; }
		public string LoanOffer_UnderwriterComment { get; set; }
		public double LoanOffer_OfferValidDays { get; set; }
		public DateTime? App_ApplyForLoan { get; set; }
		public DateTime App_ValidFor { get; set; }
		public bool LoanOffer_EmailSendingBanned_new { get; set; }
		public bool IsAutoApproval { get; set; }
	}
}
