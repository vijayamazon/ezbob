IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_NotClose]'))
DROP VIEW [dbo].[vw_NotClose]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create VIEW [dbo].[vw_NotClose]
AS
SELECT l.Id AS loanID
, l.CustomerId
, l.Date AS StartDate
, CASE WHEN cs.Name != 'Default' OR l.Status = 'PaidOff' THEN ISNULL(l.DateClosed, 0)
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
LEFT OUTER JOIN Company AS co ON co.CustomerId = c.Id
WHERE c.IsTest <> 1 and 
 (
 ((c.TypeOfBusiness = 'PShip3P' OR c.TypeOfBusiness = 'SoleTrader') AND ca.addressType = 5) OR 
 ((c.TypeOfBusiness = 'Limited' OR c.TypeOfBusiness = 'PShip' OR c.TypeOfBusiness = 'LLP')and  (ca.addressType = 3)) OR 
  (c.TypeOfBusiness = 'Entrepreneur' and  (ca.addressType = 1))
 )
 GROUP BY l.Id, l.CustomerId, l.[Date], l.DateClosed, l.MaxDelinquencyDays, l.RepaymentsNum, l.Balance, c.Gender, c.FirstName, c.MiddleInitial, c.Surname, c.RefNumber, ca.Line1, ca.Line2, ca.Line3, ca.Town, ca.County
 , ca.Postcode, c.DateOfBirth, ld.lsdate, LoanAmount.am, LoanAmount.ScheduledRepayments, c.TypeOfBusiness, co.ExperianRefNum, c.CreditResult
 , c.SortCode, l.IsDefaulted, lo.CaisAccountStatus, cs.IsEnabled, c.CollectionStatus, c.MaritalStatus, lo.ManualCaisFlag, cs.Name, l.Status
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCaisData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCaisData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCaisData]
AS
BEGIN
	SELECT 
		loanID,	
		StartDate,	
		DateClose,		
		CurrentBalance,	
		Gender,	
		FirstName,	
		MiddleInitial,	
		Surname,	
		RefNumber,	
		Line1,	
		Line2,	
		Line3,	
		Town,	
		County,	
		Postcode,	
		DateOfBirth,	
		MinLSDate,
		LoanAmount,
		ScheduledRepayments,
		CompanyType,
		ExperianRefNum,
		CustomerState,
		SortCode,
		CONVERT(BIT, IsDefaulted) AS IsDefaulted,
		CaisAccountStatus,
		MaritalStatus,
		CustomerId,
		ManualCaisFlag
	FROM 
		vw_NotClose
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MainStrategyGetPersonalInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MainStrategyGetPersonalInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MainStrategyGetPersonalInfo] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE
		@CustomerStatusIsEnabled BIT,
		@CustomerStatusIsWarning BIT,
		@NumOfMps INT,
		@NumOfLoans INT,
		@prevExperianConsumerScore INT
		
	SELECT 
		@prevExperianConsumerScore = MIN(ExperianScore) 
	FROM 
		MP_ExperianDataCache 
	WHERE 
		CustomerId = @CustomerId AND 
		Name IS NOT NULL
	
	SELECT 
		@CustomerStatusIsEnabled = IsEnabled, 
		@CustomerStatusIsWarning = IsWarning
	FROM
		CustomerStatuses,
		Customer
	WHERE
		Customer.CollectionStatus = CustomerStatuses.Id AND
		Customer.Id = @CustomerId
	
	SELECT 
		@NumOfMps = COUNT(cmp.Id)
	FROM 
		MP_CustomerMarketPlace cmp
	WHERE 
		CustomerId = @CustomerId
	
	SELECT
		@NumOfLoans = COUNT(1)
	FROM
		Loan
	WHERE
		CustomerId = @CustomerId
	
	SELECT
		@CustomerStatusIsEnabled AS CustomerStatusIsEnabled,
		@CustomerStatusIsWarning AS CustomerStatusIsWarning,
		@prevExperianConsumerScore AS PrevExperianConsumerScore,
		IsOffline,
		Name AS CustomerEmail,
		Company.TypeOfBusiness AS CompanyType,		
		ExperianRefNum,
		CASE WHEN LastStartedMainStrategyEndTime IS NULL THEN 0 ELSE 1 END AS MainStrategyExecutedBefore,
		FirstName, 
		Surname,
		Gender,
		DateOfBirth,
		ResidentialStatus AS HomeOwner,
		@NumOfMps AS NumOfMps,
		Customer.TimeAtAddress,
		AccountNumber,
		SortCode,
		GreetingMailSentDate AS RegistrationDate,
		BankAccountType,
		@NumOfLoans AS NumOfLoans
	FROM
		Customer
		LEFT OUTER JOIN Company ON CustomerId = Customer.Id
	WHERE
		Customer.Id = @CustomerId
		
END
GO

