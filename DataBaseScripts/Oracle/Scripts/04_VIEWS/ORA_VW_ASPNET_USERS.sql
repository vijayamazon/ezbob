CREATE OR REPLACE VIEW ORA_VW_ASPNET_USERS AS
SELECT ApplicationId,
       UserId,
       UserName,
       LoweredUserName,
       MobileAlias,
       IsAnonymous,
       LastActivityDate
FROM ora_aspnet_Users
WITH READ ONLY

/
