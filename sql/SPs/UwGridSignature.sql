IF OBJECT_ID('UwGridSignature') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridSignature AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridSignature
@WithTest BIT
AS
BEGIN
	SET NOCOUNT ON;

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
		( 
			CASE WHEN c.ManagerApprovedSum > 0 THEN c.ManagerApprovedSum ELSE c.SystemCalculatedSum END
		) AS CalcAmount,
		(
			CASE c.IsOffline WHEN 1 THEN 'Offline' WHEN 0 THEN 'Online' ELSE '' END
		) AS SegmentType,
		(
			CASE WHEN c.CreditResult = 'Late' OR c.IsWasLate = 1 THEN 'iswaslate' ELSE '' END
		) AS IsWasLate,
		(
			SELECT ISNULL(SUM(l.Balance), 0) FROM Loan l WHERE l.CustomerId = c.Id
		) AS OSBalance,
		c.LastStatus AS CurrentStatus,
		c.PendingStatus AS Pending,
		(
			SELECT TOP
				1 ST.NAME
			FROM
				[CustomerRelations] AS CR
				LEFT JOIN [CRMStatuses] AS ST ON CR.StatusId = ST.Id
			WHERE
				CR.CustomerId = c.Id
			ORDER BY
				CR.Timestamp DESC
		) AS LastStatus,
		(
			SELECT TOP 1
				CR.Comment
			FROM
				[CustomerRelations] AS CR
			WHERE
				CR.CustomerId=c.Id
			ORDER BY
				CR.Timestamp DESC
		) AS CRMcomment,
		ISNULL(b.ContactName, '') AS Broker,
		(CASE
			WHEN c.BrokerID IS NOT NULL THEN (
				CASE
					WHEN EXISTS(
						SELECT *
						FROM Customer CU
						INNER JOIN Loan L ON CU.Id = L.CustomerId
						WHERE CU.BrokerID = c.BrokerID
					) THEN ''
					ELSE 'FirstSale'
				END
			)
			ELSE ''
		END) AS FirstSale	
	FROM
		Customer c
			INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
			LEFT JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId
			LEFT JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
			LEFT JOIN Broker b ON b.BrokerID = c.BrokerID
	WHERE
		(
			@WithTest = 1 OR c.IsTest = 0
		)
		AND
		ISNULL(c.IsWaitingForSignature, 0) = 1
	ORDER BY
		c.Id DESC,
		t.Id
END

GO

