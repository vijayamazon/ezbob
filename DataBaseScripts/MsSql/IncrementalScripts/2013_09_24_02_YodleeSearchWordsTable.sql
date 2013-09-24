IF OBJECT_ID ('dbo.MP_YodleeSearchWords') IS NOT NULL
	DROP TABLE dbo.MP_YodleeSearchWords
GO

CREATE TABLE dbo.MP_YodleeSearchWords
	(
	  Id                                         INT IDENTITY NOT NULL
	, SearchWords                                NVARCHAR(300) NOT NULL
	, CONSTRAINT PK_MP_YodleeSearchWords PRIMARY KEY (Id)
	)
GO

CREATE INDEX IX_MP_YodleeSearchWords
	ON dbo.MP_YodleeSearchWords (SearchWords)
GO

INSERT INTO MP_YodleeSearchWords VALUES ('loan') 
INSERT INTO MP_YodleeSearchWords VALUES ('payday')
INSERT INTO MP_YodleeSearchWords VALUES ('iwoca')
INSERT INTO MP_YodleeSearchWords VALUES ('wonga')