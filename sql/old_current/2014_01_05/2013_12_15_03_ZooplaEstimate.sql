IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ZooplaEstimate' and Object_ID = Object_ID(N'Zoopla'))    
BEGIN
ALTER TABLE dbo.Zoopla
ADD ZooplaEstimate NVARCHAR(30)

ALTER TABLE dbo.Zoopla
ADD UpdateDate DATETIME
END 
GO

