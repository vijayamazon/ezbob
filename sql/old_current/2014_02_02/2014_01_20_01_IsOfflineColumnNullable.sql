
IF OBJECT_ID('DF_Customer_IsOffline') IS NOT NULL 
BEGIN 
ALTER TABLE Customer DROP CONSTRAINT DF_Customer_IsOffline
ALTER TABLE Customer ALTER COLUMN IsOffline BIT NULL
END 

GO 
