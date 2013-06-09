IF OBJECT_ID('RptLoansGiven') IS NOT NULL
	DROP PROCEDURE RptLoansGiven
GO

CREATE PROCEDURE RptLoansGiven
@DateStart DATETIME,
@DateEnd DATETIME
AS
SELECT
	l.Id AS LoanID,
	l.Date,
	c.Id AS ClientID,
	c.Name AS ClientEmail,
	c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
	lt.Name AS LoanTypeName,
	l.SetupFee,
	l.LoanAmount,
	COUNT(*) AS Period,
	SUM(s.Interest) AS PlannedInterest,
	SUM(s.AmountDue) AS PlannedRepaid
FROM
	Loan l
	INNER JOIN Customer c ON l.CustomerId = c.Id
	INNER JOIN LoanType lt ON l.LoanTypeId = lt.Id
	INNER JOIN LoanSchedule s ON l.Id = s.LoanId
WHERE
	l.Date BETWEEN @DateStart AND @DateEnd
GROUP BY
	l.Id,
	l.Date,
	c.Id,
	c.Name,
	c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname,
	lt.Name,
	l.SetupFee,
	l.LoanAmount
ORDER BY
	l.Date
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_LOANS_GIVEN')
BEGIN
	INSERT INTO ReportScheduler VALUES('RPT_LOANS_GIVEN', 'Loans Given', 'RptLoansGiven', 1, 0, 0,
	'Date,Client ID,Loan ID,Client Name,Client Email,Loan Type,Setup Fee,Amount,Period,Planned Interest,Planned Repaid',
	'Date,!ClientID,!LoanID,ClientName,ClientEmail,LoanTypeName,SetupFee,LoanAmount,Period,PlannedInterest,PlannedRepaid',
	'nimrodk@ezbob.com,alexbo+rpt@ezbob.com', 0)
END
ELSE BEGIN
	UPDATE ReportScheduler SET
		Title = 'Loans Given',
		StoredProcedure = 'RptLoansGiven',
		IsDaily = 1,
		IsWeekly = 0,
		IsMonthly = 0,
		Header = 'Date,Client ID,Loan ID,Client Name,Client Email,Loan Type,Setup Fee,Amount,Period,Planned Interest,Planned Repaid',
		Fields = 'Date,!ClientID,!LoanID,ClientName,ClientEmail,LoanTypeName,SetupFee,LoanAmount,Period,PlannedInterest,PlannedRepaid',
		ToEmail = 'nimrodk@ezbob.com,alexbo+rpt@ezbob.com',
		IsMonthToDate = 0
	WHERE
		Type = 'RPT_LOANS_GIVEN'
END
GO

IF OBJECT_ID('RptPaymentsReceived') IS NOT NULL
	DROP PROCEDURE RptPaymentsReceived
GO

CREATE PROCEDURE RptPaymentsReceived
@DateStart DATETIME,
@DateEnd DATETIME
AS
SELECT
	t.PostDate,
	t.LoanId,
	c.Id AS ClientID,
	c.Name AS ClientEmail,
	c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
	t.Amount,
	t.LoanRepayment,
	t.Interest,
	t.Fees,
	t.Rollover,
	CASE PaypointId WHEN '--- manual ---' THEN 'Manual' ELSE 'Paypoint' END AS TransactionType,
	t.Description,
	CASE
		WHEN t.LoanRepayment + t.Interest + t.Fees + t.Rollover = t.Amount
			THEN ''
		ELSE 'unmatched'
	END AS SumMatch
FROM
	LoanTransaction t
	INNER JOIN Loan l ON t.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id
WHERE
	t.PostDate BETWEEN @DateStart AND @DateEnd
	AND
	t.Status = 'Done'
	AND
	c.IsTest = 0
	AND
	t.Type = 'PaypointTransaction'
ORDER BY
	t.PostDate,
	t.Id
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_PAYMENTS_RECEIVED')
BEGIN
	INSERT INTO ReportScheduler VALUES('RPT_PAYMENTS_RECEIVED', 'Payments Received', 'RptPaymentsReceived', 1, 0, 0,
		'Date,Client ID,Loan ID,Client Name,Client Email,Paid Amount,Principal,Interest,Fees,Rollover,Payment Type,Description,Sum Match',
		'PostDate,!ClientID,!LoanId,ClientName,ClientEmail,Amount,LoanRepayment,Interest,Fees,Rollover,TransactionType,Description,{SumMatch',
		'nimrodk@ezbob.com,alexbo+rpt@ezbob.com', 0)
END
ELSE BEGIN
	UPDATE ReportScheduler SET
		Title = 'Payments Received',
		StoredProcedure = 'RptPaymentsReceived',
		IsDaily = 1,
		IsWeekly = 0,
		IsMonthly = 0,
		Header = 'Date,Loan ID,Client ID,Client Email,Client Name,Paid Amount,Principal,Interest,Fees,Rollover,Payment Type,Description,Sum Match',
		Fields = 'PostDate,!LoanId,!ClientID,ClientEmail,ClientName,Amount,LoanRepayment,Interest,Fees,Rollover,TransactionType,Description,{SumMatch',
		ToEmail = 'nimrodk@ezbob.com,alexbo+rpt@ezbob.com',
		IsMonthToDate = 0
	WHERE
		Type = 'RPT_PAYMENTS_RECEIVED'
END
GO

IF OBJECT_ID('RptOverallStats') IS NOT NULL
	DROP PROCEDURE RptOverallStats
GO

CREATE PROCEDURE RptOverallStats
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
SET NOCOUNT ON 
DECLARE @Money_Given FLOAT = (SELECT sum(Amount) 
						FROM LoanTransaction 
						WHERE Status='Done' AND 
						Type='PacnetTransaction' AND 
						LoanId IN (SELECT Id 
								   FROM Loan 
								   WHERE CustomerId 
								   NOT IN (SELECT Id FROM Customer WHERE IsTest = 1)))
DECLARE @Money_Repaid FLOAT = (SELECT sum(Principal) 
					   FROM LoanTransaction 
					   WHERE Status='Done' AND 
					   Type='PaypointTransaction' AND 
					   LoanId IN (SELECT Id 
					              FROM Loan 
					   		      WHERE CustomerId 
								  NOT IN (SELECT Id FROM Customer WHERE IsTest = 1)))
								  
DECLARE @Money_Out FLOAT = @Money_Given - @Money_Repaid

DECLARE @LastDay_CurrentMonth DATETIME = (SELECT DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,GETDATE())+1,0)))
SELECT y.LineId, y.Name, y.Value FROM
(
SELECT 11 LineId, 'Total Anual Shop Revenue that where given loans' Name, parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(ValueFloat)))),1),2) Value
FROM MP_AnalyisisFunctionValues 
WHERE AnalyisisFunctionId IN (SELECT Id FROM MP_AnalyisisFunction WHERE Name='TotalSumOfOrders')
AND AnalysisFunctionTimePeriodId IN (SELECT Id FROM MP_AnalysisFunctionTimePeriod WHERE Name='365')

UNION

SELECT 10 LineId, 'Total Money to be Repaid Until End of Month' Name, parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(AmountDue)))),1),2) Value
FROM LoanSchedule 
WHERE Status='StillToPay' AND 
([Date] BETWEEN GETDATE() AND @LastDay_CurrentMonth) AND
LoanId IN (SELECT Id FROM Loan WHERE CustomerId NOT IN (SELECT Id FROM Customer WHERE IsTest = 1))

UNION

SELECT 2 LineId, 'Total Money Given' Name, parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), @Money_Given))),1),2) Value

UNION

SELECT 3 LineId, 'Total Money Repaid' Name, parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), @Money_Repaid))),1),2) Value 

UNION

SELECT 1 LineId, 'Total Money Out' Name, parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), @Money_Out))),1),2) Value 

UNION

SELECT 7 LineId, 'Setup Fee' Name,  parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(ISNULL(Fees,0))))),1),2) Value 
FROM LoanTransaction 
WHERE Type='PacnetTransaction' 
AND Status = 'Done'

UNION

SELECT 5 LineId, 'Interest Back' Name,  parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(LoanTransaction.Interest)))),1),2) Value
FROM LoanTransaction 
WHERE Type  = 'PaypointTransaction' 
AND Status = 'Done'
AND LoanId IN (SELECT Id FROM Loan WHERE CustomerId NOT IN (SELECT Id FROM Customer WHERE IsTest = 1))

UNION

SELECT 4 LineId, 'Principal Back' Name,  parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(LoanTransaction.LoanRepayment)))),1),2) Value   
FROM LoanTransaction 
WHERE Type  = 'PaypointTransaction' 
AND Status = 'Done'
AND LoanId IN (SELECT Id FROM Loan WHERE CustomerId NOT IN (SELECT Id FROM Customer WHERE IsTest = 1))
UNION

SELECT 6 LineId, 'Fees Back' Name,  parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(LoanTransaction.Fees)))),1),2) Value   
FROM LoanTransaction 
WHERE Type  = 'PaypointTransaction' 
AND Status = 'Done'
AND LoanId IN (SELECT Id FROM Loan WHERE CustomerId NOT IN (SELECT Id FROM Customer WHERE IsTest = 1))

UNION

SELECT 8 LineId, 'Late Money' Name, parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(x.Amount)))),1),2) Value 
FROM (
SELECT  LoanId, max(LoanSchedule.AmountDue) Amount 
FROM LoanSchedule
WHERE ((Status='StillToPay' AND [Date]<GETDATE()) OR (RepaymentAmount = 0 AND Status='Late'))
AND LoanId IN (SELECT Id FROM Loan WHERE CustomerId NOT IN (SELECT Id FROM Customer WHERE IsTest = 1))
GROUP BY LoanId
) x

UNION 

SELECT 9 LineId, 'Late Principal' Name, parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(x.Amount)))),1),2) Value 
FROM (
SELECT  LoanId, max(LoanSchedule.LoanRepayment) Amount 
FROM LoanSchedule
WHERE ((Status='StillToPay' AND [Date]<GETDATE()) OR (RepaymentAmount = 0 AND Status='Late'))
AND LoanId IN (SELECT Id FROM Loan WHERE CustomerId NOT IN (SELECT Id FROM Customer WHERE IsTest = 1))
GROUP BY LoanId
) x
) y
ORDER BY y.LineId
END

GO
