SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OffersSave') IS NOT NULL
	DROP PROCEDURE NL_OffersSave
GO

IF TYPE_ID('NL_OffersList') IS NOT NULL
	DROP TYPE NL_OffersList
GO

CREATE TYPE NL_OffersList AS TABLE (
	[DecisionID] [INT] NOT NULL,
	[LoanTypeID] [INT] NOT NULL, 
	[RepaymentIntervalTypeID] [INT] NOT NULL, 
	[LoanSourceID] [INT] NOT NULL, 
	[StartTime] [DATETIME] NOT NULL,
	[EndTime] [DATETIME] NOT NULL,	
	[RepaymentCount] [INT] NOT NULL,	
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[MonthlyInterestRate] [DECIMAL] (18, 6) NOT NULL,
	[CreatedTime] [DATETIME] NOT NULL,	
	[SetupFeePercent] [DECIMAL](18, 6) NULL,
	[SetupFeeAddedToLoan] BIT NULL,
	[ServicingFeePercent] [decimal](18, 6) NULL,	
	[BrokerSetupFeePercent] [DECIMAL](18, 6) NULL,	
	[InterestOnlyRepaymentCount] [INT] NULL,
	[DiscountPlanID] [INT] NULL,
	[IsLoanTypeSelectionAllowed] [BIT] NOT NULL DEFAULT 0,
	[EmailSendingBanned] [BIT] NOT NULL DEFAULT 0,
	[Notes] [nvarchar](max) NULL	
)
GO

CREATE PROCEDURE NL_OffersSave
@Tbl NL_OffersList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_Offers (
		[DecisionID],
		[LoanTypeID] , 
		[RepaymentIntervalTypeID] , 
		[LoanSourceID] , 
		[StartTime] ,
		[EndTime] ,	
		[RepaymentCount] ,	
		[Amount] ,
		[MonthlyInterestRate] ,
		[CreatedTime] ,	
		[SetupFeePercent] ,
		[SetupFeeAddedToLoan] ,
		[ServicingFeePercent] ,	
		[BrokerSetupFeePercent] ,	
		[InterestOnlyRepaymentCount] ,
		[DiscountPlanID] ,
		[IsLoanTypeSelectionAllowed] ,
		[EmailSendingBanned] ,
		[Notes] 
	) SELECT
		[DecisionID],
		[LoanTypeID] , 
		[RepaymentIntervalTypeID] , 
		[LoanSourceID] , 
		[StartTime] ,
		[EndTime] ,	
		[RepaymentCount] ,	
		[Amount] ,
		[MonthlyInterestRate] ,
		[CreatedTime] ,	
		[SetupFeePercent] ,
		[SetupFeeAddedToLoan] ,
		[ServicingFeePercent] ,	
		[BrokerSetupFeePercent] ,	
		[InterestOnlyRepaymentCount] ,
		[DiscountPlanID] ,
		[IsLoanTypeSelectionAllowed] ,
		[EmailSendingBanned] ,
		[Notes] 
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


