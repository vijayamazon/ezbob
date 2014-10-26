IF OBJECT_ID('RptDefaultsDetailed') IS NULL
	EXECUTE('CREATE PROCEDURE RptDefaultsDetailed AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptDefaultsDetailed
	(@CutoffDate DATETIME, @MinPct NUMERIC(18,6))
AS
BEGIN

------------------ TEMP TABLES CREATIONS ------------------
if OBJECT_ID('tempdb..#SpecialLate') is not NULL
BEGIN
	DROP TABLE #SpecialLate
END

if OBJECT_ID('tempdb..#HandledLoans') is not NULL
BEGIN
	DROP TABLE #HandledLoans
END

if OBJECT_ID('tempdb..#temp1') is not NULL
BEGIN
	DROP TABLE #temp1
END

if OBJECT_ID('tempdb..#temp1') is not NULL
BEGIN
	DROP TABLE #temp2
END

if OBJECT_ID('tempdb..#CollectionPayments') is not NULL
BEGIN
	DROP TABLE #CollectionPayments
END


/*DECLARE @CutoffDate DATETIME, @MinPct NUMERIC(18,6)

SET @CutoffDate = '2014-10-01'
SET @MinPct = 0.2
*/
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


---------- GET TOTAL AMOUNT ISSUED ,COUNT & OUTSTANDING PRINCIPAL TILL CUTOFFDATE ----------
	SET @NumOfLoans =  (SELECT count(L.Id)
						FROM Loan L
						WHERE L.CustomerId NOT IN 
											  (SELECT C.Id
											   FROM Customer C 
											   WHERE Name LIKE '%ezbob%'
											   OR Name LIKE '%liatvanir%'
											   OR Name LIKE '%q@q%'
											   OR Name LIKE '%1@1%'
											   OR C.IsTest=1) 
	  						  AND L.[Date] < @CutOffDate);
	
	SET @SumOfLoans =  (SELECT sum(L.LoanAmount)
						FROM Loan L
						WHERE L.CustomerId NOT IN 
											  (SELECT C.Id
											   FROM Customer C 
											   WHERE Name LIKE '%ezbob%'
											   OR Name LIKE '%liatvanir%'
											   OR Name LIKE '%q@q%'
											   OR Name LIKE '%1@1%'
											   OR C.IsTest=1) 
	  						  AND L.[Date] < @CutOffDate);
	
	SET @OutstandingPrincipal = ((SELECT sum(L.LoanAmount)
								 FROM Loan L
								 WHERE L.CustomerId NOT IN 
														  (SELECT C.Id
														   FROM Customer C 
														   WHERE Name LIKE '%ezbob%'
														   OR Name LIKE '%liatvanir%'
														   OR Name LIKE '%q@q%'
														   OR Name LIKE '%1@1%'
														   OR C.IsTest=1) 
				  					   AND L.[Date] < @CutOffDate) -
								   (SELECT sum(T.LoanRepayment)
									FROM LoanTransaction T
									JOIN loan l ON L.Id = T.LoanId
									WHERE T.PostDate < @CutOffDate
										  AND T.Type = 'PaypointTransaction'
										  AND T.Status = 'Done'
										  AND L.CustomerId NOT IN 
																	  (SELECT C.Id
																	   FROM Customer C 
																	   WHERE Name LIKE '%ezbob%'
																	   OR Name LIKE '%liatvanir%'
																	   OR Name LIKE '%q@q%'
																	   OR Name LIKE '%1@1%'
																	   OR C.IsTest=1)))
		
	CREATE TABLE #SpecialLate
	(
		CustomerId INT,
		CustomerStatus NVARCHAR(100),
		LoanId INT, 
		Pct NUMERIC(18,6), 
		ScheduleDate DATETIME, 
		FirstTransactionDate DATETIME, 
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

--------- TEMP TABLE TO ADD ADDITIONAL COLUMNS TO FINAL TABLE -------------
/*	
	SELECT L.Id AS LoanId,
		   C.Id AS CustomerId,
		   CASE 
		   WHEN C.IsOffline = 0 THEN 'Online'
		   WHEN C.IsOffline = 1 THEN 'Offline'
		   END AS OfflineOrOnline,
		   CASE
		   WHEN C.BrokerID IS NULL THEN 'Not Broker Client'
		   ELSE 'Broker Client' 
		   END AS IsBrokerClient,
		   CASE 
		   WHEN L.LoanSourceID = 1 THEN 'Standard'
		   WHEN L.LoanSourceID = 2 THEN 'EU'
		   END AS IsEuLoan,
		   RANK() OVER (PARTITION BY L.CustomerId ORDER BY L.[Date],L.Id DESC) AS LoanNumber
	INTO #temp1
	FROM Loan L
	JOIN Customer C ON C.Id = L.CustomerId
	ORDER BY 2,6
*/

    DECLARE @tmp_sp1 TABLE 
     				   (CustomerId INT,
     				   LoanID INT,
     				   LoanDate DATETIME,
     				   IssueMonth DATETIME,
     				   LoanAmount NUMERIC (18, 0),
     				   InterestRate DECIMAL (18, 4),
     				   RepaymentPeriod INT,
     				   OutstandingPrincipal DECIMAL (18, 2),
     				   ManualSetupFeeAmount INT,
     				   SetupFee NUMERIC (37, 4),
     				   LoanNumber BIGINT,
     				   CustomerRequestedAmount DECIMAL (18, 0),
     				   ReferenceSource NVARCHAR (1000),
     				   SourceRefGroup VARCHAR (16),
     				   IsOffline BIT,
     				   Loan_Type VARCHAR (8),
     				   BrokerOrNot VARCHAR (12),
     				   Quarter VARCHAR (7),
     				   NewOldLoan VARCHAR (3)
     				   );
    INSERT INTO @tmp_sp1 
    EXECUTE RptAllLoansIssued;
	
----------------------- TEMP TABLE --------------------	
	SELECT 	S.CustomerId,
			S.CustomerStatus,
			T1.IsOffline,
			T1.BrokerOrNot,
			T1.Loan_Type,
			T1.LoanNumber,
			S.LoanId, 
			S.Pct, 
			S.ScheduleDate, 
			S.FirstTransactionDate,
			S.PayMentNum, 
			S.LateDays,
			S.LoanAmount, 
			S.LoanDate,
			S.Principal,
			CASE
			WHEN S.LateDays >= 1 AND S.LateDays <= 14 THEN '1 - 14 days missed'
			WHEN S.LateDays >= 15 AND S.LateDays <= 30 THEN '15 - 30 days missed'
			WHEN S.LateDays >= 31 AND S.LateDays <= 45 THEN '31 - 45 days missed'
			WHEN S.LateDays >= 46 AND S.LateDays <= 60 THEN '46 - 60 days missed'
			WHEN S.LateDays >= 61 AND S.LateDays <= 90 THEN '60 - 90 days missed'
			WHEN S.LateDays >= 91 THEN '90+ days missed'
			ELSE 'No Group'
			END AS MissedPaymentDays,
			CASE 
			WHEN S.LoanDate BETWEEN '2012-07-01' AND '2013-01-01' THEN 'Q4-2012'
			WHEN S.LoanDate BETWEEN '2013-01-01' AND '2013-04-01' THEN 'Q1-2013'
			WHEN S.LoanDate BETWEEN '2013-04-01' AND '2013-07-01' THEN 'Q2-2013'
			WHEN S.LoanDate BETWEEN '2013-07-01' AND '2013-10-01' THEN 'Q3-2013'
			WHEN S.LoanDate BETWEEN '2013-10-01' AND '2014-01-01' THEN 'Q4-2013'
			WHEN S.LoanDate BETWEEN '2014-01-01' AND '2014-04-01' THEN 'Q1-2014'
			WHEN S.LoanDate BETWEEN '2014-04-01' AND '2014-07-01' THEN 'Q2-2014'
			WHEN S.LoanDate BETWEEN '2014-07-01' AND '2014-10-01' THEN 'Q3-2014'
			WHEN S.LoanDate BETWEEN '2014-10-01' AND '2015-01-01' THEN 'Q4-2014'
			ELSE 'No Q'
			END AS Quarter,
			dateadd(month,S.PayMentNum,S.LoanDate) AS DefaultDate,
			T1.NewOldLoan,
			T1.IssueMonth
	INTO #temp2
	FROM #SpecialLate S
	JOIN @tmp_sp1 T1 ON T1.LoanId = S.LoanId
		
----------- COLLECTION PAYMENTS AFTER THE DEFAULT DATE -------------
	
	SELECT T.LoanId,
		   sum(T.LoanRepayment) AS PrincipalPaid,
		   sum(T.Interest) AS Interest,
		   sum(T.Fees) AS Fees
		   
	INTO #CollectionPayments
	FROM LoanTransaction T 
	LEFT JOIN #temp2 T2 ON T2.LoanId = T.LoanId
	WHERE T.PostDate >= T2.DefaultDate
		  AND T.Type = 'PaypointTransaction'
		  AND T.Status = 'Done'	
	GROUP BY T.LoanId

		
------------------------ FINAL TABLE -----------------------
	SELECT T.CustomerId,
		   T.CustomerStatus,
		   T.IsOffline AS OfflineOrOnline,
		   T.BrokerOrNot AS IsBrokerClient,
		   T.Loan_Type AS IsEuLoan,
		   T.LoanNumber,
		   T.LoanId,
		   T.ScheduleDate,
		   T.FirstTransactionDate,
		   CASE
		   WHEN T.PayMentNum > 12 THEN '12'
		   ELSE T.PayMentNum
		   END AS PaymentNum,
		   T.LateDays,
		   T.LoanAmount,
		   T.LoanDate,
		   T.Principal,
		   T.MissedPaymentDays,
		   T.Quarter,
		   T.DefaultDate,
		   C.PrincipalPaid,
		   C.Interest,
		   C.Fees,
		   T.NewOldLoan,
		   T.IssueMonth
			
	FROM #temp2 T
	LEFT JOIN #CollectionPayments C ON C.LoanId = T.LoanId
	

-------------- COLLECTION PAYMENTS FINAL TABLE -------------

	SELECT T2.CustomerId,
		   T2.CustomerStatus,
		   T2.OfflineOrOnline,
		   T2.IsBrokerClient,
		   T2.IsEuLoan,
		   T2.LoanId,
		   T2.LoanDate,
		   DATEADD(MONTH,DATEDIFF(MONTH, 0,T.PostDate), 0) AS PaidMonth,
		   T.LoanRepayment AS PrincipalPaid,
		   T.Interest AS InterestPaid,
		   T.Fees AS FeesPaid
	FROM #temp2 T2
	LEFT JOIN LoanTransaction T ON T.LoanId = T2.LoanId
	
	WHERE T.PostDate >= T2.DefaultDate
		  AND T.Type = 'PaypointTransaction'
		  AND T.Status = 'Done'	
	

	
DROP TABLE #SpecialLate
DROP TABLE #HandledLoans
DROP TABLE #temp2
DROP TABLE #CollectionPayments


END
GO

