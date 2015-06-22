IF EXISTS (SELECT * FROM syscolumns WHERE id=object_id('LoanOptions') AND name='AutoInterest')
BEGIN
	DECLARE @Command  NVARCHAR(1000)
	
	SELECT @Command = 'ALTER TABLE LoanOptions' + ' DROP CONSTRAINT ' + d.name
	FROM sys.tables t 
	JOIN sys.default_constraints d ON d.parent_object_id = t.object_id  
	JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
	WHERE t.name = 'LoanOptions' AND c.name = 'AutoInterest'
	
	--PRINT @Command
	
	EXECUTE (@Command)
	
	ALTER TABLE LoanOptions DROP COLUMN AutoInterest
END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id=object_id('LoanOptions') AND name='StopAutoInterestDate')
BEGIN
	ALTER TABLE LoanOptions DROP COLUMN StopAutoInterestDate
END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id=object_id('LoanOptions') AND name='StopLateFeeDate')
BEGIN
	ALTER TABLE LoanOptions DROP COLUMN StopLateFeeDate
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('LoanOptions') AND name='StopAutoChargeDate')
BEGIN
	ALTER TABLE LoanOptions ADD StopAutoChargeDate DATETIME 
END 
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('LoanOptions') AND name='StopLateFeeFromDate')
BEGIN
	ALTER TABLE LoanOptions ADD StopLateFeeFromDate DATETIME 
END 
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('LoanOptions') AND name='StopLateFeeToDate')
BEGIN
	ALTER TABLE LoanOptions ADD StopLateFeeToDate DATETIME
END
GO 