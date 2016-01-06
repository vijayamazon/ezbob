SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreateCampaignSourceRef') IS NULL
	EXECUTE('CREATE PROCEDURE CreateCampaignSourceRef AS SELECT 1')
GO

ALTER PROCEDURE CreateCampaignSourceRef
@CustomerID INT,
@FUrl NVARCHAR(255),
@FSource NVARCHAR(255),
@FMedium NVARCHAR(255),
@FTerm NVARCHAR(255),
@FContent NVARCHAR(255),
@FName NVARCHAR(255),
@FDate DATETIME,
@RUrl NVARCHAR(255),
@RSource NVARCHAR(255),
@RMedium NVARCHAR(255),
@RTerm NVARCHAR(255),
@RContent NVARCHAR(255),
@RName NVARCHAR(255),
@RDate DATETIME
AS
BEGIN
	UPDATE CampaignSourceRef SET
		FUrl = @FUrl,
		FSource = @FSource,
		FMedium = @FMedium,
		FTerm = @FTerm,
		FContent = @FContent,
		FName = @FName,
		FDate = @FDate,
		RUrl = @RUrl,
		RSource = @RSource,
		RMedium = @RMedium,
		RTerm = @RTerm,
		RContent = @RContent,
		RName = @RName,
		RDate = @RDate
	WHERE
		CustomerID = @CustomerID

	DECLARE @affected INT = @@ROWCOUNT

	IF @affected < 1
	BEGIN
		INSERT INTO CampaignSourceRef (
			CustomerID,
			FUrl, FSource, FMedium, FTerm, FContent, FName, FDate,
			RUrl, RSource, RMedium, RTerm, RContent, RName, RDate
		) VALUES (
			@CustomerID,
			@FUrl, @FSource, @FMedium, @FTerm, @FContent, @FName, @FDate,
			@RUrl, @RSource, @RMedium, @RTerm, @RContent, @RName, @RDate
		)
	END
END
GO
