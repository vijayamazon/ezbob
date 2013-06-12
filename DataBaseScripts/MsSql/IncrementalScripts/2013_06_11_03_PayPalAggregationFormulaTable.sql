IF OBJECT_ID ('dbo.MP_PayPalAggregationFormula') IS NOT NULL
BEGIN
	ALTER TABLE MP_PayPalAggregationFormula ALTER COLUMN Positive BIT NULL
	RETURN 
END
CREATE TABLE dbo.MP_PayPalAggregationFormula
	(
	  Id          INT IDENTITY NOT NULL
	, FormulaNum  INT NOT NULL
	, FormulaName NVARCHAR (300) NOT NULL
	, Type        NVARCHAR (300) NOT NULL
	, Status      NVARCHAR (300) NOT NULL
	, Positive    BIT
	)


INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Payment', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Payment', 'Cleared', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Payment', 'Pending', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Payment', 'Returned', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Recurring Payment', 'completed&refunded', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Payment', 'Refunded by eBay', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Payment', 'Partially Refunded', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Refund', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Cash Back Bonus', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Bonus', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Fee', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Reserve Release', 'released', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (1,'TotalNetRevenues', 'Reserve Hold', 'placed', 0)

GO
 
