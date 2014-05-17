DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthIdentifier')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthIdentifier', 'IssYGGBRMb9f6cw7XJgpVaeiMbCNGcFAeyBysLvJ', 'Sage OAuth identifier')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthSecret', 'lUuYVDVgr2LkN5whEVezncV3TwyXrYqXjgDaT49H', 'Sage OAuth secret')
	END
END

IF @Environment = 'QA'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthIdentifier')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthIdentifier', 'fERMIRu9IrIXt54bAFhxfNM8HAoCRCp6V7TDAifH', 'Sage OAuth identifier')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthSecret', 'DfKDelrywwHtdzloMiVS8XIhM3vmctXoENqsFBtV', 'Sage OAuth secret')
	END
END

IF @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthIdentifier')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthIdentifier', 'TqQ2oSwvoHv6vWhttbD8OJqxRsk9eBziE0v0yYl9', 'Sage OAuth identifier')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthSecret', 'ejjPWG0dK0EPq4mStM8dq9M5npkJ6A6KTbt3GlFK', 'Sage OAuth secret')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthIdentifier')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthIdentifier', 'wxke93qG1Y7qGlUbTWSGtUgwpNizRaqRz6Vl9ueu', 'Sage OAuth identifier')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthSecret', 'C2iVv3vOtrwpLoWmfW33nluqkiCVXUNfgYLNmuOL', 'Sage OAuth secret')
	END
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthAuthorizationEndpoint')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthAuthorizationEndpoint', 'https://app.sageone.com/oauth/authorize', 'Sage OAuth authorize endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageOAuthTokenEndpoint')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageOAuthTokenEndpoint', 'https://app.sageone.com/oauth/token', 'Sage OAuth token endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageSalesInvoicesRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageSalesInvoicesRequest', 'https://app.sageone.com/api/v1/sales_invoices', 'Sage sales invoices request endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SagePurchaseInvoicesRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SagePurchaseInvoicesRequest', 'https://app.sageone.com/api/v1/purchase_invoices', 'Sage purchase invoices request endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageIncomesRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageIncomesRequest', 'https://app.sageone.com/api/v1/SageIncomesRequest', 'Sage incomes request endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageExpendituresRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageExpendituresRequest', 'https://app.sageone.com/api/v1/expenditures', 'Sage expenditures request endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SagePaymentStatusesRequest')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SagePaymentStatusesRequest', 'https://app.sageone.com/api/v1/payment_statuses', 'Sage payment statuses request endpoint')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SageRequestForDatesPart')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SageRequestForDatesPart', 'from_date={0}&amp;to_date={1}', 'Sage date in request format')
END
GO
