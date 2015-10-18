SET QUOTED_IDENTIFIER ON
GO

UPDATE ConfigurationVariables SET Value = '{"CustomerID":0,"DoSendEmail":false,"DoMain":true,"DoFraud":true,"NewCreditLineOption":"UpdateEverythingAndApplyAutoRules","AvoidAutoDecision":1,"IsUnderwriterForced":false,"FraudMode":"FullCheck","CashRequestOriginator":"Other"}'
WHERE Name = 'FinishWizardForApproved'
GO
