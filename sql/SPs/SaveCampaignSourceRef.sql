SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCampaignSourceRef') IS NOT NULL
	DROP PROCEDURE SaveCampaignSourceRef
GO

IF TYPE_ID('CampaignSourceRefList') IS NOT NULL
	DROP TYPE CampaignSourceRefList
GO

CREATE TYPE CampaignSourceRefList AS TABLE (
	FUlr NVARCHAR(255) NULL,
	FSource NVARCHAR(255) NULL,
	FMedium NVARCHAR(255) NULL,
	FTerm NVARCHAR(255) NULL,
	FContent NVARCHAR(255) NULL,
	FName NVARCHAR(255) NULL,
	FDate DATETIME NULL,
	RUlr NVARCHAR(255) NULL,
	RSource NVARCHAR(255) NULL,
	RMedium NVARCHAR(255) NULL,
	RTerm NVARCHAR(255) NULL,
	RContent NVARCHAR(255) NULL,
	RName NVARCHAR(255) NULL,
	RDate DATETIME NULL
)
GO

CREATE PROCEDURE SaveCampaignSourceRef
@CustomerId INT,
@Tbl CampaignSourceRefList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CampaignSourceRef (
		CustomerId,
		FUlr,
		FSource,
		FMedium,
		FTerm,
		FContent,
		FName,
		FDate,
		RUlr,
		RSource,
		RMedium,
		RTerm,
		RContent,
		RName,
		RDate
	) SELECT
		@CustomerId,
		FUlr,
		FSource,
		FMedium,
		FTerm,
		FContent,
		FName,
		FDate,
		RUlr,
		RSource,
		RMedium,
		RTerm,
		RContent,
		RName,
		RDate
	FROM @Tbl
END
GO
