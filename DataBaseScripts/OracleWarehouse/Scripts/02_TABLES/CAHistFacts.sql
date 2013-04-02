create table CAHistFacts
(
ID NUMBER not null ,
MASTERID NUMBER,
Score NUMBER,
Risk NUMBER
)
PARTITION BY RANGE (ID)
(  
  PARTITION STARTUPDATA VALUES LESS THAN (500)
);

alter table CAHistFacts 
	add constraint PK_CAHistFacts 
	primary key (ID);

