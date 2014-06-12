IF OBJECT_ID('dbo.udfAvgPaidEarlyDays') IS NOT NULL
	DROP FUNCTION dbo.udfAvgPaidEarlyDays
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfAvgPaidEarlyDays (@DateStart DATETIME, @DateEnd DATETIME)
RETURNS INT
AS
BEGIN
	DECLARE @res INT
	DECLARE @lst AS TABLE (Id INT)

	INSERT INTO @lst (Id)
	SELECT DISTINCT
		lst.Id
	FROM 
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN LoanScheduleTransaction lst ON t.Id = lst.TransactionID AND lst.StatusAfter = 'PaidEarly'
	WHERE
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	SELECT
		@res = CONVERT(INT, AVG(DATEDIFF(day, t.PostDate, s.Date)))
	FROM
		@lst tmp
		INNER JOIN LoanScheduleTransaction l ON tmp.Id = l.Id
		INNER JOIN LoanTransaction t ON l.TransactionID = t.Id
		INNER JOIN LoanSchedule s ON l.ScheduleID = s.Id
	
	RETURN ISNULL(@res, 0)
END
GO
