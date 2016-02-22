IF OBJECT_ID('UwGetCounters') IS NULL
	EXECUTE('CREATE PROCEDURE UwGetCounters AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGetCounters
@isTest BIT
AS
BEGIN
	WITH wiz AS (
		SELECT
			WizardStepTypeID,
			TheLastOne
		FROM
			WizardStepTypes
		WHERE
			TheLastOne = 1
	)
	SELECT
		COUNT(DISTINCT c.Id) AS CustomerCount,
		CASE
			WHEN c.IsWaitingForSignature = 1 THEN 'Signature'
			WHEN c.CreditResult IS NULL THEN 'Registered'
			ELSE c.CreditResult
		END AS CustomerType
	FROM
		Customer c
		LEFT JOIN wiz ON c.WizardStep = wiz.WizardStepTypeID
	WHERE
		(@isTest = 1 OR c.IsTest = 0)
		AND
		(
			(c.CreditResult IS NULL AND wiz.TheLastOne = 1)
			OR
			c.CreditResult IN ('Escalated', 'WaitingForDecision', 'ApprovedPending', 'PendingInvestor')
		)
	GROUP BY
		CASE
			WHEN c.IsWaitingForSignature = 1 THEN 'Signature'
			WHEN c.CreditResult IS NULL THEN 'Registered'
			ELSE c.CreditResult
		END
END
GO
