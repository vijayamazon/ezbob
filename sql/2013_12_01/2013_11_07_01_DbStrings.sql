SELECT
	[Key] AS NewName,
	[Key] AS OldName
INTO
	#t
FROM
	DbString
WHERE
	1 = 0
	
INSERT INTO #t(NewName, OldName) VALUES ('MarketingWizardStepSignup',       'MarketingWizardStep1')
INSERT INTO #t(NewName, OldName) VALUES ('MarketingWizardStepLinkAccounts', 'MarketingWizardStep2')
INSERT INTO #t(NewName, OldName) VALUES ('MarketingWizardStepPersonalInfo', 'MarketingWizardStep3')
INSERT INTO #t(NewName, OldName) VALUES ('MarketingWizardStepCompanyInfo',  'MarketingWizardStep35')
INSERT INTO #t(NewName, OldName) VALUES ('MarketingWizardStepDone',         'MarketingWizardStep4')

DELETE
	DbString
FROM
	#t
WHERE
	DbString.[Key] = #t.NewName

INSERT INTO DbString([Key], Value)
SELECT
	#t.NewName,
	s.Value
FROM
	DbString s
	INNER JOIN #t ON s.[Key] = #t.OldName

DROP TABLE #t
GO
