IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LockCheckStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LockCheckStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE LockCheckStatus
	@pLockId BIGINT
AS
BEGIN
  DECLARE @lLockId bigint;

  SELECT
    @lLockId = [Id]
  FROM LockedObject WITH (ROWLOCK)
  WHERE [Id] = @pLockId;

  IF @lLockId is NULL
    SELECT 0;
  ELSE
    BEGIN
      UPDATE [LockedObject] WITH (ROWLOCK)
      SET [LastUpdateTime] = GETDATE()
      WHERE [Id] = @pLockId;

      SELECT 1;
    END;
END
GO
