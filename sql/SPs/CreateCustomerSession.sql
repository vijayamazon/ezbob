IF OBJECT_ID('CreateCustomerSession') IS NULL
	EXECUTE ('CREATE PROCEDURE CreateCustomerSession AS SELECT 1')
GO

ALTER PROCEDURE CreateCustomerSession
@CustomerID INT,
@StartSession DATETIME,
@Ip NVARCHAR(50),
@IsPasswdOk BIT,
@ErrorMessage NVARCHAR(50),
@EndSession DATETIME,
@SessionID INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @SessionID = 0

	INSERT INTO CustomerSession (CustomerId, StartSession, Ip, IsPasswdOk, ErrorMessage, EndSession)
		VALUES (@CustomerID, @StartSession, @Ip, @IsPasswdOk, @ErrorMessage, @EndSession)

	SET @SessionID = SCOPE_IDENTITY()

	SELECT @SessionID AS SessionID
END
GO
