IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_RejectionConstants]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_RejectionConstants]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_RejectionConstants]
AS
BEGIN
	SELECT Name, Value FROM ConfigurationVariables WHERE Name 
	IN (
		'Reject_Defaults_AccountsNum',
		'Reject_Defaults_Amount',
		'Reject_Defaults_CreditScore',
		'Reject_Defaults_MonthsNum',
		'Reject_Minimal_Seniority', 
		'LowCreditScore', 
		'TotalAnnualTurnover', 
		'TotalThreeMonthTurnover',
		'AutoRejectionException_AnualTurnover',
		'AutoRejectionException_CreditScore',
		'RejectionExceptionMaxCompanyScore',
		'RejectionExceptionMaxCompanyScoreForMpError',
		'RejectionExceptionMaxConsumerScoreForMpError',
		'RejectionCompanyScore',
		'Reject_LowOfflineAnnualRevenue',
		'Reject_LowOfflineQuarterRevenue',
		'Reject_LateLastMonthsNum',
		'Reject_NumOfLateAccounts',
		'RejectionLastValidLate',
		'Reject_Defaults_CompanyScore',
		'Reject_Defaults_CompanyAccountsNum',
		'Reject_Defaults_CompanyAmount'
		
	)
END
GO
