IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayUserAddressData]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayUserAddressData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserAddressData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AddressID] [nvarchar](256) NULL,
	[AddressOwner] [nvarchar](256) NULL,
	[AddressRecordType] [nvarchar](256) NULL,
	[AddressStatus] [nvarchar](256) NULL,
	[AddressUsage] [nvarchar](256) NULL,
	[CityName] [nvarchar](256) NULL,
	[CompanyName] [nvarchar](256) NULL,
	[CountryCode] [nvarchar](256) NULL,
	[CountryName] [nvarchar](256) NULL,
	[County] [nvarchar](256) NULL,
	[ExternalAddressID] [nvarchar](256) NULL,
	[FirstName] [nvarchar](256) NULL,
	[InternationalName] [nvarchar](256) NULL,
	[InternationalStateAndCity] [nvarchar](256) NULL,
	[InternationalStreet] [nvarchar](256) NULL,
	[LastName] [nvarchar](256) NULL,
	[Name] [nvarchar](256) NULL,
	[Phone] [nvarchar](256) NULL,
	[Phone2] [nvarchar](256) NULL,
	[Phone2AreaOrCityCode] [nvarchar](256) NULL,
	[Phone2CountryCode] [nvarchar](256) NULL,
	[Phone2CountryPrefix] [nvarchar](256) NULL,
	[Phone2LocalNumber] [nvarchar](256) NULL,
	[PhoneAreaOrCityCode] [nvarchar](256) NULL,
	[PhoneCountryCode] [nvarchar](256) NULL,
	[PhoneCountryCodePrefix] [nvarchar](256) NULL,
	[PhoneLocalNumber] [nvarchar](256) NULL,
	[PostalCode] [nvarchar](256) NULL,
	[StateOrProvince] [nvarchar](256) NULL,
	[Street] [nvarchar](256) NULL,
	[Street1] [nvarchar](256) NULL,
	[Street2] [nvarchar](256) NULL,
 CONSTRAINT [PK_MP_EbayUserAddressData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
