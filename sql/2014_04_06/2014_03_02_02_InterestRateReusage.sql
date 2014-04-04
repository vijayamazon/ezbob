IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MinInterestRateToReuse')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MinInterestRateToReuse', 0.02, 'Minimum interest rate to reuse when calculating interest rate during new credit line')
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ForbiddenForReuse' and Object_ID = Object_ID(N'DiscountPlan'))
BEGIN 
	ALTER TABLE DiscountPlan ADD ForbiddenForReuse BIT NOT NULL DEFAULT 0

END
GO
UPDATE DiscountPlan SET ForbiddenForReuse = 1 WHERE Name = 'One month free'

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetLastOfferForAutomatedDecision') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetLastOfferForAutomatedDecision
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetLastOfferForAutomatedDecision 
	(@CustomerId INT)
AS
BEGIN
	DECLARE 
		@RowNum INT,
		@ManualDecisionDate DATETIME,
		@count INT,
		@ReApprovalFullAmountNew FLOAT,
		@ReApprovalRemainingAmountNew FLOAT,
		@ReApprovalFullAmountOld FLOAT,
		@ReApprovalRemainingAmountOld  FLOAT,
		@ManualFundsAdded FLOAT,
		@InterestRate DECIMAL(18,7),
		@LoanId INT,
		@Today DATETIME,
		@MinInterestRateToReuse DECIMAL(18,7),
		@TempInterestRate DECIMAL(18,7)
	
	SET @InterestRate = -1	
	SET @Today = GETUTCDATE()
	
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
		SELECT TOP 1 @TempInterestRate = InterestRate FROM LoanSchedule WHERE LoanId = @LoanId AND Date > @Today ORDER BY Date ASC 
		
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
	
	SET @RowNum = 2

	SELECT 
		@ManualDecisionDate = max(cr.UnderwriterDecisionDate)
	FROM 
		CashRequests cr 
	WHERE 
		cr.IdCustomer = @CustomerId AND 
		cr.UnderwriterDecision = 'Approved'

	SELECT 
		@count=COUNT(1) 
	FROM 
		LoanSchedule
	WHERE 
		Status='Late' AND
		LoanId IN
		(
			SELECT 
				Id 
			FROM 
				Loan 
			WHERE 
				RequestCashId =
				(
					SELECT 
						Id 
					FROM
					(
						SELECT 
							ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
							cr.Id
						FROM 
							CashRequests cr
						WHERE 
							cr.IdCustomer = @CustomerId
					) p
				    WHERE 
						p.row = @RowNum
				)
		) 

	IF @count = 0
	BEGIN
		SELECT 
			@ReApprovalFullAmountNew = 
			(
				SELECT DISTINCT 
					ReApprovalFullAmountNew 
				FROM
					(
						SELECT
							cr.ManagerApprovedSum as ReApprovalFullAmountNew
						FROM 
							CashRequests cr 
							LEFT JOIN Loan l ON l.CustomerId = cr.IdCustomer
						WHERE 
							cr.IdCustomer = @CustomerId AND
							l.Id IS NULL AND 
							cr.id = 
							(
								SELECT
									Id 
								FROM
									(
										SELECT 
											ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
											cr.Id 
										FROM 
											CashRequests cr
										WHERE 
											cr.IdCustomer = @CustomerId
									) p
								WHERE p.row = @RowNum
							) AND
							cr.UnderwriterDecision= 'Approved' AND
							l.Id IS NULL AND
							DATEADD(DD, 30, @ManualDecisionDate) >= GETUTCDATE() AND
							@ManualDecisionDate >= 
							(
								SELECT 
									max(created) 
								FROM 
									MP_CustomerMarketPlace 
								WHERE 
									CustomerId = @CustomerId
							)
					) AS ReApprovalFullAmountNew
			)
	END
	ELSE
	BEGIN
		SELECT @ReApprovalFullAmountNew = NULL
	END
	
	IF @count = 0
	BEGIN
		SELECT 
			@ReApprovalRemainingAmountNew =
			(
				(
					SELECT DISTINCT 
						ReApprovalRemainingAmountNew 
					FROM 
					(
						SELECT 
							cr.ManagerApprovedSum - sum(l.LoanAmount) AS ReApprovalRemainingAmountNew
						FROM 
							CashRequests cr
							LEFT JOIN Loan l ON l.CustomerId = cr.IdCustomer
						WHERE 
							cr.IdCustomer = @CustomerId AND 
							cr.id = 
							(
								SELECT 
									Id 
								FROM
								(
									SELECT 
										ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
										cr.Id 
									FROM 
										CashRequests cr
									WHERE 
										cr.IdCustomer = @CustomerId
								) p
								WHERE 
									p.row = @RowNum
							) AND 
							cr.UnderwriterDecision = 'Approved' AND 
							cr.HasLoans = 1 AND 
							cr.CreationDate <= 
							(
								SELECT 
									Min(l1.date) 
								FROM 
									Loan l1
							) AND 
							DATEADD(DD, 30, @ManualDecisionDate) >= GETUTCDATE() AND 
							@ManualDecisionDate >= 
							(
								SELECT 
									max(created) 
								FROM 
									MP_CustomerMarketPlace 
								WHERE 
									CustomerId = @CustomerId
							) AND 
							l.Status != 'Late' 
						GROUP BY
							ManagerApprovedSum
					) AS ReApprovalRemainingAmountNew
				)
			)
	END
	ELSE 
	BEGIN
		SELECT @ReApprovalRemainingAmountNew = NULL
	END
	
	IF @count = 0
	BEGIN
		SELECT 
			@ReApprovalFullAmountOld = 
			(
				SELECT DISTINCT 
					ReApprovalFullAmount 
				FROM
				(
					SELECT
						cr.ManagerApprovedSum AS ReApprovalFullAmount
					FROM 
						CashRequests cr 
						LEFT JOIN Loan l ON l.CustomerId = cr.IdCustomer
					WHERE 
						cr.IdCustomer = @CustomerId AND 
						cr.id = 
						(
							SELECT 
								Id 
							FROM
								(
									SELECT 
										ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
										cr.Id
									FROM 
										CashRequests cr
									WHERE 
										cr.IdCustomer = @CustomerId
								) p
							WHERE 
								p.row = @RowNum
						) AND 
						cr.UnderwriterDecision= 'Approved' AND 
						@ManualDecisionDate >= 
						(
							SELECT 
								max(created) 
							FROM 
								MP_CustomerMarketPlace 
							WHERE 
								CustomerId = @CustomerId
						) AND 
						cr.HasLoans = 0 AND 
						l.Id IS NOT NULL AND 
						DATEADD(DD, 28, @ManualDecisionDate) >= GETUTCDATE()) AS ReApprovalFullAmount
			)
	END
	ELSE 
	BEGIN
		SELECT @ReApprovalFullAmountOld = NULL
	END

	IF @count=0
	BEGIN
		SELECT 
			@ReApprovalRemainingAmountOld=
			(
				(
					SELECT DISTINCT 
						ReApprovalRemainingAmountOld 
					FROM 
					(
						SELECT 
							cr.ManagerApprovedSum - sum(l.LoanAmount) AS ReApprovalRemainingAmountOld
						FROM 
							CashRequests cr 
							LEFT JOIN Loan l ON l.RequestCashId = cr.id
						WHERE 
							cr.IdCustomer = @CustomerId AND 
							cr.id = 
							(
								SELECT 
									Id 
								FROM
									(
										SELECT 
											ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
											cr.Id 
										FROM 
											CashRequests cr
										WHERE 
											cr.IdCustomer = @CustomerId
									) p
								WHERE 
									p.row = @RowNum
							) AND 
							cr.UnderwriterDecision = 'Approved' AND 
							@ManualDecisionDate >= (SELECT max(created) FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId) AND 
							l.Id IS NOT NULL AND 
							cr.CreationDate >= (SELECT Min(l1.date) FROM Loan l1) AND 
							DATEADD(DD, 28, @ManualDecisionDate) >= GETUTCDATE() AND l.Status != 'Late' 
						GROUP BY
							ManagerApprovedSum
					) AS ReApprovalRemainingAmountOld
				)
			)
	END
	ELSE 
	BEGIN
		SELECT @ReApprovalRemainingAmountOld = null
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
		IsLoanTypeSELECTionAllowed,
		DiscountPlanId,
		LoanSourceID,
		Convert(INT,IsCustomerRepaymentPeriodSELECTionAllowed) AS IsCustomerRepaymentPeriodSelectionAllowed,
		UseBrokerSetupFee,
		ManualSetupFeeAmount,
		ManualSetupFeePercent
	FROM 
		CashRequests cr
	WHERE 
		Id =
		(
			SELECT 
				Id 
			FROM
			(
				SELECT 
					ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
					cr.Id 
				FROM 
					CashRequests cr
				WHERE 
					cr.IdCustomer = @CustomerId
			) p
			WHERE 
				p.row = @RowNum
		)
END
GO
