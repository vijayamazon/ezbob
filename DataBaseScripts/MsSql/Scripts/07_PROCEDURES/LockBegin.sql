IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LockBegin]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LockBegin]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE LockBegin
	@pLockType nvarchar(100),
	@pLockTimeout int
AS
BEGIN
  DECLARE @lLockId bigint,
  @lLastUpdateTime datetime,
  @lTimeoutPeriod int,
  @lLocked int;

  SELECT
    @lLockId = [Id],
    @lLastUpdateTime = [LastUpdateTime],
    @lTimeoutPeriod = [TimeoutPeriod]
  FROM LockedObject WITH (ROWLOCK)
  WHERE [Type] = @pLockType;

  SET @lLocked = -1;
  IF NOT(@lLockId is NULL)
    IF(cast(DATEDIFF(second, DATEADD(second, @lTimeoutPeriod, @lLastUpdateTime), GETDATE()) as bigint) <= 0)
      SET @lLocked = 1;
    ELSE
      SET @lLocked = 0;

  IF (@lLocked = 1)
    SELECT -1;
  ELSE
    BEGIN
      IF @lLocked = 0
        DELETE FROM [LockedObject]
        WHERE [Type] = @pLockType;

      INSERT INTO [LockedObject]
          ([Type]
          ,[TimeoutPeriod]
          ,[LastUpdateTime])
      VALUES
          (@pLockType
          ,@pLockTimeout
          ,GETDATE())
      SELECT @@IDENTITY;
    END;
END
GO
