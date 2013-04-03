IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnGetNodesFromPatch]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnGetNodesFromPatch]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Script Date: 23.10.2007
-- IN: @Path - xml path, for exampl: application/sate
-- OUT: return table consist rrom Nodes xml
CREATE FUNCTION [dbo].[fnGetNodesFromPatch](@Path nvarchar(1000))
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
