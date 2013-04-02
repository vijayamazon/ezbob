ALTER TABLE dbo.Customer ADD
	BankAccountType nvarchar(50) NULL
GO

UPDATE [dbo].[Customer]
   SET [BankAccountType] = 'Unknown'
 WHERE [BankAccountType] is null
GO