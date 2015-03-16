IF OBJECT_ID('UiEventBrowserVersion') IS NULL
	EXECUTE('CREATE PROCEDURE UiEventBrowserVersion AS SELECT 1')
GO

ALTER PROCEDURE UiEventBrowserVersion
@Version NVARCHAR(1024)
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
