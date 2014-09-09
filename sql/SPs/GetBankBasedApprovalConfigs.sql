IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBankBasedApprovalConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetBankBasedApprovalConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetBankBasedApprovalConfigs]
AS
BEGIN
	SELECT
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore') AS BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalPersonalScoreThreshold') AS BankBasedApprovalPersonalScoreThreshold,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinAge') AS BankBasedApprovalMinAge,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinAmlScore') AS BankBasedApprovalMinAmlScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinCompanySeniorityDays') AS BankBasedApprovalMinCompanySeniorityDays,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinBusinessScore') AS BankBasedApprovalMinBusinessScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalBelowAverageRiskMinBusinessScore') AS BankBasedApprovalBelowAverageRiskMinBusinessScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalBelowAverageRiskMaxBusinessScore') AS BankBasedApprovalBelowAverageRiskMaxBusinessScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalBelowAverageRiskMinPersonalScore') AS BankBasedApprovalBelowAverageRiskMinPersonalScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalBelowAverageRiskMaxPersonalScore') AS BankBasedApprovalBelowAverageRiskMaxPersonalScore,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinOffer') AS BankBasedApprovalMinOffer,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalHomeOwnerCap') AS BankBasedApprovalHomeOwnerCap,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalNotHomeOwnerCap') AS BankBasedApprovalNotHomeOwnerCap,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalEuCap') AS BankBasedApprovalEuCap,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'OfferValidForHours') AS OfferValidForHours,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinNumberOfDays') AS BankBasedApprovalMinNumberOfDays,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalIsSilent') AS BankBasedApprovalIsSilent,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalSilentTemplateName') AS BankBasedApprovalSilentTemplateName,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalSilentToAddress') AS BankBasedApprovalSilentToAddress,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinNumberOfPayers') AS BankBasedMinNumberOfPayers,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalMinAnnualizedTurnover') AS BankBasedMinAnnualizedTurnover,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalIsEnabled') AS BankBasedApprovalIsEnabled,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalNumOfMonthsToLookForDefaults') AS BankBasedApprovalNumOfMonthsToLookForDefaults,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalNumOfMonthBackForVatCheck') AS BankBasedApprovalNumOfMonthBackForVatCheck,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'MinLoanAmount') AS MinLoanAmount
END
GO
