IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_SaveResponse]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_SaveResponse]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_SaveResponse] 
	(@pReqId bigint,
	@pResponse nvarchar(max))
AS
BEGIN
	INSERT INTO [dbo].[DataSource_Responses]
           ([RequestId],[Response])
     VALUES
           (@pReqId, @pResponse);
END
GO
