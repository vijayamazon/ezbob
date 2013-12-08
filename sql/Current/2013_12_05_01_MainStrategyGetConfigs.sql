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
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CreditScore') AS Reject_Defaults_CreditScore,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_AccountsNum') AS Reject_Defaults_AccountsNum,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Minimal_Seniority') AS Reject_Minimal_Seniority,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BWABusinessCheck') AS BWABusinessCheck,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'EnableAutomaticApproval') AS EnableAutomaticApproval,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'EnableAutomaticReApproval') AS EnableAutomaticReApproval,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'EnableAutomaticRejection') AS EnableAutomaticRejection,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'EnableAutomaticReRejection') AS EnableAutomaticReRejection,		
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'MaxCapHomeOwner') AS MaxCapHomeOwner,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'MaxCapNotHomeOwner') AS MaxCapNotHomeOwner,
		
		
		
		
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'LowCreditScore') AS LowCreditScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TotalAnnualTurnover') AS LowTotalAnnualTurnover,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TotalThreeMonthTurnover') AS LowTotalThreeMonthTurnover,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_Amount') AS Reject_Defaults_Amount,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_MonthsNum') AS Reject_Defaults_MonthsNum,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'AutoRejectionException_CreditScore') AS AutoRejectionException_CreditScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'AutoRejectionException_AnualTurnover') AS AutoRejectionException_AnualTurnover,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'AutoReApproveMaxNumOfOutstandingLoans') AS AutoReApproveMaxNumOfOutstandingLoans,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'AutoApproveIsSilent') AS AutoApproveIsSilent,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'AutoApproveSilentTemplateName') AS AutoApproveSilentTemplateName,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'AutoApproveSilentToAddress') AS AutoApproveSilentToAddress
END
GO
