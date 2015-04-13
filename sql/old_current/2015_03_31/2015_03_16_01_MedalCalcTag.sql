SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculationsAV') AND name = 'TrailTagID')
BEGIN
	ALTER TABLE MedalCalculationsAV ADD TrailTagID BIGINT NULL
	ALTER TABLE MedalCalculationsAV ADD CONSTRAINT FK_MedalCalculationsAV_Tag FOREIGN KEY(TrailTagID) REFERENCES DecisionTrailTags (TrailTagID)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MedalCalculations') AND name = 'TrailTagID')
BEGIN
	ALTER TABLE MedalCalculations ADD TrailTagID BIGINT NULL
	ALTER TABLE MedalCalculations ADD CONSTRAINT FK_MedalCalculations_Tag FOREIGN KEY(TrailTagID) REFERENCES DecisionTrailTags (TrailTagID)
END
GO
