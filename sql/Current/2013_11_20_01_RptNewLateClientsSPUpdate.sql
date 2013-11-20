IF OBJECT_ID('RptNewLateClients') IS NULL
	EXECUTE('CREATE PROCEDURE RptNewLateClients AS SELECT 1')
GO

ALTER PROCEDURE RptNewLateClients
@DateStart DATE,
@DateEnd DATE
AS
SELECT C.Id,C.Name,C.FirstName,C.Surname,AmountDue 
FROM LoanSchedule S,Customer C,Loan L 
WHERE 
	C.IsTest = 0 AND 
	C.Id = L.CustomerId AND 
	S.LoanId = L.Id AND 
	S.Date >= @DateStart AND 
	S.Date < @DateEnd AND 
	S.Status IN ('StillToPay','Late') AND 
	C.CollectionStatus in (0,6)

GO