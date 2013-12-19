IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetBasicCustomerData') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetBasicCustomerData
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetBasicCustomerData 
	(@CustomerId INT)
AS
BEGIN	
	DECLARE 
		@NumOfLoans INT

	SELECT 
		@NumOfLoans = count(1) 
	FROM 
		Loan 
	WHERE 
		CustomerId = @CustomerId
		
	SELECT
		FirstName,
		Surname,
		Fullname,
		Name AS Mail,
		IsOffline,
		@NumOfLoans AS NumOfLoans
	FROM
		Customer
	WHERE
		Id = @CustomerId
END
GO
