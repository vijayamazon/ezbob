CREATE OR REPLACE PROCEDURE LockRelease
(
  pLockId number
)
AS
BEGIN

  DELETE FROM LockedObject
  WHERE Id = pLockId;
END;
/