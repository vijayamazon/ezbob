create or replace type rec_taskList is object
(
   ApplicationId NUMBER,
   Param1 VARCHAR2(512),
   Param2 VARCHAR2(512),
   CurrentNodeName VARCHAR2(500),
   CurrentNodeId NUMBER,
   StrategyName VARCHAR2(512),
   Version NUMBER,
   ApplicationIdExecute NUMBER
);
/
create or replace type tab_taskList is table of rec_tasklist;
/
create or replace type tbl_result as table of varchar2(255);
/
create or replace type rec_GetTableFromList is object
(
   Item varchar2(8000) --NOT NULL
);
/
create or replace type rec_fnGetNodesFromPath is object
( nodeid number, name varchar2 (255) );
/
create or replace type tab_fnGetNodesFromPath
is table of rec_fnGetNodesFromPath;
/