SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanStatesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanStatesSave
GO

IF TYPE_ID('NL_LoanStatesList') IS NOT NULL
	DROP TYPE NL_LoanStatesList
GO

CREATE TYPE NL_LoanStatesList AS TABLE (
	[LoanID] INT NOT NULL,
	[InsertDate] DATETIME NOT NULL,
	[OutstandingPrincipal] DECIMAL(18, 6) NOT NULL,
	[OutstandingInterest] DECIMAL(18, 6) NOT NULL,
	[OutstandingFee] DECIMAL(18, 6) NOT NULL,
	[PaidPrincipal] DECIMAL(18, 6) NOT NULL,
	[PaidInterest] DECIMAL(18, 6) NOT NULL,
	[PaidFee] DECIMAL(18, 6) NOT NULL,
	[Balance] DECIMAL(18, 6) NOT NULL,
	[LateDays] INT NOT NULL,
	[Notes] NVARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE NL_LoanStatesSave
@Tbl NL_LoanStatesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanStates (
		[LoanID],
		[InsertDate],
		[OutstandingPrincipal],
		[OutstandingInterest],
		[OutstandingFee],
		[PaidPrincipal],
		[PaidInterest],
		[PaidFee],
		[Balance],
		[LateDays],
		[Notes]
	) SELECT
		[LoanID],
		[InsertDate],
		[OutstandingPrincipal],
		[OutstandingInterest],
		[OutstandingFee],
		[PaidPrincipal],
		[PaidInterest],
		[PaidFee],
		[Balance],
		[LateDays],
		[Notes]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


