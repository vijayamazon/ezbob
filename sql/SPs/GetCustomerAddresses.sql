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

	DECLARE
		@Line1 VARCHAR(255), 
		@Line2 VARCHAR(255),
		@Line3 VARCHAR(255),
		@Line4 VARCHAR(255),
		@Line5 VARCHAR(255),
		@Line6 VARCHAR(255),
		@Line1Prev VARCHAR(255), 
		@Line2Prev VARCHAR(255),
		@Line3Prev VARCHAR(255),
		@Line4Prev VARCHAR(255),
		@Line5Prev VARCHAR(255),
		@Line6Prev VARCHAR(255)

	SELECT
		@line1 = ca.Line1,
		@Line2 = ca.Line2,
		@Line3 = ca.Line3,
		@Line4 = ca.Town,
		@Line5 = ca.County,
		@Line6 = ca.Postcode
	FROM
		CustomerAddress ca
	WHERE
		ca.addressType = '1'
		AND 
		ca.CustomerId = @CustomerId
		AND 
		ca.Line1 IS NOT NULL

	SELECT
		@Line1Prev = ca.Line1,
		@Line2Prev = ca.Line2, 
		@Line3Prev = ca.Line3,
		@Line4Prev = ca.Town, 
		@Line5Prev = ca.County,
		@Line6Prev = ca.Postcode
	FROM
		CustomerAddress ca
	WHERE 
		ca.addressType = '2'
		AND
		ca.CustomerId = @CustomerId
		AND
		ca.Line1 IS NOT NULL

	SELECT
		@Line1 AS Line1, 
		@Line2 AS Line2,
		@Line3 AS Line3,
		@Line4 AS Line4,
		@Line5 AS Line5,
		@Line6 AS Line6,
		@Line1Prev AS Line1Prev, 
		@Line2Prev AS Line2Prev,
		@Line3Prev AS Line3Prev,
		@Line4Prev AS Line4Prev,
		@Line5Prev AS Line5Prev,
		@Line6Prev AS Line6Prev      
END
GO
