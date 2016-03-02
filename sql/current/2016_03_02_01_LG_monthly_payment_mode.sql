SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'LogicalGlueMonthlyPaymentMode')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueMonthlyPaymentMode',
		'NoOpenNoInterest',
		'Enum (closed list of strings). Available values are: NoOpenNoInterest, WithOpenNoInterest, NoOpenWithInterest, WithOpenWithInterest. ' +
		'"WithOpen"/"NoOpen" means add/do not add currently open loans to monthly payment sent to LG. ' +
		'"WitnInterest"/"NoInterest" means add/do not add calculated interest of loan first payment. Affects both requested loan amount and open loans.',
		0
	)
END
GO
