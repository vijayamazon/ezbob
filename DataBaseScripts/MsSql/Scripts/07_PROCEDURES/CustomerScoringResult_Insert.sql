IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerScoringResult_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CustomerScoringResult_Insert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CustomerScoringResult_Insert]
    @pCustomerId    int,
    @pAC_Parameters nvarchar(MAX),
    @AC_Descriptors nvarchar(MAX),
    @Result_Weight  nvarchar(MAX),
    @pResult_MAXPossiblePoints  nvarchar(MAX),
    @pMedal         nvarchar(20),
    @pScorePoints  [numeric](8, 3),
    @pScoreResult  [numeric](8, 3)
AS
BEGIN
    insert into CustomerScoringResult
    (
	CustomerId,
	AC_Parameters,
	AC_Descriptors,
	Result_Weights,
	Result_MAXPossiblePoints,
	Medal,
	ScorePoints,
	ScoreResult	
     )

  values
    (
     @pCustomerId,
     @pAC_Parameters,
     @AC_Descriptors,
     @Result_Weight,
     @pResult_MAXPossiblePoints,
     @pMedal,
     @pScorePoints,
     @pScoreResult
    )

END
GO
