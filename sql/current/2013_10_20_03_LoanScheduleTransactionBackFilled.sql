IF NOT EXISTS(SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanScheduleTransactionBackFilled') AND name = 'IsBad')
	ALTER TABLE LoanScheduleTransactionBackFilled
		ADD IsBad BIT NOT NULL
			CONSTRAINT DF_LoanScheduleTransactionBackFilled_IsBad DEFAULT (0)
GO

