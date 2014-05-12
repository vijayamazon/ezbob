IF OBJECT_ID('AddCciHistory') IS NULL
	EXECUTE('CREATE PROCEDURE AddCciHistory AS SELECT 1')
GO

ALTER PROCEDURE AddCciHistory
	(@CustomerId INT,
	 @CciMark BIT)
AS
BEGIN
	INSERT INTO CciHistory (CustomerId, ChangeDate, CciMark) VALUES (@CustomerId, GETUTCDATE(), @CciMark)	
END
GO
