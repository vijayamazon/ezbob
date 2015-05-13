SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoansSave') IS NOT NULL
	DROP PROCEDURE NL_LoansSave
GO

IF TYPE_ID('NL_LoansList') IS NOT NULL
	DROP TYPE NL_LoansList
GO

CREATE TYPE NL_LoansList AS TABLE (
	[OfferID] INT NULL,
	[LoanTypeID] INT NOT NULL,
	[RepaymentIntervalTypeID] INT NULL,
	[LoanStatusID] INT NULL,
	[EzbobBankAccountID] INT NULL,
	[LoanSourceID] INT NULL,
	[Position] INT NOT NULL,
	[TakenAmount] DECIMAL(18, 6) NOT NULL,
	[CreationTime] DATETIME NOT NULL,
	[IssuedTime] DATETIME NULL,
	[RepaymentCount] INT NULL,
	[Refnum] NVARCHAR(10) NULL,
	[DateClosed] DATETIME NULL,
	[InterestRate] DECIMAL(18, 6) NOT NULL,
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
		[RepaymentIntervalTypeID],
		[LoanStatusID],
		[EzbobBankAccountID],
		[LoanSourceID],
		[Position],
		[TakenAmount],
		[CreationTime],
		[IssuedTime],
		[RepaymentCount],
		[Refnum],
		[DateClosed],
		[InterestRate],
		[InterestOnlyRepaymentCount],
		[OldLoanID]
	) SELECT
		[OfferID],
		[LoanTypeID],
		[RepaymentIntervalTypeID],
		[LoanStatusID],
		[EzbobBankAccountID],
		[LoanSourceID],
		[Position],
		[TakenAmount],
		[CreationTime],
		[IssuedTime],
		[RepaymentCount],
		[Refnum],
		[DateClosed],
		[InterestRate],
		[InterestOnlyRepaymentCount],
		[OldLoanID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


