IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianScore]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianScore]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianScore] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE @ExperianScore INT
	
	SELECT
		@ExperianScore = ExperianScore
	FROM
		MP_ExperianDataCache
	WHERE 
		CustomerId = @CustomerId AND
		DirectorId = 0
		
	IF @ExperianScore IS NULL
		SELECT @ExperianScore = 0
	
	SELECT @ExperianScore AS ExperianScore
END
GO
