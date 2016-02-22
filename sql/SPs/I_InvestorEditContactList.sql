SET QUOTED_IDENTIFIER ON
GO



IF OBJECT_ID('I_InvestorEditContactList') IS NOT NULL
	DROP PROCEDURE I_InvestorEditContactList
GO

IF TYPE_ID('I_InvestorContactsList') IS NOT NULL
	DROP TYPE I_InvestorContactsList
GO


CREATE TYPE I_InvestorContactsList AS TABLE (
	[InvestorContactID] INT NOT NULL,
	[InvestorID] INT NOT NULL,
	[PersonalName] NVARCHAR(255) NULL,
	[LastName] NVARCHAR(255) NULL,
	[Email] NVARCHAR(255) NULL,
	[Role] NVARCHAR(255) NULL,
	[Comment] NVARCHAR(255) NULL,
	[IsPrimary] BIT NOT NULL,
	[Mobile] NVARCHAR(30) NULL,
	[OfficePhone] NVARCHAR(30) NULL,
	[IsActive] BIT NOT NULL,
	[IsGettingAlerts] BIT NOT NULL,
	[IsGettingReports] BIT NOT NULL,
	[Timestamp] DATETIME NOT NULL
)
GO



CREATE PROCEDURE I_InvestorEditContactList
@Tbl I_InvestorContactsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE 
		I_InvestorContact
	SET 
		I_InvestorContact.[InvestorID] = tbl.[InvestorID],
		I_InvestorContact.[IsActive] = tbl.[IsActive],
		I_InvestorContact.[IsPrimary] = tbl.[IsPrimary],
		I_InvestorContact.[IsGettingAlerts] = tbl.[IsGettingAlerts],
		I_InvestorContact.[IsGettingReports] = tbl.[IsGettingReports],
			I_InvestorContact.[Timestamp] = tbl.[Timestamp]

	FROM
		I_InvestorContact b 
	INNER JOIN 
		@Tbl tbl 
	ON
		b.InvestorContactID = tbl.InvestorContactID
	
		
END
GO

