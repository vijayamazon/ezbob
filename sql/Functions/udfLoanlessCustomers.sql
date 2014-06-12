IF OBJECT_ID('dbo.udfLoanlessCustomers') IS NOT NULL
	DROP FUNCTION dbo.udfLoanlessCustomers
GO

CREATE FUNCTION dbo.udfLoanlessCustomers (@DateStart DATETIME, @DateEnd DATETIME)
RETURNS @output TABLE (CustomerCount INT, LoanSum BIGINT)
AS
BEGIN
	DECLARE @l_all AS TABLE (
		LoanID INT,
		CustomerID INT,
		LoanAmount DECIMAL(18, 0),
		DateStart DATETIME,
		DateClosed DATETIME,
		Status NVARCHAR(50)
	)

	DECLARE @l_finished AS TABLE (CustomerID INT)
	DECLARE @l_not_finished AS TABLE (CustomerID INT)
	DECLARE @l_population AS TABLE (CustomerID INT)
	DECLARE @l_result AS TABLE (CustomerID INT)
	DECLARE @l_avg AS TABLE (CustomerID INT, LoanAmount DECIMAL(18, 0))

	------------------------------------------------------------------------------

	INSERT INTO @l_all(LoanID, CustomerID, LoanAmount, DateStart, DateClosed, Status)
	SELECT
		l.Id AS LoanID,
		l.CustomerId,
		l.LoanAmount,
		l.Date,
		l.DateClosed,
		l.Status
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		l.Date < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO @l_finished (CustomerID)
	SELECT DISTINCT
		CustomerID
	FROM
		@l_all
	WHERE
		Status = 'PaidOff'
		AND
		DateClosed < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO @l_not_finished (CustomerID)
	SELECT DISTINCT
		CustomerID
	FROM
		@l_all
	WHERE
		Status != 'PaidOff'
		OR
		DateClosed IS NULL
		OR
		DateClosed >= @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO @l_population (CustomerID)
	SELECT DISTINCT
		CustomerID
	FROM
		@l_all
	WHERE
		Status = 'PaidOff'
		AND
		@DateStart <= DateClosed AND DateClosed < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO @l_result (CustomerID)
	SELECT
		p.CustomerID
	FROM
		@l_population p
		INNER JOIN @l_finished f ON p.CustomerID = f.CustomerID
		LEFT JOIN @l_not_finished n ON p.CustomerID = n.CustomerID
	WHERE
		n.CustomerID IS NULL

	------------------------------------------------------------------------------

	INSERT INTO @l_avg (CustomerID, LoanAmount)
	SELECT
		a.CustomerID,
		AVG(a.LoanAmount) AS LoanAmount
	FROM
		@l_all a
		INNER JOIN @l_result r ON a.CustomerID = r.CustomerID
	GROUP BY
		a.CustomerID

	------------------------------------------------------------------------------

	INSERT INTO @output (CustomerCount, LoanSum)
	SELECT
		COUNT(*),
		CONVERT(BIGINT, SUM(LoanAmount))
	FROM
		@l_avg

	------------------------------------------------------------------------------

	RETURN
END
GO
