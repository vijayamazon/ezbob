IF OBJECT_ID('AV_RejectionConstants') IS NULL
	EXECUTE('CREATE PROCEDURE AV_RejectionConstants AS SELECT 1')
GO

ALTER PROCEDURE AV_RejectionConstants
AS
BEGIN 
SELECT Name, Value FROM ConfigurationVariables WHERE Name IN ('Reject_Defaults_AccountsNum','Reject_Defaults_Amount','Reject_Defaults_CreditScore','Reject_Defaults_MonthsNum','Reject_Minimal_Seniority', 'LowCreditScore', 'TotalAnnualTurnover', 'TotalThreeMonthTurnover','AutoRejectionException_AnualTurnover','AutoRejectionException_CreditScore')
END 