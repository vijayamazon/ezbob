IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_InsertKeyName]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_InsertKeyName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_InsertKeyName] 
	(@pKeyName nvarchar(max))
AS
BEGIN
	IF NOT EXISTS(SELECT KeyNameId FROM dbo.DataSource_KeyFields
  WHERE KeyName = @pKeyName)
  BEGIN
	INSERT INTO [dbo].[DataSource_KeyFields]([KeyName])
     VALUES(@pKeyName)
  END
END
GO
