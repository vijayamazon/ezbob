IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_DeleteNodeLinks]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_DeleteNodeLinks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_DeleteNodeLinks]
  (
    @pNodeId int
  )
AS
BEGIN
   delete from export_templatenoderel 
   where nodeid = @pNodeId;
END;
GO
