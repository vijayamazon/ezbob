IF OBJECT_ID (N'dbo.vw_NotClose') IS NOT NULL
	DROP VIEW dbo.vw_NotClose
GO

CREATE VIEW [dbo].[vw_NotClose]
AS
SELECT l.Id AS loanID
, l.CustomerId
, l.Date AS StartDate
, CASE WHEN cs.IsDefault = 0 OR l.Status = 'PaidOff' THEN ISNULL(l.DateClosed, 0)
	ELSE max(csh.Timestamp) END AS DateClose
, ISNULL(l.MaxDelinquencyDays, 0) as MaxDelinquencyDays
, l.RepaymentsNum AS RepaymentPeriod
, l.Balance AS CurrentBalance
, c.Gender
, c.FirstName
, c.MiddleInitial
, c.Surname
, c.RefNumber
, ca.Line1
, ca.Line2
, ca.Line3
, ca.Town
, ca.County
, ca.Postcode
, c.DateOfBirth
, ld.lsdate as  MinLSDate
, LoanAmount.am AS LoanAmount
, LoanAmount.ScheduledRepayments AS ScheduledRepayments
, c.TypeOfBusiness as CompanyType
, co.ExperianRefNum
, co.ExperianCompanyName
, c.CreditResult as CustomerState
, c.SortCode
, CONVERT(BIT, l.IsDefaulted) AS IsDefaulted
, lo.CaisAccountStatus
, cs.IsEnabled AS CustomerStatusIsEnabled
, c.MaritalStatus
, lo.ManualCaisFlag
FROM         
(
  SELECT 
	  SUM(l.LoanAmount) AS am	  
     , l.Id
     , COUNT(ls.Id) as ScheduledRepayments
 FROM dbo.LoanSchedule AS ls 
  LEFT OUTER JOIN dbo.Loan AS l ON l.Id = ls.LoanId
    GROUP BY l.Id
 ) AS LoanAmount

 LEFT OUTER JOIN dbo.Loan AS l ON l.Id = LoanAmount.Id 
 LEFT OUTER JOIN MinLoanSchedule as ld ON ld.Id = LoanAmount.Id 
 LEFT OUTER JOIN dbo.Customer AS c ON c.Id = l.CustomerId 
LEFT OUTER JOIN dbo.CustomerAddress AS ca ON ca.customerId = c.Id
LEFT OUTER JOIN dbo.LoanOptions AS lo ON lo.LoanId = l.Id
LEFT JOIN CustomerStatuses AS cs ON cs.Id = c.CollectionStatus
LEFT OUTER JOIN dbo.LoanTransaction AS lt ON lt.LoanId = l.Id AND lt.Status='Done' AND lt.Type = 'PaypointTransaction'
LEFT OUTER JOIN CustomerStatusHistory AS csh ON csh.CustomerId = c.Id
LEFT OUTER JOIN Company AS co ON c.CompanyId = co.Id
WHERE c.IsTest <> 1 and 
 (
 ((c.TypeOfBusiness = 'PShip3P' OR c.TypeOfBusiness = 'SoleTrader' OR c.TypeOfBusiness = 'PShip') AND ca.addressType = 5) OR 
 ((c.TypeOfBusiness = 'Limited' OR c.TypeOfBusiness = 'LLP')and  (ca.addressType = 3)) OR 
  (c.TypeOfBusiness = 'Entrepreneur' and  (ca.addressType = 1))
 )
 GROUP BY l.Id, l.CustomerId, l.[Date], l.DateClosed, l.MaxDelinquencyDays, l.RepaymentsNum, l.Balance, l.IsDefaulted,l.Status,
  c.DateOfBirth, c.Gender, c.FirstName, c.MiddleInitial, c.Surname, c.RefNumber,c.CreditResult,c.SortCode,c.TypeOfBusiness, c.CollectionStatus, c.MaritalStatus,
  ca.Line1, ca.Line2, ca.Line3, ca.Town, ca.County, ca.Postcode,
  ld.lsdate, LoanAmount.am, LoanAmount.ScheduledRepayments, lo.CaisAccountStatus, lo.ManualCaisFlag, 
  co.ExperianRefNum, co.ExperianCompanyName,
  cs.IsDefault, cs.IsEnabled

GO

