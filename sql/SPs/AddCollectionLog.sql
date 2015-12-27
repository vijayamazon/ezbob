SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AddCollectionLog') IS NULL
	EXECUTE('CREATE PROCEDURE AddCollectionLog AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[AddCollectionLog]
	@CustomerID INT,
	@LoanID INT,
	@Type NVARCHAR(30),
	@Method NVARCHAR(30),
	@LoanHistoryID BIGINT = null,	
	@Comments NVARCHAR(max) = null,
	@Now DATETIME
AS
BEGIN
	INSERT INTO CollectionLog (CustomerID, LoanID, TimeStamp, Type, Method, LoanHistoryID, Comments) VALUES
	(@CustomerID, @LoanID, @Now, @Type, @Method, @LoanHistoryID, @Comments);
	
	SELECT CAST(SCOPE_IDENTITY() AS INT) AS CollectionLogID;
END