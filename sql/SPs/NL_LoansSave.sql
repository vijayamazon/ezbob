SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoansSave') IS NOT NULL
	DROP PROCEDURE NL_LoansSave
GO

IF TYPE_ID('NL_LoansList') IS NOT NULL
	DROP TYPE NL_LoansList
GO

CREATE TYPE NL_LoansList AS TABLE (
	[OfferID] BIGINT NOT NULL,
	[LoanTypeID] INT NOT NULL,
	[LoanStatusID] INT NULL,
	[EzbobBankAccountID] INT NULL,
	[LoanSourceID] INT NULL,
	[Position] INT NOT NULL,	
	[CreationTime] DATETIME NOT NULL,	
	[Refnum] NVARCHAR(50) NULL,
	[DateClosed] DATETIME NULL,	
	[InterestOnlyRepaymentCount] INT NULL,
	[OldLoanID] INT NOT NULL
)
GO
	
CREATE PROCEDURE NL_LoansSave
@Tbl NL_LoansList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_Loans (
		[OfferID],
		[LoanTypeID],
		[LoanStatusID],
		[EzbobBankAccountID],
		[LoanSourceID],
		[Position],
		[CreationTime],
		[Refnum],
		[DateClosed],
		[InterestOnlyRepaymentCount],
		[OldLoanID]
	) SELECT
		[OfferID],
		[LoanTypeID],
		[LoanStatusID],
		[EzbobBankAccountID],
		[LoanSourceID],
		[Position],
		[CreationTime],
		[Refnum],
		[DateClosed],
		[InterestOnlyRepaymentCount],
		[OldLoanID]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO
