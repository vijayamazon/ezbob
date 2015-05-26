IF OBJECT_ID('UpdateExperianLtdScoreText') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateExperianLtdScoreText AS SELECT 1')
GO

ALTER PROCEDURE UpdateExperianLtdScoreText
	(@id BIGINT,
	 @text1 NVARCHAR(560),
	 @text2 NVARCHAR(110),
	 @text3 NVARCHAR(110),
	 @text4 NVARCHAR(110),
	 @text5 NVARCHAR(110),
	 @context NVARCHAR(300))
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE ExperianLtd
	SET CreditText1=@text1,
		CreditText2=@text2,
		CreditText3=@text3,
		CreditText4=@text4,
		CreditText5=@text5,
		ConclusionText=@context
	WHERE ServiceLogID=@id
	
END
GO