IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Percents' and Object_ID = Object_ID(N'SuggestedAmount'))    
BEGIN

ALTER TABLE SuggestedAmount ADD Percents DECIMAL(18, 6) NOT NULL DEFAULT(0)

END 
GO
