IF OBJECT_ID('ScoringModel') IS NOT NULL
BEGIN
	DROP TABLE ScoringModel
END
GO
IF OBJECT_ID('Security_AccountLog') IS NOT NULL
BEGIN
	DROP TABLE Security_AccountLog
END
GO
IF OBJECT_ID('Security_Branch') IS NOT NULL
BEGIN
	DROP TABLE Security_Branch
END
GO
IF OBJECT_ID('Security_log4net') IS NOT NULL
BEGIN
	DROP TABLE Security_log4net
END
GO
IF OBJECT_ID('ServiceRegistration') IS NOT NULL
BEGIN
	DROP TABLE ServiceRegistration
END
GO
IF OBJECT_ID('SingleRunApplication') IS NOT NULL
BEGIN
	DROP TABLE SingleRunApplication
END
GO
IF OBJECT_ID('SV_ReportingInfo') IS NOT NULL
BEGIN
	DROP TABLE SV_ReportingInfo
END
GO
IF OBJECT_ID('SystemCalendar_BaseRelation') IS NOT NULL
BEGIN
	DROP TABLE SystemCalendar_BaseRelation
END
GO
IF OBJECT_ID('SystemCalendar_Calendar') IS NOT NULL
BEGIN
	DECLARE @DropStatement NVARCHAR(MAX)
	DECLARE cur CURSOR FOR 
		SELECT 
			'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(parent_object_id) +
			'.[' + OBJECT_NAME(parent_object_id) + 
			'] DROP CONSTRAINT ' + name AS DropStatement
		FROM sys.foreign_keys
		WHERE referenced_object_id = object_id('SystemCalendar_Calendar')
	OPEN cur
	FETCH NEXT FROM cur INTO @DropStatement
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE(@DropStatement)

		FETCH NEXT FROM cur INTO @DropStatement
	END
	CLOSE cur
	DEALLOCATE cur
	DROP TABLE SystemCalendar_Calendar
END
GO
IF OBJECT_ID('SystemCalendar_Day') IS NOT NULL
BEGIN
	DROP TABLE SystemCalendar_Day
END
GO
IF OBJECT_ID('SystemCalendar_Entry') IS NOT NULL
BEGIN
	DROP TABLE SystemCalendar_Entry
END
GO
















IF OBJECT_ID (N'dbo.vw_collection') IS NOT NULL
	DROP VIEW dbo.vw_collection
GO
IF OBJECT_ID (N'dbo.vw_CollectionFinal') IS NOT NULL
	DROP VIEW dbo.vw_CollectionFinal
GO
IF OBJECT_ID (N'dbo.vw_UpdateUsersMarketPlaces') IS NOT NULL
	DROP VIEW dbo.vw_UpdateUsersMarketPlaces
GO
IF OBJECT_ID (N'dbo.vw_UsersMarketPlaces') IS NOT NULL
	DROP VIEW dbo.vw_UsersMarketPlaces
GO







IF OBJECT_ID (N'dbo.GetCustomerMarketplaceUpdatingCounters') IS NOT NULL
	DROP FUNCTION dbo.GetCustomerMarketplaceUpdatingCounters
GO
IF OBJECT_ID (N'dbo.GetExpensesPayPalTransactionsByPayerAndRange') IS NOT NULL
	DROP FUNCTION dbo.GetExpensesPayPalTransactionsByPayerAndRange
GO
IF OBJECT_ID (N'dbo.GetExpensesPayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetExpensesPayPalTransactionsByRange
GO
IF OBJECT_ID (N'dbo.GetIncomePayPalTransactionsByPayerAndRange') IS NOT NULL
	DROP FUNCTION dbo.GetIncomePayPalTransactionsByPayerAndRange
GO
IF OBJECT_ID (N'dbo.GetIncomePayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetIncomePayPalTransactionsByRange
GO
IF OBJECT_ID (N'dbo.GetTransactionsCountPayPalTransactionsByRange') IS NOT NULL
	DROP FUNCTION dbo.GetTransactionsCountPayPalTransactionsByRange
GO


































