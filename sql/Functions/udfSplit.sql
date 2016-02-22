SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID (N'dbo.udfSplit') IS NOT NULL
	DROP FUNCTION dbo.udfSplit
GO

CREATE FUNCTION dbo.udfSplit(@String NVARCHAR(4000), @Delimiter NCHAR(1))
RETURNS @output TABLE (Id INT, Data NVARCHAR(4000))
AS
BEGIN
	;WITH udfSplit AS (
		SELECT
			0 AS stpos,
			CHARINDEX(@Delimiter, @String) AS endpos
		UNION ALL
		SELECT
			endpos + 1,
			CHARINDEX(@Delimiter, @String, endpos + 1)
		FROM
			udfSplit
		WHERE
			endpos > 0
	)
	INSERT INTO @output(Id, Data)
	SELECT
		ROW_NUMBER() OVER (ORDER BY (SELECT 1)),
		SUBSTRING(@String, stpos, COALESCE(NULLIF(endpos, 0), LEN(@String) + 1) - stpos)
	FROM
		udfSplit

	RETURN
END
GO
