IF OBJECT_ID('LandRegistryLoad')IS NULL 
	EXECUTE('CREATE PROCEDURE LandRegistryLoad AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LandRegistryLoad
	@CustomerID INT 
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Id
		, CustomerId
		, InsertDate
		, Postcode
		, TitleNumber
		, RequestType
		, ResponseType
		, Request
		, Response
		, AttachmentPath
		, AddressId
	FROM 
		LandRegistry
	WHERE
		CustomerId = @CustomerID
END
GO
		
IF OBJECT_ID('LandRegistryOwnersLoad')IS NULL 
	EXECUTE('CREATE PROCEDURE LandRegistryOwnersLoad AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LandRegistryOwnersLoad
	@LandRegistryIDs IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SELECT
		Id
		, LandRegistryId
		, FirstName
		, LastName
		, CompanyName
		, CompanyRegistrationNumber
	FROM 
		LandRegistryOwner o
	INNER JOIN 
		@LandRegistryIDs l ON l.Value = o.LandRegistryId
END
GO
		
	