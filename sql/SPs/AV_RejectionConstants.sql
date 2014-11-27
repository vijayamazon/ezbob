IF OBJECT_ID('AV_RejectionConstants') IS NULL
	EXECUTE('CREATE PROCEDURE AV_RejectionConstants AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[AV_RejectionConstants]
AS
BEGIN
	DECLARE @AutoRejectionException_AnualTurnover INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'AutoRejectionException_AnualTurnover')
	DECLARE @AutoRejectionException_CreditScore INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'AutoRejectionException_CreditScore')
	DECLARE @LowCreditScore INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'LowCreditScore')
	DECLARE @Reject_Defaults_AccountsNum INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_AccountsNum')
	DECLARE @Reject_Defaults_Amount INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_Amount')
	DECLARE @Reject_Defaults_CompanyAccountsNum INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyAccountsNum')
	DECLARE @Reject_Defaults_CompanyAmount INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyAmount')
	DECLARE @Reject_Defaults_CompanyMonthsNum INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyMonthsNum')
	DECLARE @Reject_Defaults_CompanyScore INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CompanyScore')
	DECLARE @Reject_Defaults_CreditScore INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_CreditScore')
	DECLARE @Reject_Defaults_MonthsNum INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Defaults_MonthsNum')
	DECLARE @Reject_LateLastMonthsNum INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_LateLastMonthsNum')
	DECLARE @Reject_Minimal_Seniority INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_Minimal_Seniority')
	DECLARE @Reject_NumOfLateAccounts INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'Reject_NumOfLateAccounts')
	DECLARE @RejectionCompanyScore INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'RejectionCompanyScore')
	DECLARE @RejectionExceptionMaxCompanyScore INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'RejectionExceptionMaxCompanyScore')
	DECLARE @RejectionExceptionMaxCompanyScoreForMpError INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'RejectionExceptionMaxCompanyScoreForMpError')
	DECLARE @RejectionExceptionMaxConsumerScoreForMpError INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'RejectionExceptionMaxConsumerScoreForMpError')
	DECLARE @RejectionLastValidLate INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'RejectionLastValidLate')
	DECLARE @TotalAnnualTurnover INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'TotalAnnualTurnover')
	DECLARE @TotalThreeMonthTurnover INT = (SELECT CAST(Value AS INT) FROM ConfigurationVariables WHERE Name = 'TotalThreeMonthTurnover')
	
	SELECT
		@AutoRejectionException_AnualTurnover AS AutoRejectionException_AnualTurnover
		,@AutoRejectionException_CreditScore AS AutoRejectionException_CreditScore
		,@LowCreditScore AS LowCreditScore
		,@Reject_Defaults_AccountsNum AS Reject_Defaults_AccountsNum
		,@Reject_Defaults_Amount AS Reject_Defaults_Amount
		,@Reject_Defaults_CompanyAccountsNum AS Reject_Defaults_CompanyAccountsNum
		,@Reject_Defaults_CompanyAmount AS Reject_Defaults_CompanyAmount
		,@Reject_Defaults_CompanyMonthsNum AS Reject_Defaults_CompanyMonthsNum
		,@Reject_Defaults_CompanyScore AS Reject_Defaults_CompanyScore
		,@Reject_Defaults_CreditScore AS Reject_Defaults_CreditScore
		,@Reject_Defaults_MonthsNum AS Reject_Defaults_MonthsNum
		,@Reject_Minimal_Seniority AS Reject_Minimal_Seniority
		,@Reject_NumOfLateAccounts AS Reject_NumOfLateAccounts
		,@RejectionCompanyScore AS RejectionCompanyScore
		,@RejectionExceptionMaxCompanyScore AS RejectionExceptionMaxCompanyScore
		,@RejectionExceptionMaxCompanyScoreForMpError AS RejectionExceptionMaxCompanyScoreForMpError
		,@RejectionExceptionMaxConsumerScoreForMpError AS RejectionExceptionMaxConsumerScoreForMpError
		,@RejectionLastValidLate AS RejectionLastValidLate
		,@RejectionLastValidLate AS RejectionLastValidLate
		,@TotalAnnualTurnover AS TotalAnnualTurnover
		,@TotalThreeMonthTurnover AS TotalThreeMonthTurnover
END
GO
