IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerScoringResult_Insert]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CustomerScoringResult_Insert]
GO
