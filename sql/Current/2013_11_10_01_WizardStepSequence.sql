IF OBJECT_ID('WizardStepSequence') IS NULL
BEGIN
	CREATE TABLE WizardStepSequence (
		ID INT NOT NULL,
		StepName NVARCHAR(64) NOT NULL,
		OnlineProgressBarPct INT NULL,
		OfflineProgressBarPct INT NULL,
		WizardStepType INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_WizardStepSequence PRIMARY KEY (ID),
		CONSTRAINT CHK_WSS_StepName CHECK (StepName IN ('link', 'details')),
		CONSTRAINT CHK_WSS_Online CHECK (OnlineProgressBarPct IS NULL OR OnlineProgressBarPct IN (10, 20, 30, 40, 50, 60, 70, 80, 90)),
		CONSTRAINT CHK_WSS_Offline CHECK (OfflineProgressBarPct IS NULL OR OfflineProgressBarPct IN (10, 20, 30, 40, 50, 60, 70, 80, 90))
	)
	
	CREATE UNIQUE INDEX IDX_WSS_StepName ON WizardStepSequence(StepName)

	INSERT INTO WizardStepSequence (ID, StepName, OnlineProgressBarPct, OfflineProgressBarPct, WizardStepType) VALUES
		(1, 'link', 30, 60, 2),
		(2, 'details', 70, 30, 5)
END
GO
