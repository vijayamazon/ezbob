IF NOT EXISTS(SELECT * FROM syscolumns WHERE id=object_id('LoanSchedule') AND name='DatePaid')
BEGIN
	ALTER TABLE LoanSchedule ADD DatePaid DATETIME 
END
GO
