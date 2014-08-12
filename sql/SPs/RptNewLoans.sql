IF OBJECT_ID('RptNewLoans') IS NULL
	EXECUTE('CREATE PROCEDURE RptNewLoans AS SELECT 1')
GO

ALTER PROCEDURE RptNewLoans
	(@DateStart DATETIME,
	 @DateEnd DATETIME)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		L.customerId AS CustomerId,
		L.loanAmount, 
		CASE WHEN BrokerID IS NULL THEN C.ReferenceSource ELSE 'Broker' END AS SourceRef,
		C.GoogleCookie,
		datepart(mm,C.GreetingMailSentDate) AS MonthPart
	FROM Loan L,Customer C
	WHERE
		C.IsTest = 0 AND 
		C.Id = L.CustomerId AND 
		L.[Date] > @DateStart AND 
		L.[Date] < @DateEnd AND 
		L.CustomerId NOT IN (SELECT customerId FROM Loan WHERE [Date] < @DateStart)
	ORDER BY SourceRef
END
GO
