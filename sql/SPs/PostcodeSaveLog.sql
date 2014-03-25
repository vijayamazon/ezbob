IF OBJECT_ID('PostcodeSaveLog') IS NULL
	EXECUTE('CREATE PROCEDURE PostcodeSaveLog AS SELECT 1')
GO

ALTER PROCEDURE PostcodeSaveLog
@RequestType NVARCHAR(200),
@Url NTEXT,
@Status NVARCHAR(200),
@ResponseData NTEXT,
@ErrorMessage NTEXT,
@UserID INT,
@InsertDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @Email NVARCHAR(255)

	SELECT
		@Email = EMail
	FROM
		Security_User
	WHERE
		UserId = @UserID

	------------------------------------------------------------------------------

	DECLARE @CustomerID INT

	SELECT
		@CustomerID = Id
	FROM
		Customer
	WHERE
		Name = @Email

	------------------------------------------------------------------------------

	INSERT INTO PostcodeServiceLog (
		CustomerId, InsertDate, RequestType, RequestData, ResponseData,
		Status, ErrorMessage, UserID
	) VALUES (
		@CustomerID, @InsertDate, @RequestType, @Url, @ResponseData,
		@Status, @ErrorMessage, @UserID
	)
END
GO
