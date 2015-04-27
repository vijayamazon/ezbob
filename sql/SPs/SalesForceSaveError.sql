IF OBJECT_ID('SalesForceSaveError') IS NULL
	EXECUTE('CREATE PROCEDURE SalesForceSaveError AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SalesForceSaveError
@Now DATETIME,
@CustomerID INT,
@Type NVARCHAR(30),
@Model NVARCHAR(max),
@Error NVARCHAR(max)
AS
BEGIN
	INSERT INTO SalesForceLog (Created, CustomerID, Type, Model, Error) 
	VALUES (@Now, @CustomerID, @Type, @Model, @Error)
END
GO
