SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_PortfolioSave') IS NOT NULL
	DROP PROCEDURE I_PortfolioSave
GO

IF TYPE_ID('I_PortfolioList') IS NOT NULL
	DROP TYPE I_PortfolioList
GO

CREATE TYPE I_PortfolioList AS TABLE (
	[InvestorID] INT NOT NULL,
	[ProductTypeID] INT NOT NULL,
	[LoanID] INT NOT NULL,
	[LoanPercentage] DECIMAL(18, 6) NOT NULL,
	[InitialTerm] INT NOT NULL,
	[GradeId] INT,
	[Timestamp] DATETIME NOT NULL,
	[NLLoanID] BIGINT NULL
)
GO

CREATE PROCEDURE I_PortfolioSave
@Tbl I_PortfolioList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_Portfolio (
		[InvestorID],
		[LoanID],
		[ProductTypeID],
		[LoanPercentage],
		[InitialTerm],
		[GradeId],
		[Timestamp],
		[NLLoanID] 
	) SELECT
		[InvestorID],
		[LoanID],
		[ProductTypeID],
		[LoanPercentage],
		[InitialTerm],
		[GradeId],
		[Timestamp],
		[NLLoanID] 
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO
