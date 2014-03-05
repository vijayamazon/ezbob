IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_SaveRequest]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_SaveRequest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_SaveRequest] 
	(@pAppId bigint,
  @pRequest nvarchar(max),
  @pReqId bigint out)
AS
BEGIN
	INSERT INTO [dbo].[DataSource_Requests]
           ([ApplicationId],[Request])
     VALUES (@pAppId, @pRequest);
     SET @pReqId = @@IDENTITY;
END
GO
