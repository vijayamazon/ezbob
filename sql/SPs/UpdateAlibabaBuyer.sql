IF OBJECT_ID('UpdateAlibabaBuyer') IS  NULL
	EXECUTE('CREATE PROCEDURE UpdateAlibabaBuyer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateAlibabaBuyer
	@CustomerId INT,
	@ContractId BIGINT,
	@BussinessName NVARCHAR(100),
	@street1 NVARCHAR(100),
	@street2 NVARCHAR(100),
	@City NVARCHAR(100),
	@State NVARCHAR(100),
	@Zip NVARCHAR(100),
	@Country NVARCHAR(100),
	@AuthRepFname NVARCHAR(100),
	@AuthRepLname NVARCHAR(100),
	@Phone NVARCHAR(100),
	@Fax NVARCHAR(100),
	@Email NVARCHAR(100),
	@OrderRequestCountLastYear INT,
	@ConfirmShippingDocAndAmount BIT,
	@FinancingType NVARCHAR(100),
	@ConfirmReleaseFunds BIT  
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE AlibabaBuyer
	SET	ContractId=@ContractId,
	BussinessName=@BussinessName,
	street1=@street1,
	street2=@street2,
	City=@City,
	[State]=@State,
	Zip=@Zip,
	Country=@Country,
	AuthRepFname=@AuthRepFname,
	AuthRepLname=@AuthRepLname,
	Phone=@Phone,
	Fax=@Fax,
	Email=@Email,
	OrderRequestCountLastYear=@OrderRequestCountLastYear,
	ConfirmShippingDocAndAmount=@ConfirmShippingDocAndAmount,
	FinancingType=@FinancingType,
	ConfirmReleaseFunds=@ConfirmReleaseFunds  
	WHERE CustomerId = @CustomerId
		
END
GO
