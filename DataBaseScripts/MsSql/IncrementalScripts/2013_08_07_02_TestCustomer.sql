IF OBJECT_ID ('dbo.TestCustomer') IS NOT NULL
	DROP TABLE dbo.TestCustomer
ELSE
BEGIN

CREATE TABLE dbo.TestCustomer
	(
	  Id                           INT IDENTITY NOT NULL
	, Pattern                      NVARCHAR(50) NOT NULL
	, CONSTRAINT PK_TestCustomer   PRIMARY KEY (Id)
	)
INSERT INTO TestCustomer (Pattern) VALUES ('@ezbob.com')
INSERT INTO TestCustomer (Pattern) VALUES ('@ezbob.co.uk')
INSERT INTO TestCustomer (Pattern) VALUES ('@scorto.com')
INSERT INTO TestCustomer (Pattern) VALUES ('@scroto.com.ua')
INSERT INTO TestCustomer (Pattern) VALUES ('@vitally.com')

END 
