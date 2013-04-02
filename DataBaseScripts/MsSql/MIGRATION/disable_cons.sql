alter table [dbo].[Security_UserRoleRelation] NOCHECK constraint FK_Security_UserRoleRelation_Security_Role ;
alter table [dbo].[Security_UserRoleRelation] NOCHECK constraint FK_Security_UserRoleRelation_Security_User ;

alter table [dbo].[Strategy_Node] NOCHECK constraint FK_Strategy_Node_Security_Application ;
alter table [dbo].[Strategy_Node] NOCHECK constraint FK_Strategy_Node_Strategy_NodeGroup ;


ALTER TABLE [dbo].[DataSource_StrategyRel] NOCHECK CONSTRAINT [FK_DSSTR_STRATEGY_STRATEGYID] ;

ALTER TABLE [dbo].[BehavioralReports] NOCHECK CONSTRAINT [FK_BRep_STRATEGY_STRATEGYID] ;

ALTER TABLE [dbo].[Strategy_Schedule] NOCHECK CONSTRAINT [FK_Schedule_StrategyId] ;

ALTER TABLE [dbo].[StrategyTasks] NOCHECK CONSTRAINT [FK_STRATEGYTASK_AREA] ;

ALTER TABLE [dbo].[TaskedStrategies] NOCHECK CONSTRAINT [FK_TASKEDSTRATEGY_TASK] ;

ALTER TABLE [dbo].[TaskedStrategies] NOCHECK CONSTRAINT [FK_TASKEDSTRATEGY_STRAT] ;

ALTER TABLE [dbo].[TaskedStrategyParams] NOCHECK CONSTRAINT [FK_TSPARAM_TS] ;
