SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadFullCompaniesHouse') IS NULL
	EXECUTE('CREATE PROCEDURE LoadFullCompaniesHouse AS SELECT 1')
GO

ALTER PROCEDURE LoadFullCompaniesHouse
	@CompanyRefNum NVARCHAR(20)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CompaniesHouseOfficerOrderID INT = (SELECT TOP 1 o.CompaniesHouseOfficerOrderID FROM CompaniesHouseOfficerOrder o WHERE o.CompanyRefNum = @CompanyRefNum ORDER BY o.CompaniesHouseOfficerOrderID DESC)
	
	IF (@CompaniesHouseOfficerOrderID IS NULL)
	BEGIN
		SELECT 'CompaniesHouseOfficerOrder' AS DatumType, 0 AS CompaniesHouseOfficerOrderID
	END
	ELSE
	BEGIN
		-- officers order
		
		SELECT 
			'CompaniesHouseOfficerOrder' AS DatumType, 
			[CompaniesHouseOfficerOrderID],
			[CompanyRefNum],
			[Timestamp],
			[ActiveCount],
			[Etag],
			[ItemsPerPage],
			[Kind],
			[Link],
			[ResignedCount],
			[StartIndex],
			[TotalResults],
			[Error]
		FROM
			CompaniesHouseOfficerOrder
		WHERE
			CompaniesHouseOfficerOrderID = @CompaniesHouseOfficerOrderID 
		
		-- officers items
		SELECT
			'CompaniesHouseOfficerOrderItem' AS DatumType,
			[CompaniesHouseOfficerOrderItemID], 
			[CompaniesHouseOfficerOrderID],
			[AddressLine1],
			[AddressLine2],
			[CareOf],
			[Country],
			[Locality],
			[PoBox],
			[Postcode],
			[Premises],
			[Region],
			[AppointedOn],
			[CountryOfResidence],
			[DobDay],
			[DobMonth],
			[DobYear],
			[Link],
			[Name],
			[Nationality],
			[Occupation],
			[OfficerRole],
			[ResignedOn]
		FROM
			CompaniesHouseOfficerOrderItem
		WHERE
		   CompaniesHouseOfficerOrderID = @CompaniesHouseOfficerOrderID	  
		   	
		-- appointment order		
		SELECT
			'CompaniesHouseOfficerAppointmentOrder' AS DatumType, 
			o.[CompaniesHouseOfficerAppointmentOrderID],
			o.[CompaniesHouseOfficerOrderItemID],
			o.[DobDay],
			o.[DobMonth],
			o.[DobYear],
			o.[Etag],
			o.[IsCorporateOfficer],
			o.[ItemsPerPage],
			o.[Kind],
			o.[Link],
			o.[Name],
			o.[StartIndex],
			o.[TotalResults]
		FROM
			CompaniesHouseOfficerAppointmentOrder o 
		INNER JOIN 
			CompaniesHouseOfficerOrderItem oi ON oi.CompaniesHouseOfficerOrderItemID = o.CompaniesHouseOfficerOrderItemID	
		WHERE
			oi.CompaniesHouseOfficerOrderID = @CompaniesHouseOfficerOrderID
		
		-- appointments order items
		
		SELECT
			'CompaniesHouseOfficerAppointmentOrderItem' AS DatumType, 
			i.[CompaniesHouseOfficerAppointmetOrderItemID],
			i.[CompaniesHouseOfficerAppointmentOrderID],
			i.[AddressLine1],
			i.[AddressLine2],
			i.[CareOf],
			i.[Country],
			i.[Locality],
			i.[PoBox],
			i.[Postcode],
			i.[Premises],
			i.[Region],
			i.[AppointedBefore],
			i.[AppointedOn],
			i.[CompanyName],
			i.[CompanyNumber],
			i.[CompanyStatus],
			i.[CountryOfResidence],
			i.[IdentificationType],
			i.[LegalAuthority],
			i.[LegalForm],
			i.[PlaceRegistered],
			i.[RegistrationNumber],
			i.[IsPre1992Appointment],
			i.[Link],
			i.[Name],
			i.[Forename],
			i.[Honours],
			i.[OtherForenames],
			i.[Surname],
			i.[Title],
			i.[Nationality],
			i.[Occupation],
			i.[OfficerRole],
			i.[ResignedOn]
		FROM
			CompaniesHouseOfficerAppointmentOrderItem i 
		INNER JOIN 
			CompaniesHouseOfficerAppointmentOrder o ON o.CompaniesHouseOfficerAppointmentOrderID = i.CompaniesHouseOfficerAppointmentOrderID
		INNER JOIN 
			CompaniesHouseOfficerOrderItem oi ON oi.CompaniesHouseOfficerOrderItemID = o.CompaniesHouseOfficerOrderItemID  
		WHERE 
			oi.CompaniesHouseOfficerOrderID = @CompaniesHouseOfficerOrderID
							
		END
END

GO

