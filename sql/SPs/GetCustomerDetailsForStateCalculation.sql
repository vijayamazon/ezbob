IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerDetailsForStateCalculation]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerDetailsForStateCalculation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerDetailsForStateCalculation] 
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
		CreditResult,
		Status,
		ApplyForLoan,
		ValidFor,
		@MinLoanAmount AS MinLoanAmount,
		CAST(CASE WHEN @NumOfLateLoans = 0 THEN 0 ELSE 1 END AS BIT) AS HasLateLoans,
		IsEnabled
	FROM
		Customer,
		CustomerStatuses
	WHERE
		Customer.Id = @CustomerId AND 
		Customer.CollectionStatus = CustomerStatuses.Id
END
GO
