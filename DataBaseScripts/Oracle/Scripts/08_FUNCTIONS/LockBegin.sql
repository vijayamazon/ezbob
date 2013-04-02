CREATE OR REPLACE FUNCTION LockBegin
  (
    pLockType in varchar2,
    pLockTimeout in number
  ) return number
AS
  lLockId NUMBER;
  lLastUpdateTime DATE;
  lTimeoutPeriod NUMBER;
  lLocked NUMBER;

  l_Result NUMBER;

BEGIN
  lLocked := -1;

  BEGIN
    SELECT Id, LastUpdateTime, TimeoutPeriod
      INTO lLockId, lLastUpdateTime, lTimeoutPeriod
    FROM LockedObject
    WHERE Type = pLockType;
  exception when others then 
    lLockId := NULL; 
  END;

  IF NOT(lLockId is NULL) THEN
    IF((lLastUpdateTime + lTimeoutPeriod/86400) > sysdate) THEN
      lLocked := 1;
    ELSE
      lLocked := 0;
    END IF;
  END IF;

  IF (lLocked = 1) THEN
    l_Result := -1;
  ELSE
    BEGIN

      IF (lLocked = 0) THEN
        DELETE FROM LockedObject
        WHERE Type = pLockType;
      END IF;

      SELECT SEQ_LockedObject.Nextval into l_Result from dual;

      INSERT INTO LockedObject
          (Id
          ,Type
          ,TimeoutPeriod
          ,LastUpdateTime)
      VALUES
          (l_Result
          ,pLockType
          ,pLockTimeout
          ,sysdate);
    END;
  END IF;
  RETURN l_Result;
END;
/
