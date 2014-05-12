IF OBJECT_ID('AddCciHistory') IS NULL
	EXECUTE('CREATE PROCEDURE AddCciHistory AS SELECT 1')
GO

ALTER PROCEDURE AddCciHistory
	(@Username VARCHAR(100),
	 @CustomerId INT,
	 @CciMark BIT)
AS
BEGIN
	INSERT INTO CciHistory (Username, CustomerId, ChangeDate, CciMark) VALUES (@Username, @CustomerId, GETUTCDATE(), @CciMark)	
END
GO
