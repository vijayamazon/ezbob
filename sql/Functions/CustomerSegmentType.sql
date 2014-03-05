IF OBJECT_ID (N'dbo.CustomerSegmentType') IS NOT NULL
	DROP FUNCTION dbo.CustomerSegmentType
GO

CREATE FUNCTION [dbo].[CustomerSegmentType]
(	@IsOffline BIT
)
RETURNS NVARCHAR(20)
AS
BEGIN
	IF @IsOffline IS NULL
		RETURN 'Unknown'

	RETURN CASE @IsOffline
		WHEN 0 THEN 'Online'
		WHEN 1 THEN 'Offline'
		ELSE 'Unexpected'
	END
END

GO

