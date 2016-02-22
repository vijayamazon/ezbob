SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_GradeRangeSave') IS NOT NULL
	DROP PROCEDURE I_GradeRangeSave
GO

IF TYPE_ID('I_GradeRangeList') IS NOT NULL
	DROP TYPE I_GradeRangeList
GO

CREATE TYPE I_GradeRangeList AS TABLE (
	[GradeID] INT NULL,
	[SubGradeID] INT NULL,
	[LoanSourceID] INT NOT NULL,
	[OriginID] INT NOT NULL,
	[IsFirstLoan] BIT NOT NULL,
	[MinSetupFee] DECIMAL(18, 6) NOT NULL,
	[MaxSetupFee] DECIMAL(18, 6) NOT NULL,
	[MinInterestRate] DECIMAL(18, 6) NOT NULL,
	[MaxInterestRate] DECIMAL(18, 6) NOT NULL,
	[MinLoanAmount] DECIMAL(18, 6) NOT NULL,
	[MaxLoanAmount] DECIMAL(18, 6) NOT NULL,
	[MinTerm] INT NOT NULL,
	[MaxTerm] INT NOT NULL,
	[IsActive] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_GradeRangeSave
@Tbl I_GradeRangeList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_GradeRange (
		[GradeID],
		[SubGradeID],
		[LoanSourceID],
		[OriginID],
		[IsFirstLoan],
		[MinSetupFee],
		[MaxSetupFee],
		[MinInterestRate],
		[MaxInterestRate],
		[MinLoanAmount],
		[MaxLoanAmount],
		[MinTerm],
		[MaxTerm],
		[IsActive],
		[Timestamp]
	) SELECT
		[GradeID],
		[SubGradeID],
		[LoanSourceID],
		[OriginID],
		[IsFirstLoan],
		[MinSetupFee],
		[MaxSetupFee],
		[MinInterestRate],
		[MaxInterestRate],
		[MinLoanAmount],
		[MaxLoanAmount],
		[MinTerm],
		[MaxTerm],
		[IsActive],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


