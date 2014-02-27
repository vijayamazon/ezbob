IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore', 850, 'Minimum value to get bank based approval for personal score in scenario where company score doesn''t exist')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalPersonalScoreThreshold')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalPersonalScoreThreshold', 560, 'Minimum value to get bank based approval for personal score in scenario where company score exists')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinAge')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinAge', 18, 'Minimum age to get bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinAmlScore')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinAmlScore', 70, 'Minimum aml score to get bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinCompanySeniorityDays')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinCompanySeniorityDays', 1065, 'Minimum company seniority to get bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinBusinessScore')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinBusinessScore', 31, 'Minimum business score to get bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalBelowAverageRiskMinBusinessScore')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalBelowAverageRiskMinBusinessScore', 51, 'Minimum business score to get bank based approval at below average risk terms')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalBelowAverageRiskMaxBusinessScore')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalBelowAverageRiskMaxBusinessScore', 80, 'Maximum business score to get bank based approval at below average risk terms')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalBelowAverageRiskMinPersonalScore')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalBelowAverageRiskMinPersonalScore', 901, 'Minimum personal score to get bank based approval at below average risk terms')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalBelowAverageRiskMaxPersonalScore')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalBelowAverageRiskMaxPersonalScore', 1000, 'Maximum personal score to get bank based approval at below average risk terms')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinOffer')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinOffer', 2250, 'Minimum offer allowed to get via bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalHomeOwnerCap')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalHomeOwnerCap', 20000, 'Maximum offer allowed for home owner to get via bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalNotHomeOwnerCap')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalNotHomeOwnerCap', 10000, 'Maximum offer allowed for not home owner to get via bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalEuCap')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalEuCap', 10000, 'Maximum offer allowed by EU to get via bank based approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinNumberOfDays')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinNumberOfDays', 85, 'Minimum number of days back that transaction must exist for approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalIsSilent')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalIsSilent', 'True', 'If true then bank based auto approvals will be diverted to manual flow and a mail notification will be sent')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalSilentTemplateName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalSilentTemplateName', 'BankBasedApprovalSilentNotification', 'Template name in mandrill for the silent mail')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalSilentToAddress')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalSilentToAddress', 'yulys@ezbob.com', 'Address for silent mode of bank based auto approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinNumberOfPayers')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinNumberOfPayers', '3', 'Minimal number of different payers required for approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalMinAnnualizedTurnover')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalMinAnnualizedTurnover', '150000', 'Minimal turnover required for approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalNumOfMonthsToLookForDefaults')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalNumOfMonthsToLookForDefaults', '24', 'Number of months back that if defaults exists within them there will be no approval')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalIsEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalIsEnabled', 'True', 'Determines if bank based approvals are enabled')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BankBasedApprovalNumOfMonthBackForVatCheck')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BankBasedApprovalNumOfMonthBackForVatCheck', '3', 'Determines how far back we check for vat returns')
END

GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetBankBasedApprovalConfigs') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetBankBasedApprovalConfigs
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetBankBasedApprovalConfigs 
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
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedMinNumberOfPayers') AS BankBasedMinNumberOfPayers,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedMinAnnualizedTurnover') AS BankBasedMinAnnualizedTurnover,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalIsEnabled') AS BankBasedApprovalIsEnabled,
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalNumOfMonthsToLookForDefaults') AS BankBasedApprovalNumOfMonthsToLookForDefaults,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'BankBasedApprovalNumOfMonthBackForVatCheck') AS BankBasedApprovalNumOfMonthBackForVatCheck		
END
GO
