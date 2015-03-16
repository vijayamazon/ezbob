IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'ExperianDirectorID' AND id = OBJECT_ID('Esigners'))
BEGIN
	ALTER TABLE Esigners ADD ExperianDirectorID INT NULL

	ALTER TABLE Esigners ADD CONSTRAINT FK_Esigner_ExperianDirector FOREIGN KEY (ExperianDirectorID) REFERENCES ExperianDirectors (ExperianDirectorID)
END
GO
