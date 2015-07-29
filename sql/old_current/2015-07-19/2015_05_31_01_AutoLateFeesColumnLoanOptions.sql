IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='AutoLateFees' AND id=object_id('LoanOptions'))
BEGIN 
	ALTER TABLE LoanOptions ADD AutoLateFees BIT DEFAULT(1) NOT NULL
END
GO