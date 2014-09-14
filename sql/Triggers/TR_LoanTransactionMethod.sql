IF EXISTS (SELECT 1 FROM sys.objects WHERE [type] = 'TR' AND [name] = 'TR_LoanTransactionMethod')
	DROP TRIGGER TR_LoanTransactionMethod
GO
