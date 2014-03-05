IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ViewHelper_MonthlyTurnover]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[ViewHelper_MonthlyTurnover]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ViewHelper_MonthlyTurnover]
AS
BEGIN
	DECLARE @CustomerId INT,
		@latestAggr DATETIME,
		@analysisFuncId INT,
		@mpName NVARCHAR(255),
		@mpId INT,
		@mpTypeId INT,
		@monthTimePeroidId INT,
		@TotalMonthlyTurnover NUMERIC(18,4),
		@AggrVal NUMERIC(18,4)
		
SELECT @monthTimePeroidId = Id FROM MP_AnalysisFunctionTimePeriod WHERE Name = '30'
		
SELECT DISTINCT IdCustomer AS CustumerId, convert(NUMERIC(18,4),0) AS MonthlyTurnover INTO #tmp FROM CashRequests

DECLARE customerCursor CURSOR FOR SELECT CustumerId FROM #tmp
OPEN customerCursor
FETCH NEXT FROM customerCursor INTO @CustomerId
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @TotalMonthlyTurnover = 0
	DECLARE marketplaceCursor CURSOR FOR SELECT Id, MarketPlaceId FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId
	OPEN marketplaceCursor
	FETCH NEXT FROM marketplaceCursor INTO @mpId, @mpTypeId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @mpName = Name FROM MP_MarketplaceType WHERE Id = @mpTypeId
		IF @mpName = 'Pay Pal'
		BEGIN
			SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='TotalNetInPayments'
		END
		ELSE
		BEGIN
			IF @mpName = 'PayPoint'
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='SumOfAuthorisedOrders'
			END
			ELSE
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='TotalSumOfOrders'
			END
		END
		
		SELECT @latestAggr = Max(Updated) FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@mpId
		IF @latestAggr IS NOT NULL
		BEGIN
			SELECT @AggrVal = ValueFloat FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@mpId AND Updated = @latestAggr AND AnalysisFunctionTimePeriodId = @monthTimePeroidId
			
			SET @TotalMonthlyTurnover = @TotalMonthlyTurnover + @AggrVal
		END
		
	
		FETCH NEXT FROM marketplaceCursor INTO @mpId, @mpTypeId
	END
	CLOSE marketplaceCursor
	DEALLOCATE marketplaceCursor 
	
	UPDATE #tmp SET MonthlyTurnover=@TotalMonthlyTurnover WHERE CustumerId = @CustomerId

	FETCH NEXT FROM customerCursor INTO @CustomerId
END
CLOSE customerCursor
DEALLOCATE customerCursor

SELECT CashRequests.Id,CashRequests.IdCustomer,CashRequests.IdUnderwriter,CashRequests.CreationDate,CashRequests.SystemDecision,CashRequests.UnderwriterDecision,CashRequests.SystemDecisionDate,CashRequests.UnderwriterDecisionDate,CashRequests.EscalatedDate,CashRequests.SystemCalculatedSum,CashRequests.ManagerApprovedSum,CashRequests.MedalType,CashRequests.EscalationReason,CashRequests.APR,CashRequests.RepaymentPeriod,CashRequests.ScorePoints,CashRequests.ExpirianRating,CashRequests.AnualTurnover,CashRequests.InterestRate,CashRequests.UseSetupFee,CashRequests.EmailSendingBanned,CashRequests.LoanTypeId,CashRequests.UnderwriterComment,CashRequests.HasLoans,CashRequests.LoanTemplate,CashRequests.IsLoanTypeSelectionAllowed,CashRequests.DiscountPlanId,CashRequests.LoanSourceID,CashRequests.OfferStart,CashRequests.OfferValidUntil,CashRequests.IsCustomerRepaymentPeriodSelectionAllowed, #tmp.MonthlyTurnover 
FROM CashRequests, #tmp
WHERE #tmp.CustumerId = CashRequests.IdCustomer

DROP TABLE #tmp
END
GO
