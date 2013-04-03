IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetOpenCreditLineByUnderwriter]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetOpenCreditLineByUnderwriter]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetOpenCreditLineByUnderwriter]
()
RETURNS TABLE 
AS
RETURN 
(
	SELECT IdUnderwriter, SUM(SystemCalculatedSum - l.loanAmount) as OpenCreditLine from CashRequests 
LEFT OUTER JOIN 
(SELECT RequestCashId, SUM(LoanAmount) as loanAmount FROM Loan
WHERE RequestCashId IS NOT NULL
Group by RequestCashId) as l ON CashRequests.Id = l.RequestCashId
WHERE UnderwriterDecisionDate > (GetDate() - 24)
Group BY IdUnderwriter
)
GO
