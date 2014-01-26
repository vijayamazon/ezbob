IF OBJECT_ID('CHK_WSS_Offline', 'C') IS NOT NULL
	ALTER TABLE WizardStepSequence DROP CONSTRAINT CHK_WSS_Offline

GO


UPDATE dbo.WizardStepSequence
SET OfflineProgressBarPct = 3
WHERE StepName='link'
GO

UPDATE dbo.WizardStepSequence
SET OfflineProgressBarPct = 6
WHERE StepName='details'
GO

UPDATE dbo.WizardStepSequence
SET OfflineProgressBarPct = 5
WHERE StepName='companydetails'
GO
