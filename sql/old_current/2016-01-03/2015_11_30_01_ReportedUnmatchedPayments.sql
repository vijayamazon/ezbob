SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ReportedUnmatchedPayments') IS NULL
BEGIN
	CREATE TABLE ReportedUnmatchedPayments (
		PaymentID INT NOT NULL,
		LastKnownTimestamp BINARY(8) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ReportedUnmatchedPayments PRIMARY KEY (PaymentID),
		CONSTRAINT FK_ReportedUnmatchedPayments_LoanTransaction FOREIGN KEY (PaymentID) REFERENCES LoanTransaction (Id)
	)
END
GO
