UPDATE dbo.ConfigurationVariables
SET Value = 'https://app.sageone.com/api/v1/incomes'
WHERE Name='SageIncomesRequest'
GO
