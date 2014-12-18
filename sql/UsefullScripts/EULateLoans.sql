SELECT x.CustomerId, x.CustomerRefNumber, 
	   x.loanId, x.LoanRefNumber, CAST(x.LoanDate AS DATE) LoanDate, x.LoanAmount, x.Principal,
	   min(CAST(x.ScheduleDate AS DATE)) LateSince, datediff(day, min(x.ScheduleDate), getdate()) LateDays, 
	   sum(x.AmountDue-x.LoanRepayment) InterestDue
FROM
(
	SELECT c.Id CustomerId, c.RefNumber CustomerRefNumber,
	 l.Id loanId, l.[Date] LoanDate, l.RefNum LoanRefNumber, l.LoanAmount, l.Principal,
	 ls.[Date] ScheduleDate, ls.AmountDue, ls.LoanRepayment, ls.Interest
	FROM Customer c
	 INNER JOIN Loan l ON c.Id=l.CustomerId
	 INNER JOIN LoanSource s ON s.LoanSourceID = l.LoanSourceID
	 INNER JOIN LoanSchedule ls ON ls.LoanId = l.Id
	 WHERE c.IsTest=0 
	 AND s.LoanSourceName='EU'
	 AND ls.[Date]<getutcdate()
	 AND (ls.Status='Late' OR ls.Status='StillToPay')
	 --AND l.[Date]>='2014-10-01'
)x
GROUP BY x.CustomerId, x.CustomerRefNumber, x.loanId, x.LoanDate, x.LoanRefNumber, x.LoanAmount, x.Principal
ORDER BY x.LoanDate