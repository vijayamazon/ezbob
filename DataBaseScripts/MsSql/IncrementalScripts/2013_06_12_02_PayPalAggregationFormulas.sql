--formula 2
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Payment', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Payment', 'Cleared', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Payment', 'Pending', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Payment', 'Returned', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Recurring Payment', 'completed&refunded', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Payment', 'Refunded by eBay', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Payment', 'Partially Refunded', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Refund', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Cash Back Bonus', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Bonus', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Fee', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'PayPal Smart Connect Payment', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Debit Card Signature Purchase', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Debit Card PIN Purchase', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'PayPal Services', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (2,'TotalNetExpenses', 'Debit Card Credit Received', 'Completed', 1)

--formula 3
INSERT INTO MP_PayPalAggregationFormula VALUES (3,'NumOfTotalTransactions', 'Payment', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (3,'NumOfTotalTransactions', 'Payment', 'Cleared', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (3,'NumOfTotalTransactions', 'Recurring Payment', 'Completed', 1)

--formula 4
INSERT INTO MP_PayPalAggregationFormula VALUES (4,'RevenuesForTransactions', 'Payment', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (4,'RevenuesForTransactions', 'Payment', 'Cleared', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (4,'RevenuesForTransactions', 'Recurring Payment', 'Completed', 1)

--formula 5
INSERT INTO MP_PayPalAggregationFormula VALUES (5,'NetNumOfRefundsAndReturns', 'Payment', 'Returned', NULL)
INSERT INTO MP_PayPalAggregationFormula VALUES (5,'NetNumOfRefundsAndReturns', 'Payment', 'Refunded by eBay', NULL)
INSERT INTO MP_PayPalAggregationFormula VALUES (5,'NetNumOfRefundsAndReturns', 'Payment', 'Partially Refunded', NULL)
INSERT INTO MP_PayPalAggregationFormula VALUES (5,'NetNumOfRefundsAndReturns', 'Refund', 'Completed', NULL)
INSERT INTO MP_PayPalAggregationFormula VALUES (5,'NetNumOfRefundsAndReturns', 'Payment', 'Returned', NULL)

--formula 6
INSERT INTO MP_PayPalAggregationFormula VALUES (6,'TransferAndWireOut', 'Transfer', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (6,'TransferAndWireOut', 'Wire Transfer', 'Completed', 0)
INSERT INTO MP_PayPalAggregationFormula VALUES (6,'TransferAndWireOut', 'Debit Card ATM Withdrawal', 'Completed', 0)

--formula 7
INSERT INTO MP_PayPalAggregationFormula VALUES (7,'TransferAndWireIn', 'Transfer', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (7,'TransferAndWireIn', 'Wire Transfer', 'Completed', 1)
INSERT INTO MP_PayPalAggregationFormula VALUES (7,'TransferAndWireIn', 'Debit Card ATM Withdrawal', 'Completed', 1)

