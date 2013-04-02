CREATE OR REPLACE VIEW ORA_VW_ASPNET_SESSIONS AS
SELECT SessionId, Created, Expires, LockDate, LockDateLocal, LockCookie, Timeout,
        Locked, LENGTH(SessionItemShort) + LENGTH(SessionItemLong) DataSize, Flags
  FROM ora_aspnet_Sessions
   WITH READ ONLY
   
   /
