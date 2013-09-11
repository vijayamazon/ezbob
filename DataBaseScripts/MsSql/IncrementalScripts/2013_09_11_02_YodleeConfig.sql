IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'MaxYodleeOtherCategoryAmount')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
	VALUES(
		'MaxYodleeOtherCategoryAmount',
		'500',
		'Set the max sum for yodlee category total amount to calculate as other category in Yodlee details cash flow report in Underwriter'
	)
GO
