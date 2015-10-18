SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_CashRequestOrigins') IS NOT NULL
BEGIN
	SELECT
		ID = CONVERT(INT, NULL),
		Origin = CashRequestOrigin
	INTO
		#n
	FROM
		NL_CashRequestOrigins
	WHERE
		1 = 0

	INSERT INTO #n VALUES
		(7, 'ForcedWizardCompletion'),
		(8, 'Approved'),
		(9, 'Manual'),
		(10, 'NewCreditLineSkipAll'),
		(11, 'NewCreditLineSkipAndGoAuto'),
		(12, 'NewCreditLineUpdateAndGoManual'),
		(13, 'NewCreditLineUpdateAndGoAuto')

	SET IDENTITY_INSERT NL_CashRequestOrigins ON

	INSERT INTO NL_CashRequestOrigins(CashRequestOriginID, CashRequestOrigin)
	SELECT
		ID,
		Origin
	FROM
		#n
	WHERE 
		ID NOT IN (SELECT CashRequestOriginID FROM NL_CashRequestOrigins)

	SET IDENTITY_INSERT NL_CashRequestOrigins OFF

	DROP TABLE #n
END
GO
