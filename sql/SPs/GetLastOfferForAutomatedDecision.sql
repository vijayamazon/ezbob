IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastOfferForAutomatedDecision]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLastOfferForAutomatedDecision]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastOfferForAutomatedDecision] 
	(@CustomerId INT, @Now DATETIME)
AS
BEGIN
	DECLARE
		@LastApprovalDate DATETIME,
		@count INT,
		@ReApprovalFullAmountNew FLOAT,
		@ReApprovalRemainingAmountNew FLOAT,
		@ReApprovalFullAmountOld FLOAT,
		@ReApprovalRemainingAmountOld  FLOAT,
		@ManualFundsAdded FLOAT,
		@InterestRate DECIMAL(18,7),
		@LoanId INT,
		@MinInterestRateToReuse DECIMAL(18,7),
		@TempInterestRate DECIMAL(18,7),
		@PreviousFilledCashRequest INT,
		@NumOfLates INT,
		@LastCreatedMp DATETIME
	
	SET @InterestRate = -1	
		
	-- Cursor for active loans
	DECLARE cur CURSOR FOR 
		SELECT 
			Loan.Id
		FROM 
			Loan,
			CashRequests,
			DiscountPlan
		WHERE 
			Loan.Status != 'PaidOff' AND
			Loan.CustomerId = @CustomerId AND
			Loan.RequestCashId = CashRequests.Id AND
			CashRequests.DiscountPlanId = DiscountPlan.Id AND
			DiscountPlan.ForbiddenForReuse = 0 AND
			Loan.Modified = 0
	
	-- Deduce interest from active loans
	OPEN cur
	FETCH NEXT FROM cur INTO @LoanId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT TOP 1 @TempInterestRate = InterestRate FROM LoanSchedule WHERE LoanId = @LoanId AND Date > @Now ORDER BY Date ASC 
		
		IF @TempInterestRate IS NULL
			SELECT TOP 1 @TempInterestRate = InterestRate FROM LoanSchedule WHERE LoanId = @LoanId ORDER BY Date DESC
			
		IF @InterestRate = -1 OR @InterestRate > @TempInterestRate
		BEGIN
			SET @InterestRate = @TempInterestRate
		END
	
		FETCH NEXT FROM cur INTO @LoanId
	END
	CLOSE cur
	DEALLOCATE cur
	
	-- Check past loans if couldn't deduce interest rate by active loans
	IF @InterestRate = -1
	BEGIN		
		DECLARE pastLoansCursor CURSOR FOR 
			SELECT 
				Loan.Id
			FROM 
				Loan,
				CashRequests,
				DiscountPlan
			WHERE 
				Loan.Status = 'PaidOff' AND
				Loan.CustomerId = @CustomerId AND
				Loan.RequestCashId = CashRequests.Id AND
				CashRequests.DiscountPlanId = DiscountPlan.Id AND
				DiscountPlan.ForbiddenForReuse = 0 AND
				Loan.Modified = 0
	
		OPEN pastLoansCursor
		FETCH NEXT FROM pastLoansCursor INTO @LoanId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			DECLARE @LastPaymentDate DATETIME
			
			SELECT TOP 1 @LastPaymentDate = PostDate FROM LoanTransaction WHERE Type = 'PaypointTransaction' AND LoanId = @LoanId AND Status = 'Done' ORDER BY PostDate DESC			
			SELECT TOP 1 @TempInterestRate = InterestRate FROM LoanSchedule WHERE LoanId = @LoanId AND Date > @LastPaymentDate ORDER BY Date ASC 
			
			IF @TempInterestRate IS NULL
				SELECT TOP 1 @TempInterestRate = InterestRate FROM LoanSchedule WHERE LoanId = @LoanId ORDER BY Date DESC
				
			IF @InterestRate = -1 OR @InterestRate > @TempInterestRate
			BEGIN
				SET @InterestRate = @TempInterestRate
			END
		
			FETCH NEXT FROM pastLoansCursor INTO @LoanId
		END
		CLOSE pastLoansCursor
		DEALLOCATE pastLoansCursor	
	END
	
	-- Don't set interest for less than
	SELECT @MinInterestRateToReuse = CONVERT(DECIMAL(18,7), Value) FROM ConfigurationVariables WHERE Name = 'MinInterestRateToReuse'
	IF @InterestRate < @MinInterestRateToReuse 
	BEGIN
		SET @InterestRate = -1
	END	

	SELECT 
		@LastApprovalDate = max(cr.UnderwriterDecisionDate)
	FROM 
		CashRequests cr 
	WHERE 
		cr.IdCustomer = @CustomerId AND 
		cr.UnderwriterDecision = 'Approved'

	SELECT @PreviousFilledCashRequest = MAX(Id) FROM CashRequests WHERE IdCustomer = @CustomerId AND MedalType IS NOT NULL	
	
	SELECT 
		@LastCreatedMp = MAX(created) 
	FROM 
		MP_CustomerMarketPlace 
	WHERE 
		CustomerId = @CustomerId
	
	SELECT 
		@NumOfLates = COUNT(1) 
	FROM 
		LoanSchedule
	WHERE 
		Status='Late' AND
		LoanId = @PreviousFilledCashRequest

	IF @NumOfLates = 0 AND -- No lates...		
		DATEADD(DD, 30, @LastApprovalDate) >= @Now AND -- Last approval was in last 30 days
		@LastApprovalDate >= @LastCreatedMp -- No New MPs
	BEGIN
		SELECT 
			@ReApprovalFullAmountNew = ManagerApprovedSum
		FROM
			CashRequests
			LEFT JOIN Loan ON CustomerId = IdCustomer
		WHERE 
			IdCustomer = @CustomerId AND
			Loan.Id IS NULL AND 
			CashRequests.Id = @PreviousFilledCashRequest AND
			UnderwriterDecision = 'Approved'
			
		SELECT 
			@ReApprovalRemainingAmountNew = ManagerApprovedSum - SUM(LoanAmount) 
		FROM 
			CashRequests
			LEFT JOIN Loan ON CustomerId = IdCustomer
		WHERE 
			IdCustomer = @CustomerId AND 
			CashRequests.Id = @PreviousFilledCashRequest AND
			UnderwriterDecision = 'Approved' AND 
			HasLoans = 1 AND 
			CashRequests.CreationDate <= 
			(
				SELECT 
					MIN(l1.date) 
				FROM 
					Loan l1
			) AND 							
			Loan.Status != 'Late' 
		GROUP BY
			ManagerApprovedSum	
		
		SELECT 
			@ReApprovalFullAmountOld = ManagerApprovedSum
		FROM
			CashRequests
			LEFT JOIN Loan ON CustomerId = IdCustomer
		WHERE 
			IdCustomer = @CustomerId AND 
			CashRequests.Id = @PreviousFilledCashRequest AND 
			UnderwriterDecision = 'Approved' AND
			HasLoans = 0 AND 
			Loan.Id IS NOT NULL
			
		SELECT 
			@ReApprovalRemainingAmountOld = ManagerApprovedSum - SUM(Loan.LoanAmount)
		FROM
			CashRequests
			LEFT JOIN Loan ON CustomerId = IdCustomer
		WHERE 
			IdCustomer = @CustomerId AND 
			CashRequests.Id = @PreviousFilledCashRequest AND
			UnderwriterDecision = 'Approved' AND
			Loan.Id IS NOT NULL AND 
			CashRequests.CreationDate >= 
			(
				SELECT 
					MIN(l1.date) 
				FROM 
					Loan l1
			) AND 
			Loan.Status != 'Late' 
		GROUP BY
			ManagerApprovedSum
	END
	ELSE
	BEGIN
		SELECT @ReApprovalFullAmountNew = NULL
		SELECT @ReApprovalRemainingAmountNew = NULL
		SELECT @ReApprovalFullAmountOld = NULL
		SELECT @ReApprovalRemainingAmountOld = NULL
	END
		
	SELECT
		@ReApprovalFullAmountNew AS 'ReApprovalFullAmountNew', 
		@ReApprovalRemainingAmountNew AS 'ReApprovalRemainingAmount', 
		@ReApprovalFullAmountOld AS 'ReApprovalFullAmountOld',
		@ReApprovalRemainingAmountOld AS 'ReApprovalRemainingAmountOld',
		APR,
		RepaymentPeriod,
		ExpirianRating,
		(CASE @InterestRate WHEN -1 THEN InterestRate ELSE @InterestRate END) AS InterestRate,
		UseSetupFee,
		LoanTypeId,
		IsLoanTypeSelectionAllowed,
		DiscountPlanId,
		LoanSourceID,
		Convert(INT, IsCustomerRepaymentPeriodSelectionAllowed) AS IsCustomerRepaymentPeriodSelectionAllowed,
		UseBrokerSetupFee,
		ManualSetupFeeAmount,
		ManualSetupFeePercent
	FROM 
		CashRequests
	WHERE 
		Id = @PreviousFilledCashRequest
END
GO
