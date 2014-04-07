IF OBJECT_ID('RptReferenceSources') IS NULL
	EXECUTE('CREATE PROCEDURE RptReferenceSources AS SELECT 1')
GO

ALTER PROCEDURE RptReferenceSources
	(@DateStart DATETIME,
	 @DateEnd DATETIME)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		CustomerId,
		l.LoanAmount,
		C.ReferenceSource,
		C.IsOffline,
		datepart(mm,C.GreetingMailSentDate) AS MonthPart,
		datepart(yy,C.GreetingMailSentDate) AS YearPart,
		C.GoogleCookie,L.[Date] 
	FROM 
		Loan L,
		Customer C 
	WHERE 
		C.IsTest = 0 AND 
		C.Id = L.CustomerId AND 
		L.[Date] > @DateStart AND 
		L.[Date] < @DateEnd AND 
		L.CustomerId IN 
		( 
			SELECT 
				customerId 
			FROM 
				Loan 
			GROUP BY 
				CustomerId 
			HAVING 
				count(1) = 1
		) 
	ORDER BY 
		C.ReferenceSource 
END
GO
