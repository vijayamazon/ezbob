IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TotalTimeToWaitForExperianCompanyCheck')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TotalTimeToWaitForExperianCompanyCheck', '60', 'Total time to wait for experian company check in seconds')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IntervalWaitForExperianCompanyCheck')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IntervalWaitForExperianCompanyCheck', '30000', 'Interval to wait for  experian company check in milliseconds')
END

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MainStrategyGetConfigs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MainStrategyGetConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MainStrategyGetConfigs] 
AS
BEGIN
	SELECT
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CreditScore') AS Reject_Defaults_CreditScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_AccountsNum') AS Reject_Defaults_AccountsNum,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'Reject_Minimal_Seniority') AS Reject_Minimal_Seniority,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_MonthsNum') AS Reject_Defaults_MonthsNum,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_Amount') AS Reject_Defaults_Amount,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BWABusinessCheck') AS BWABusinessCheck,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'EnableAutomaticApproval') AS EnableAutomaticApproval,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'EnableAutomaticReApproval') AS EnableAutomaticReApproval,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'EnableAutomaticRejection') AS EnableAutomaticRejection,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'EnableAutomaticReRejection') AS EnableAutomaticReRejection,		
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'MaxCapHomeOwner') AS MaxCapHomeOwner,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'MaxCapNotHomeOwner') AS MaxCapNotHomeOwner,		
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'LowCreditScore') AS LowCreditScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TotalAnnualTurnover') AS LowTotalAnnualTurnover,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TotalThreeMonthTurnover') AS LowTotalThreeMonthTurnover,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'DefaultFeedbackValue') AS DefaultFeedbackValue,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TotalTimeToWaitForMarketplacesUpdate') AS TotalTimeToWaitForMarketplacesUpdate,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'IntervalWaitForMarketplacesUpdate') AS IntervalWaitForMarketplacesUpdate,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TotalTimeToWaitForExperianCompanyCheck') AS TotalTimeToWaitForExperianCompanyCheck,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'IntervalWaitForExperianCompanyCheck') AS IntervalWaitForExperianCompanyCheck
END
GO
