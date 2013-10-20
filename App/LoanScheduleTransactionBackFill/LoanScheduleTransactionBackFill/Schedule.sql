SELECT DISTINCT
	s.Id AS ItemID,
	s.Date AS ItemDate,
	s.LoanId AS LoanID,
	s.LoanRepayment AS Principal
FROM
	LoanSchedule s
	INNER JOIN Loan l ON s.LoanId = l.Id
	INNER JOIN LoanTransaction t ON l.Id = t.LoanId
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	LEFT JOIN LoanScheduleTransaction lst ON t.Id = lst.TransactionID
WHERE
	t.Type = 'PaypointTransaction'
	AND
	t.Status = 'Done'
	AND
	(
		t.PostDate < 'July 28 2013 06:54:06.807'
		OR
		t.PostDate BETWEEN 'August 31 2013 23:20:43.047' AND 'September 15 2013 16:41:59.737'
	)
	AND
	lst.Id IS NULL
ORDER BY
	s.LoanId,
	s.Date