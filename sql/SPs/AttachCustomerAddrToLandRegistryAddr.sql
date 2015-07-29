IF OBJECT_ID('AttachCustomerAddrToLandRegistryAddr') IS NULL 
	EXECUTE('CREATE PROCEDURE AttachCustomerAddrToLandRegistryAddr AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AttachCustomerAddrToLandRegistryAddr
@LandRegistryAddressID INT,
@CustomerAddressID INT,
@IsOwnerAccordingToLandRegistry BIT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE LandRegistry SET
		AddressId = @CustomerAddressID
	WHERE
		Id = @LandRegistryAddressID

	IF @IsOwnerAccordingToLandRegistry = 1
	BEGIN
		UPDATE CustomerAddress SET
			IsOwnerAccordingToLandRegistry = 1
		WHERE
			AddressId = @CustomerAddressID
	END
END
GO
