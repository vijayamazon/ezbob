UPDATE dbo.MP_PayPalAggregationFormula
SET Status = 'Released'
WHERE Status = 'released'
GO 

UPDATE dbo.MP_PayPalAggregationFormula
SET Status = 'Placed'
WHERE Status = 'placed'
GO

UPDATE dbo.MP_PayPalAggregationFormula
SET Status = 'Completed'
WHERE Status = 'completed&refunded'
GO

INSERT INTO dbo.MP_PayPalAggregationFormula
	(
	FormulaNum
	, FormulaName
	, Type
	, Status
	, Positive
	)
VALUES
	(
	1
	, 'TotalNetRevenues'
	, 'Recurring Payment'
	, 'Refunded'
	, 1
	)
GO

INSERT INTO dbo.MP_PayPalAggregationFormula
	(
	FormulaNum
	, FormulaName
	, Type
	, Status
	, Positive
	)
VALUES
	(
	2
	, 'TotalNetExpenses'
	, 'Recurring Payment'
	, 'Refunded'
	, 0
	)
GO


DELETE TOP(1) FROM dbo.MP_PayPalAggregationFormula
WHERE FormulaNum=5 AND Type='Payment' AND Status='Returned'
GO



