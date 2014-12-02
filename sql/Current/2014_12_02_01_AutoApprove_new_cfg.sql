SET QUOTED_IDENTIFIER ON
GO

SELECT
	Name,
	Value,
	Description
INTO
	#a
FROM
	ConfigurationVariables
WHERE
	1 = 0

INSERT INTO #a(Name, Value, Description) VALUES
	('AutoApproveOnlineTurnoverAge', '3', 'Online marketplace data should be at least this number of months old.'),
	('AutoApproveOnlineTurnoverDropQuarterRatio', '0.75', 'Last quarter turnover (annualized) of online marketplace should be at least this percent of annual turnover.'),
	('AutoApproveOnlineTurnoverDropMonthRatio', '0.75', 'Last month turnover (annualized) of online marketplace should be at least this percent of annual turnover.'),
	('AutoApproveHmrcTurnoverAge', '3', 'HMRC marketplace data should be at least this number of months old.'),
	('AutoApproveHmrcTurnoverDropQuarterRatio', '0.75', 'Last quarter turnover (annualized) of HMRC marketplace should be at least this percent of annual turnover.'),
	('AutoApproveHmrcTurnoverDropHalfYearRatio', '0.75', 'Last half year turnover (annualized) of HMRC marketplace should be at least this percent of annual turnover.'),
	('AutoRejectConsumerCheckAge', '1', 'Customer will not be auto rejected if Experian consumer check happned this number of month ago or earlier.')

INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted)
SELECT
	#a.Name,
	#a.Value,
	#a.Description,
	0
FROM
	#a
	LEFT JOIN ConfigurationVariables v ON #a.Name = v.Name
WHERE
	v.Id IS NULL

DROP TABLE #a
GO
