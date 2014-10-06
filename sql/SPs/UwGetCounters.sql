IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UwGetCounters]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UwGetCounters]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UwGetCounters] 
	(@isTest BIT)
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
			c.CreditResult IN ('Escalated', 'WaitingForDecision', 'ApprovedPending')
		)
		AND
		ISNULL(c.IsWaitingForSignature, 0) = 0
	GROUP BY
		CASE
			WHEN c.CreditResult IS NULL THEN 'Registered'
			ELSE c.CreditResult
		END
END
GO
