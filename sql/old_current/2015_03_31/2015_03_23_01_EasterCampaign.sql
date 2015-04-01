SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LotteryPrizes') AND name = 'Description')
BEGIN
	ALTER TABLE LotteryPrizes ADD Description NVARCHAR(255) NULL
END
GO

-------------------------------------------------------------------------------

CREATE TABLE #prizes (
	Position INT IDENTITY(1, 1) NOT NULL,
	Amount DECIMAL(18, 2),
	Counter INT,
	IsForCustomer BIT
)
GO

-------------------------------------------------------------------------------

WITH prz AS (
	SELECT 1 AS Counter, 1000 AS Amount
	UNION ALL
	SELECT 2 AS Counter, 750 AS Amount
	UNION ALL
	SELECT 5 AS Counter, 100 AS Amount
	UNION ALL
	SELECT 12 AS Counter, 50 AS Amount
	UNION ALL
	SELECT 100 AS Counter, 25 AS Amount
), pmax AS (
	SELECT MAX(Counter) AS MaxCounter
	FROM prz
), tmp AS (
	SELECT 1 n
	UNION ALL
	SELECT n + 1 FROM tmp INNER JOIN pmax ON tmp.n < pmax.MaxCounter
)
INSERT INTO #prizes (Amount, Counter, IsForCustomer)
SELECT
	prz.Amount,
	tmp.n,
	1
FROM
	prz
	INNER JOIN tmp ON prz.Counter >= tmp.n
ORDER BY
	prz.Amount
OPTION (MAXRECURSION 130)
GO

-------------------------------------------------------------------------------

WITH prz AS (
	SELECT 1 AS Counter, 1000 AS Amount
	UNION ALL
	SELECT 2 AS Counter, 500 AS Amount
	UNION ALL
	SELECT 2 AS Counter, 250 AS Amount
	UNION ALL
	SELECT 115 AS Counter, 50 AS Amount
), pmax AS (
	SELECT MAX(Counter) AS MaxCounter
	FROM prz
), tmp AS (
	SELECT 1 n
	UNION ALL
	SELECT n + 1 FROM tmp INNER JOIN pmax ON tmp.n < pmax.MaxCounter
)
INSERT INTO #prizes (Amount, Counter, IsForCustomer)
SELECT
	prz.Amount,
	tmp.n,
	0
FROM
	prz
	INNER JOIN tmp ON prz.Counter >= tmp.n
ORDER BY
	prz.Amount
OPTION (MAXRECURSION 130)
GO

-------------------------------------------------------------------------------

SELECT
	Code,
	Description,
	CONVERT(BIGINT, 0) AS CodeID
INTO
	#code
FROM
	LotteryCodes
WHERE
	1 = 0
GO

-------------------------------------------------------------------------------

INSERT INTO #code (Code, Description) VALUES ('easter2015', 'Easter capmaign 2015 for customers and brokers')
GO

-------------------------------------------------------------------------------

INSERT INTO LotteryCodes (Code, Description)
SELECT
	c.Code,
	c.Description
FROM
	#code c
	LEFT JOIN LotteryCodes lc ON c.Code = lc.Code
WHERE
	lc.CodeID IS NULL
GO

-------------------------------------------------------------------------------

UPDATE #code SET
	CodeID = lc.CodeID
FROM
	LotteryCodes lc
	INNER JOIN #code c ON lc.Code = c.Code

-------------------------------------------------------------------------------

DECLARE @ID BIGINT = 1 + (SELECT MAX(LotteryID) FROM Lotteries)

IF NOT EXISTS (SELECT * FROM Lotteries l INNER JOIN #code c ON l.CodeID = c.CodeID WHERE l.IsForCustomer = 1)
BEGIN
	INSERT INTO Lotteries (LotteryID,
		LotteryName, MailTemplateName, StartDate, EndDate,
		IsActive, MinParticipantCount, IsForCustomer,
		LoanCount, LoanAmount, LotteryEnlistingTypeID,
		LotteryPriority, IsForNew, CodeID
	) SELECT
		@ID,
		'Easter 2015 for customers', '', 'April 1 2015', 'April 30 2015',
		1, NULL, 1,
		1, NULL, lt.LotteryEnlistingTypeID,
		NULL, 1, c.CodeID
	FROM
		#code c,
		LotteryEnlistingTypes lt
	WHERE
		lt.LotteryEnlistingType = 'MaxCount'

	DECLARE @PID BIGINT = (SELECT MAX(PrizeID) FROM LotteryPrizes)

	INSERT INTO LotteryPrizes(PrizeID, LotteryID, Amount, Description)
	SELECT
		@PID + Position,
		@ID,
		Amount,
		CONVERT(NVARCHAR, Counter)
	FROM
		#prizes
	WHERE
		IsForCustomer = 1

	INSERT INTO LotteryExcludedCustomerOrigins (LotteryID, CustomerOriginID)
	SELECT
		@ID,
		CustomerOriginID
	FROM
		CustomerOrigin
	WHERE
		Name != 'ezbob'
END
GO

-------------------------------------------------------------------------------

DECLARE @ID BIGINT = 1 + (SELECT MAX(LotteryID) FROM Lotteries)

IF NOT EXISTS (SELECT * FROM Lotteries l INNER JOIN #code c ON l.CodeID = c.CodeID WHERE l.IsForCustomer = 0)
BEGIN
	INSERT INTO Lotteries (LotteryID,
		LotteryName, MailTemplateName, StartDate, EndDate,
		IsActive, MinParticipantCount, IsForCustomer,
		LoanCount, LoanAmount, LotteryEnlistingTypeID,
		LotteryPriority, IsForNew, CodeID
	) SELECT
		@ID,
		'Easter 2015 for brokers', '', 'April 1 2015', 'April 30 2015',
		1, NULL, 0,
		1, NULL, lt.LotteryEnlistingTypeID,
		NULL, 1, c.CodeID
	FROM
		#code c,
		LotteryEnlistingTypes lt
	WHERE
		lt.LotteryEnlistingType = 'MaxCount'

	DECLARE @PID BIGINT = (SELECT MAX(PrizeID) FROM LotteryPrizes)

	INSERT INTO LotteryPrizes(PrizeID, LotteryID, Amount, Description)
	SELECT
		@PID + Position,
		@ID,
		Amount,
		CONVERT(NVARCHAR, Counter)
	FROM
		#prizes
	WHERE
		IsForCustomer = 0

	INSERT INTO LotteryExcludedCustomerOrigins (LotteryID, CustomerOriginID)
	SELECT
		@ID,
		CustomerOriginID
	FROM
		CustomerOrigin
	WHERE
		Name != 'ezbob'
END
GO

-------------------------------------------------------------------------------

DROP TABLE #code
DROP TABLE #prizes
GO
