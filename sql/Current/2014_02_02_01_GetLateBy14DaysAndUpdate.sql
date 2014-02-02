IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetLateBy14DaysAndUpdate') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetLateBy14DaysAndUpdate
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetLateBy14DaysAndUpdate 
AS
BEGIN
	CREATE TABLE #LateBy14DaysLoans
	(
		LoanId INT,
		Is14DaysLate BIT,
		SignDate DATETIME,
		FirstName NVARCHAR(250),
		LoanAmount NUMERIC(18,0),
		Principal NUMERIC(18,2),
		Interest NUMERIC(18,2),
		Fees NUMERIC(18,2),
		Total NUMERIC(18,2),
		Email NVARCHAR(128)
	)

	DECLARE 
		@LoanId INT,
		@Is14DaysLate BIT,
		@Today DATETIME

	SELECT @Today = CONVERT(DATE, getdate())

	DECLARE cur CURSOR FOR 
		SELECT 
			Loan.Id, 
			Is14DaysLate 
		FROM 
			Loan,
			Customer,
			CustomerStatuses 
		WHERE 
		Loan.Status='Late' AND 
		Loan.CustomerId = Customer.Id AND 
		Customer.CollectionStatus = CustomerStatuses.Id AND
		CustomerStatuses.IsEnabled = 1
		
	OPEN cur
	FETCH NEXT FROM cur INTO @LoanId, @Is14DaysLate
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @EarliestLate DATETIME
		SELECT @EarliestLate = CONVERT(DATE, min(Date)) FROM LoanSchedule WHERE LoanId=@LoanId AND Status='Late'
			
		IF dateadd(dd, 14, @EarliestLate) <= @Today
		BEGIN    
			DECLARE @LoanAmount NUMERIC(18,0),
				@Principal NUMERIC(18,2),
				@Interest NUMERIC(18,2),
				@Fees NUMERIC(18,2),
				@Total NUMERIC(18,2),
				@FirstName NVARCHAR(250),
				@SignDate DATETIME,
				@Email NVARCHAR(128)
			
			SELECT 
				@FirstName = Customer.FirstName, 
				@SignDate = Loan.[Date], 
				@LoanAmount = Loan.LoanAmount,
					@Email = Customer.Name
			FROM 
				Customer, 
				Loan 
			WHERE 
				Loan.Id = @LoanId AND 
				Loan.CustomerId = Customer.Id
					
			SELECT 
				@Principal = sum(LoanRepayment),
				@Interest = sum(Interest),
				@Fees = sum(Fees)
			FROM 
				LoanSchedule WHERE (Status = 'Late' OR Status = 'StillToPay') AND LoanId=@LoanId
				
			SELECT @Total = @Principal + @Interest + @Fees
				
			INSERT INTO #LateBy14DaysLoans VALUES (@LoanId, @Is14DaysLate, @SignDate, @FirstName, @LoanAmount, @Principal, @Interest, @Fees, @Total, @Email)
		END

		FETCH NEXT FROM cur INTO @LoanId, @Is14DaysLate
	END
	CLOSE cur
	DEALLOCATE cur

	UPDATE Loan SET Is14DaysLate = 0 WHERE Is14DaysLate = 1 AND Id NOT IN (SELECT LoanId FROM #LateBy14DaysLoans)
	SELECT LoanId, Is14DaysLate, convert(VARCHAR(12), SignDate, 107) as SignDate, FirstName, LoanAmount, Principal, Interest, Fees, Total, Email FROM #LateBy14DaysLoans
	DROP TABLE #LateBy14DaysLoans
END
GO
