SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE Medals ALTER COLUMN Id INT NOT NULL
GO

IF (SELECT OBJECT_ID FROM sys.all_objects WHERE name = 'PK_Medals') IS NULL
	ALTER TABLE Medals ADD CONSTRAINT PK_Medals PRIMARY KEY (Id)
GO

IF (SELECT Id FROM dbo.Medals WHERE Medal = 'NoClassification') IS NULL
	INSERT INTO Medals (Medal) VALUES('NoClassification')
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('MedalCalculations') AND name = 'MedalNameID')
	ALTER TABLE MedalCalculations ADD MedalNameID INT NOT NULL CONSTRAINT DF_MedalCalculations_NameID DEFAULT (1)
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('MedalCalculationsAV') AND name = 'MedalNameID')
	ALTER TABLE MedalCalculationsAV ADD MedalNameID INT NOT NULL CONSTRAINT DF_MedalCalculationsAV_NameID DEFAULT (1)
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_Medals')
	ALTER TABLE MedalCalculations ADD CONSTRAINT FK_MedalCalculations_Medals FOREIGN KEY(MedalNameID) REFERENCES Medals (Id)
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_Medals')
	ALTER TABLE MedalCalculationsAV ADD CONSTRAINT FK_MedalCalculationsAV_Medals FOREIGN KEY(MedalNameID)REFERENCES Medals (Id)
GO

UPDATE MedalCalculations SET
	MedalCalculations.MedalNameID = Medals.Id
FROM
	MedalCalculations
	INNER JOIN Medals ON MedalCalculations.Medal = Medals.Medal
GO

UPDATE MedalCalculationsAV SET
	MedalCalculationsAV.MedalNameID = Medals.Id
FROM
	MedalCalculationsAV
	INNER JOIN Medals ON MedalCalculationsAV.Medal = Medals.Medal
GO
