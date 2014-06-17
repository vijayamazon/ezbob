IF OBJECT_ID('AutoDecisionRecord') IS NULL
	EXECUTE('CREATE PROCEDURE AutoDecisionRecord AS SELECT 1')
GO

ALTER PROCEDURE AutoDecisionRecord
	(@CustomerId INT
	,@DecisionName VARCHAR(20)
	,@Date DATETIME)
	 
AS
BEGIN
	DECLARE @DecisionType INT

	SELECT 
		@DecisionType = Id
	FROM 
		AutoDecisionTypes
	WHERE 
		Name = @DecisionName
		
	INSERT INTO AutoDecisionHistory (CustomerId, DecisionType, Date) VALUES (@CustomerId, @DecisionType, @Date)
	
	SELECT CONVERT(INT, @@IDENTITY)
END
GO
