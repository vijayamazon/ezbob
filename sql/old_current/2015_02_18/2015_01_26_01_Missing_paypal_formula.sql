IF NOT EXISTS (SELECT * FROM PayPalTotalsFormulae WHERE FormulaName = 'NetSumOfRefundsAndReturns')
BEGIN
	-- ID is 90 because 90 = 0x5A
	INSERT INTO PayPalTotalsFormulae (FormulaID, FormulaName, IsSum) VALUES (90, 'NetSumOfRefundsAndReturns', 1)

	INSERT INTO PayPalTotalsFormulaTerms (FormulaID, TransactionType, TransactionStatus, TakePositive) VALUES
		(90, 'Payment', 'Partially Refunded', NULL),
		(90, 'Payment', 'Refunded by eBay', NULL),
		(90, 'Payment', 'Returned', NULL),
		(90, 'Refund', 'Completed', NULL)
END
GO