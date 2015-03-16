SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF OBJECT_ID('LotteryExcludedCustomerOrigins') IS NULL
BEGIN
	CREATE TABLE LotteryExcludedCustomerOrigins (
		EntryID BIGINT IDENTITY NOT NULL,
		LotteryID BIGINT NOT NULL,
		CustomerOriginID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LotteryExcludedCustomerOrigins PRIMARY KEY (EntryID),
		CONSTRAINT FK_LotteryExcludedCustomerOrigins_Lottery FOREIGN KEY (LotteryID) REFERENCES Lotteries (LotteryID),
		CONSTRAINT FK_LotteryExcludedCustomerOrigins_Origin FOREIGN KEY (CustomerOriginID) REFERENCES CustomerOrigin (CustomerOriginID),
		CONSTRAINT UC_LotteryExcludedCustomerOrigins UNIQUE (LotteryID, CustomerOriginID)
	)

	INSERT INTO LotteryExcludedCustomerOrigins (LotteryID, CustomerOriginID)
	SELECT DISTINCT
		l.LotteryID,
		o.CustomerOriginID
	FROM
		Lotteries l
		INNER JOIN LotteryCodes c ON l.CodeID = c.CodeID AND c.Code = 'val2015'
		INNER JOIN CustomerOrigin o ON o.Name = 'Everline'
END
GO
