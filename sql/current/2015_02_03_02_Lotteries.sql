UPDATE LotteryEnlistingTypes SET
	LotteryEnlistingType = 'MaxCount',
	Description = 'Has loans and total number of taken loans is less or equal to LoanCount'
WHERE
	LotteryEnlistingType = 'LoanCount'
GO

UPDATE LotteryEnlistingTypes SET
	LotteryEnlistingType = 'MaxCountOrMinAmount',
	Description = 'Has at least LoanCount loans or taken amount is at least LoanAmount'
WHERE
	LotteryEnlistingType = 'LoanOrAmount'
GO
