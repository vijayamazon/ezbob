IF(object_id('LandRegistryOwner') IS NULL) 
BEGIN

CREATE TABLE LandRegistryOwner
(
	 Id INT NOT NULL IDENTITY(1,1)
	,LandRegistryId INT NOT NULL
	,FirstName NVARCHAR(100)
	,LastName NVARCHAR(100)
	,CompanyName NVARCHAR(100)
	,CompanyRegistrationNumber NVARCHAR(100)
	,CONSTRAINT PK_LandRegistryOwner PRIMARY KEY (Id)
	,CONSTRAINT FK_LandRegistryOwner_LandRegistry FOREIGN KEY (LandRegistryId) REFERENCES LandRegistry(Id)
)

END 
GO
