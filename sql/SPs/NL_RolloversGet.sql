IF OBJECT_ID('NL_RolloversGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_RolloversGet AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_RolloversGet]
@LoanID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[LoanRolloverID]
		,[LoanHistoryID]
		,[CreatedByUserID]
		,[DeletedByUserID]
		,[LoanFeeID]	
		,[CreationTime]
		,[ExpirationTime]
		,[CustomerActionTime]
		,[IsAccepted]
		,[DeletionTime]
	FROM  [dbo].[NL_LoanRollovers]
END

