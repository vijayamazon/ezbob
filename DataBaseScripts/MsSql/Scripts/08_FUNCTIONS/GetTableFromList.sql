IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTableFromList]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetTableFromList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetTableFromList] (@List ntext, @Delimiter varchar(10)=',')  
RETURNS @tbl TABLE (Item nvarchar(max) NOT NULL) AS
BEGIN
  DECLARE
    @idxb int, @idxe int, 
    @item nvarchar(max),
    @lend int, @lenl int, @i int
  SET @lend=DATALENGTH(@Delimiter)
  SET @lenl=DATALENGTH(@List) 
  SET @idxb=1
  WHILE SUBSTRING(@List,@idxb,@lend)=@Delimiter AND @idxb<@lenl-@lend+1 SET @idxb=@idxb+@lend
  SET @idxe=@idxb
  WHILE @idxb<=@lenl AND @idxe<=@lenl
  BEGIN
    IF SUBSTRING(@List,@idxe+1,@lend)=@Delimiter 
    BEGIN
      SET @item=SUBSTRING(@List,@idxb,@idxe-@idxb+1)
      INSERT INTO @tbl (Item) VALUES (@item)
      SET @idxb=@idxe+@lend+1
      WHILE SUBSTRING(@List,@idxb,@lend)=@Delimiter AND @idxb<@lenl-@lend+1 SET @idxb=@idxb+@lend
      SET @idxe=@idxb
    END
    ELSE IF @idxe=@lenl
    BEGIN
      SET @item=SUBSTRING(@List,@idxb,@idxe-@idxb+1)
      IF @item<>@Delimiter  
        INSERT INTO @tbl (Item) VALUES (@item)
      RETURN
    END
    ELSE
    BEGIN
      SET @idxe=@idxe+1
    END 
  END
  RETURN
END
GO
