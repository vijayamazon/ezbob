SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('tempdb..#cv') IS NOT NULL 
BEGIN
	DROP TABLE #cv
END 

SELECT
	Name,
	Value
INTO
	#cv
FROM
	ConfigurationVariables
WHERE
	1 = 0
GO

INSERT INTO #cv (Name, Value) VALUES
	('BrokerCommissionHasLoan', '0.01'),
	('EzbobCommissionHasLoan', '0.05'),
	('BrokerCommissionBigLoan', '0.05'),
	('BrokerCommissionBigLoanRest', '0.025'),
	('EzbobCommissionBigLoan', '0.02'),
	('BrokerCommissionMediumLoan', '0.05'),
	('EzbobCommissionMediumLoan', '0.015'),
	('BrokerCommissionSmallLoan', '0.05'),
	('EzbobCommissionSmallLoan', '0.02'),
	('MonthsSinceFirstLoan', '3'),
	('BigLoanAmount', '50000'),
	('MediumLoanAmount', '35000')
GO

INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted)
SELECT
	n.Name,
	n.Value,
	n.Name,
	0
FROM
	#cv n
	LEFT JOIN ConfigurationVariables c ON n.Name = c.Name
	WHERE c.Id IS NULL

IF OBJECT_ID('tempdb..#cv') IS NOT NULL 
BEGIN
	DROP TABLE #cv
END 
GO

