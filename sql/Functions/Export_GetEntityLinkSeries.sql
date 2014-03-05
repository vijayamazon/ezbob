IF OBJECT_ID (N'dbo.Export_GetEntityLinkSeries') IS NOT NULL
	DROP FUNCTION dbo.Export_GetEntityLinkSeries
GO

CREATE FUNCTION [dbo].[Export_GetEntityLinkSeries]
(	@pEntityLinkId int
)
RETURNS @ret TABLE
(	
	SeriaId nvarchar(max)
)
AS
BEGIN
DECLARE @iXml xml;
DECLARE @SeriaIds nvarchar(max);

	SELECT @iXml = (
	  SELECT CONVERT(NVARCHAR, eli.SeriaId, 15) + ';'
	  FROM 
		(SELECT DISTINCT el.SeriaId
		 FROM entitylink as el
		 WHERE el.EntityType='ExportTemplate'
		   AND el.EntityId = @pEntityLinkId) as eli
	  FOR XML PATH);
	
	SET @SeriaIds = (@iXml.value('.','nvarchar(max)'));
	
	INSERT INTO @ret(SeriaId)
	VALUES (LEFT(@SeriaIds, LEN(@SeriaIds) - 1));
	RETURN;	
END

GO

