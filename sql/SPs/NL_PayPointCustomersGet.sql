IF OBJECT_ID('NL_PayPointCustomersGet') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_PayPointCustomersGet AS SELECT 1')
GO


ALTER PROCEDURE NL_PayPointCustomersGet AS BEGIN DECLARE @Now DATETIME
SET @Now = GETUTCDATE()
SELECT ls.LoanScheduleID AS LoanScheduleId,
       l.LoanId,
       c.FirstName,
       c.Fullname,
       c.Id AS CustomerId,
       c.Name AS Email,
       c.TypeOfBusiness,
       ls.PlannedDate AS DueDate,
       ISNULL(lo.ReductionFee, 1) AS ReductionFee,
       l.RefNum,
       CAST(CASE WHEN
              (SELECT MAX(ls1.PlannedDate)
               FROM NL_LoanSchedules ls1
               WHERE ls.LoanHistoryID = ls1.LoanHistoryID) = ls.PlannedDate THEN 1 ELSE 0 END AS BIT) AS LastInstallment INTO #GetCustomersForPayPoint
FROM Customer c
JOIN vw_NL_LoansCustomer v ON v.CustomerID = c.Id
JOIN NL_Loans l ON v.LoanID = v.LoanID
JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus 
	AND cs.IsEnabled = 1
LEFT JOIN LoanOptions lo ON lo.LoanId = l.LoanID
JOIN(SELECT LoanID, MAX(LoanHistoryID) AS maxLoanHistoryID 
			   FROM NL_LoanHistory 
			   GROUP BY LoanID) lhgb 
			   on lhgb.LoanID = l.LoanID
JOIN  NL_LoanSchedules ls ON ls.LoanHistoryID = lhgb.maxLoanHistoryID
JOIN NL_LoanScheduleStatuses lss on lss.LoanScheduleStatusID = ls.LoanScheduleStatusID
AND (lss.LoanScheduleStatus = 'StillToPay'
     OR lss.LoanScheduleStatus = 'Late')
AND ls.PlannedDate <= @Now
WHERE (lo.AutoPayment IS NULL
       OR lo.AutoPayment = 1
       OR (lo.AutoPayment = 0
           AND lo.StopAutoChargeDate IS NOT NULL
           AND @Now < lo.StopAutoChargeDate))
  AND ls.PlannedDate = (SELECT min(l1.PlannedDate) FROM NL_LoanSchedules l1
						JOIN NL_LoanScheduleStatuses lss1 on lss1.LoanScheduleStatusID = l1.LoanScheduleStatusID 
						WHERE l1.LoanHistoryID =ls.LoanHistoryID AND (lss1.LoanScheduleStatus = 'StillToPay' OR lss1.LoanScheduleStatus = 'Late'))
  AND c.ExternalCollectionStatusID IS NULL
  AND DATEDIFF(DAY, ls.PlannedDate, @Now)<=30
  SELECT LoanScheduleId,
         LoanId,
         FirstName,
         Fullname,
         CustomerId,
         Email,
         TypeOfBusiness,
         DueDate,
         ReductionFee,
         RefNum,
         LastInstallment
  FROM #GetCustomersForPayPoint WHERE LoanScheduleId NOT IN
    (SELECT LoanScheduleId
     FROM PaymentRollover
     WHERE ExpiryDate > @Now)
ORDER BY LoanScheduleId DESC END