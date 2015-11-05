IF OBJECT_ID('NL_AddLog') IS NOT NULL
	DROP PROCEDURE NL_AddLog
GO


IF TYPE_ID('NL_LogType') IS NOT NULL
	DROP TYPE NL_LogType
GO


CREATE TYPE NL_LogType AS TABLE (
		Sevirity NVARCHAR(MAX) NOT NULL,
		UserID INT NULL,
		CustomerID INT NULL,
		Args NVARCHAR(MAX) NULL,
		[Result] NVARCHAR(MAX) NULL,
		Referrer NVARCHAR(MAX) NOT NULL,
		Description NVARCHAR(MAX) NOT NULL,
		[Exception] NVARCHAR(MAX) NULL,
		Stacktrace NVARCHAR(MAX) NULL,
		TimeStamp DATETIME NOT NULL		
)
GO


CREATE PROCEDURE NL_AddLog
@Tbl NL_LogType READONLY
AS
BEGIN
	SET NOCOUNT ON;	

	INSERT INTO NL_Log (
		[Sevirity],
		[UserID],
		[CustomerID],
		[Args],
		[Result],
		[Referrer] ,
		[Description] ,
		[Exception] ,
		[Stacktrace],		
		[TimeStamp] 
	) SELECT
		[Sevirity],
		[UserID],
		[CustomerID],
		[Args],
		[Result],
		[Referrer],
		[Description],		
		[Exception],
		[Stacktrace],
		[TimeStamp] 
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO