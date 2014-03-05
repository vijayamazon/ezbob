IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Paging_Cursor]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Paging_Cursor]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Paging_Cursor] 
	(@pTables nvarchar(1000),
@pPK nvarchar(100),
@pSort nvarchar(200) = NULL,
@pPageNumber int = 1,
@pPageSize int = 10,
@pFields nvarchar(1000) = '*',
@pFilter nvarchar(1000) = NULL,
@pGroup nvarchar(1000) = NULL)
AS
BEGIN
	DECLARE @PKTable nvarchar(100)
DECLARE @PKName nvarchar(100)
DECLARE @type nvarchar(100)
DECLARE @prec int

IF CHARINDEX('.', @pPK) > 0
	BEGIN
		SET @PKTable = SUBSTRING(@pPK, 0, CHARINDEX('.',@pPK))
		SET @PKName = SUBSTRING(@pPK, CHARINDEX('.',@pPK) + 1, LEN(@pPK))
	END
ELSE
	BEGIN
		SET @PKTable = @pTables
		SET @PKName = @pPK
	END

SELECT @type=t.name, @prec=c.prec
FROM sysobjects o 
JOIN syscolumns c on o.id=c.id
JOIN systypes t on c.xusertype=t.xusertype
WHERE o.name = @PKTable AND c.name = @PKName

IF CHARINDEX('char', @type) > 0
   SET @type = @type + '(' + CAST(@prec AS nvarchar) + ')'

IF @type is NULL
	SET @type = 'bigint'

DECLARE @strPageSize nvarchar(50)
DECLARE @strStartRow nvarchar(50)
DECLARE @strFilter nvarchar(1000)
DECLARE @strGroup nvarchar(1000)

/*Default Sorting*/
IF @pSort IS NULL OR @pSort = ''
	SET @pSort = @pPK

/*Default Page Number*/
IF @pPageNumber < 1
	SET @pPageNumber = 1

/*Set paging variables.*/
SET @strPageSize = CAST(@pPageSize AS nvarchar(50))
SET @strStartRow = CAST(((@pPageNumber - 1)*@pPageSize + 1) AS nvarchar(50))

/*Set filter & group variables.*/
IF @pFilter IS NOT NULL AND @pFilter != ''
	SET @strFilter = ' WHERE ' + @pFilter + ' '
ELSE
	SET @strFilter = ''
IF @pGroup IS NOT NULL AND @pGroup != ''
	SET @strGroup = ' GROUP BY ' + @pGroup + ' '
ELSE
	SET @strGroup = ''

/*Execute dynamic query*/	
EXEC(
'
DECLARE @PageSize int
SET @PageSize = ' + @strPageSize + '

DECLARE @PK ' + @type + '
DECLARE @tblPK TABLE (
            PK  ' + @type + ' NOT NULL PRIMARY KEY
            )
Insert into dbo.Log_TraceLog(Message) values ( ''Before Declare cursor'')
DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT '  + @pPK + ' FROM ' + @pTables + @strFilter + ' ' + @strGroup + ' ORDER BY ' + @pSort + '
OPEN PagingCursor

FETCH RELATIVE ' + @strStartRow + ' FROM PagingCursor INTO @PK
SET NOCOUNT ON

WHILE @PageSize > 0 AND @@FETCH_STATUS = 0
BEGIN
            INSERT @tblPK (PK)  VALUES (@PK)
            FETCH NEXT FROM PagingCursor INTO @PK
            SET @PageSize = @PageSize - 1
END

CLOSE       PagingCursor
DEALLOCATE  PagingCursor

SELECT ' + @pFields + ' FROM ' + @pTables + ' JOIN @tblPK tblPK ON ' + @pPK + ' = tblPK.PK ' + @strFilter + ' ' + @strGroup + ' ORDER BY ' + @pSort 
)
END
GO
