SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF EXISTS (SELECT * FROM sysobjects WHERE name = 'MP_YodleeGroup' AND type = 'U')
BEGIN
	------------------------------------------------------------------------------

	CREATE TABLE MP_YodleeTransactionBaseTypes(
		BaseTypeID INT NOT NULL,
		BaseTypeName NVARCHAR(100) NOT NULL,
		CONSTRAINT PK_MP_YodleeTransactionBaseTypes PRIMARY KEY (BaseTypeID),
		CONSTRAINT UC_MP_YodleeTransactionBaseTypes UNIQUE (BaseTypeName)
	)

	------------------------------------------------------------------------------

	INSERT INTO MP_YodleeTransactionBaseTypes (BaseTypeID, BaseTypeName)
	SELECT DISTINCT
		transactionBaseTypeId,
		transactionBaseType
	FROM
		MP_YodleeOrderItemBankTransaction

	------------------------------------------------------------------------------

	CREATE TABLE MP_YodleeMainGroups (
		MainGroupID INT IDENTITY(1, 1) NOT NULL,
		MainGroupName NVARCHAR(100) NOT NULL,
		Priority INT NOT NULL,
		CONSTRAINT PK_MP_YodleeMainGroups PRIMARY KEY (MainGroupID),
		CONSTRAINT UC_MP_YodleeMainGroups UNIQUE (MainGroupName)
	)

	------------------------------------------------------------------------------

	INSERT INTO MP_YodleeMainGroups (MainGroupName, Priority)
	SELECT DISTINCT
		MainGroup,
		Priority
	FROM
		MP_YodleeGroup

	------------------------------------------------------------------------------

	CREATE TABLE MP_YodleeSubGroups (
		SubGroupID INT NOT NULL,
		MainGroupID INT NOT NULL,
		SubGroupName NVARCHAR(100) NULL,
		BaseTypeID INT NULL,
		CONSTRAINT PK_MP_YodleeSubGroups PRIMARY KEY (SubGroupID),
		CONSTRAINT FK_MP_YodleeSubGroups_MainGroup FOREIGN KEY (MainGroupID) REFERENCES MP_YodleeMainGroups,
		CONSTRAINT FK_MP_YodleeSubGroups_BaseType FOREIGN KEY (BaseTypeID) REFERENCES MP_YodleeTransactionBaseTypes(BaseTypeID)
	)

	------------------------------------------------------------------------------

	INSERT INTO MP_YodleeSubGroups (SubGroupID, MainGroupID, SubGroupName, BaseTypeID)
	SELECT
		g.Id,
		mg.MainGroupID,
		g.SubGroup,
		t.BaseTypeID
	FROM
		MP_YodleeGroup g
		INNER JOIN MP_YodleeMainGroups mg ON g.MainGroup = mg.MainGroupName
		LEFT JOIN MP_YodleeTransactionBaseTypes t ON g.BaseType = t.BaseTypeName

	------------------------------------------------------------------------------

	EXECUTE sp_rename 'MP_YodleeGroup', 'MP_YodleeGroup_SafeToRemove', 'OBJECT'
END
GO

IF OBJECT_ID('MP_YodleeGroup') IS NOT NULL
	DROP VIEW MP_YodleeGroup
GO

-----------------------------------------------------------------------------------

CREATE VIEW MP_YodleeGroup AS
SELECT
	Id = sg.SubGroupID,
	MainGroup = mg.MainGroupName,
	SubGroup = sg.SubGroupName,
	BaseType = t.BaseTypeName,
	Priority = mg.Priority
FROM
	MP_YodleeMainGroups mg
	INNER JOIN MP_YodleeSubGroups sg ON mg.MainGroupID = sg.MainGroupID
	LEFT JOIN MP_YodleeTransactionBaseTypes t ON sg.BaseTypeID = t.BaseTypeID
GO

-----------------------------------------------------------------------------------

IF OBJECT_ID('FK_MP_YodleeOrderItemBankTransaction_BaseType') IS NULL
BEGIN
	ALTER TABLE MP_YodleeOrderItemBankTransaction
		ADD CONSTRAINT FK_MP_YodleeOrderItemBankTransaction_BaseType
			FOREIGN KEY (transactionBaseTypeId)
			REFERENCES MP_YodleeTransactionBaseTypes (BaseTypeID)
END
GO

-----------------------------------------------------------------------------------

ALTER TABLE MP_YodleeOrderItemBankTransaction DROP CONSTRAINT FK_MP_YodleeGroup_MP_YodleeOrderItemBankTransaction
GO

ALTER TABLE MP_YodleeOrderItemBankTransaction
	ADD CONSTRAINT FK_MP_YodleeGroup_MP_YodleeOrderItemBankTransaction
	FOREIGN KEY (EzbobCategory) REFERENCES MP_YodleeSubGroups (SubGroupID)
GO

-----------------------------------------------------------------------------------

ALTER TABLE MP_YodleeGroupRuleMap DROP CONSTRAINT FK_MP_YodleeGroupRuleMap_MP_YodleeGroup
GO

ALTER TABLE MP_YodleeGroupRuleMap
	ADD CONSTRAINT FK_MP_YodleeGroupRuleMap_MP_YodleeGroup
	FOREIGN KEY (GroupId) REFERENCES MP_YodleeSubGroups (SubGroupID)
GO

-----------------------------------------------------------------------------------

-- DROP TABLE MP_YodleeGroup_SafeToRemove
-- DROP TABLE MP_YodleeSubGroups
-- DROP TABLE MP_YodleeMainGroups
-- DROP TABLE MP_YodleeTransactionBaseTypes

-----------------------------------------------------------------------------------

IF OBJECT_ID('YodleeGroupAggregation') IS NULL
BEGIN
	CREATE TABLE YodleeGroupAggregation (
		YodleeGroupAggregationID BIGINT IDENTITY(1, 1) NOT NULL,
		TheMonth DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		MainGroupID INT NULL,
		SubGroupID INT NULL,
		BaseTypeID INT NOT NULL,
		Value NUMERIC(18, 2) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_YodleeGroupAggregation PRIMARY KEY (YodleeGroupAggregationID),
		CONSTRAINT FK_YodleeGroupAggregation_History FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id),
		CONSTRAINT FK_YodleeGroupAggregation_MainGroup FOREIGN KEY (MainGroupID) REFERENCES MP_YodleeMainGroups (MainGroupID),
		CONSTRAINT FK_YodleeGroupAggregation_SubGroup FOREIGN KEY (SubGroupID) REFERENCES MP_YodleeSubGroups (SubGroupID),
		CONSTRAINT FK_YodleeGroupAggregation_BaseType FOREIGN KEY (BaseTypeID) REFERENCES MP_YodleeTransactionBaseTypes (BaseTypeID),
		CONSTRAINT CHK_YodleeGroupAggregation_Groups CHECK (
			(MainGroupID IS NOT NULL AND SubGroupID IS     NULL)
			OR
			(MainGroupID IS     NULL AND SubGroupID IS NOT NULL)
		)
	)
END
GO
