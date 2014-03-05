IF OBJECT_ID (N'dbo.GetNumOfDefaultAccounts') IS NOT NULL
	DROP FUNCTION dbo.GetNumOfDefaultAccounts
GO

CREATE FUNCTION [dbo].[GetNumOfDefaultAccounts]
(	@CustomerId INT, @Months INT, @Amount int
)
RETURNS TABLE 
AS
RETURN 
(
	select COUNT(eda.Id) as NumOfDefaultAccounts
	FROM [ExperianDefaultAccount] eda
	where eda.CustomerId = @CustomerId and
	eda.date > dateadd(MM, -@Months, getdate()) and Balance > @Amount
	and eda.[DateAdded] = (select max(eda1.DateAdded) as maxdate FROM [ExperianDefaultAccount] eda1
								  where eda1.CustomerId = @CustomerId 
					       )
)

GO

