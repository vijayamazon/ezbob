IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteSignalById]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteSignalById]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteSignalById] 
	(@pId bigint)
AS
BEGIN
	delete from Signal WITH(ROWLOCK)
    where Id = @pId
END
GO
