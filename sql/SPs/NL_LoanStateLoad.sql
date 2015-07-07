IF OBJECT_ID('NL_LoanStateLoad') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanStateLoad AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE NL_LoanStateLoad
@LoanID int, 
	@StateDate datetime 
AS
BEGIN
	
	SET NOCOUNT ON;

	--SELECT 




END
GO