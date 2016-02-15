SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LandRegistryDBSave') IS NOT NULL
	DROP PROCEDURE LandRegistryDBSave
GO

IF TYPE_ID('LandRegistryDBList') IS NOT NULL
	DROP TYPE LandRegistryDBList
GO

CREATE TYPE LandRegistryDBList AS TABLE (
	[CustomerId] INT NOT NULL,
	[InsertDate] DATETIME NOT NULL,
	[Postcode] NVARCHAR(15) NULL,
	[TitleNumber] NVARCHAR(30) NULL,
	[RequestType] NVARCHAR(20) NULL,
	[ResponseType] NVARCHAR(20) NULL,
	[Request] NVARCHAR(MAX) NULL,
	[Response] NVARCHAR(MAX) NULL,
	[AttachmentPath] NVARCHAR(300) NULL,
	[CustomerAddressID] INT NULL
)
GO

CREATE PROCEDURE LandRegistryDBSave
@Tbl LandRegistryDBList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO LandRegistryDB (
		[CustomerId],
		[InsertDate],
		[Postcode],
		[TitleNumber],
		[RequestType],
		[ResponseType],
		[Request],
		[Response],
		[AttachmentPath],
		[CustomerAddressID]
	) SELECT
		[CustomerId],
		[InsertDate],
		[Postcode],
		[TitleNumber],
		[RequestType],
		[ResponseType],
		[Request],
		[Response],
		[AttachmentPath],
		[CustomerAddressID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


