IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUnpaidLoans]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUnpaidLoans]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUnpaidLoans] 
AS
BEGIN
	SELECT 
		Id
	FROM 
		Loan
	WHERE
		Status != 'PaidOff'
END

GO



