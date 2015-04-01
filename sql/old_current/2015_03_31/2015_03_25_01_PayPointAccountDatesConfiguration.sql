IF NOT EXISTS (SELECT * FROM syscolumns WHERE name='LoanFromDate' AND id=object_id('PayPointAccount'))
BEGIN
	ALTER TABLE PayPointAccount ADD LoanFromDate DATETIME
	ALTER TABLE PayPointAccount ADD LoanToDate DATETIME
END 
GO

UPDATE PayPointAccount SET LoanFromDate = '2012-01-01', LoanToDate = '2015-01-12' WHERE Mid='orange06'
UPDATE PayPointAccount SET LoanFromDate = '2015-01-12', LoanToDate = '2025-01-01' WHERE Mid='orange07'
GO
