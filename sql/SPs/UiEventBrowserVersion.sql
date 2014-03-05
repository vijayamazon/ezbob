IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UiEventBrowserVersion]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UiEventBrowserVersion]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UiEventBrowserVersion] 
	(@Version NVARCHAR(1024))
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @VersionID INT

	SELECT
		@VersionID = BrowserVersionID
	FROM
		BrowserVersions
	WHERE
		BrowserVersion = @Version

	IF @VersionID IS NULL
	BEGIN
		INSERT INTO BrowserVersions(BrowserVersion)
			VALUES (@Version)

		SET @VersionID = SCOPE_IDENTITY()
	END

	SELECT @VersionID AS BrowserVersionID
END
GO
