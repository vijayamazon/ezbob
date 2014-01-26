IF OBJECT_ID('AV_IsCustomerOffline') IS NULL
	EXECUTE('CREATE PROCEDURE AV_IsCustomerOffline AS SELECT 1')
GO

ALTER PROCEDURE AV_IsCustomerOffline
@CustomerId INT
AS
BEGIN 
	SELECT (CASE IsOffline WHEN 1 THEN 'True' ELSE 'False' END) AS IsOffline FROM Customer WHERE Id=@CustomerId
END 

GO
