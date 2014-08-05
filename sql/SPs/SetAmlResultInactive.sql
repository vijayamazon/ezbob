IF OBJECT_ID('SetAmlResultInactive') IS NULL
	EXECUTE('CREATE PROCEDURE SetAmlResultInactive AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SetAmlResultInactive
	(@LookupKey NVARCHAR(500),
	 @ServiceLogId BIGINT)
AS
BEGIN
	SET NOCOUNT ON;
	Update AmlResults SET IsActive = 0 WHERE LookupKey = @LookupKey AND ServiceLogId = @ServiceLogId
END
GO
