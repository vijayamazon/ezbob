SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'FreeAgentApiBase')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'FreeAgentApiBase',
		'https://api.freeagent.com',
		'Base URL of FreeAgent API requests',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'FreeAgentExpensesCategoriesRequest')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'FreeAgentExpensesCategoriesRequest',
		'/v2/categories',
		'Loads list of all the expense categories.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'FreeAgentInvoicesRequestNestedPart')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'FreeAgentInvoicesRequestNestedPart',
		'nested_invoice_items=true',
		'Instructs to include invoice items nested into the list of invoices which increases request size but removes the need to request the invoices separately to see invoice item information.',
		0
	)
END
GO

UPDATE ConfigurationVariables SET
	Value = '/v2/invoices'
WHERE
	Name = 'FreeAgentInvoicesRequest'
	AND
	Value LIKE 'http%'
GO

UPDATE ConfigurationVariables SET
	Value = 'view=last_{0}_months'
WHERE
	Name = 'FreeAgentInvoicesRequestMonthPart'
	AND
	Value LIKE '&%'
GO

UPDATE ConfigurationVariables SET
	Value = '/v2/expenses'
WHERE
	Name = 'FreeAgentExpensesRequest'
	AND
	Value LIKE 'http%'
GO

UPDATE ConfigurationVariables SET
	Value = 'from_date={0}-{1}-{2}'
WHERE
	Name = 'FreeAgentExpensesRequestDatePart'
	AND
	Value LIKE '?%'
GO

UPDATE ConfigurationVariables SET
	Value = '/v2/users'
WHERE
	Name = 'FreeAgentUsersRequest'
	AND
	Value LIKE 'http%'
GO

UPDATE ConfigurationVariables SET
	Value = '/v2/company'
WHERE
	Name = 'FreeAgentCompanyRequest'
	AND
	Value LIKE 'http%'
GO

