IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_SaveKey]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_SaveKey]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_SaveKey] 
	(@pRequestId bigint,
	@pKeyNameId bigint,
	@pKeyValue nvarchar(max))
AS
BEGIN
	INSERT INTO [dbo].[DataSource_KeyData]
           ([RequestId],[KeyNameId],[Value])
     VALUES
           (@pRequestId, @pKeyNameId, @pKeyValue)
END
GO
