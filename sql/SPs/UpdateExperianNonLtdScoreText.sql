IF OBJECT_ID('UpdateExperianNonLtdScoreText') IS NULL
                EXECUTE('CREATE PROCEDURE UpdateExperianNonLtdScoreText AS SELECT 1')
GO
 
ALTER PROCEDURE UpdateExperianNonLtdScoreText
                (@ServiceLogId BIGINT,
                @RiskText NVARCHAR(70),
                @CreditText NVARCHAR(560),
                @ConcludingText NVARCHAR(200),
                @NocText NVARCHAR(200),
                @PossiblyRelatedDataText NVARCHAR(200))
AS
BEGIN
                SET NOCOUNT ON;
               
                UPDATE ExperianNonLimitedResults
                SET RiskText=@RiskText,
                    CreditText=@CreditText,
                    ConcludingText=@ConcludingText,
                    NocText=@NocText,
                    PossiblyRelatedDataText=@PossiblyRelatedDataText                                
                WHERE ServiceLogID=@ServiceLogId
               
END
GO