IF OBJECT_ID (N'dbo.udfGetErrorMsg') IS NOT NULL
	DROP FUNCTION dbo.udfGetErrorMsg
GO

CREATE FUNCTION [dbo].[udfGetErrorMsg]
RETURNS NVARCHAR(1024)
AS
BEGIN
	RETURN
		'Error #' + CONVERT(NVARCHAR, ERROR_NUMBER()) +
			' severity ' + CONVERT(NVARCHAR, ERROR_SEVERITY()) +
			' state ' + CONVERT(NVARCHAR, ERROR_STATE()) +
			' in procedure ' + ERROR_PROCEDURE() +
			' at line ' + CONVERT(NVARCHAR, ERROR_LINE()) +
			': ' + ERROR_MESSAGE()
END

GO

