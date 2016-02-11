SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanInterestFreezeSave') IS NOT NULL
	DROP PROCEDURE NL_LoanInterestFreezeSave
GO

IF TYPE_ID('NL_LoanInterestFreezeList') IS NOT NULL
	DROP TYPE NL_LoanInterestFreezeList
GO

CREATE TYPE NL_LoanInterestFreezeList AS TABLE (	
	[LoanID] BIGINT NOT NULL,
	[StartDate] DATE NULL,
	[EndDate] DATE NULL,
	[InterestRate] DECIMAL(18, 6) NOT NULL,
	[ActivationDate] DATETIME NULL,
	[DeactivationDate] DATETIME NULL,
	[AssignedByUserID] INT NOT NULL,
	[DeletedByUserID] INT NULL,
	[OldID] INT NULL
)
GO


CREATE PROCEDURE NL_LoanInterestFreezeSave
@Tbl NL_LoanInterestFreezeList READONLY
AS
BEGIN
	SET NOCOUNT ON;	

	INSERT INTO NL_LoanInterestFreeze (		
		[LoanID] ,
		[StartDate] ,
		[EndDate] ,
		[InterestRate],
		[ActivationDate] ,
		[DeactivationDate] ,
		[AssignedByUserID] ,
		[DeletedByUserID],
		[OldID]
	) SELECT		
		[LoanID] ,		
		[StartDate] ,
		[EndDate] ,
		[InterestRate],
		[ActivationDate] ,
		[DeactivationDate] ,
		[AssignedByUserID] ,
		[DeletedByUserID],
		[OldID]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


