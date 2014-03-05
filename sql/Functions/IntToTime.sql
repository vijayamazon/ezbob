IF OBJECT_ID (N'dbo.IntToTime') IS NOT NULL
	DROP FUNCTION dbo.IntToTime
GO

CREATE FUNCTION [dbo].[IntToTime]
(	@sec int
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

