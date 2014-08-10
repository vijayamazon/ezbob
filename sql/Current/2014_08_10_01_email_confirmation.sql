IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsConfirmed' AND id = OBJECT_ID('EmailConfirmationStates'))
	ALTER TABLE EmailConfirmationStates ADD IsConfirmed BIT NOT NULL CONSTRAINT DF_EmailConfirmationStates_IsConfirmed DEFAULT(0)
GO

IF OBJECT_ID('DF_EmailConfirmationStates_IsConfirmed') IS NOT NULL
BEGIN
	UPDATE EmailConfirmationStates SET IsConfirmed = 1 WHERE EmailStateID IN (2, 4, 5)

	ALTER TABLE EmailConfirmationStates DROP CONSTRAINT DF_EmailConfirmationStates_IsConfirmed
END
GO
