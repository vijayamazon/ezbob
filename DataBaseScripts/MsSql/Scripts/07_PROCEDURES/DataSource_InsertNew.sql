IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_InsertNew]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_InsertNew]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_InsertNew]
(
 @pDataSourceName nvarchar(max),
 @pDataSourceType nvarchar(max),
 @pDocument nvarchar(max),
 @pSignedData nvarchar(max),
 @pDisplayName nvarchar(max),
 @pDescription nvarchar(max),
 @pUserId int
)
AS
BEGIN

  DECLARE @id int;

  UPDATE DataSource_Sources
  SET [TerminationDate] = GETDATE()
  WHERE [TerminationDate] IS NULL and [DisplayName] = @pDisplayName;
  
  INSERT INTO DataSource_Sources
   ([Name],
    [Description],
    [Type],
    [Document],
    [SignedDocument],
    [CreationDate],
    [DisplayName],
    [UserId])
  values
   (@pDataSourceName,
    @pDescription,
    @pDataSourceType,
    @pDocument,
    @pSignedData,
    GETDATE(),
    @pDisplayName,
    @pUserId);

  set @id = @@IDENTITY;

  SELECT @id;
  RETURN @id;

END
GO
