-- doledbp.sql - drops Informix OLE DB Provider support objects
-- DBA runs this file to drop the provider support objects in sysmaster

drop procedure informix.oledbmakebool;
drop procedure informix.oledbidxunique;
drop procedure informix.oledbidxcoll;
drop procedure informix.oledbidxnulls;
drop procedure informix.oledbidxintegr;
drop procedure informix.oledbidxclustered;
drop procedure informix.oledbmakexstid;
drop procedure informix.oledbmakexstid1;
drop procedure informix.oledbcolscale;
drop procedure informix.oledbcolscale1;
drop procedure informix.oledbcolprec;
drop procedure informix.oledbcollen;
drop procedure informix.oledbcolnullable;
drop procedure informix.oledbcoldef;
drop procedure informix.oledbisnotnull;
drop procedure informix.oledbissystab;
drop procedure informix.oledbconstrrule;
drop procedure informix.oledbkeypart;
drop procedure informix.oledbcolflags;
drop procedure informix.oledbnvl;
drop procedure informix.oledbwithcheck;
drop procedure informix.oledbhaswcheck;
drop procedure informix.oledbconcatview;
drop procedure informix.oledbconcatproc;
drop procedure informix.oledbconcatcheck;
drop table informix.oledbordinals;
drop table informix.oledbusagetypes;
drop table informix.oledbprivtypes;
drop table informix.oledbconstrtypes;
drop table informix.oledbconstrdefers;
drop table informix.oledbtypes;
drop table informix.oledbbmk;
drop table informix.oledbtabtypes;
drop table informix.oledboleobjects;
drop table informix.oledbversion;

