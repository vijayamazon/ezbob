IF(object_id('AutoDecisionTypes') IS NULL) 
BEGIN

CREATE TABLE AutoDecisionTypes
(
	 Id INT
	,Name VARCHAR(20)
	,CONSTRAINT PK_AutoDecisionTypes PRIMARY KEY (Id)
)

INSERT INTO AutoDecisionTypes (Id, Name) VALUES (1, 'Manual')
INSERT INTO AutoDecisionTypes (Id, Name) VALUES (2, 'Rejection')
INSERT INTO AutoDecisionTypes (Id, Name) VALUES (3, 'Re-Rejection')
INSERT INTO AutoDecisionTypes (Id, Name) VALUES (4, 'Approval')
INSERT INTO AutoDecisionTypes (Id, Name) VALUES (5, 'Re-Approval')
INSERT INTO AutoDecisionTypes (Id, Name) VALUES (6, 'Bank Based Approval')

END 
GO

IF(object_id('AutoDecisionHistory') IS NULL) 
BEGIN

CREATE TABLE AutoDecisionHistory
(
	 Id INT NOT NULL IDENTITY(1,1)
	,CustomerId INT
	,DecisionType INT
	,Date DATETIME
	,CONSTRAINT PK_AutoDecisionHistory PRIMARY KEY (Id)
)

END 
GO

IF(object_id('AutoDecisionHistoryConditions') IS NULL) 
BEGIN

CREATE TABLE AutoDecisionHistoryConditions
(
	 Id INT NOT NULL IDENTITY(1,1)
	,DecisionId INT
	,DecisionType INT
	,Satisfied BIT
	,Description VARCHAR(MAX)
	,CONSTRAINT PK_AutoDecisionHistoryConditions PRIMARY KEY (Id)
)

END 
GO
