IF OBJECT_ID('RptCustomerStatus') IS NULL
	EXECUTE('CREATE PROCEDURE RptCustomerStatus AS SELECT 1')
GO

ALTER PROCEDURE RptCustomerStatus
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN

	;WITH FirstNotRepaidLoan AS 
	(
		SELECT c.Id, min(ls.[Date]) AS FirstMissedRepaymentDate FROM Customer c 
		INNER JOIN Loan l ON c.Id=l.CustomerId
		INNER JOIN LoanSchedule ls ON l.Id=ls.LoanId
		LEFT JOIN LoanScheduleTransaction lst ON ls.Id=lst.ScheduleID
		WHERE c.IsTest=0 AND c.CollectionStatus<>0 AND lst.ID IS NULL AND ls.[Date]<getdate()
		GROUP BY c.Id
	)
	SELECT C.Id CustomerId,C.Fullname,CS.Name Status,sum(L.Principal) AS Principal, 
		CASE WHEN LS.LoanSourceName = 'EU' THEN 'EU' ELSE '' END AS EU,
		C.TypeOfBusiness,CP.Description ResidentialStatus, FirstNotRepaidLoan.FirstMissedRepaymentDate 
	FROM Customer C INNER JOIN CustomerStatuses CS ON C.CollectionStatus=CS.Id 
	INNER JOIN Loan L ON C.Id=L.CustomerId
	INNER JOIN LoanSource LS ON LS.LoanSourceID=L.LoanSourceID
	INNER JOIN CustomerPropertyStatuses CP ON CP.Id=C.PropertyStatusId
	LEFT JOIN  FirstNotRepaidLoan ON C.Id = FirstNotRepaidLoan.Id
	WHERE C.IsTest = 0 
	AND CollectionStatus != 0  
	AND L.Principal > 0 
	GROUP BY C.Id,C.Fullname,CS.Name,LS.LoanSourceName,C.TypeOfBusiness,CP.Description,FirstNotRepaidLoan.FirstMissedRepaymentDate 
	ORDER BY CS.Name,sum(L.Principal) DESC
END 


GO