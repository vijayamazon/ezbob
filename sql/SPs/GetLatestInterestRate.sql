IF OBJECT_ID('GetLatestInterestRate') IS NULL
	EXECUTE('CREATE PROCEDURE GetLatestInterestRate AS SELECT 1')
GO

ALTER PROCEDURE GetLatestInterestRate
	(@CustomerId INT, 
	 @Today DATETIME)
AS
BEGIN
	DECLARE 
		@InterestRate DECIMAL(18,7),
		@LoanId INT,
		@MinInterestRateToReuse DECIMAL(18,7),
		@TempInterestRate DECIMAL(18,7)
		
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
	
	SELECT @InterestRate AS InterestRate
END
GO
