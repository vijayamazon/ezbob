SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OffersSave') IS NOT NULL
	DROP PROCEDURE NL_OffersSave
GO

IF TYPE_ID('NL_OffersList') IS NOT NULL
	DROP TYPE NL_OffersList
GO

CREATE TYPE NL_OffersList AS TABLE (
	[DecisionID] [BIGINT] NOT NULL,
	[LoanTypeID] [INT] NOT NULL  DEFAULT 1,
	[RepaymentIntervalTypeID] [INT]	NOT NULL DEFAULT  1,
	[LoanSourceID] [INT] NOT NULL DEFAULT 1, 
	[StartTime] [DATETIME] NOT NULL,
	[EndTime] [DATETIME] NOT NULL,	
	[RepaymentCount] [INT] NOT NULL,	
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[MonthlyInterestRate] [DECIMAL] (18, 6) NOT NULL,
	[CreatedTime] [DATETIME] NOT NULL,
	[BrokerSetupFeePercent] [DECIMAL](18, 6) NULL,	
	[SetupFeeAddedToLoan] BIT NOT NULL DEFAULT 0,	
	[Notes] [nvarchar](max) NULL,
	[InterestOnlyRepaymentCount] [INT] NULL,
	[DiscountPlanID] [INT] NULL,
	[IsLoanTypeSelectionAllowed] [BIT] NOT NULL DEFAULT 0,
	[SendEmailNotification] [BIT] NOT NULL DEFAULT 1,	
	[IsRepaymentPeriodSelectionAllowed] [BIT] DEFAULT 0,
	[IsAmountSelectionAllowed] [BIT] NOT NULL DEFAULT 1,
	[ProductSubTypeID] INT NULL
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
		[BrokerSetupFeePercent] ,	
		[SetupFeeAddedToLoan] ,	
		[Notes] ,		
		[InterestOnlyRepaymentCount] ,
		[DiscountPlanID] ,
		[IsLoanTypeSelectionAllowed] ,
		[SendEmailNotification] ,		
		[IsRepaymentPeriodSelectionAllowed],
		[IsAmountSelectionAllowed] ,
		[ProductSubTypeID]		
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
		[BrokerSetupFeePercent] ,	
		[SetupFeeAddedToLoan] ,	
		[Notes] ,		
		[InterestOnlyRepaymentCount] ,
		[DiscountPlanID] ,
		[IsLoanTypeSelectionAllowed] ,
		[SendEmailNotification] ,		
		[IsRepaymentPeriodSelectionAllowed],
		[IsAmountSelectionAllowed] ,
		[ProductSubTypeID]
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


