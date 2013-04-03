IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PAGING_CURSOR_COUNT]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[PAGING_CURSOR_COUNT]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PAGING_CURSOR_COUNT]
  @pTableName nvarchar(4000),
  @pCondition nvarchar(4000)  = null
AS
BEGIN
  DECLARE @strSELECT nvarchar(4000);
  DECLARE @ParmDefinition nvarchar(500);
  DECLARE @CntValue int;

  SET @ParmDefinition = N'@CntOUT Int OUTPUT';

  SET @strSELECT = N' SELECT @CntOUT = count(*) FROM '+@pTableName+
      CASE 
        WHEN @pCondition is null then
           ''
        ELSE
          ' WHERE  '+ @pCondition
      END;


EXECUTE sp_executesql @strSELECT,
  @ParmDefinition,
  @CntOUT = @CntValue OUTPUT;

Select @CntValue

END;
GO
