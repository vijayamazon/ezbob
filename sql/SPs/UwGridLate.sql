IF OBJECT_ID('UwGridLate') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridLate AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridLate
@WithTest BIT,
@Now DATETIME
AS
BEGIN
	SELECT
		c.Id AS CustomerID,
		ISNULL(c.MedalType, '') AS Medal,
		ISNULL(g.Name, '') AS Grade,
		dbo.udfGetMpsTypes(c.Id) AS MpTypeName,
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
			AND s.[Date] <= @Now
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
			SELECT DATEDIFF(day, ISNULL(MIN(s.[Date]), @Now), @Now)
			FROM [Loan] l
			LEFT JOIN [LoanSchedule] s ON l.Id = s.LoanId
			WHERE l.[CustomerId] = c.Id
			AND s.[Date] <= @Now
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
		LEFT JOIN CustomerLogicalGlueHistory lg ON c.Id = lg.CustomerID AND c.CompanyId = lg.CompanyID AND lg.IsActive = 1
		LEFT JOIN I_Grade g ON lg.GradeID = g.GradeID
	WHERE
		(@WithTest = 1 OR c.IsTest = 0)
		AND
		c.CreditResult = 'Late'
	ORDER BY
		c.Id DESC
END
GO
