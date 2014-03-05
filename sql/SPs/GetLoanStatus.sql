IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLoanStatus]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLoanStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLoanStatus] 
	(@LoanId INT)
AS
BEGIN
	SELECT 
		l.Status 
	FROM 
		loan l
	WHERE
		id = @LoanId
END
GO
