IF OBJECT_ID('LoadAutoRejectionConfigs') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoRejectionConfigs AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoRejectionConfigs
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		v.Name,
		v.Value
	FROM
		ConfigurationVariables v
	WHERE
		v.Name IN (
			'AutoRejectConsumerCheckAge',
			'AutoRejectionException_AnualTurnover',
			'AutoRejectionException_CreditScore',
			'LowCreditScore',
			'Reject_Defaults_AccountsNum',
			'Reject_Defaults_Amount',
			'Reject_Defaults_CompanyAccountsNum',
			'Reject_Defaults_CompanyAmount',
			'Reject_Defaults_CompanyScore',
			'Reject_Defaults_CompanyMonthsNum',
			'Reject_Defaults_CreditScore',
			'Reject_Defaults_MonthsNum',
			'Reject_LateLastMonthsNum',
			'Reject_Minimal_Seniority',
			'Reject_NumOfLateAccounts',
			'RejectionCompanyScore',
			'RejectionExceptionMaxCompanyScore',
			'RejectionExceptionMaxCompanyScoreForMpError',
			'RejectionExceptionMaxConsumerScoreForMpError',
			'RejectionLastValidLate',
			'TotalAnnualTurnover',
			'TotalThreeMonthTurnover'
		)
END
GO
