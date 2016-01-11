SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanStatesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanStatesSave
GO

IF TYPE_ID('NL_LoanStatesList') IS NOT NULL
	DROP TYPE NL_LoanStatesList
GO

CREATE TYPE NL_LoanStatesList AS TABLE (
	[LoanID] BIGINT NOT NULL,
	[InsertDate] DATETIME NOT NULL,
	[NumberOfPayments] INT NOT NULL,
	[OutstandingPrincipal] DECIMAL(18, 6) NOT NULL,
	[OutstandingInterest] DECIMAL(18, 6) NOT NULL,
	[OutstandingFee] DECIMAL(18, 6) NOT NULL,
	[PaidPrincipal] DECIMAL(18, 6) NOT NULL,
	[PaidInterest] DECIMAL(18, 6) NOT NULL,
	[PaidFee] DECIMAL(18, 6) NOT NULL,
	[LateDays] INT NOT NULL,
	[LatePrincipal] DECIMAL(18, 6) NOT NULL,
	[LateInterest] DECIMAL(18, 6) NOT NULL,
	[WrittenOffPrincipal] DECIMAL(18, 6) NOT NULL,
	[WrittenOffInterest] DECIMAL(18, 6) NOT NULL,
	[WrittentOffFees] DECIMAL(18, 6) NOT NULL,
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
		[NumberOfPayments],
		[OutstandingPrincipal],
		[OutstandingInterest],
		[OutstandingFee],
		[PaidPrincipal],
		[PaidInterest],
		[PaidFee],
		[LateDays],
		[LatePrincipal],
		[LateInterest],
		[WrittenOffPrincipal],
		[WrittenOffInterest],
		[WrittentOffFees],
		[Notes]
	) SELECT
		[LoanID],
		[InsertDate],
		[NumberOfPayments],
		[OutstandingPrincipal],
		[OutstandingInterest],
		[OutstandingFee],
		[PaidPrincipal],
		[PaidInterest],
		[PaidFee],
		[LateDays],
		[LatePrincipal],
		[LateInterest],
		[WrittenOffPrincipal],
		[WrittenOffInterest],
		[WrittentOffFees],
		[Notes]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


