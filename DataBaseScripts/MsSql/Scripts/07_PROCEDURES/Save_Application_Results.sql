IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Save_Application_Results]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Save_Application_Results]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Save_Application_Results]
  @pApplicationId bigint,
  @pName nvarchar(255),
  @pValue nvarchar(MAX),
  @pType nvarchar(20),
  @pDirection int
AS
BEGIN
    INSERT INTO Application_Result ([ApplicationId], [Name], [Value], [Type], [Direction])
    VALUES (@pApplicationId, @pName, @pValue, @pType, @pDirection)
END
GO
