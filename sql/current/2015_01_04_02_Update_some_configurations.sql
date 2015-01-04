UPDATE ConfigurationVariables SET
	Value = '0,1,2,3,U'
WHERE
	Name = 'AutoApproveAllowedCaisStatusesWithLoan'
GO

UPDATE ConfigurationVariables SET
	Value = '0,1,2,U'
WHERE
	Name = 'AutoApproveAllowedCaisStatusesWithoutLoan'
GO
