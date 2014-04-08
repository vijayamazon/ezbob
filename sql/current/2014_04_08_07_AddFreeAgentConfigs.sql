IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentOAuthIdentifier')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentOAuthIdentifier', '1BQ5Np7Wjmiq0YEl3Gn-tw', 'FreeAgent OAuth identifier')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentOAuthSecret')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentOAuthSecret', 'k84-Inqfs408L8JoHIvfYw', 'FreeAgent OAuth secret')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentOAuthAuthorizationEndpoint')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentOAuthAuthorizationEndpoint', 'https://api.freeagent.com/v2/approve_app', 'FreeAgent OAuth authorize endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentOAuthTokenEndpoint')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentOAuthTokenEndpoint', 'https://api.freeagent.com/v2/token_endpoint', 'FreeAgent OAuth token endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentInvoicesRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentInvoicesRequest', 'https://api.freeagent.com/v2/invoices?nested_invoice_items=true', 'FreeAgent invoices request')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentInvoicesRequestMonthPart')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentInvoicesRequestMonthPart', '&view=last_{0}_months', 'FreeAgent invoices request month part')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentCompanyRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentCompanyRequest', 'https://api.freeagent.com/v2/company', 'FreeAgent company request')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentUsersRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentUsersRequest', 'https://api.freeagent.com/v2/users', 'FreeAgent users request')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentExpensesRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentExpensesRequest', 'https://api.freeagent.com/v2/expenses', 'FreeAgent expenses request')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FreeAgentExpensesRequestDatePart')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FreeAgentExpensesRequestDatePart', '?from_date={0}-{1}-{2}', 'FreeAgent expenses request date part')
END
GO
