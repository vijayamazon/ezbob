IF OBJECT_ID('CustomerAnalyticsUpdateLocalData') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerAnalyticsUpdateLocalData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CustomerAnalyticsUpdateLocalData
@CustomerID BIGINT,
@AnalyticsDate DATETIME,
@AnnualTurnover DECIMAL(18, 6),
@TotalSumOfOrdersForLoanOffer DECIMAL(18, 6),
@MarketplaceSeniorityYears DECIMAL(18, 6),
@MaxFeedback INT,
@MPsNumber INT,
@FirstRepaymentDatePassed BIT,
@EzbobSeniorityMonths DECIMAL(18, 6),
@OnTimeLoans INT,
@LatePayments INT,
@EarlyPayments INT
AS
BEGIN
	SET NOCOUNT ON;

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	UPDATE CustomerAnalyticsLocalData SET
		IsActive = 0
	WHERE
		CustomerID = @CustomerID
		AND
		ISActive = 1

	INSERT INTO CustomerAnalyticsLocalData (CustomerID, AnalyticsDate, IsActive, AnnualTurnover,
		TotalSumOfOrdersForLoanOffer, MarketplaceSeniorityYears, MaxFeedback, MPsNumber,
		FirstRepaymentDatePassed, EzbobSeniorityMonths, OnTimeLoans, LatePayments, EarlyPayments
	) VALUES (
		@CustomerID, @AnalyticsDate, 1, @AnnualTurnover,
		@TotalSumOfOrdersForLoanOffer, @MarketplaceSeniorityYears, @MaxFeedback, @MPsNumber,
		@FirstRepaymentDatePassed, @EzbobSeniorityMonths, @OnTimeLoans, @LatePayments, @EarlyPayments
	)
	
	COMMIT TRANSACTION
END
GO
