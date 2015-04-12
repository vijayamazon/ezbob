IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanScheduleTransactionBackFilled') AND name = 'Step')
BEGIN
	ALTER TABLE LoanScheduleTransactionBackFilled ADD Step INT NOT NULL CONSTRAINT DF_lstbf_step DEFAULT(0)

	EXECUTE('UPDATE LoanScheduleTransactionBackFilled SET Step = 1')
	
	EXECUTE('ALTER TABLE LoanScheduleTransactionBackFilled DROP CONSTRAINT DF_lstbf_step')
END
GO

