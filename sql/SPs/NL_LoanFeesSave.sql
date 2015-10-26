SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanFeesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanFeesSave
GO

IF TYPE_ID('NL_LoanFeesList') IS NOT NULL
	DROP TYPE NL_LoanFeesList
GO

CREATE TYPE NL_LoanFeesList AS TABLE (	
	[LoanID] BIGINT NULL,
	[LoanFeeTypeID] INT NOT NULL,
	[AssignedByUserID] INT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[CreatedTime] DATETIME NOT NULL,
	[AssignTime] DATETIME NOT NULL,
	[DeletedByUserID] INT NULL,
	[DisabledTime] DATETIME NULL,
	[Notes] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE NL_LoanFeesSave
@Tbl NL_LoanFeesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanFees (		
		[LoanID],
		[LoanFeeTypeID],
		[AssignedByUserID],
		[Amount],
		[CreatedTime],
		[AssignTime],
		[DeletedByUserID],
		[DisabledTime],
		[Notes]
	) SELECT		
		[LoanID],
		[LoanFeeTypeID],
		[AssignedByUserID],
		[Amount],
		[CreatedTime],
		[AssignTime],
		[DeletedByUserID],
		[DisabledTime],
		[Notes]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


