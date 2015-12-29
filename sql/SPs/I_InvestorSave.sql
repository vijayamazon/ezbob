SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorSave') IS NOT NULL
	DROP PROCEDURE I_InvestorSave
GO

IF TYPE_ID('I_InvestorList') IS NOT NULL
	DROP TYPE I_InvestorList
GO

CREATE TYPE I_InvestorList AS TABLE (
	[InvestorTypeID] INT NOT NULL,
	[Name] NVARCHAR(255) NULL,
	[MonthlyFundingCapital] DECIMAL(18, 6) NULL,
	[FundsTransferDate] INT NULL,
	[DiscountServicingFeePercent] DECIMAL(18, 6) NULL,
	[FundingLimitForNotification] DECIMAL(18, 6) NULL,
	[IsActive] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_InvestorSave
@Tbl I_InvestorList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	--Check if investor exists
	IF EXISTS (SELECT * FROM I_Investor i INNER JOIN @Tbl t ON i.Name=t.Name)
	BEGIN
		SELECT 0 AS ScopeID
	END
	

	INSERT INTO I_Investor (
		[InvestorTypeID],
		[Name],
		[MonthlyFundingCapital],
		[FundsTransferDate],
		[DiscountServicingFeePercent],
		[FundingLimitForNotification],
		[IsActive],
		[Timestamp]
	) SELECT
		[InvestorTypeID],
		[Name],
		[MonthlyFundingCapital],
		[FundsTransferDate],
		[DiscountServicingFeePercent],
		[FundingLimitForNotification],
		[IsActive],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


