SET QUOTED_IDENTIFIER ON
GO

UPDATE ConfigurationVariables SET
	Value = '12'
WHERE
	Name = 'LatePaymentCharge'
	AND
	Value = '20'
GO
