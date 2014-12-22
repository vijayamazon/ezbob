SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

DECLARE @NewFormula BIT = 0

-------------------------------------------------------------------------------

SELECT
	FormulaNum,
	FormulaName,
	Type,
	Status,
	Positive,
	CONVERT(BIT, NULL) AS IsSum
INTO
	#f
FROM
	MP_PayPalAggregationFormula
WHERE
	1 = 0

-------------------------------------------------------------------------------

INSERT INTO #f (FormulaNum, FormulaName, Type,       Status,     Positive, IsSum) VALUES
	(  1, '', '',  '', 0,                                                 1),
	(  2, '', '',  '', 0,                                                 1),
	(  3, '', '',  '', 0,                                                 0),
	(  4, '', '',  '', 0,                                                 1),
	(  5, '', '',  '', 0,                                                 0),
	(  6, '', '',  '', 0,                                                 1),
	(  7, '', '',  '', 0,                                                 1),
	(101, 'TotalNetInPayments',        'Payment',  'Completed', 1,        1),
	(102, 'TotalNetOutPayments',       'Transfer', 'Completed', 1,        1),
	(103, 'TransactionsNumber',        'Payment',  'Completed', 1,        0)

-------------------------------------------------------------------------------

IF OBJECT_ID('PayPalTotalsFormulae') IS NULL
BEGIN
	CREATE TABLE PayPalTotalsFormulae (
		FormulaID INT NOT NULL,
		FormulaName NVARCHAR(64) NOT NULL,
		IsSum BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_PayPalTotalsFormulae PRIMARY KEY (FormulaID),
		CONSTRAINT UC_PayPalTotalsFormulae UNIQUE (FormulaName)
	)

	SET @NewFormula = 1
END

-------------------------------------------------------------------------------

IF OBJECT_ID('PayPalTotalsFormulaTerms') IS NULL
BEGIN
	CREATE TABLE PayPalTotalsFormulaTerms (
		FormulaTermID INT IDENTITY(1, 1) NOT NULL,
		FormulaID INT NOT NULL,
		TransactionType NVARCHAR(128) NOT NULL,
		TransactionStatus NVARCHAR(128) NOT NULL,
		TakePositive BIT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_PayPalTotalsFormulaTerms PRIMARY KEY (FormulaTermID),
		CONSTRAINT FK_PayPalTotalsFormulaTerms_Formula FOREIGN KEY (FormulaID) REFERENCES PayPalTotalsFormulae (FormulaID),
		CONSTRAINT UC_PayPalTotalsFormulaTerms UNIQUE (FormulaID, TransactionType, TransactionStatus)
	)

	SET @NewFormula = 1
END

-------------------------------------------------------------------------------

IF @NewFormula = 1
BEGIN
	INSERT INTO PayPalTotalsFormulae (FormulaID, FormulaName, IsSum)
	SELECT DISTINCT
		f.FormulaNum,
		f.FormulaName,
		#f.IsSum
	FROM
		MP_PayPalAggregationFormula f
		INNER JOIN #f ON f.FormulaNum = #f.FormulaNum

	--------------------------------------------------------------------------

	INSERT INTO PayPalTotalsFormulae (FormulaID, FormulaName, IsSum)
	SELECT DISTINCT
		#f.FormulaNum,
		#f.FormulaName,
		#f.IsSum
	FROM
		#f
	WHERE
		#f.FormulaNum > 10

	--------------------------------------------------------------------------

	INSERT INTO PayPalTotalsFormulaTerms (FormulaID, TransactionType, TransactionStatus, TakePositive)
	SELECT DISTINCT
		f.FormulaNum,
		f.Type,
		f.Status,
		f.Positive
	FROM
		MP_PayPalAggregationFormula f

	--------------------------------------------------------------------------

	INSERT INTO PayPalTotalsFormulaTerms (FormulaID, TransactionType, TransactionStatus, TakePositive)
	SELECT DISTINCT
		#f.FormulaNum,
		#f.Type,
		#f.Status,
		#f.Positive
	FROM
		#f
	WHERE
		#f.FormulaNum > 10
END

-------------------------------------------------------------------------------

DROP TABLE #f
GO
