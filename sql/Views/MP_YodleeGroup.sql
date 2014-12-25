IF OBJECT_ID('MP_YodleeGroup') IS NOT NULL
	DROP VIEW MP_YodleeGroup
GO

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
