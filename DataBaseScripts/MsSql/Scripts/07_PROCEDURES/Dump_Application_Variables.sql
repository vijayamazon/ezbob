IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dump_Application_Variables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Dump_Application_Variables]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Dump_Application_Variables]
  @pDumpId bigint,
  @pName nvarchar(255),
  @pValue nvarchar(MAX),
  @pType nvarchar(20),
  @pDirection int
AS
BEGIN
   DECLARE @DumpId INT

   SELECT @DumpId = Id 
   FROM Application_VariablesDumpData WITH (NOLOCK)
   WHERE
      DumpId = @pDumpId AND
      Name  = @pName;
	
   IF (@DumpId IS NULL)
      INSERT INTO Application_VariablesDumpData
         ( [DumpId]
         , [Name]
         , [Value]
         , [Type]
         , [Direction])
       VALUES
         ( @pDumpId
         , @pName
         , @pValue
         , @pType
         , @pDirection)
   ELSE
      UPDATE Application_VariablesDumpData
         SET [Value] = @pValue

      WHERE
         DumpId = @pDumpId AND
         Name  = @pName;
END
GO
