SET QUOTED_IDENTIFIER ON
GO

UPDATE ConfigurationVariables SET Value = '200000' WHERE Name = 'AutoApproveMaxTodayLoans'
GO
