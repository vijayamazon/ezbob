UPDATE dbo.WizardStepSequence
SET OfflineProgressBarPct = 60
WHERE WizardStepType = 2
GO

UPDATE dbo.WizardStepSequence
SET OfflineProgressBarPct = 30
WHERE WizardStepType = 5
GO

UPDATE dbo.WizardStepSequence
SET OfflineProgressBarPct = 50
WHERE WizardStepType = 6
GO