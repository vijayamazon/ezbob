
IF OBJECT_ID('RptSalesReport') IS NULL
	EXECUTE ('CREATE PROCEDURE RptSalesReport AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptSalesReport (@DateStart DATETIME,@DateEnd   DATETIME)
AS
BEGIN
if OBJECT_ID('tempdb..#ApprovedData') is not NULL
BEGIN
                DROP TABLE #ApprovedData
END
if OBJECT_ID('tempdb..#CRMSales') is not NULL
BEGIN
                DROP TABLE #CRMSales
END
------------------ APPROVED DATA -------------------
SELECT DISTINCT
                   R.IdCustomer AS CustomerId,
                   convert(DATE,R.UnderwriterDecisionDate) AS ApprovedDate,
                   R.ManagerApprovedSum,
                   L.LoanAmount,
                   L.[Date] AS LoanDate,
                   L.Id AS LoanId
INTO #ApprovedData
FROM CashRequests R
LEFT JOIN Loan L ON L.RequestCashId = R.Id
WHERE R.IdCustomer NOT IN
                                                                                                  (SELECT C.Id
                                                                                                   FROM Customer C
                                                                                                   WHERE Name LIKE '%ezbob%'
                                                                                                   OR Name LIKE '%liatvanir%'
                                                                                                   OR Name LIKE '%q@q%'
                                                                                                   OR Name LIKE '%1@1%'
                                                                                                   OR C.IsTest=1)
                  AND R.UnderwriterDecision = 'approved'
                  AND R.UnderwriterDecisionDate > @DateStart
                  AND R.UnderwriterDecisionDate < @DateEnd
                
 
-------------------- SALES DATA --------------------
SELECT DISTINCT
                   CR.CustomerId,
                   CR.Timestamp AS CRM_SaleDate,
                   CR.UserName AS CRM_Username,
                   CS.Name AS CRM_Status,
                   CA.Name AS CRM_Action
                 
INTO #CRMSales               
FROM CustomerRelations CR
JOIN Customer C ON C.Id = CR.CustomerId
JOIN CRMStatuses CS ON CS.Id = CR.StatusId
JOIN CRMActions CA ON CA.Id = CR.ActionId
WHERE CR.Timestamp > @DateStart
      AND CR.Timestamp < @DateEnd
      AND CS.Name = 'Sale'
      AND CR.UserName IN ('rosb','clareh','travism','sarahd','sarahb','everline', 'scotth')
--    AND C.BrokerID is NULL
ORDER BY 2
 
-------------------- ISSUED LOANS --------------------
DECLARE @IssuedLoans TABLE
                (
                CustomerId INT
                , LoanID INT
                , LoanDate DATETIME
                , IssueMonth DATETIME
                , LoanAmount NUMERIC (18, 0)
                , InterestRate DECIMAL (18, 4)
                , RepaymentPeriod INT
                , OutstandingPrincipal DECIMAL (18, 2)
                , SetupFee DECIMAL (18, 4)
                , LoanNumber BIGINT
                , CustomerRequestedAmount DECIMAL (18, 0)
                , ReferenceSource NVARCHAR (1000)
                , SourceRefGroup NVARCHAR (255)
                , SourceRefMedium NVARCHAR (255)
                , IsOffline BIT
                , Loan_Type VARCHAR (8)
                , BrokerOrNot VARCHAR (12)
                , Quarter VARCHAR (7)
                , NewOldLoan VARCHAR (3)
                , AlibabaOrNot VARCHAR (10)
                );
 
INSERT INTO @IssuedLoans
EXECUTE RptAllLoansIssued;
 
--------------------- FINAL TABLE --------------------
SELECT DISTINCT
                   A.CustomerId,
                   CU.Fullname,
                   CU.DaytimePhone,
                   CU.MobilePhone,
                   A.ApprovedDate,
                   A.ManagerApprovedSum AS ApprovedAmount,
                   C.CRM_Username,
                   convert(DATE,C.CRM_SaleDate) AS CRM_SaleDate,
      C.CRM_Status,
                   A.LoanId,
                   A.LoanAmount,
                   A.LoanDate,
                   L.NewOldLoan,
                   CASE
                   WHEN CU.BrokerID IS NULL THEN 'NotBroker'
                   ELSE 'BrokerClient'
                   END AS BrokerOrNot
                  
FROM #ApprovedData A
JOIN #CRMSales C ON C.CustomerId = A.CustomerId
JOIN Customer CU ON CU.Id = A.CustomerId
LEFT JOIN @IssuedLoans L ON L.LoanId = A.LoanId
WHERE A.LoanId IS NOT NULL
END

GO