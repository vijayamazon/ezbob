SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanRolloversSave') IS NOT NULL
	DROP PROCEDURE NL_LoanRolloversSave
GO

IF OBJECT_ID('NL_LoanRolloverUpdate') IS NOT NULL
	DROP PROCEDURE NL_LoanRolloverUpdate
GO

IF TYPE_ID('NL_LoanRolloversList') IS NOT NULL
	DROP TYPE NL_LoanRolloversList
GO

CREATE TYPE NL_LoanRolloversList AS TABLE (
	[LoanHistoryID] BIGINT NOT NULL,
	[CreatedByUserID] INT NOT NULL,
	[DeletedByUserID] INT NULL,
	[LoanFeeID] BIGINT NULL,	
	[CreationTime] DATETIME NOT NULL,
	[ExpirationTime] DATETIME NOT NULL,
	[CustomerActionTime] DATETIME NULL,
	[IsAccepted] BIT NULL,
	[DeletionTime] DATETIME NULL
)
GO

CREATE PROCEDURE NL_LoanRolloversSave
@Tbl NL_LoanRolloversList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanRollovers (
		[LoanHistoryID],
		[CreatedByUserID],
		[DeletedByUserID],
		[LoanFeeID],		
		[CreationTime],
		[ExpirationTime],
		[CustomerActionTime],
		[IsAccepted],
		[DeletionTime]
	) SELECT
		[LoanHistoryID],
		[CreatedByUserID],
		[DeletedByUserID],
		[LoanFeeID],		
		[CreationTime],
		[ExpirationTime],
		[CustomerActionTime],
		[IsAccepted],
		[DeletionTime]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


