SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OffersSave') IS NOT NULL
	DROP PROCEDURE NL_OffersSave
GO

IF TYPE_ID('NL_OffersList') IS NOT NULL
	DROP TYPE NL_OffersList
GO

CREATE TYPE NL_OffersList AS TABLE (
	[DecisionID] INT NOT NULL,
	[LoanTypeID] INT NOT NULL,
	[RepaymentIntervalTypeID] INT NOT NULL,
	[StartTime] DATETIME NOT NULL,
	[EndTime] DATETIME NOT NULL,
	[CreatedTime] DATETIME NOT NULL,
	[LoanSourceID] INT NOT NULL,
	[RepaymentCount] INT NOT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL,
	[MonthlyInterestRate] DECIMAL(18, 6) NOT NULL,
	[SetupFeePercent] DECIMAL(18, 6) NOT NULL,
	--[DistributedSetupFeePercent] DECIMAL(18, 6) NOT NULL,	
	[BrokerSetupFeePercent] DECIMAL(18, 6) NULL,
	[Notes] NVARCHAR(MAX) NULL,
	[InterestOnlyRepaymentCount] INT NULL,
	[DiscountPlanID] INT NOT NULL,
	[IsLoanTypeSelectionAllowed] BIT NOT NULL,
	[EmailSendingBanned] BIT NOT NULL
)
GO

CREATE PROCEDURE NL_OffersSave
@Tbl NL_OffersList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_Offers (
		[DecisionID],
		[LoanTypeID],
		[RepaymentIntervalTypeID],
		[StartTime],
		[EndTime],
		[CreatedTime],
		[LoanSourceID],
		[RepaymentCount],
		[Amount],
		[MonthlyInterestRate],
		[SetupFeePercent],
		--[DistributedSetupFeePercent],
		[BrokerSetupFeePercent],
		[Notes],
		[InterestOnlyRepaymentCount],
		[DiscountPlanID],
		[IsLoanTypeSelectionAllowed],
		[EmailSendingBanned]
	) SELECT
		[DecisionID],
		[LoanTypeID],
		[RepaymentIntervalTypeID],
		[StartTime],
		[EndTime],
		[CreatedTime],
		[LoanSourceID],
		[RepaymentCount],
		[Amount],
		[MonthlyInterestRate],
		[SetupFeePercent],
		--[DistributedSetupFeePercent],
		[BrokerSetupFeePercent],
		[Notes],
		[InterestOnlyRepaymentCount],
		[DiscountPlanID],
		[IsLoanTypeSelectionAllowed],
		[EmailSendingBanned]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


