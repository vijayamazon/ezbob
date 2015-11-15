IF OBJECT_ID('AddCollectionLog') IS NULL
	CREATE PROCEDURE AddCollectionLog
GO

ALTER PROCEDURE AddCollectionLog
@CustomerID INT,
@LoanID INT,
@Type NVARCHAR(30),
@Method NVARCHAR(30),
@LoanHistoryID BIGINT,
@Comments NVARCHAR(no limit),
@Now DATETIME
AS
BEGIN
	INSERT INTO CollectionLog (CustomerID, LoanID, TimeStamp, Type, Method, LoanHistoryID, Comments) VALUES
	(@CustomerID, @LoanID, @Now, @Type, @Method, @LoanHistoryID, @Comments)
	
	SELECT CAST(SCOPE_IDENTITY() AS INT) AS CollectionLogID
END