IF OBJECT_ID ('dbo.PostcodeNuts') IS NULL
BEGIN	
	CREATE TABLE dbo.PostcodeNuts
		(
		  PostcodeNutsID                 INT IDENTITY NOT NULL
		, Postcode                       NVARCHAR (10)
		, NutsCode                       NVARCHAR (200)
		, Nuts                           NVARCHAR (200)
		, Quality                        INT
		, Eastings						 BIGINT
		, Northings						 BIGINT	
		, Country                        NVARCHAR (200)
		, Nhs_ha                         NVARCHAR (200)
		, Longitude                      DECIMAL(9,6)
		, Latitude                       DECIMAL(8,6)
		, ParliamentaryConstituency      NVARCHAR (200)
		, EuropeanElectoralRegion        NVARCHAR (200)
		, PrimaryCareTrust               NVARCHAR (200)
	    , Region                         NVARCHAR (200)
	    , Lsoa                           NVARCHAR (200)
	    , Msoa                           NVARCHAR (200)
	    , Incode                         NVARCHAR (10)
	    , Outcode                        NVARCHAR (10)
	    , AdminDistrict                  NVARCHAR (200)
	    , AdminDistrictCode              NVARCHAR (50)
	    , Parish                         NVARCHAR (200)
	    , ParishCode                     NVARCHAR (50)
	    , AdminCounty                    NVARCHAR (200)
	    , AdminCountyCode                NVARCHAR (50)
	    , AdminWard                      NVARCHAR (200)
	    , AdminWardCode                  NVARCHAR (50)
	    , Ccg                            NVARCHAR (200)
	    , CcgCode                        NVARCHAR (50)
	    , Status                         INT
	    , CONSTRAINT PK_PostcodeNuts PRIMARY KEY (PostcodeNutsID)
		)

	CREATE INDEX IX_PostcodeNuts_Postcode ON dbo.PostcodeNuts (Postcode)
END	
GO

