IF OBJECT_ID('NL_AcceptedRollovers') IS NULL
	EXECUTE('CREATE PROCEDURE NL_AcceptedRollovers AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_AcceptedRollovers]
@LoanID BIGINT,
@Now DATETIME  = NULL
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

	FROM  [dbo].[NL_LoanRollovers] r  --join [dbo].[NL_LoanHistory]  h on h.LoanHistoryID = r.LoanHistoryID and h.[LoanID]=@LoanID and @Now between r.[CreationTime] and r.[ExpirationTime]	
END

GO