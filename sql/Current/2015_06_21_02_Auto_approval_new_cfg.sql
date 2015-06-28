SET QUOTED_IDENTIFIER ON
GO

UPDATE ConfigurationVariables SET Value = '600000' WHERE Name = 'AutoApproveMaxOutstandingOffers'
UPDATE ConfigurationVariables SET Value = '500000' WHERE Name = 'AutoApproveMaxTodayLoans'
UPDATE ConfigurationVariables SET Value = '20' WHERE Name = 'AutoApproveMaxDailyApprovals'
UPDATE ConfigurationVariables SET Value = '0.5' WHERE Name = 'AutoApproveTurnoverDropQuarterRatio'
GO
