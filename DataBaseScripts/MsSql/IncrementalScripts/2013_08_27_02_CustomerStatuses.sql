IF OBJECT_ID ('dbo.CustomerStatuses') IS NOT NULL
	DROP TABLE dbo.CustomerStatuses
GO
CREATE TABLE dbo.CustomerStatuses
	(
	  Id INT NOT NULL
	, Name NVARCHAR(100)
	, CONSTRAINT PK_CustomerStatuses PRIMARY KEY (Id)
	)
GO

INSERT INTO CustomerStatuses VALUES (0, 'Enabled')
INSERT INTO CustomerStatuses VALUES (1, 'Disabled')
INSERT INTO CustomerStatuses VALUES (2, 'Fraud')
INSERT INTO CustomerStatuses VALUES (3, 'Legal')
INSERT INTO CustomerStatuses VALUES (4, 'Default')
INSERT INTO CustomerStatuses VALUES (5, 'Fraud Suspect')
INSERT INTO CustomerStatuses VALUES (6, 'Risky')
INSERT INTO CustomerStatuses VALUES (7, 'Bad')

GO
 