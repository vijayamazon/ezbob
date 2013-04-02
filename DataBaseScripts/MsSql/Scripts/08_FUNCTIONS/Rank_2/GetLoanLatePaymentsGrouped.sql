IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLoanLatePaymentsGrouped]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetLoanLatePaymentsGrouped]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLoanLatePaymentsGrouped]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT dbo.Loan.RequestCashId, SUM(lp.LatePaymentsAmount) LatePaymentsAmount FROM 
	dbo.Loan LEFT OUTER JOIN GetLatePaymentsGrouped() as lp on lp.LoanId = dbo.Loan.Id
	Group by dbo.Loan.RequestCashId
)
GO
