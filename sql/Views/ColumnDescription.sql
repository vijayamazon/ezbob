SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ColumnDescription') IS NOT NULL
	DROP VIEW ColumnDescription
GO

CREATE VIEW ColumnDescription AS 
SELECT o.name AS 'Table', c.name AS 'Column', ep.value AS 'Description' FROM sys.all_objects o 
INNER JOIN sys.all_columns c ON o.object_id = c.object_id
INNER JOIN sys.extended_properties ep ON ep.minor_id = c.column_id and ep.major_id = o.object_id 
WHERE ep.value <> '' AND ep.value IS NOT NULL

GO