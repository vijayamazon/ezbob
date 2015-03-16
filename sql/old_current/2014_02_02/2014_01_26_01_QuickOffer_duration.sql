IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'QuickOfferDurationHours')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('QuickOfferDurationHours', '24', 'Integer. Quick offer is valid for this number of hours since its issue.')
END
GO

IF OBJECT_ID('dbo.udfQuickOfferDuration') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfQuickOfferDuration() RETURNS INT AS BEGIN RETURN 24 END')
GO

ALTER FUNCTION dbo.udfQuickOfferDuration()
RETURNS INT
AS
BEGIN
	DECLARE @qodh INT
	DECLARE @default_qodh INT = 24

	SELECT
		@qodh = CASE
			WHEN ISNUMERIC(Value) = 1 THEN CONVERT(INT, Value)
			ELSE NULL
		END
	FROM
		ConfigurationVariables
	WHERE
		Name = 'QuickOfferDurationHours'

	SELECT @qodh = ISNULL(@qodh, @default_qodh)

	IF @qodh < 0
		SELECT @qodh = @default_qodh

	RETURN @qodh
END
GO
