﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LockRelease]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LockRelease]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE LockRelease
	@pLockId BIGINT
AS
BEGIN
  DELETE FROM [LockedObject]
  WHERE [Id] = @pLockId;
END
GO
