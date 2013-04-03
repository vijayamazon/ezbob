IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BehavioralReport_GetPath]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BehavioralReport_GetPath]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BehavioralReport_GetPath]
  @pId int
AS
BEGIN

  select Path 
  from BehavioralReports
  where id = @pId;

END
GO
