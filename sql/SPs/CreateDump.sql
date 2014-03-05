IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateDump]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateDump]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateDump] 
	(@pApplicationId  BIGINT,
	@pName           NVARCHAR(MAX))
AS
BEGIN
	DECLARE @DumpId INT

   SELECT @DumpId = Id 
   FROM Application_VariablesDump WITH (NOLOCK)
   WHERE
      ApplicationId = @pApplicationId AND
      Name  = @pName;
   UPDATE Application_VariablesDump
      SET LastUpdateDate = GETDATE()
    WHERE id = @DumpId;
	
   IF (@DumpId IS NULL)
   begin	
      INSERT INTO [Application_VariablesDump]
         ( [ApplicationId] 
         , [Name]
         , LastUpdateDate) 
      VALUES 
         ( @pApplicationId 
         , @pName
         , GETDATE()); 

	select @@IDENTITY;
	return @@IDENTITY;
   end

   select @DumpId;
   return @DumpId
END
GO
