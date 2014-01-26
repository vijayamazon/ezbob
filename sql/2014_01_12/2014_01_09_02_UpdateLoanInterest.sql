IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLoanInterest]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLoanInterest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLoanInterest]
	(@LoanId INT,
	 @InterestDue DECIMAL(18,4))
AS
BEGIN
	UPDATE
		Loan
	SET
		InterestDue = @InterestDue
	WHERE
		Id != @LoanId
END

GO



