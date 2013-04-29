IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID('RptSourceRef') AND OBJECTPROPERTY(id,N'IsProcedure') = 1) 
BEGIN
	PRINT 'RptSourceRef exists, droping'
	DROP PROCEDURE RptSourceRef 
END
GO 
CREATE PROCEDURE RptSourceRef
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN 

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#temp1') is not NULL 
BEGIN
	DROP TABLE #temp1
END

---- SignUp ----
CREATE TABLE #temp1
				(	
					Type NVARCHAR(50),
					CustomerId INT, 
					EmailAddress NVARCHAR(128), 
					SignUpDate DATETIME, 
					FullName NVARCHAR(200), 
					SourceRef NVARCHAR(200), 
					WizardStep INT,
					LatestRequestDate DATETIME,
					UnderwriterDecision NVARCHAR(50),
					ApprovedSum DECIMAL(18,0),
					AmountIssued NUMERIC(18,0),
					InterestRate DECIMAL(18,4),
					LoanDate DATETIME
				 )

INSERT INTO #temp1 (Type,
					CustomerId, 
					EmailAddress, 
					SignUpDate, 
					FullName, 
					SourceRef, 
					WizardStep)
SELECT 'Register',C.Id,C.Name,C.GreetingMailSentDate,C.Fullname,C.ReferenceSource,C.WizardStep
FROM Customer C
WHERE 	C.Name NOT LIKE '%test%'
		AND C.Name NOT LIKE '%@ezbob%'
		AND C.Name NOT LIKE '%q@q%'
		AND C.Name NOT LIKE '%w@w%'
		AND C.IsTest=0
		AND C.GreetingMailSentDate >= @DateStart
		AND C.GreetingMailSentDate < @DateEnd
ORDER BY 1

---- Cash Request ----
INSERT INTO #temp1 (Type,
					CustomerId, 
					EmailAddress, 
					SignUpDate, 
					FullName, 
					SourceRef, 
					WizardStep,
					LatestRequestDate,
					UnderwriterDecision,
					ApprovedSum
					)					
SELECT 	'Applied',R.IdCustomer,C.Name,C.GreetingMailSentDate,C.Fullname,C.ReferenceSource,C.WizardStep,max(R.CreationDate),
		R.UnderwriterDecision,R.ManagerApprovedSum
FROM CashRequests R
JOIN Customer C ON C.Id = R.IdCustomer
WHERE 	C.Name NOT LIKE '%test%'
		AND C.Name NOT LIKE '%@ezbob%'
		AND C.Name NOT LIKE '%q@q%'
		AND C.Name NOT LIKE '%w@w%'
		AND C.IsTest=0
		AND R.CreationDate >= @DateStart
		AND R.CreationDate < @DateEnd
GROUP BY R.IdCustomer,C.Name,C.GreetingMailSentDate,C.Fullname,C.ReferenceSource,C.WizardStep,R.UnderwriterDecision,R.ManagerApprovedSum
ORDER BY 1


---- Approved ----
INSERT INTO #temp1 (Type,
					CustomerId, 
					EmailAddress, 
					SignUpDate, 
					FullName, 
					SourceRef, 
					WizardStep,
					LatestRequestDate,
					UnderwriterDecision,
					ApprovedSum,
					InterestRate					
					)	
SELECT 'Approved',R.IdCustomer,C.Name,C.GreetingMailSentDate,C.Fullname,C.ReferenceSource,C.WizardStep,max(R.CreationDate),
		R.UnderwriterDecision,R.ManagerApprovedSum,R.InterestRate
FROM CashRequests R
JOIN Customer C ON C.Id = R.IdCustomer
WHERE 	C.Name NOT LIKE '%test%'
		AND C.Name NOT LIKE '%@ezbob%'
		AND C.Name NOT LIKE '%q@q%'
		AND C.Name NOT LIKE '%w@w%'
		AND C.IsTest=0
	  	AND R.UnderwriterDecision = 'approved'
		AND R.CreationDate >= @DateStart
		AND R.CreationDate < @DateEnd	
GROUP BY R.IdCustomer,C.Name,C.GreetingMailSentDate,C.Fullname,C.ReferenceSource,C.WizardStep,
		 R.UnderwriterDecision,R.ManagerApprovedSum,R.InterestRate
ORDER BY 1

----  Took Loans -----
INSERT INTO #temp1 (Type,
					CustomerId, 
					EmailAddress, 
					SignUpDate, 
					FullName, 
					SourceRef, 
					WizardStep,
					LatestRequestDate,
					UnderwriterDecision,
					ApprovedSum,
					InterestRate,
					LoanDate,
					AmountIssued
					)	
SELECT 'TookLoan',R.IdCustomer,C.Name,C.GreetingMailSentDate,C.Fullname,C.ReferenceSource,C.WizardStep,max(R.CreationDate),
		R.UnderwriterDecision,R.ManagerApprovedSum,R.InterestRate,L.[Date],L.LoanAmount
FROM Customer C
JOIN Loan L ON L.CustomerId = C.Id
JOIN CashRequests R ON L.RequestCashId = R.Id
WHERE 
	  C.Name NOT LIKE '%test%'
	  AND C.Name NOT LIKE '%@ezbob%'
	  AND C.Name NOT LIKE '%q@q%'
	  AND C.Name NOT LIKE '%w@w%'
	  AND C.IsTest=0	
	  AND L.[Date] >= @DateStart
	  AND L.[Date] < @DateEnd
GROUP BY R.IdCustomer,C.Name,C.GreetingMailSentDate,C.Fullname,C.ReferenceSource,C.WizardStep,
		 R.UnderwriterDecision,R.ManagerApprovedSum,R.InterestRate,L.[Date],L.LoanAmount
ORDER BY 5

SELECT * FROM #temp1

SET NOCOUNT OFF;

END
GO
