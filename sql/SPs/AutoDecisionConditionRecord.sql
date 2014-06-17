IF OBJECT_ID('AutoDecisionConditionRecord') IS NULL
	EXECUTE('CREATE PROCEDURE AutoDecisionConditionRecord AS SELECT 1')
GO

ALTER PROCEDURE AutoDecisionConditionRecord
	(@DecisionId INT
	,@DecisionName VARCHAR(20)
	,@Satisfied BIT
	,@Description VARCHAR(MAX))
	 
AS
BEGIN
	DECLARE @DecisionType INT

	SELECT 
		@DecisionType = Id
	FROM 
		AutoDecisionTypes
	WHERE 
		Name = @DecisionName
		
	INSERT INTO AutoDecisionHistoryConditions (DecisionId, DecisionType ,Satisfied, Description) VALUES (@DecisionId, @DecisionType, @Satisfied, @Description)
END
GO
