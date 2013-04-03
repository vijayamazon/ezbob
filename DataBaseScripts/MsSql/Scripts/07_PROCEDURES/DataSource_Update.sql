IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_Update]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_Update]
(
  @pId bigint,
  @pDescription nvarchar(max),
  @pUserId int,
  @pSignedData nvarchar(max),
  @pDocument nvarchar(max)
)
AS
BEGIN

  UPDATE DataSource_Sources
  SET
    [Description] = @pDescription,
    [UserId] = @pUserId,
    [Document] = @pDocument,
    [SIGNEDDOCUMENT] = @pSignedData
  WHERE [Id] = @pId;

END
GO
