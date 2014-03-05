IF OBJECT_ID (N'dbo.udfQuickOfferDuration') IS NOT NULL
	DROP FUNCTION dbo.udfQuickOfferDuration
GO

CREATE FUNCTION [dbo].[udfQuickOfferDuration]()
RETURNS INT
AS
BEGIN
	DECLARE @qodh INT

	SELECT
		@qodh = OfferDurationHours
	FROM
		QuickOfferConfiguration
	WHERE
		ID = 1

	RETURN @qodh
END

GO

