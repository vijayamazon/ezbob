

IF object_id('DirectorHistory') IS NULL
BEGIN
	
	CREATE TABLE dbo.DirectorHistory
		(
		DirectorHistoryId     INT IDENTITY NOT NULL,
		DirectorID            INT,
		CustomerID            INT,
		CompanyID             INT,
		Name                  NVARCHAR (512),
		DateOfBirth           DATETIME,
		Middle                NVARCHAR (512),
		Surname               NVARCHAR (512),
		Gender                NVARCHAR (1),
		Email                 NVARCHAR (128),
		Phone                 NVARCHAR (50),
		ExperianConsumerScore INT,
		IsShareholder         BIT,
		IsDirector            BIT,
		UserID                INT,
		TimestampCounter      ROWVERSION,
		CONSTRAINT PK_DirectorHistory PRIMARY KEY (DirectorHistoryId),
		CONSTRAINT FK_DirectorHistory_UserId FOREIGN KEY (UserID) REFERENCES dbo.Security_User (UserId),
		CONSTRAINT FK_DirectorHistory_Company FOREIGN KEY (CompanyID) REFERENCES dbo.Company (Id),
		CONSTRAINT FK_DirectorHistory_Director FOREIGN KEY (DirectorID) REFERENCES dbo.Director (id)
		)
		
END 		
GO

