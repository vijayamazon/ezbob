IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IDX_Business' AND object_id = OBJECT_ID('Business'))
	DROP INDEX Business.IDX_Business
GO

ALTER TABLE Business ALTER COLUMN Address NVARCHAR(300) NOT NULL
GO

ALTER TABLE Business ALTER COLUMN Name NVARCHAR(100) NOT NULL
GO

CREATE UNIQUE INDEX IDX_Business ON Business (RegistrationNo, Name, Address)
GO
