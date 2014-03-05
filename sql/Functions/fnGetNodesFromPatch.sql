IF OBJECT_ID (N'dbo.fnGetNodesFromPatch') IS NOT NULL
	DROP FUNCTION dbo.fnGetNodesFromPatch
GO

CREATE FUNCTION [dbo].[fnGetNodesFromPatch]
RETURNS @retNodes TABLE
(	
	NodeID int IDENTITY(1,1) NOT NULL
  ,	Name nvarchar(255) NOT NULL
)
AS
BEGIN
	DECLARE 
			@nameNode nvarchar(255),
			@indexStart int,
			@indexFinish int;

	
	SET @nameNode = ''
	SET @indexStart = 1
	SET	@indexFinish = 0	
	
	-- delete last '/'
	SET @indexFinish = CHARINDEX('/', @Path, LEN(@Path) - 1)
	WHILE (@indexFinish <> 0)
	BEGIN
		SET @Path = SUBSTRING(@Path, 1, LEN(@Path) - 1) 
		SET @indexFinish = CHARINDEX('/', @Path, LEN(@Path) - 1)
	END	
	
	SET @indexFinish = CHARINDEX('/', @Path)
	
	-- delete first '/'
	WHILE(@indexFinish = 1)
	BEGIN
		SET @Path = SUBSTRING(@Path, @indexStart + 1, LEN(@Path) - 1)		
		SET @indexFinish = CHARINDEX('/', @Path, @indexStart)
	END;
	-- gets nodes
	WHILE (@indexStart <> 0)
	BEGIN
		IF (@indexFinish = 0)		
			SET @nameNode = SUBSTRING(@Path, @indexStart, LEN(@Path) - (@indexStart - 1))		
		ELSE
			SET @nameNode = SUBSTRING(@Path, @indexStart, @indexFinish - @indexStart)		
		
		IF (@indexFinish = 0)
			SET @indexStart = 0;
		ELSE
		BEGIN
			SET @indexStart = @indexFinish + 1
			SET @indexFinish = CHARINDEX('/', @Path, @indexStart)				
		END

		INSERT INTO @retNodes
		VALUES (@nameNode)		
	END;
	RETURN;	
END

GO

