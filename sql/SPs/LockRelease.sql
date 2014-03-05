IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LockRelease]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[LockRelease]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LockRelease] 
	(@pLockId BIGINT)
AS
BEGIN
	DELETE FROM [LockedObject]
  WHERE [Id] = @pLockId;
END
GO
