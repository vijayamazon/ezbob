UPDATE dbo.ReportScheduler
SET Header = 'Id,Email,Full Name,Underwriter Decision,Underwriter Decision Date,Manager Approved Sum,Underwriter Comment,Loan Amount,CRM Status,Comment,Segment Type'
	, Fields = '!Id,Email,FullName,UnderwriterDecision,UnderwriterDecisionDate,ManagerApprovedSum,UnderwriterComment,LoanAmount,CRMStatus,Comment,SegmentType'
WHERE Type = 'RPT_SALE_STATS'
GO
