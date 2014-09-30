IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenerateHistoricalDelinquencyPerformanceData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GenerateHistoricalDelinquencyPerformanceData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GenerateHistoricalDelinquencyPerformanceData]
	(@CutoffDate DATETIME, @MinPct NUMERIC(18,6))
AS
BEGIN
	DECLARE 
		@LoanId INT,
		@LoanIssueDate DATETIME,
		@LoanAmount NUMERIC(18,0),
		@LoanStatus NVARCHAR(50),
		@CustomerId INT,
		@ScheduleId INT, 
		@OrigSum NUMERIC(38,2), 
		@Pct NUMERIC(18,6),
		@ScheduleDate DATETIME,
		@LastTransactionId INT,
		@LastTransactionDate DATETIME,
		@LateDays INT,
		@LoanDate DATETIME,
		@PayMentNum INT,
		@NumOfLoans INT,
		@SumOfLoans NUMERIC(18,0),
		@OutstandingPrincipal INT,
		@NumOfSpecialLateLoans INT,
		@SumOfSpecialLateLoans NUMERIC(18,0),
		@OutstandingSpecialLatePrincipal INT,
		@Principal DECIMAL(18,2),
		@NumOfSpecialLate1_14Loans INT,
		@SumOfSpecialLate1_14Loans NUMERIC(18,0),
		@OutstandingSpecialLate1_14Principal INT,	
		@NumOfSpecialLate15_30Loans INT,
		@SumOfSpecialLate15_30Loans NUMERIC(18,0),
		@OutstandingSpecialLate15_30Principal INT,	
		@NumOfSpecialLate31_45Loans INT,
		@SumOfSpecialLate31_45Loans NUMERIC(18,0),
		@OutstandingSpecialLate31_45Principal INT,	
		@NumOfSpecialLate46_60Loans INT,
		@SumOfSpecialLate46_60Loans NUMERIC(18,0),
		@OutstandingSpecialLate46_60Principal INT,	
		@NumOfSpecialLateOver60Loans INT,
		@SumOfSpecialLateOver60Loans NUMERIC(18,0),
		@OutstandingSpecialLateOver60Principal INT,
		@StatusAfterLastTransaction NVARCHAR(50),
		@StatusAfterLastTransactionFromLoanSchedule NVARCHAR(50),
		@Threshold NUMERIC(18,6),
		@CustomerStatus NVARCHAR(100)

	SET @Threshold = 5 -- Hardcoded value. Used to avoid the entries in the LoanScheduleTransaction table that are there because of rounding mistakes

	SELECT 
		@NumOfLoans = Count(1), 
		@SumOfLoans = SUM(LoanAmount), 
		@OutstandingPrincipal = SUM(Principal) 
	FROM 
		Loan, 
		Customer 
	WHERE 
		Loan.Date < @CutoffDate AND 
		Loan.CustomerId = Customer.Id AND 
		Customer.IsTest = 0
			
	CREATE TABLE #SpecialLate
	(
		CustomerId INT,
		CustomerStatus NVARCHAR(100),
		LoanId INT, 
		Pct NUMERIC(18,6), 
		ScheduleDate DATETIME, 
		LastTransactionDate DATETIME, 
		PayMentNum INT, 
		LateDays INT, 
		LoanAmount INT, 
		LoanDate DATETIME,
		Principal DECIMAL(18,2)
	)
	
	CREATE TABLE #HandledLoans
	(
		LoanId INT
	)

	DECLARE cur CURSOR FOR 
		SELECT 
			Loan.Id, 
			Loan.CustomerId, 
			Loan.Date, 
			Loan.LoanAmount, 
			Loan.Status, 
			Loan.Principal,
			CustomerStatuses.Name
		FROM 
			Loan, 
			Customer,
			CustomerStatuses
		WHERE 
			Loan.Date < @CutoffDate AND 
			Loan.CustomerId = Customer.Id AND 
			Customer.IsTest = 0 AND
			Customer.CollectionStatus = CustomerStatuses.Id
	OPEN cur
	FETCH NEXT FROM cur INTO @LoanId, @CustomerId, @LoanIssueDate, @LoanAmount, @LoanStatus, @Principal, @CustomerStatus
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT
			lst.ScheduleID,
			-SUM(lst.PrincipalDelta) AS PaidSum
		INTO
			#s
		FROM
			LoanScheduleTransaction lst
			INNER JOIN Loan l ON lst.LoanID = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			ABS(lst.PrincipalDelta) + ABS(lst.FeesDelta) + ABS(lst.InterestDelta) > @Threshold AND
			lst.LoanID = @LoanId AND
			lst.[Date] < @CutoffDate
		GROUP BY
			lst.ScheduleID	 
		
		SELECT
			s.Id AS ScheduleID,
			s.[Date] AS ScheduleDate,
			ISNULL(#s.PaidSum, 0) + s.LoanRepayment AS OrigSum,
			CONVERT(NUMERIC(18, 6), 0) AS Pct
		INTO
			#t
		FROM
			LoanSchedule s
			INNER JOIN Loan l ON s.LoanID = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			LEFT JOIN #s ON #s.ScheduleID = s.Id
		WHERE
			s.LoanID = @LoanId AND
			s.[Date] < @CutoffDate
		
		UPDATE #t SET
			Pct = CONVERT(NUMERIC(18, 6), PaidSum) / CONVERT(NUMERIC(18, 6), OrigSum)
		FROM
			#t
			INNER JOIN #s ON #t.ScheduleID = #s.ScheduleID
		WHERE
			OrigSum != 0	
		
		SET @PayMentNum = 0
		
		DECLARE cur2 CURSOR FOR 
			SELECT ScheduleId,OrigSum,Pct,ScheduleDate FROM #t ORDER BY ScheduleDate ASC
		OPEN cur2
		FETCH NEXT FROM cur2 INTO @ScheduleId, @OrigSum, @Pct, @ScheduleDate
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF NOT EXISTS (SELECT 1 FROM #HandledLoans WHERE LoanId = @LoanId)
			BEGIN
				SET @PayMentNum = @PayMentNum + 1			
				
				IF @Pct < @MinPct AND @OrigSum > 0
				BEGIN				
					SELECT 
						@LastTransactionId = Max(TransactionID) 
					FROM 
						LoanScheduleTransaction 
					WHERE 
						ScheduleID = @ScheduleID AND 
						ABS(PrincipalDelta) + ABS(FeesDelta) + ABS(InterestDelta) > @Threshold
						
					IF @LastTransactionId IS NOT NULL
					BEGIN
						SELECT @LastTransactionDate = LoanTransaction.PostDate FROM LoanTransaction WHERE Id = @LastTransactionId
						SELECT @StatusAfterLastTransaction = StatusAfter FROM LoanScheduleTransaction WHERE ScheduleID = @ScheduleId AND TransactionID = @LastTransactionId
					END
					
					SELECT @StatusAfterLastTransactionFromLoanSchedule = Status FROM LoanSchedule WHERE Id = @ScheduleId
					
					IF @LastTransactionDate IS NOT NULL 
						AND 
						(
							@StatusAfterLastTransaction = 'Paid' OR 
							@StatusAfterLastTransaction = 'PaidOnTime' OR 
							@StatusAfterLastTransaction = 'PaidEarly'
						)
						AND 
						(
							@StatusAfterLastTransactionFromLoanSchedule = 'Paid' OR 
							@StatusAfterLastTransactionFromLoanSchedule = 'PaidOnTime' OR 
							@StatusAfterLastTransactionFromLoanSchedule = 'PaidEarly'
						)
						SELECT @LateDays = datediff(dd, @ScheduleDate, @LastTransactionDate)
					ELSE
						SELECT @LateDays = datediff(dd, @ScheduleDate, @CutoffDate)
								
					SELECT @LoanAmount = LoanAmount, @LoanDate = Date FROM Loan WHERE Id = @LoanId
					
					INSERT INTO #SpecialLate VALUES (@CustomerId, @CustomerStatus, @LoanId, @Pct, @ScheduleDate, @LastTransactionDate, @PayMentNum, @LateDays, @LoanAmount, @LoanDate, @Principal)
					INSERT INTO #HandledLoans VALUES (@LoanId)
					
					SELECT @LastTransactionId = NULL, @LastTransactionDate = NULL, @LateDays = NULL, @LoanAmount = NULL, @LoanDate = NULL, @StatusAfterLastTransaction = NULL 				
				END
			END
			
			FETCH NEXT FROM cur2 INTO @ScheduleId, @OrigSum, @Pct, @ScheduleDate
		END
		CLOSE cur2
		DEALLOCATE cur2	
		
		DROP TABLE #t
		DROP TABLE #s

		FETCH NEXT FROM cur INTO @LoanId, @CustomerId, @LoanIssueDate, @LoanAmount, @LoanStatus, @Principal, @CustomerStatus
	END
	CLOSE cur
	DEALLOCATE cur

	SELECT @NumOfSpecialLateLoans = Count(1), @SumOfSpecialLateLoans = SUM(LoanAmount), @OutstandingSpecialLatePrincipal = SUM(Principal) FROM #SpecialLate

	SELECT @NumOfSpecialLate1_14Loans = Count(1), @SumOfSpecialLate1_14Loans = SUM(LoanAmount), @OutstandingSpecialLate1_14Principal = SUM(Principal) FROM #SpecialLate 
	WHERE LateDays >= 1 AND LateDays <= 14

	SELECT @NumOfSpecialLate15_30Loans = Count(1), @SumOfSpecialLate15_30Loans = SUM(LoanAmount), @OutstandingSpecialLate15_30Principal = SUM(Principal) FROM #SpecialLate 
	WHERE LateDays >= 15 AND LateDays <= 30

	SELECT @NumOfSpecialLate31_45Loans = Count(1), @SumOfSpecialLate31_45Loans = SUM(LoanAmount), @OutstandingSpecialLate31_45Principal = SUM(Principal) FROM #SpecialLate 
	WHERE LateDays >= 31 AND LateDays <= 45

	SELECT @NumOfSpecialLate46_60Loans = Count(1), @SumOfSpecialLate46_60Loans = SUM(LoanAmount), @OutstandingSpecialLate46_60Principal = SUM(Principal) FROM #SpecialLate 
	WHERE LateDays >= 46 AND LateDays <= 60

	SELECT @NumOfSpecialLateOver60Loans = Count(1), @SumOfSpecialLateOver60Loans = SUM(LoanAmount), @OutstandingSpecialLateOver60Principal = SUM(Principal) FROM #SpecialLate 
	WHERE LateDays > 60

	SELECT 
		@NumOfLoans AS NumOfLoans, @SumOfLoans AS SumOfLoans, @OutstandingPrincipal AS OutstandingPrincipal, 
		@NumOfSpecialLateLoans AS NumOfSpecialLateLoans, @SumOfSpecialLateLoans AS SumOfSpecialLateLoans, @OutstandingSpecialLatePrincipal AS OutstandingSpecialLatePrincipal,
		@NumOfSpecialLate1_14Loans AS NumOfSpecialLate1_14Loans, @SumOfSpecialLate1_14Loans AS SumOfSpecialLate1_14Loans, @OutstandingSpecialLate1_14Principal AS OutstandingSpecialLate1_14Principal,
		@NumOfSpecialLate15_30Loans AS NumOfSpecialLate15_30Loans, @SumOfSpecialLate15_30Loans AS SumOfSpecialLate15_30Loans, @OutstandingSpecialLate15_30Principal AS OutstandingSpecialLate15_30Principal,
		@NumOfSpecialLate31_45Loans AS NumOfSpecialLate31_45Loans, @SumOfSpecialLate31_45Loans AS SumOfSpecialLate31_45Loans, @OutstandingSpecialLate31_45Principal AS OutstandingSpecialLate31_45Principal,
		@NumOfSpecialLate46_60Loans AS NumOfSpecialLate46_60Loans, @SumOfSpecialLate46_60Loans AS SumOfSpecialLate46_60Loans, @OutstandingSpecialLate46_60Principal AS OutstandingSpecialLate46_60Principal,
		@NumOfSpecialLateOver60Loans AS NumOfSpecialLateOver60Loans, @SumOfSpecialLateOver60Loans AS SumOfSpecialLateOver60Loans, @OutstandingSpecialLateOver60Principal AS OutstandingSpecialLateOver60Principal

	SELECT 
		CustomerId,
		CustomerStatus,
		LoanId, 
		Pct, 
		ScheduleDate, 
		LastTransactionDate, 
		PayMentNum, 
		LateDays, 
		LoanAmount, 
		LoanDate,
		Principal 
	FROM
		#SpecialLate

	DROP TABLE #SpecialLate
	DROP TABLE #HandledLoans
END
GO
