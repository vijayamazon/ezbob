IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLoanByRequestCashId]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetLoanByRequestCashId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLoanByRequestCashId]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	Select dbo.Loan.RequestCashId, Sum(Late30) as Late30Amount, 
Sum(Late30Num) as Late30, 
Sum(Late60) as Late60Amount, 
Sum(Late60Num) as Late60, 
Sum(Late90) as Late90Amount, 
Sum(Late90Num) as Late90, 
Sum(OnTime) as PaidAmount, 
Sum(OnTimeNum) as Paid, 
Sum(PastDues) as DefaultsAmount, 
Sum(PastDuesNum) as Defaults,
Sum(Balance) as Exposure
FROM dbo.Loan
Where dbo.Loan.RequestCashId IS NOT NULL 
Group By(dbo.Loan.RequestCashId) 
)
GO
