IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UwGridCollection]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UwGridCollection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UwGridCollection] 
	(@WithTest BIT)
AS
BEGIN
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
		@WithTest = 1 OR c.IsTest = 0
	)
	AND
	cs.Name IN ('Legal', 'Default')
ORDER BY
	c.Id DESC
END
GO
