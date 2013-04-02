CREATE OR REPLACE VIEW ORA_VW_ASPNET_APPLICATIONS AS
SELECT ApplicationName,
       LoweredApplicationName,
       ApplicationId,
       Description
FROM ora_aspnet_Applications
WITH READ ONLY

/
