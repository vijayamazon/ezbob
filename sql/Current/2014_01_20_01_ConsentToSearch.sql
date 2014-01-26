IF EXISTS (SELECT * from sys.columns 
            WHERE Name = N'LimitedConsentToSearch' and Object_ID = Object_ID(N'Customer'))
BEGIN 
EXEC sp_rename 'Customer.[LimitedConsentToSearch]', 'ConsentToSearch', 'COLUMN'
ALTER TABLE Customer DROP COLUMN NonLimitedConsentToSearch
ALTER TABLE Customer ADD IsDirector BIT 
END 
GO
