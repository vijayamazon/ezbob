SET QUOTED_IDENTIFIER ON

IF OBJECT_ID('NL_LoanPaymentLinkHistoryReasons') IS NULL
BEGIN
	CREATE TABLE NL_LoanPaymentLinkHistoryReasons (
		ReasonID INT NOT NULL,
		Reason NVARCHAR(50) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanPaymentLinkHistoryReasons PRIMARY KEY (ReasonID),
		CONSTRAINT UC_NL_LoanPaymentLinkHistoryReasons UNIQUE (Reason),
		CONSTRAINT CHK_NL_LoanPaymentLinkHistoryReasons CHECK (LTRIM(RTRIM(Reason)) != '')
	)

	INSERT INTO NL_LoanPaymentLinkHistoryReasons (ReasonID, Reason) VALUES
		(1, 'Added'),
		(2, 'Dropped')
END
GO

IF OBJECT_ID('NL_LoanPaymentLinkHistory') IS NULL
BEGIN
	CREATE TABLE NL_LoanPaymentLinkHistory (
		LinkID BIGINT IDENTITY(1, 1) NOT NULL,
		ReasonPaymentID INT NOT NULL,
		ReasonID INT NOT NULL,
		CreationTime DATETIME NOT NULL,
		PaymentID INT NOT NULL,
		LoanScheduleID INT NULL,
		PrincipalPaid DECIMAL(18, 6) NULL,
		InterestPaid DECIMAL(18, 6) NULL,
		LoanFeeID INT NULL,
		FeePaid DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanPaymentLinkHistory PRIMARY KEY (LinkID),
		CONSTRAINT FK_NL_LoanPaymentLinkHistory_ReasonPayment FOREIGN KEY (ReasonPaymentID) REFERENCES NL_Payments (PaymentID),
		CONSTRAINT FK_NL_LoanPaymentLinkHistory_Reason FOREIGN KEY (ReasonID) REFERENCES NL_LoanPaymentLinkHistoryReasons (ReasonID),
		CONSTRAINT FK_NL_LoanPaymentLinkHistory_Payment FOREIGN KEY (PaymentID) REFERENCES NL_Payments (PaymentID),
		CONSTRAINT FK_NL_LoanPaymentLinkHistory_Schedule FOREIGN KEY (LoanScheduleID) REFERENCES NL_LoanSchedules (LoanScheduleID),
		CONSTRAINT FK_NL_LoanPaymentLinkHistory_Fee FOREIGN KEY (LoanFeeID) REFERENCES NL_LoanFees (LoanFeeID),
		CONSTRAINT CHK_NL_LoanPaymentLinkHistory_Schedule_Or_Fee CHECK (
			(LoanScheduleID IS NOT NULL AND LoanFeeID IS NULL)
			OR
			(LoanScheduleID IS NULL AND LoanFeeID IS NOT NULL)
		),
		CONSTRAINT CHK_NL_LoanPaymentLinkHistory_Schedule CHECK (
			(LoanScheduleID IS NOT NULL AND PrincipalPaid IS NOT NULL AND InterestPaid IS NOT NULL)
			OR
			(LoanScheduleID IS NULL AND PrincipalPaid IS NULL AND InterestPaid IS NULL)
		),
		CONSTRAINT CHK_NL_LoanPaymentLinkHistory_Fee CHECK (
			(LoanFeeID IS NOT NULL AND FeePaid IS NOT NULL)
			OR
			(LoanFeeID IS NULL AND FeePaid IS NULL)
		)
	)
END
GO
