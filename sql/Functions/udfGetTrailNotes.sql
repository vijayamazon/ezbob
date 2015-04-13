IF OBJECT_ID('dbo.udfGetTrailNotes') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfGetTrailNotes() RETURNS NVARCHAR(255) AS BEGIN RETURN '''' END')
GO


SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION dbo.udfGetTrailNotes(@TrailID INT)
RETURNS NVARCHAR(2000)
AS
BEGIN
	DECLARE @TrailNotes NVARCHAR(2000) = ''

	------------------------------------------------------------------------------

	SET @TrailNotes = (
		SELECT notes.TrailNote AS 'data()'
		FROM DecisionTrailNotes notes
		WHERE notes.TrailID=@TrailID
		FOR XML PATH('')
	)


	------------------------------------------------------------------------------
	RETURN @TrailNotes
END

GO
