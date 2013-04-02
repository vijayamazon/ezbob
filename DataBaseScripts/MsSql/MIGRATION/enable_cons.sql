
alter table [dbo].[Security_UserRoleRelation] CHECK constraint FK_Security_UserRoleRelation_Security_Role ;
alter table [dbo].[Security_UserRoleRelation] CHECK constraint FK_Security_UserRoleRelation_Security_User ;

alter table [dbo].[Strategy_Node] CHECK constraint FK_Strategy_Node_Security_Application ;
alter table [dbo].[Strategy_Node] CHECK constraint FK_Strategy_Node_Strategy_NodeGroup ;


ALTER TABLE [dbo].[DataSource_StrategyRel] CHECK CONSTRAINT [FK_DSSTR_STRATEGY_STRATEGYID] ;

ALTER TABLE [dbo].[BehavioralReports] CHECK CONSTRAINT [FK_BRep_STRATEGY_STRATEGYID] ;

ALTER TABLE [dbo].[Strategy_Schedule] CHECK CONSTRAINT [FK_Schedule_StrategyId] ;

ALTER TABLE [dbo].[StrategyTasks] CHECK CONSTRAINT [FK_STRATEGYTASK_AREA] ;

ALTER TABLE [dbo].[TaskedStrategies] CHECK CONSTRAINT [FK_TASKEDSTRATEGY_TASK] ;

ALTER TABLE [dbo].[TaskedStrategies] CHECK CONSTRAINT [FK_TASKEDSTRATEGY_STRAT] ;

ALTER TABLE [dbo].[TaskedStrategyParams] CHECK CONSTRAINT [FK_TSPARAM_TS] ;
