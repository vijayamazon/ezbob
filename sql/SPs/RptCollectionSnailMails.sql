IF OBJECT_ID('RptCollectionSnailMails') IS NULL
	EXECUTE('CREATE PROCEDURE RptCollectionSnailMails AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptCollectionSnailMails
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT l.CustomerID, ll.RefNum LoanRef, l.TimeStamp, l.Type, l.Method, c.Name
	FROM CollectionSnailMailMetadata c
	INNER JOIN CollectionLog l ON l.CollectionLogID = c.CollectionLogID
	INNER JOIN Loan ll ON ll.Id = l.LoanID
	WHERE l.TimeStamp>=@DateStart AND l.TimeStamp<@DateEnd
END
GO