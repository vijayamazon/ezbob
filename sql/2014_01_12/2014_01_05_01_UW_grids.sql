IF OBJECT_ID('UwGridOffline') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridOffline AS SELECT 1')
GO

ALTER PROCEDURE UwGridOffline
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	c.GreetingMailSentDate AS RegDate,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.IsOffline = 1
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridAll') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridAll AS SELECT 1')
GO

ALTER PROCEDURE UwGridAll
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	c.SystemCalculatedSum AS CalcAmount,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	c.ManagerApprovedSum AS ApprovedSum,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult IS NOT NULL
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridRegistered') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridRegistered AS SELECT 1')
GO

ALTER PROCEDURE UwGridRegistered
@WithTest BIT,
@WithAll BIT
AS
SELECT
	c.Id AS CustomerID,
	c.Name AS Email,
	(CASE w.TheLastOne WHEN 1 THEN 'registered' ELSE 'credit calculation' END) AS UserStatus,
	c.GreetingMailSentDate AS RegDate,
	ISNULL(t.Name, '') AS MpTypeName,
	(CASE
		WHEN m.UpdatingStart IS NULL THEN 'Never started'
		WHEN m.UpdateError IS NOT NULL AND LTRIM(RTRIM(m.UpdateError)) != '' THEN 'Error'
		WHEN m.UpdatingStart IS NOT NULL AND m.UpdatingEnd IS NULL THEN 'In progress'
		ELSE 'Done'
	END) AS MpStatus,	
	w.WizardStepTypeDescription AS WizardStep,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult IS NOT NULL
	AND
	(
		@WithAll = 1 OR (@WithAll != 1 AND c.GreetingMailSentDate >= DATEADD(day, -7, GETDATE()))
	)
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridRejected') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridRejected AS SELECT 1')
GO

ALTER PROCEDURE UwGridRejected
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	c.DateRejected,
	c.RejectedReason,
	c.NumApproves,
	c.NumRejects
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult = 'Rejected'
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridCollection') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridCollection AS SELECT 1')
GO

ALTER PROCEDURE UwGridCollection
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	c.MobilePhone,
	c.DaytimePhone,
	c.AmountTaken,
	(
		SELECT TOP 1 ST.NAME
		FROM [CustomerRelations] AS CR LEFT JOIN [CRMStatuses] AS ST ON CR.StatusId = ST.Id
		WHERE CR.CustomerId = c.Id
		ORDER BY CR.Timestamp DESC
	) AS CRMstatus,
	(SELECT TOP 1 CR.Comment FROM [CustomerRelations] AS CR WHERE CR.CustomerId=c.Id ORDER BY CR.Timestamp DESC) AS CRMcomment,
	cs.Name AS CollectionStatus
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	INNER JOIN CustomerStatuses cs on c.CollectionStatus = cs.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	cs.Name IN ('Legal', 'Default')
ORDER BY
	c.Id DESC
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridSales') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridSales AS SELECT 1')
GO

ALTER PROCEDURE UwGridSales
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	c.MobilePhone,
	c.DaytimePhone,
	c.AmountTaken,
	(
		SELECT TOP 1 ST.NAME
		FROM [CustomerRelations] AS CR LEFT JOIN [CRMStatuses] AS ST ON CR.StatusId = ST.Id
		WHERE CR.CustomerId = c.Id
		ORDER BY CR.Timestamp DESC
	) AS CRMstatus,
	(SELECT TOP 1 CR.Comment FROM [CustomerRelations] AS CR WHERE CR.CustomerId=c.Id ORDER BY CR.Timestamp DESC) AS CRMcomment,
	c.ManagerApprovedSum AS ApprovedSum,
	(SELECT MAX(l.[UnderwriterDecisionDate]) FROM [CashRequests] l WHERE l.IdCustomer = c.Id) AS OfferDate,
	(SELECT COUNT(*) FROM [CashRequests] cr WHERE (GETUTCDATE() - CR.[CreationDate]) < 5 AND CR.IdCustomer = c.Id) AS Interactions
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.ManagerApprovedSum > c.AmountTaken
ORDER BY
	c.Id DESC
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridLoans') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridLoans AS SELECT 1')
GO

ALTER PROCEDURE UwGridLoans
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	c.FirstLoanDate,
	c.LastLoanDate,
	c.LastLoanAmount,
	c.AmountTaken,
	c.TotalPrincipalRepaid,	
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	(
		SELECT TOP 1 s.[Date]
		FROM [LoanSchedule] s LEFT JOIN [loan] l ON l.[Id] = s.[LoanId]
		WHERE l.[CustomerId] = c.Id AND s.[Status] IN ('StillToPay','Late')
		ORDER BY s.[Date]
	) AS NextRepaymentDate,
	c.Status
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult IS NOT NULL
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridApproved') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridApproved AS SELECT 1')
GO

ALTER PROCEDURE UwGridApproved
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	c.SystemCalculatedSum AS CalcAmount,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	c.DateApproved AS ApproveDate,
	c.ManagerApprovedSum AS ApprovedSum,
	c.AmountTaken,
	c.NumApproves AS ApprovesNum,
	c.NumRejects AS RejectsNum,
	c.ValidFor AS OfferExpireDate
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult = 'Approved'
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridLate') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridLate AS SELECT 1')
GO

ALTER PROCEDURE UwGridLate
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	c.SystemCalculatedSum AS CalcAmount,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	c.DateApproved AS ApproveDate,
	c.ManagerApprovedSum AS ApprovedSum,
	c.AmountTaken,
	c.NumApproves AS ApprovesNum,
	c.NumRejects AS RejectsNum,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	(
		SELECT MIN(s.[Date])
		FROM [Loan] l
		LEFT JOIN [LoanSchedule] s ON l.Id = s.[LoanId]
		WHERE l.[CustomerId] = c.Id
		AND s.[Date] <= GETUTCDATE()
		AND s.[Status] = 'Late'
	) AS LatePaymentDate,
	(
		SELECT ISNULL(SUM(s.[LoanRepayment]),0)
		FROM [LoanSchedule] s
		LEFT JOIN [Loan] l ON l.[Id] = s.[LoanId]
		WHERE s.[Status] LIKE 'Late'
		AND l.[CustomerId] = c.Id
	) AS LatePaymentAmount,
	(
		SELECT DATEDIFF(day, ISNULL(MIN(s.[Date]), GETUTCDATE()), GETUTCDATE())
		FROM [Loan] l
		LEFT JOIN [LoanSchedule] s ON l.Id = s.LoanId
		WHERE l.[CustomerId] = c.Id
		AND s.[Date] <= GETUTCDATE()
		AND s.[Status] = 'Late'
	) AS Delinquency,
	(
		SELECT TOP 1 ST.NAME
		FROM [CustomerRelations] AS CR LEFT JOIN [CRMStatuses] AS ST ON CR.StatusId = ST.Id
		WHERE CR.CustomerId = c.Id
		ORDER BY CR.Timestamp DESC
	) AS CRMstatus,
	(SELECT TOP 1 CR.Comment FROM [CustomerRelations] AS CR WHERE CR.CustomerId = c.Id ORDER BY CR.Timestamp DESC) AS CRMcomment
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult = 'Late'
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridWaiting') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridWaiting AS SELECT 1')
GO

ALTER PROCEDURE UwGridWaiting
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	c.SystemCalculatedSum AS CalcAmount,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	c.LastStatus AS CurrentStatus
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult = 'WaitingForDecision'
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridEscalated') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridEscalated AS SELECT 1')
GO

ALTER PROCEDURE UwGridEscalated
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	c.SystemCalculatedSum AS CalcAmount,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	c.LastStatus AS CurrentStatus,
	c.DateEscalated AS EscalationDate,
	c.UnderwriterName AS Underwriter,
	c.EscalationReason AS Reason
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult = 'Escalated'
ORDER BY
	c.Id DESC,
	t.Id
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('UwGridPending') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridPending AS SELECT 1')
GO

ALTER PROCEDURE UwGridPending
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS OfferStart,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	c.SystemCalculatedSum AS CalcAmount,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	c.LastStatus AS CurrentStatus,
	c.PendingStatus AS Pending
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR (@WithTest != 1 AND c.IsTest = 0)
	)
	AND
	c.CreditResult = 'ApprovedPending'
ORDER BY
	c.Id DESC,
	t.Id
GO
