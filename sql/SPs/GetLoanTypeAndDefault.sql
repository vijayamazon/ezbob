IF OBJECT_ID('GetLoanTypeAndDefault') IS NULL 
	EXECUTE('CREATE PROCEDURE GetLoanTypeAndDefault AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetLoanTypeAndDefault
@LoanTypeID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		LoanTypeID,
		DefaultLoanTypeID
	FROM
		dbo.udfGetLoanTypeAndDefault(@LoanTypeID)
END
GO
