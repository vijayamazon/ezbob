IF OBJECT_ID('RptCustomerStatus') IS NULL
	EXECUTE('CREATE PROCEDURE RptCustomerStatus AS SELECT 1')
GO

ALTER PROCEDURE RptCustomerStatus
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT C.Id CustomerId,C.Fullname,CS.Name Status,L.Principal, 
		CASE WHEN LS.LoanSourceName = 'EU' THEN 'EU' ELSE '' END AS EU
	FROM Customer C,CustomerStatuses CS,Loan L, LoanSource LS
	WHERE L.CustomerId = C.Id 
	AND CS.Id = C.CollectionStatus 
	AND C.IsTest = 0 
	AND CollectionStatus != 0  
	AND L.Principal > 0 
	AND L.LoanSourceID = LS.LoanSourceID
	ORDER BY CS.Name,L.Principal DESC
END 

GO