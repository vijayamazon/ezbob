SET QUOTED_IDENTIFIER ON
GO

SELECT
	ID = CONVERT(INT, NULL),
	Origin = CashRequestOrigin
INTO
	#n
FROM
	NL_CashRequestOrigins
WHERE
	1 = 0
GO

INSERT INTO #n VALUES
	(7, 'ForcedWizardCompletion'),
	(8, 'Approved'),
	(9, 'Manual'),
	(10, 'NewCreditLineSkipAll'),
	(11, 'NewCreditLineSkipAndGoAuto'),
	(12, 'NewCreditLineUpdateAndGoManual'),
	(13, 'NewCreditLineUpdateAndGoAuto')
GO

SET IDENTITY_INSERT NL_CashRequestOrigins ON
GO

INSERT INTO NL_CashRequestOrigins(CashRequestOriginID, CashRequestOrigin)
SELECT
	ID,
	Origin
FROM
	#n
WHERE 
	ID NOT IN (SELECT CashRequestOriginID FROM NL_CashRequestOrigins)
GO

SET IDENTITY_INSERT NL_CashRequestOrigins OFF
GO

DROP TABLE #n
GO
