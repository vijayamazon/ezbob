IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IntToTime]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IntToTime]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 29.11.2012
-- Description:	Convert seconds to min/hours
-- =============================================

CREATE FUNCTION [dbo].[IntToTime]
(	
  @sec int
)
RETURNS VARCHAR (max) 
AS
BEGIN
	DECLARE @res VARCHAR (max)
SELECT @res= RIGHT((@sec / 60), 10) + ':' + 
 right('0' + rtrim(convert(char(2), @sec % 60)),2)
 RETURN @res
end
GO
