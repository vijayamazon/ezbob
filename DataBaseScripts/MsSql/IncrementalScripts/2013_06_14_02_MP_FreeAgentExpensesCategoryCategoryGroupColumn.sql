
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'category_group' and Object_ID = Object_ID(N'MP_FreeAgentExpenseCategory'))
BEGIN
	ALTER TABLE MP_FreeAgentExpenseCategory ADD category_group NVARCHAR(250)
END
GO
