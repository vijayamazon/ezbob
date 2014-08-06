IF OBJECT_ID('GetCustomerAddresses') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerAddresses AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerAddresses
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @IsOwnerOfMainAddress BIT
	SELECT 
		@IsOwnerOfMainAddress = cps.IsOwnerOfMainAddress 
	FROM 
		Customer c, 
		CustomerPropertyStatuses cps
	WHERE 
		c.PropertyStatusId = cps.Id AND 
		c.Id = @CustomerId

	SELECT
		ca.addressId AS AddressId,
		ca.Line1,
		ca.Line2,
		ca.Line3,
		ca.Town AS City,
		ca.County,
		ca.Postcode AS PostCode
	FROM
		CustomerAddress ca
	WHERE
		ca.CustomerId = @CustomerId AND
		ca.Line1 IS NOT NULL AND
		((ca.addressType = 1 AND @IsOwnerOfMainAddress = 1) OR ca.addressType = 11)
END
GO
