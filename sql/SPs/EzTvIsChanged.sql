IF OBJECT_ID('EzTvIsChanged') IS NULL
	EXECUTE('CREATE PROCEDURE EzTvIsChanged AS SELECT 1')
GO

IF(SELECT Value FROM ConfigurationVariables WHERE Name='Environment')='Prod'
EXEC('ALTER PROCEDURE EzTvIsChanged
   	  AS 
	  BEGIN 
		SELECT   OBJECT_NAME(OBJECT_ID) AS ''Table'', 
		         LAST_VALUE AS ''Val''
		FROM     SYS.IDENTITY_COLUMNS 
		WHERE OBJECT_ID IN (93959411,1618104805,1845581613,1283847986, 1225159510)
	  END')
ELSE
EXEC('ALTER PROCEDURE EzTvIsChanged
	  AS 
	  BEGIN
		SELECT   OBJECT_NAME(OBJECT_ID) AS ''Table'', 
		         LAST_VALUE AS ''Val''
		FROM     SYS.IDENTITY_COLUMNS 
		WHERE OBJECT_NAME(OBJECT_ID) IN (''Loan'', ''Security_User'', ''CashRequests'', ''Broker'', ''LoanTransaction'')
	  END')
GO
