IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_UpdateEliminationWarning]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_UpdateEliminationWarning]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_UpdateEliminationWarning] 
	(@Id int,
 @Warning varchar(max))
AS
BEGIN
	UPDATE [dbo].[MP_CustomerMarketPlace]
   SET  [Warning] = @Warning 
		
 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
