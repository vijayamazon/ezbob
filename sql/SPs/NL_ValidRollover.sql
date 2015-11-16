IF OBJECT_ID('NL_ValidRollover') IS NULL
	EXECUTE('CREATE PROCEDURE NL_ValidRollover AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_ValidRollover]
@LoanID BIGINT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
	[LoanRolloverID]
	, r.[LoanHistoryID]
	,[CreatedByUserID]
	,[DeletedByUserID]
	,[LoanFeeID]	
	,[CreationTime]
	,[ExpirationTime]
	,[CustomerActionTime]
	,[IsAccepted]
	,[DeletionTime]

	FROM  [dbo].[NL_LoanRollovers] r join [dbo].[NL_LoanHistory]  h on h.LoanHistoryID = r.LoanHistoryID 
	and h.[LoanID]=@LoanID and @Now between r.[CreationTime] and r.[ExpirationTime]	
END

GO