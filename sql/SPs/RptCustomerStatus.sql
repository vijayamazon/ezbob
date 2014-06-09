ALTER PROCEDURE RptCustomerStatus
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT C.Id CustomerId,C.Fullname,S.Name Status,L.Principal 
	FROM Customer C,CustomerStatuses S,Loan L 
	WHERE L.CustomerId = C.Id 
	AND S.Id = C.CollectionStatus 
	AND C.IsTest = 0 
	AND CollectionStatus != 0  
	AND L.Principal > 0 
	ORDER BY S.Name,L.Principal DESC
END 

GO