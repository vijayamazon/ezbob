CREATE OR REPLACE FUNCTION LockCheckStatus
(
  pLockId   NUMBER
)
RETURN NUMBER
as
  lLockId NUMBER;
begin
  SELECT Id
  INTO  lLockId
  FROM LockedObject
  WHERE Id = pLockId;

  IF (lLockId is NULL) THEN
    lLockId := 0;
  ELSE
    BEGIN
      UPDATE LockedObject
      SET LastUpdateTime = sysdate
      WHERE Id = pLockId;

      lLockId := 1;
    END;
  END IF;

return lLockId;

end;
/