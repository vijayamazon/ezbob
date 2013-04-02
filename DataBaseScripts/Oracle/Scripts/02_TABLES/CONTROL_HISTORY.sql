-- Create table
create table CONTROL_HISTORY
(
  HISROTYID          NUMBER not null,
  APPLICATIONID      NUMBER not null,
  STRATEGYID         NUMBER not null,
  USERID             NUMBER not null,
  NODEID             NUMBER not null,
  CHANGESTIME        DATE default CURRENT_TIMESTAMP not null,
  CONTROLNAME        VARCHAR2(255) not null,
  CONTROLVALUE       CLOB,
  CURRENTNODEPOSTFIX VARCHAR2(1000) not null,
  SECURITYAPPID      NUMBER
);