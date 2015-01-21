IF object_id('RptChannelMarketing') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RptChannelMarketing AS SELECT 1')
END
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE RptChannelMarketing
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
	CASE WHEN cr.UnderwriterDecision='Rejected' THEN 1 ELSE 0 END AS NumRejects,
	CASE WHEN c.BrokerId IS NULL THEN 0 ELSE 1 END AS IsBroker
	 FROM Customer c LEFT JOIN CashRequests cr ON c.Id=cr.IdCustomer
	WHERE c.GreetingMailSentDate>'2014-01-01'),
	CustomerApprovesRejects AS 
	(	SELECT cd.CustomerID CustomerID, isnull(sum(cd.NumApproves),0) AS NumApproves, isnull(sum(cd.NumRejects),0) AS NumRejects, cd.IsBroker
	    FROM CustomerDecisions cd
		GROUP BY cd.CustomerID, cd.IsBroker
	)
	SELECT
		SUM(crl.Amount) RequestedAmount,
		SUM(CASE WHEN c.WizardStep IN (1,5,6,2,4) THEN 1 ELSE 0 END) AS Registrations, 
		SUM(CASE WHEN c.WizardStep IN (5,6,2,4) THEN 1 ELSE 0 END) AS Personal, 
		SUM(CASE WHEN c.WizardStep IN (6,2,4) THEN 1 ELSE 0 END) AS 'Company', 
		SUM(CASE WHEN c.WizardStep IN (2,4) THEN 1 ELSE 0 END) AS 'DataSources', 
		SUM(CASE WHEN c.WizardStep = 4 THEN 1 ELSE 0 END) AS Applications, 
		SUM(CASE WHEN car.NumApproves > 0 THEN 1 ELSE 0 END) AS NumOfApproved,
		SUM(CASE WHEN car.NumRejects > 0 AND c.NumApproves = 0 THEN 1 ELSE 0 END) AS NumOfRejected, 
		CASE WHEN car.IsBroker = 0 THEN s.RSource ELSE 'Broker' END  AS Source,
		CASE WHEN car.IsBroker = 0 THEN s.RMedium ELSE '' END AS Medium
	INTO
		#temp1
	FROM
		Customer c
		INNER JOIN CustomerRequestedLoan crl ON crl.CustomerId = c.Id
		LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
		LEFT JOIN Loan l ON l.CustomerId = c.Id
		LEFT JOIN CustomerApprovesRejects car ON car.CustomerID = c.Id
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
		c.IsTest = 0
	GROUP BY
		s.RSource,
		s.RMedium,
		car.IsBroker

	-- loans (new loans)
	
	SELECT
		isnull(COUNT(l.Id),0) AS NumOfLoans,
		isnull(SUM(L.loanAmount),0) AS LoanAmount, 
		CASE WHEN c.BrokerID IS NULL THEN s.RSource ELSE 'Broker' END  AS Source,
		CASE WHEN c.BrokerID IS NULL THEN s.RMedium ELSE '' END AS Medium
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
		l.CustomerId NOT IN (SELECT customerId FROM Loan WHERE [Date] < @DateStart)
	GROUP BY
		s.RSource,
		s.RMedium,
		c.BrokerID

	-- total

	SELECT
		A.Source, 
		A.Medium,
		sum(A.Registrations) AS Registrations,
		sum(A.Personal) AS Personal,
		sum(A.Company) AS Company,
		sum(A.DataSources) AS DataSources,
		sum(A.Applications) AS Applications, 
		sum(A.NumOfApproved) AS NumOfApproved, 
		sum(A.NumOfRejected) AS NumOfRejected, 
		sum(A.RequestedAmount) AS RequestedAmount,
		isnull(sum(B.NumOfLoans),0) AS NumOfLoans, 
		isnull(sum(B.LoanAmount),0) AS LoanAmount
		
	FROM
		#temp1 A
		FULL JOIN #temp2 B ON A.Source = B.Source AND A.Medium = B.Medium
	WHERE A.Registrations IS NOT NULL
	GROUP BY 
		A.Source, 
		A.Medium
				
	DROP TABLE #temp2
	DROP TABLE #temp1
END
GO