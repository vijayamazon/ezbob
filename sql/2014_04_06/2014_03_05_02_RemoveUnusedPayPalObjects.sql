IF OBJECT_ID (N'dbo.GetExpensesPayPalTransactionsByPayerAndRange') IS NOT NULL
	DROP FUNCTION dbo.GetExpensesPayPalTransactionsByPayerAndRange
GO
IF OBJECT_ID (N'dbo.GetExpensesPayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetExpensesPayPalTransactionsByRange
GO
IF OBJECT_ID (N'dbo.GetIncomePayPalTransactionsByPayer') IS NOT NULL
	DROP FUNCTION dbo.GetIncomePayPalTransactionsByPayer
GO
IF OBJECT_ID (N'dbo.GetIncomePayPalTransactionsByPayerAndRange') IS NOT NULL
	DROP FUNCTION dbo.GetIncomePayPalTransactionsByPayerAndRange
GO
IF OBJECT_ID (N'dbo.GetIncomePayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetIncomePayPalTransactionsByRange
GO
IF OBJECT_ID (N'dbo.GetTotalExpensesPayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetTotalExpensesPayPalTransactions
GO
IF OBJECT_ID (N'dbo.GetTotalIncomePayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetTotalIncomePayPalTransactions
GO
IF OBJECT_ID (N'dbo.GetTotalTransactionsPayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetTotalTransactionsPayPalTransactions
GO
IF OBJECT_ID (N'dbo.GetTransactionsCountPayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetTransactionsCountPayPalTransactionsByRange
GO
IF OBJECT_ID (N'dbo.GetBiggestExpensesPayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetBiggestExpensesPayPalTransactions
GO
IF OBJECT_ID (N'dbo.GetBiggestIncomePayPalTransactions') IS NOT NULL
	DROP FUNCTION dbo.GetBiggestIncomePayPalTransactions
GO
IF OBJECT_ID (N'dbo.GetExpensesPayPalTransactionsByPayer') IS NOT NULL
	DROP FUNCTION dbo.GetExpensesPayPalTransactionsByPayer
GO
