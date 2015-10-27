
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AddCollectionLog') IS NULL
	EXECUTE('CREATE PROCEDURE AddCollectionLog AS SELECT 1')
GO


ALTER PROCEDURE AddCollectionLog
@CustomerID INT,
@LoanID INT,
@Type NVARCHAR(30),
@Method NVARCHAR(30),
@Now DATETIME
AS
BEGIN
	INSERT INTO CollectionLog (CustomerID, LoanID, TimeStamp, Type, Method) VALUES
	(@CustomerID, @LoanID, @Now, @Type, @Method)
	
	SELECT CAST(SCOPE_IDENTITY() AS INT) AS CollectionLogID
END
GO
