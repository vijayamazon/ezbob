UPDATE dbo.ConfigurationVariables
SET Value = 'oauth/authorize'
WHERE Name = 'SageOAuthAuthorizationEndpoint'
GO

UPDATE dbo.ConfigurationVariables
SET Value = 'oauth/token'
WHERE Name = 'SageOAuthTokenEndpoint'
GO

UPDATE dbo.ConfigurationVariables
SET Value = 'api/v1/sales_invoices'
WHERE Name = 'SageSalesInvoicesRequest'
GO

UPDATE dbo.ConfigurationVariables
SET  Value = 'api/v1/purchase_invoices'
WHERE Name = 'SagePurchaseInvoicesRequest'
GO

UPDATE dbo.ConfigurationVariables
SET  Value = 'api/v1/incomes'
WHERE Name = 'SageIncomesRequest'
GO

UPDATE dbo.ConfigurationVariables
SET Value = 'api/v1/expenditures'
WHERE Name = 'SageExpendituresRequest'
GO

UPDATE dbo.ConfigurationVariables
SET Value = 'api/v1/payment_statuses'
WHERE Name = 'SagePaymentStatusesRequest'
GO