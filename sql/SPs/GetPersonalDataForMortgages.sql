IF OBJECT_ID('GetPersonalDataForMortgages') IS NULL
	EXECUTE('CREATE PROCEDURE GetPersonalDataForMortgages AS SELECT 1')
GO

ALTER PROCEDURE GetPersonalDataForMortgages
@CustomerId INT
AS
BEGIN
	DECLARE
		@FirstName NVARCHAR(250),
		@Surname NVARCHAR(250),
		@Gender CHAR(1),
		@DateOfBirth DATETIME,
		@Line1 VARCHAR(200),
		@Line2 VARCHAR(200),
		@Line3 VARCHAR(200),
		@Town VARCHAR(200),
		@County VARCHAR(200),
		@Postcode VARCHAR(200)

	SELECT
		@FirstName = FirstName,
		@Surname = Surname,
		@Gender = Gender,
		@DateOfBirth = DateOfBirth
	FROM 
		Customer 
	WHERE 
		Id = @CustomerId

	SELECT
		@Line1 = Line1,
		@Line2 = Line2,
		@Line3 = Line3,
		@Town = Town,
		@County = County,
		@Postcode = Postcode
	FROM
		CustomerAddress
	WHERE
		CustomerId = @CustomerId AND
		addressType = 1	
			
	SELECT
		@FirstName AS FirstName,
		@Surname AS Surname,
		@Gender AS Gender,
		@DateOfBirth AS DateOfBirth,
		@Line1 AS Line1,
		@Line2 AS Line2,
		@Line3 AS Line3,
		@Town AS Town,
		@County AS County,
		@Postcode AS Postcode
END
GO
