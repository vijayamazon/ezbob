IF OBJECT_ID('GetEntriesForAmlBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE GetEntriesForAmlBackfill AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetEntriesForAmlBackfill
AS
BEGIN
	SELECT Id, CustomerId FROM MP_ServiceLog WHERE ServiceType = 'AML A check' AND CustomerId IS NOT NULL ORDER BY InsertDate ASC	
END
GO
