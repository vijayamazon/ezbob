IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptTstRpt]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptTstRpt]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure RptTstRpt
@DateStart datetime,
@DateEnd datetime
as
select 'Test report' as SomeData, @DateStart as FromTime, @DateEnd as ToTime
GO
