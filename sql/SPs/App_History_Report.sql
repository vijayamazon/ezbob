IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_History_Report]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_History_Report]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_History_Report] 
	(@pNodeId          int,
    @pApplicationId   bigint,
    @pS1              int output,
    @pl1              int output,
    @plimitisover     int output)
AS
BEGIN
	SET NOCOUNT ON;

  declare @l_lock         datetime;
  declare @l_unlock       datetime;
  declare @l_lock_prev    datetime;
  declare @l_unlock_prev  datetime;
  declare @l_s1           int;
   
  set @l_s1 = 0;
  set @l_lock_prev  = dateadd (yy,-100, getdate()) ;
  set @l_unlock_prev = dateadd (yy,-100, getdate()) ;
  set @plimitisover  = 0;

 while 1 = 1  

begin
      
      select @l_lock = min(actiondatetime)
       from application_history
       where applicationid = @papplicationid
         and currentnodeid = @pNodeId
         and actiontype = 0
         and securityapplicationid <> -1
         and actiondatetime > @l_lock_prev;
     
    if @l_lock is null 
      break;
      
    select @l_unlock = isnull(min(actiondatetime), getdate())
           from application_history
     where applicationid = @papplicationid
       and currentnodeid = @pNodeId
       and actiontype = 1
       and securityapplicationid <> -1
       and actiondatetime >= @l_lock;
  
    set @l_s1          = @l_s1 + datediff(ss, @l_lock, @l_unlock);
    set @l_lock_prev   = @l_lock;
    set @l_unlock_prev = @l_unlock;
      
end ;

set @ps1 = @l_s1;

    select @pl1 = EXECUTIONDURATION
    from Strategy_Node
    where nodeid = @pNodeId;

  if @pl1 is null set @plimitisover = 0;
  else if @ps1 > @pl1 
    set @plimitisover = 1;
END
GO
