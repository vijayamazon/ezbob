-- SET QUOTED_IDENTIFIER ON
-- GO
-- IF OBJECT_ID('GetNewLoanIdByOldLoanId') IS NULL
	-- EXECUTE('CREATE PROCEDURE GetNewLoanIdByOldLoanId AS SELECT 1')
-- GO

-- ALTER PROCEDURE GetNewLoanIdByOldLoanId
-- @LoanID INT
-- AS
-- BEGIN
	-- SET NOCOUNT ON;

	-- SELECT TOP 1 LoanID AS NewLoanID FROM NL_loans WHERE OldLoanID=@LoanID
-- END
-- GO


