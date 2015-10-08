IF OBJECT_ID('RptSalesForceErrors') IS NULL
	EXECUTE('CREATE PROCEDURE RptSalesForceErrors AS SELECT 1')
GO

ALTER PROCEDURE RptSalesForceErrors
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;


	SELECT  s.Created, 
			s.CustomerID, 
			s.Type, 
			s.Model, 
			s.Error  
	FROM SalesForceLog s 
	LEFT JOIN Customer c ON c.Id=s.CustomerID
	WHERE 
		c.IsTest = 0 
		AND 
		Created > @DateStart 
		AND 
		s.Created <= @DateEnd

END
GO