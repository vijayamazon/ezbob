execute immediate 'ALTER TABLE Security_Application ADD Description VARCHAR2(255) NULL';
delete from APPLICATION_HISTORY where SecurityApplicationId in (1001, 1002, 1003, 1004, 1005);
delete from SECURITY_SESSION where AppId in (1001, 1002, 1003, 1004, 1005);
delete from Security_Application where ApplicationId in (1001, 1002, 1003, 1004, 1005);

UPDATE Security_Application
   SET Name = 'ScortoCoreWeb'
 WHERE ApplicationId = 1;
 
 UPDATE Security_Application
   SET Name = 'CreditInspector'
 WHERE ApplicationId = 6;
 
 UPDATE Security_Application
   SET ApplicationType = 0
 WHERE NAME = 'SE (Autonodes)';