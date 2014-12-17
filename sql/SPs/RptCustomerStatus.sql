
IF OBJECT_ID('RptCustomerStatus') IS NULL
	EXECUTE('CREATE PROCEDURE RptCustomerStatus AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptCustomerStatus
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN

	;WITH FirstNotRepaidLoan AS 
	(
		SELECT c.Id, min(ls.[Date]) AS FirstMissedRepaymentDate 
		FROM Customer c 
		INNER JOIN Loan l ON c.Id=l.CustomerId
		INNER JOIN LoanSchedule ls ON l.Id=ls.LoanId
		LEFT JOIN LoanScheduleTransaction lst ON ls.Id=lst.ScheduleID
		WHERE c.IsTest=0 AND c.CollectionStatus<>0 AND lst.ID IS NULL AND ls.[Date]<getdate()
		GROUP BY c.Id
	),
	PersonalAddress AS
	(
		SELECT c.Id AS CustomerID, (isnull(Line1, '') + ' ' + isnull(a.Line2, '') + ' ' + isnull(a.Line3,'') + ' ' + isnull(a.Town, '') + ' ' + isnull(a.County, '') + ' ' + isnull(a.Postcode, '')) AS Address 
		FROM Customer c INNER JOIN CustomerAddress a ON c.Id=a.CustomerId 
		WHERE c.IsTest=0 AND c.CollectionStatus<>0 AND a.addressType=1
	),
	CompanyAddress AS
	(
		SELECT c.Id AS CustomerID, (isnull(a.Organisation,'') + ' ' + isnull(Line1, '') + ' ' + isnull(a.Line2, '') + ' ' + isnull(a.Line3,'') + ' ' + isnull(a.Town, '') + ' ' + isnull(a.County, '') + ' ' + isnull(a.Postcode, '')) AS Address 
		FROM Customer c 
		INNER JOIN Company co ON c.CompanyId = co.Id 
		INNER JOIN CustomerAddress a ON a.CompanyId = co.Id 
		WHERE c.IsTest=0 AND c.CollectionStatus<>0 AND a.addressType IN (3,5)
	)

	SELECT C.Id CustomerId,C.Fullname,CS.Name Status,sum(L.Principal) AS Principal,  sum(L.Balance) AS Balance,
		CASE WHEN LS.LoanSourceName = 'EU' THEN 'EU' ELSE '' END AS EU,
		C.TypeOfBusiness,CP.Description ResidentialStatus, FirstNotRepaidLoan.FirstMissedRepaymentDate, pa.Address AS PersonalAddress, ca.Address AS CompanyAddress
	FROM Customer C INNER JOIN CustomerStatuses CS ON C.CollectionStatus=CS.Id 
	INNER JOIN Loan L ON C.Id=L.CustomerId
	INNER JOIN LoanSource LS ON LS.LoanSourceID=L.LoanSourceID
	INNER JOIN CustomerPropertyStatuses CP ON CP.Id=C.PropertyStatusId
	LEFT JOIN  FirstNotRepaidLoan ON C.Id = FirstNotRepaidLoan.Id
	LEFT JOIN PersonalAddress pa ON C.Id = pa.CustomerID
	LEFT JOIN CompanyAddress ca ON C.Id = ca.CustomerID
	WHERE C.IsTest = 0 
	AND C.CollectionStatus != 0  
	AND L.Principal > 0 
	GROUP BY C.Id,C.Fullname,CS.Name,LS.LoanSourceName,C.TypeOfBusiness,CP.Description,FirstNotRepaidLoan.FirstMissedRepaymentDate,pa.Address,ca.Address
	ORDER BY CS.Name,sum(L.Principal) DESC
END 

GO
