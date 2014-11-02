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
		@PreviousFilledCashRequest INT,
		@NumOfLates INT,
		@LastCreatedMp DATETIME

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
		LoanSchedule,
		Loan
	WHERE 
		LoanSchedule.Status = 'Late' AND
		LoanId = Loan.Id AND
		Loan.CustomerId = @CustomerId

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
		InterestRate,
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
