IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsDirector' AND id = OBJECT_ID('Director'))
	ALTER TABLE Director ADD IsDirector BIT NULL
GO

UPDATE Director SET IsDirector = 1 WHERE IsDirector IS NULL
GO

UPDATE Director SET IsShareholder = 0 WHERE IsShareholder IS NULL
GO

