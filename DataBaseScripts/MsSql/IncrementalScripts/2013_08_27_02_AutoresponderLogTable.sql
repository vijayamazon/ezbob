IF OBJECT_ID ('dbo.AutoresponderLog') IS NOT NULL
	DROP TABLE dbo.AutoresponderLog
GO

CREATE TABLE dbo.AutoresponderLog
	(
	  Id                                   INT IDENTITY NOT NULL
	, Email                                NVARCHAR (300) NOT NULL
	, Name                                 NVARCHAR (300) 
	, DateOfAutoResponse                   DATETIME
	, CONSTRAINT PK_AutoresponderLog PRIMARY KEY (Id)
	)
GO

CREATE INDEX IX_AutoresponderLog_Email
	ON dbo.AutoresponderLog (Email)
GO


