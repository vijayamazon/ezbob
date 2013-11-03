IF OBJECT_ID ('LoanScheduleTransactionBackFilled') IS NULL
	CREATE TABLE LoanScheduleTransactionBackFilled (
		LoanScheduleTransactionID BIGINT NOT NULL,
		TimestampCounter ROWVERSION
	)
GO
