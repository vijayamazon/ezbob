IF OBJECT_ID('RptCampaignReport') IS NULL
	EXECUTE('CREATE PROCEDURE RptCampaignReport AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptCampaignReport
@DateStart DATE,
@DateEnd DATE
AS
BEGIN
	SET NOCOUNT ON;

	-- customers
	WITH 
	CustomerDecisions AS 
	(SELECT c.Id CustomerID, 
	CASE WHEN cr.UnderwriterDecision='Approved' THEN 1 ELSE 0 END AS NumApproves,
	CASE WHEN cr.UnderwriterDecision='Rejected' THEN 1 ELSE 0 END AS NumRejects
	 FROM Customer c LEFT JOIN CashRequests cr ON c.Id=cr.IdCustomer
	WHERE @DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd),
	CustomerApprovesRejects AS 
	(	SELECT cd.CustomerID CustomerID, isnull(sum(cd.NumApproves),0) AS NumApproves, isnull(sum(cd.NumRejects),0) AS NumRejects FROM
		CustomerDecisions cd
		GROUP BY cd.CustomerID
	)
	SELECT
		SUM(isnull(crl.Amount,0)) RequestedAmount,
		SUM(CASE WHEN c.WizardStep IN (1,5,6,2,4) THEN 1 ELSE 0 END) AS Registrations, 
		SUM(CASE WHEN c.WizardStep IN (5,6,2,4) THEN 1 ELSE 0 END) AS Personal, 
		SUM(CASE WHEN c.WizardStep IN (6,2,4) THEN 1 ELSE 0 END) AS 'Company', 
		SUM(CASE WHEN c.WizardStep IN (2,4) THEN 1 ELSE 0 END) AS 'DataSources', 
		SUM(CASE WHEN c.WizardStep = 4 THEN 1 ELSE 0 END) AS Applications, 
		SUM(CASE WHEN car.NumApproves > 0 THEN 1 ELSE 0 END) AS NumOfApproved,
		SUM(CASE WHEN car.NumRejects > 0 AND c.NumApproves = 0 THEN 1 ELSE 0 END) AS NumOfRejected, 
		s.RSource AS Source,
		s.RMedium AS Medium,
		s.RName AS Name,
		s.RTerm AS Term
	INTO
		#temp1
	FROM
		Customer c
		LEFT JOIN CustomerRequestedLoan crl ON crl.CustomerId = c.Id
		LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
		LEFT JOIN Loan l ON l.CustomerId = c.Id
		LEFT JOIN CustomerApprovesRejects car ON car.CustomerID = c.Id
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
		c.IsTest = 0
		AND
		s.RSource IN ('Google', 'Gmail', 'Facebook', 'Bing')
		AND 
		s.RSource IS NOT NULL
		AND
		c.BrokerID IS NULL
	GROUP BY
		s.RSource,
		s.RMedium,
		s.RName,
		s.RTerm

	-- loans (new loans)

	SELECT
		isnull(COUNT(l.Id),0) AS NumOfLoans,
		isnull(SUM(L.loanAmount),0) AS LoanAmount, 
		s.RSource AS Source,
		s.RMedium AS Medium,
		s.RName AS Name,
		s.RTerm AS Term
	INTO
		#temp2	
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id
		INNER JOIN CampaignSourceRef s ON s.CustomerId = c.Id
	WHERE
		c.IsTest = 0
		AND 
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd
		AND 
		l.CustomerId NOT IN (SELECT CustomerId FROM Loan WHERE [Date] < @DateStart)
		AND
	    s.RSource IN ('Google', 'Gmail', 'Facebook', 'Bing')
	   	AND 
	  	s.RSource IS NOT NULL
	  	AND
		c.BrokerID IS NULL
	GROUP BY
		s.RSource,
		s.RMedium,
		s.RName,
		s.RTerm

	-- total

	SELECT
		A.Source, 
		A.Medium,
		A.Term,
		A.Name,
		A.Registrations,
		A.Personal,
		A.Company,
		A.DataSources,
		A.Applications, 
		A.NumOfApproved, 
		A.NumOfRejected, 
		A.RequestedAmount,
		isnull(B.NumOfLoans,0) AS NumOfLoans, 
		isnull(B.LoanAmount,0) AS LoanAmount
		
	FROM
		#temp1 A
		FULL OUTER JOIN #temp2 B 
		ON 
			A.Source = B.Source 
			AND 
			(A.Medium = B.Medium OR (A.Medium IS NULL AND B.Medium IS NULL)) 
			AND 
			(A.Name = B.Name OR (A.Name IS NULL AND B.Name IS NULL))
			AND 
			(A.Term = B.Term OR (A.Term IS NULL AND B.Term IS NULL))
	WHERE A.Registrations IS NOT NULL
  	
	DROP TABLE #temp2
	DROP TABLE #temp1
END
GO