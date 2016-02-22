IF OBJECT_ID('NL_LoanUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanUpdate AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_LoanUpdate]	
	@LoanID bigint,
	@LoanStatusID int = NULL,
	@DateClosed datetime = NULL	
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CDate DATETIME;

	SET @CDate = ISNULL(@DateClosed, GETDATE())
	
	-- update loan status and/or close date	for loan
	UPDATE [NL_Loans] SET [LoanStatusID] = ISNULL(@LoanStatusID, LoanStatusID),	[DateClosed] = ISNULL(@CDate, DateClosed)	WHERE [LoanID] = @LoanID;	
		
END