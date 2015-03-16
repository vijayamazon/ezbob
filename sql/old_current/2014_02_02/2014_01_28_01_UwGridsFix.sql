ALTER PROCEDURE UwGridApproved
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult = 'Approved'
ORDER BY
	c.Id DESC,
	t.Id

GO

ALTER PROCEDURE UwGridRejected
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CustomerStatus,
	ISNULL(c.Fullname, '') AS FullName,
	c.Name AS Email,
	w.WizardStepTypeDescription AS WizardStep,
	(CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END) AS SegmentType,
	(CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END) AS IsWasLate,
	(SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id) AS OSBalance,
	c.DateRejected,
	c.RejectedReason AS Reason,
	c.NumApproves AS ApprovesNum,
	c.NumRejects AS RejectsNum
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
	LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
WHERE
	(
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult = 'Rejected'
ORDER BY
	c.Id DESC,
	t.Id

GO

ALTER PROCEDURE UwGridLoans
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult IS NOT NULL
ORDER BY
	c.Id DESC,
	t.Id

GO


ALTER PROCEDURE UwGridLate
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult = 'Late'
ORDER BY
	c.Id DESC,
	t.Id

GO

ALTER PROCEDURE UwGridAll
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult IS NOT NULL
ORDER BY
	c.Id DESC,
	t.Id

GO

ALTER PROCEDURE UwGridEscalated
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
	c.GreetingMailSentDate AS RegDate,
	c.Status AS CurrentStatus,
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult = 'Escalated'
ORDER BY
	c.Id DESC,
	t.Id

GO

ALTER PROCEDURE UwGridWaiting
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult = 'WaitingForDecision'
ORDER BY
	c.Id DESC,
	t.Id


GO

ALTER PROCEDURE UwGridPending
@WithTest BIT
AS
SELECT
	c.Id AS CustomerID,
	ISNULL(c.MedalType, '') AS Medal,
	ISNULL(t.Name, '') AS MpTypeName,
	c.ApplyForLoan AS ApplyDate,
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	c.CreditResult = 'ApprovedPending'
ORDER BY
	c.Id DESC,
	t.Id

GO







