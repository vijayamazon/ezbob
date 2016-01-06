SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreateOrLoadUploadedHmrcMarketplace') IS NULL
	EXECUTE('CREATE PROCEDURE CreateOrLoadUploadedHmrcMarketplace AS SELECT 1')
GO

ALTER PROCEDURE CreateOrLoadUploadedHmrcMarketplace
@CustomerID INT,
@SecurityData VARBINARY(MAX),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Hmrc UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	DECLARE @ExitCode INT = NULL
	DECLARE @MpID INT = NULL
	DECLARE @OtherCustomerID INT = NULL

	DECLARE @Email NVARCHAR(128)
	DECLARE @OriginID INT
	DECLARE @HmrcID INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId = @Hmrc)

	SELECT
		@Email = Name,
		@OriginID = OriginID
	FROM
		Customer
	WHERE
		Id = @CustomerID

	SELECT
		@MpID = m.Id,
		@OtherCustomerID = m.CustomerID
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN Customer c ON m.CustomerId = c.Id
	WHERE
		m.DisplayName = @Email
		AND
		c.OriginID = @OriginID
		AND
		m.MarketPlaceId = @HmrcID

	IF @OtherCustomerID IS NULL
	BEGIN
		BEGIN TRY
			INSERT INTO MP_CustomerMarketPlace (SecurityData, CustomerId, MarketPlaceId, DisplayName, Created, Updated)
				VALUES (@SecurityData, @CustomerID, @HmrcID, @Email, @Now, @Now)

			SET @MpID = SCOPE_IDENTITY()
			SET @ExitCode = 0
		END TRY
		BEGIN CATCH
			SET @ExitCode = -1
		END CATCH
	END
	ELSE
		SET @ExitCode = CASE WHEN @OtherCustomerID = @CustomerID THEN 0 ELSE -2 END

	SELECT
		ExitCode = @ExitCode,
		MpID = @MpID,
		OtherCustomerID = @OtherCustomerID
END
GO
