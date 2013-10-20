SELECT DISTINCT
	t.Id AS ItemID,
	t.PostDate AS ItemDate,
	t.LoanId AS LoanID,
	t.LoanRepayment AS Principal,
	t.Interest AS Interest,
	t.Fees AS Fees,
	l.LoanTypeId AS LoanTypeID,
	l.AgreementModel,
	l.LoanAmount,
	lst.TransactionID AS ProcessedID
FROM
	LoanTransaction t
	INNER JOIN Loan l ON t.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	LEFT JOIN LoanScheduleTransaction lst ON t.Id = lst.TransactionID
WHERE
	t.Type = 'PaypointTransaction'
	AND
	t.Status = 'Done'
ORDER BY
	t.LoanId,
	t.PostDate
