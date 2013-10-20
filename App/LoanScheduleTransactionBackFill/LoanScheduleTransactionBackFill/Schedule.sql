SELECT DISTINCT
	s.Id AS ItemID,
	s.Date AS ItemDate,
	s.LoanId AS LoanID,
	s.LoanRepayment AS Principal,
	lss.ScheduleID AS ProcessedID
FROM
	LoanSchedule s
	INNER JOIN Loan l ON s.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	LEFT JOIN LoanScheduleTransaction lss ON s.Id = lss.ScheduleID
ORDER BY
	s.LoanId,
	s.Date