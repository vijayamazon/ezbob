IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRejectionConfigs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetRejectionConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetRejectionConfigs] 
AS
BEGIN
	SELECT
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'AutoRejectionException_AnualTurnover') AS AutoRejectionException_AnualTurnover,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CreditScore') AS Reject_Defaults_CreditScore,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Minimal_Seniority') AS Reject_Minimal_Seniority,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'LowCreditScore') AS LowCreditScore,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_AccountsNum') AS Reject_Defaults_AccountsNum,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'AutoRejectionException_CreditScore') AS AutoRejectionException_CreditScore,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_MonthsNum') AS Reject_Defaults_MonthsNum,
		(SELECT convert(INT, Value) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_Amount') AS Reject_Defaults_Amount
END
GO
