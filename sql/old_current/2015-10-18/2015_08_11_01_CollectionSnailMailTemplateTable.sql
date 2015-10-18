IF object_id('CollectionSnailMailTemplate') IS NULL
BEGIN
CREATE TABLE CollectionSnailMailTemplate(
	CollectionSnailMailTemplateID INT NOT NULL IDENTITY(1,1),
	Type NVARCHAR(30),	
	IsLimited BIT NOT NULL,
	IsActive BIT NOT NULL,
	OriginID INT,
	FileName NVARCHAR(100),
	TemplateName NVARCHAR(100),
	Template VARBINARY(MAX),
	CONSTRAINT PK_CollectionSnailMailTemplate PRIMARY KEY (CollectionSnailMailTemplateID),
	CONSTRAINT FK_CollectionSnailMailTemplate_Origin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID)
)	
END
GO