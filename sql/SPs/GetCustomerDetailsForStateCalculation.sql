IF OBJECT_ID('GetCustomerDetailsForStateCalculation') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerDetailsForStateCalculation AS SELECT 1')
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetCustomerDetailsForStateCalculation] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE
		@MinLoanAmount INT,
		@NumOfLateLoans INT
		
	SELECT 
		@NumOfLateLoans = COUNT(1)
	FROM
		Loan
	WHERE
		CustomerId = @CustomerId AND
		Status = 'Late'
	
	SELECT 
		@MinLoanAmount = CONVERT(INT, Value) 
	FROM 
		ConfigurationVariables 
	WHERE 
		Name = 'MinLoanAmount'
	
	SELECT
		c.CreditResult,
		c.Status,
		c.ApplyForLoan,
		c.ValidFor,
		@MinLoanAmount AS MinLoanAmount,
		CAST(CASE WHEN @NumOfLateLoans = 0 THEN 0 ELSE 1 END AS BIT) AS HasLateLoans,
		cs.IsEnabled, 
		c.BlockTakingLoan
	FROM
		Customer c 
	INNER JOIN 
		CustomerStatuses cs 
	ON 
		c.CollectionStatus = cs.Id
	WHERE
		c.Id = @CustomerId

END
GO
