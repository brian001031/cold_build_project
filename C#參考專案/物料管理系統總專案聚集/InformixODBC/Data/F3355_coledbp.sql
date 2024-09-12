-- coledbp.sql - creates Informix OLE DB Provider support objects
-- DBA runs this file (against sysmaster only) to create SQL objects
-- provider needs to connect to any database on the server

-- versions of the server and OLE DB support
create table informix.oledbversion(
	srvrver varchar(18),
	oledbver smallint, -- minor
	oleobjsmodtime datetime year to fraction,
	nullc char(1));
insert into informix.oledbversion(srvrver, oledbver)
	select owner, 2 from systables where tabname = ' VERSION';
revoke all on informix.oledbversion from public;
grant select on informix.oledbversion to public;

-- table types
create table informix.oledbtabtypes(
	typecode char(1),
	issystem varchar(5),
	typedesc varchar(18),
	primary key(typecode, issystem));
insert into informix.oledbtabtypes values( 'T', 'False', 'TABLE' );
insert into informix.oledbtabtypes values( 'T', 'True', 'SYSTEM TABLE' );
insert into informix.oledbtabtypes values( 'V', 'False', 'VIEW' );
insert into informix.oledbtabtypes values( 'V', 'True', 'VIEW' );
insert into informix.oledbtabtypes values( 'P', 'False', 'SYNONYM' );
insert into informix.oledbtabtypes values( 'S', 'False', 'SYNONYM' );
revoke all on informix.oledbtabtypes from public;
grant select on informix.oledbtabtypes to public;

-- bookmark by table type and fragmentation
create table informix.oledbbmk(
	tabtype char(1),
	fragmented smallint,
	withrowids smallint,
	hasbmk varchar(5),
	bmktype int,
	bmkdtype smallint,
	bmkmaxlen int,
	primary key(tabtype, fragmented, withrowids));
insert into informix.oledbbmk(tabtype, fragmented, withrowids, hasbmk,
		bmktype, bmkdtype, bmkmaxlen)
	values('T', 0, 0, 'True', 1, 3, 4);
insert into informix.oledbbmk(tabtype, fragmented, withrowids, hasbmk,
		bmktype, bmkdtype, bmkmaxlen)
	values('T', -1, 1, 'True', 1, 3, 4);
insert into informix.oledbbmk(tabtype, fragmented, withrowids, hasbmk)
	values('T', -1, 0, 'False');
insert into informix.oledbbmk(tabtype, fragmented, withrowids, hasbmk) 
	values('V', 0, 0, 'False');
insert into informix.oledbbmk(tabtype, fragmented, withrowids, hasbmk) 
	values('P', 0, 0, 'False');
insert into informix.oledbbmk(tabtype, fragmented, withrowids, hasbmk) 
	values('S', 0, 0, 'False');
revoke all on informix.oledbbmk from public;
grant select on informix.oledbbmk to public;

-- data types (types in <> are not included in PROVIDER_TYPES)
create table informix.oledbtypes(
	typename varchar(128),
	datatype smallint,
	columnsize int,
	literalprefix varchar(40),
	literalsuffix varchar(40),
	createparams varchar(18),
	isnullable varchar(5),
	casesensitive varchar(5),
	searchable int,
	unsignedattribute varchar(5),
	fixedprecscale varchar(5),
	autouniquevalue varchar(5),
	minimumscale smallint,
	maximumscale smallint,
	islong varchar(5),
	bestmatch varchar(5),
	is_fixedlength varchar(5),
	xstid int, -- xid shifted + tid
	udonly varchar(5),
	primary key(xstid));
insert into informix.oledbtypes values ( 
	'character', 129, 32767, '''', '''', 'length', 'True', 'True', 
	4, null, 'False', 'False', null, null, 'False', 'True', 'True', 0, 'False' );
insert into informix.oledbtypes values ( 
	'smallint', 2, 5, null, null, null, 'True', 'False',
	3, 'False', 'True', 'False', null, null, 'False', 'True', 'True', 1, 'False' );
insert into informix.oledbtypes values ( 
	'integer', 3, 10, null, null, null, 'True', 'False',
	3, 'False', 'True', 'False', null, null, 'False', 'True', 'True', 2, 'False' );
insert into informix.oledbtypes values ( 
	'float', 5, 15, null, null, null, 'True', 'False',
	3, 'False', 'False', 'False', null, null, 'False', 'True', 'True', 3, 'False' );
insert into informix.oledbtypes values ( 
	'smallfloat', 4, 6, null, null, null, 'True', 'False', 
	3, 'False', 'False', 'False', null, null, 'False', 'True', 'True', 4, 'False' );
insert into informix.oledbtypes values ( 
	'decimal', 139, 32, null, null, 'precision', 'True', 'False',
	3, 'False', 'False', 'False', null, null, 'False', 'True', 'False', 5, 'False' );
insert into informix.oledbtypes values ( 
	'serial', 3, 10, null, null, null, 'False', 'False',
	3, 'False', 'True', 'True', null, null, 'False', 'False', 'True', 6, 'False' );
insert into informix.oledbtypes values ( 
	'date', 133, 4, '''', '''', null, 'True', 'False',
	3, null, 'False', 'False', null, null, 'False', 'True', 'True', 7, 'False' );
insert into informix.oledbtypes values(
	'money', 131, 32, null, null, 'precision,scale', 'True', 'False',
	3, 'False', 'True', 'False', 0, 32, 'False', 'True', 'True', 8, 'False' );
insert into informix.oledbtypes values (
	'datetime year to fraction(5)', 135, 25, '''', '''', null, 'True', 'False',
	3, null, 'False', 'False', null, null, 'False', 'True', 'True', 10, 'False' );
insert into informix.oledbtypes values (
	'<byte>', 128, 2147483647, null, null, null, 'False', 'False',
	1, null, 'False', 'False', null, null, 'False', 'False', 'False', 11, 'False' );
insert into informix.oledbtypes values (
	'<text>', 129, 2147483647, null, null, null, 'False', 'False',
	1, null, 'False', 'False', null, null, 'False', 'False', 'False', 12, 'False' );
insert into informix.oledbtypes values ( 
	'varchar', 129, 255, '''', '''', 'max length', 'True', 'True',  
	4, null, 'False', 'False', null, null, 'False', 'False', 'False', 13, 'False' );
insert into informix.oledbtypes values ( 
	'interval day to fraction(4)', 131, 12, null, null, null,  'True', 'False',
	3, null, 'True', 'False', 1, 5, 'False', 'False', 'True', 14, 'False' );
insert into informix.oledbtypes values ( 
	'nchar', 129, 255, '''', '''', 'length', 'True', 'True',
	4, null, 'False', 'False', null, null, 'False', 'False', 'True', 15, 'False' );
insert into informix.oledbtypes values ( 
	'nvarchar', 129, 255, '''', '''', 'max length', 'True', 'True', 
	4, null, 'False',  'False', null, null, 'False', 'False', 'False', 16, 'False' );
insert into informix.oledbtypes values ( 
	'int8', 20, 19, null, null, null, 'True', 'False',
	3, 'False', 'True', 'False', null, null, 'False', 'True', 'True', 17, 'True' );
insert into informix.oledbtypes values ( 
	'serial8', 20, 19, null, null, null, 'False', 'False',
	3, 'False', 'True', 'True', null, null, 'False', 'False', 'True', 18, 'True' );
insert into informix.oledbtypes values (
	'lvarchar', 129, 2048, '''', '''', null, 'True', 'True',
	4, null, 'False', 'False', null, null, 'False', 'False', 'False', 296, 'True' );
insert into informix.oledbtypes values (
	'boolean', 11, 1, 'sysmaster:informix.oledbmakebool(''', ''')', null, 'True', 'False',
	3, null, 'False', 'False', null, null, 'False', 'True', 'True', 1321, 'True' );
insert into informix.oledbtypes values( 
	'<blob>', 128, 2147483647, null, null, null, 'True', 'False',
	1, null, 'False', 'False', null, null, 'True', 'False', 'False', 2601, 'True' );
insert into informix.oledbtypes values( 
	'<clob>', 129, 2147483647, null, null, null, 'True', 'False',
	1, null, 'False', 'False', null, null, 'True', 'False', 'False', 2857, 'True' );
insert into informix.oledbtypes values (
	'<udtvar>', 128, 0, '''', '''', null, 'True', 'True',
	1, null, 'False', 'False', null, null, 'False', 'False', 'False', 40, 'True' );
insert into informix.oledbtypes values (
	'<udtfixed>', 128, 0, '''', '''', null, 'True', 'False',
	1, null, 'False', 'False', null, null, 'False', 'False', 'True', 41, 'True' );
insert into informix.oledbtypes values (
	'<set>', 12, 0, '''', '''', null, 'True', 'True',
	1, null, 'False', 'False', null, null, 'False', 'False', 'False', 19, 'True' );
insert into informix.oledbtypes values (
	'<multiset>', 12, 0, '''', '''', null, 'True', 'True',
	1, null, 'False', 'False', null, null, 'False', 'False', 'False', 20, 'True' );
insert into informix.oledbtypes values (
	'<list>', 12, 0, '''', '''', null, 'True', 'True',
	1, null, 'False', 'False', null, null, 'False', 'True', 'False', 21, 'True' );
insert into informix.oledbtypes values (
	'<row>', 12, 0, '''', '''', null, 'True', 'True',
	1, null, 'False', 'False', null, null, 'False', 'False', 'False', 22, 'True' );
revoke all on informix.oledbtypes from public;
grant select on informix.oledbtypes to public;

-- storage and OLE objects (used only with UD)
create table informix.oledboleobjects(
	database varchar(32) not null,
	owner varchar(32) not null,
	name varchar(128) not null,
	comadapter varchar(5) not null,	
	persiststorage varchar(5) not null,		
	progid varchar(128),
	primary key(database, owner, name));
revoke all on informix.oledboleobjects from public;
grant select on informix.oledboleobjects to public;

-- privilege types
create table informix.oledbprivtypes(
	code char(1),
	name varchar(16),
	is_grantable varchar(5),
	primary key(code));
insert into informix.oledbprivtypes values ('s', 'SELECT', 'False');
insert into informix.oledbprivtypes values ('S', 'SELECT', 'True');
insert into informix.oledbprivtypes values ('u', 'UPDATE', 'False');
insert into informix.oledbprivtypes values ('U', 'UPDATE', 'True');
insert into informix.oledbprivtypes values ('i', 'INSERT', 'False');
insert into informix.oledbprivtypes values ('I', 'INSERT', 'True');
insert into informix.oledbprivtypes values ('d', 'DELETE', 'False');
insert into informix.oledbprivtypes values ('D', 'DELETE', 'True');
insert into informix.oledbprivtypes values ('r', 'REFERENCES', 'False');
insert into informix.oledbprivtypes values ('R', 'REFERENCES', 'True');
revoke all on informix.oledbprivtypes from public;
grant select on informix.oledbprivtypes to public;

-- constraint types
create table informix.oledbconstrtypes(
	code char(1),
	name varchar(16),
	isprimary varchar(5),
	isunique varchar(5),
	primary key(code));
insert into informix.oledbconstrtypes values('C', 'CHECK', 'False', 'False');
insert into informix.oledbconstrtypes values('N', 'CHECK', 'False', 'False');
insert into informix.oledbconstrtypes values('P', 'PRIMARY KEY', 'True', 'True');
insert into informix.oledbconstrtypes values('F', 'FOREIGN KEY', 'False', 'False');
insert into informix.oledbconstrtypes values('U', 'UNIQUE', 'False', 'True');
revoke all on informix.oledbconstrtypes from public;
grant select on informix.oledbconstrtypes to public;

-- constraint deferral status (we consider filtering non-deferred)
create table informix.oledbconstrdefers(
	objstate char(1),
	isdeferred varchar(5),
	deferrable varchar(5),
	issystab varchar(5),
	code smallint,
	primary key(objstate, issystab));
insert into informix.oledbconstrdefers values('D', 'True', 'True', 'False', 2);
insert into informix.oledbconstrdefers values('E', 'False', 'True', 'False', 3);
insert into informix.oledbconstrdefers values('E', 'False', 'False', 'True', 1);
insert into informix.oledbconstrdefers values('F', 'False', 'True', 'False', 1);
revoke all on informix.oledbconstrdefers from public;
grant select on informix.oledbconstrdefers to public;

-- usage types
create table informix.oledbusagetypes(
	tabname char(18),
	usagetype varchar(18),
	primary key(tabname));
insert into informix.oledbusagetypes values( ' GL_CTYPE', 'CHARACTER SET' );
insert into informix.oledbusagetypes values( ' GL_COLLATE', 'COLLATION' );
revoke all on informix.oledbusagetypes from public;
grant select on informix.oledbusagetypes to public;

-- procedure parameter ordinals
create table informix.oledbordinals(
	ordinal int,
	primary key(ordinal));
insert into informix.oledbordinals values( 1 );
insert into informix.oledbordinals values( 2 );
insert into informix.oledbordinals values( 3 );
insert into informix.oledbordinals values( 4 );
insert into informix.oledbordinals values( 5 );
insert into informix.oledbordinals values( 6 );
insert into informix.oledbordinals values( 7 );
insert into informix.oledbordinals values( 8 );
insert into informix.oledbordinals values( 9 );
insert into informix.oledbordinals values( 10 );
insert into informix.oledbordinals values( 11 );
insert into informix.oledbordinals values( 12 );
insert into informix.oledbordinals values( 13 );
insert into informix.oledbordinals values( 14 );
insert into informix.oledbordinals values( 15 );
insert into informix.oledbordinals values( 16 );
insert into informix.oledbordinals values( 17 );
insert into informix.oledbordinals values( 18 );
insert into informix.oledbordinals values( 19 );
insert into informix.oledbordinals values( 20 );
insert into informix.oledbordinals values( 21 );
insert into informix.oledbordinals values( 22 );
insert into informix.oledbordinals values( 23 );
insert into informix.oledbordinals values( 24 );
insert into informix.oledbordinals values( 25 );
insert into informix.oledbordinals values( 26 );
insert into informix.oledbordinals values( 27 );
insert into informix.oledbordinals values( 28 );
insert into informix.oledbordinals values( 29 );
insert into informix.oledbordinals values( 30 );
insert into informix.oledbordinals values( 31 );
insert into informix.oledbordinals values( 32 );
revoke all on informix.oledbordinals from public;
grant select on informix.oledbordinals to public;

-- True if system table, else False
create procedure informix.oledbissystab(tabid int)
returning varchar(5);
if tabid < 100 then return 'True';
else return 'False';
end if;
end procedure;

-- True if the value is not null, else False
create procedure informix.oledbisnotnull(val char(1)) 
returning varchar(5);
if val is null then return 'False';
else return 'True';
end if;
end procedure;

-- default
create procedure informix.oledbcoldef(deftype char(1), deftext char(127))
returning varchar(127);
if deftype = 'U' then return 'USER';
elif deftype = 'C' then return 'CURRENT';
elif deftype = 'N' then return null;
elif deftype = 'T' then return 'TODAY';
elif deftype = 'S' then return DBSERVERNAME;
else return trim(deftext);
end if;
end procedure;

-- True if nullable, else False
create procedure informix.oledbcolnullable(coltype smallint)
returning varchar(5);
if mod(coltype, 512) < 256 then return 'True';
else return 'False';
end if;
end procedure;

-- length if var-len type, else null
create procedure informix.oledbcollen(oledbtype smallint, collength smallint)
returning smallint;
if oledbtype in (8, 129, 130) then return collength;
else return null;
end if;
end procedure;

-- precision if appropriate, else null
create procedure informix.oledbcolprec(oledbtype smallint, collength smallint)
returning smallint;
if oledbtype in (131, 139) then return collength / 256; 
elif oledbtype = 2 then return 5;
elif oledbtype = 3 then return 10;
elif oledbtype = 20 then return 19;
elif oledbtype = 4 then return 7;
elif oledbtype = 5 then return 15;
else return null;
end if;
end procedure;

-- scale if appropriate, else null
create procedure informix.oledbcolscale1(xstid int, collength smallint)
returning smallint;
define topart smallint;
if xstid = 8 then
	return mod(collength, 256);
elif xstid = 14 then
	let topart = mod(collength, 16);
	if( topart < 11 ) then
		return 0;
	else
		return topart - 10;
	end if;
else
	return null;
end if;
end procedure;

-- scale if appropriate, else null
-- deprecated as of 1.2
create procedure informix.oledbcolscale(oledbtype smallint, collength smallint)
returning smallint;
define colprec smallint;
if oledbtype = 131 then
	let colprec = collength / 256;
	return collength - colprec * 256;
else 
	return null;
end if;
end procedure;

-- DBCOLUMNFLAGS
create procedure informix.oledbcolflags(xstid int, coltype int)
returning int;
define flags int;
let flags = 24; -- fixed and maybe writable
if mod(coltype, 512) < 256 and xstid <> 6 and xstid <> 18 then 
	let flags = flags + 96; -- col accepts nulls
end if;
if xstid in (6, 18) then 
	let flags = flags + 248; -- serial, not writable
elif xstid in (2601, 2857) then 
	let flags = flags + 112; -- smartblob, long and var-len
elif xstid in (11, 12, 13, 16, 19, 20, 21, 22, 40, 296) then
	let flags = flags - 16; -- var-len
end if;
return flags;  
end procedure;

-- xid shifted + tid, suitable for match with oledbtypes
-- pretends that decimal is float if decasr8 = 1
create procedure informix.oledbmakexstid1(tid smallint, xid smallint, 
	decasr8 smallint)
returning int;
if 256 < mod(tid, 512) then let tid = tid - 256; end if;
if 2048 < mod(tid, 4096) then let tid = tid - 2048; end if;
if tid = 16425 then -- distinct of boolean
	return 1321;
elif tid = 8232 then -- distinct of lvarchar
	return 296;
elif tid = 4118 then -- named row
	return 22;
elif tid in (19, 20, 21, 22) then -- unnamed complex
	return tid;
elif tid = 40 and xid <> 1 then -- udtvar
	return 40;
elif tid = 41 and xid not in (5, 10, 11) then -- udtfixed
	return 41;
elif decasr8 = 1 and tid = 5 and xid = 0 then
	return 3; 
else
	return 256 * xid + tid;
end if;
end procedure;

-- xid shifted + tid, suitable for match with oledbtypes
-- deprecated as of 1.2
create procedure informix.oledbmakexstid(tid smallint, xid smallint)
returning int;
if 256 < mod(tid, 512) then let tid = tid - 256; end if;
if 2048 < mod(tid, 4096) then let tid = tid - 2048; end if;
if tid = 16425 then -- distinct of boolean
	return 1321;
elif tid = 8232 then -- distinct of lvarchar
	return 296;
elif tid = 4118 then -- named row
	return 22;
elif tid in (19, 20, 21, 22) then -- unnamed complex
	return tid;
elif tid = 40 and xid <> 1 then -- udtvar
	return 40;
elif tid = 41 and xid not in (5, 10, 11) then -- udtfixed
	return 41;
else
	return 256 * xid + tid;
end if;
end procedure;
	
-- constraint rule
create procedure informix.oledbconstrrule(code char(1))
returning varchar(10);
if code = 'C' then return 'CASCADE';
else return 'RESTRICT';
end if;
end procedure;

-- ordinal if in the key, else 0
create procedure informix.oledbkeypart(
	part1 int, part2 int, part3 int, part4 int, part5 int, part6 int, 
	part7 int, part8 int, part9 int, part10 int, part11 int, part12 int, 
	part13 int, part14 int, part15 int, part16 int, val int)
returning int;
if abs(part1) = val then return 1;
elif abs(part2) = val then return 2;
elif abs(part3) = val then return 3;
elif abs(part4) = val then return 4;
elif abs(part5) = val then return 5;
elif abs(part6) = val then return 6;
elif abs(part7) = val then return 7;
elif abs(part8) = val then return 8;
elif abs(part9) = val then return 9;
elif abs(part10) = val then return 10;
elif abs(part11) = val then return 11;
elif abs(part12) = val then return 12;
elif abs(part13) = val then return 13;
elif abs(part14) = val then return 14;
elif abs(part15) = val then return 15;
elif abs(part16) = val then return 16;
else return 0;
end if;
end procedure;

-- alternate if value is null else value
create procedure informix.oledbnvl(v varchar(127), a varchar(127))
returning varchar(127);
if (v is null) then return a;
else return v;
end if;
end procedure;

-- True if contains with check option, else False
-- deprecated as of 1.1
create procedure informix.oledbwithcheck(
	v1 char(64), v2 char(64), v3 char(64), v4 char(64))
returning varchar(5);
if (v1 like '%with_check_option%' or v2 like '%with_check_option%' 
	or v3 like '%with_check_option%' or v4 like '%with_check_option%')
then return 'True';
else return 'False';
end if;
end procedure;

-- 1.1 replacement for oledbwithcheck
create procedure informix.oledbhaswcheck(
	v1 char(64), v2 char(64), v3 char(64), v4 char(64),
	v5 char(64), v6 char(64), v7 char(64), v8 char(64),
	v9 char(64), v10 char(64), v11 char(64), v12 char(64),
	v13 char(64), v14 char(64), v15 char(64), v16 char(64))
returning varchar(5);
if (v1 like '%with_check_option%' or v2 like '%with_check_option%' 
	or v3 like '%with_check_option%' or v4 like '%with_check_option%'
	or v5 like '%with_check_option%' or v6 like '%with_check_option%'
	or v7 like '%with_check_option%' or v8 like '%with_check_option%'
	or v9 like '%with_check_option%' or v10 like '%with_check_option%'
	or v11 like '%with_check_option%' or v12 like '%with_check_option%'
	or v13 like '%with_check_option%' or v14 like '%with_check_option%'
	or v15 like '%with_check_option%' or v16 like '%with_check_option%')
then return 'True';
else return 'False';
end if;
end procedure;

-- True if index is clustered, else False
create procedure informix.oledbidxclustered(c char(1))
returning varchar(5);
if c = 'C' then return 'True';
else return 'False';
end if;
end procedure;

-- True if index is integrated, else False
create procedure informix.oledbidxintegr(tablecols int, part1 int, part2 int,
part3 int, part4 int, part5 int, part6 int, part7 int, part8 int, part9
int, part10 int, part11 int, part12 int, part13 int, part14 int, part15
int, part16 int)
returning varchar(5);
define indxcols int;
let indxcols=0;
if part1=0 then let indxcols=0;
elif part2=0 then let indxcols=1;
elif part3=0 then let indxcols=2; 
elif part4=0 then let indxcols=3; 
elif part5=0 then let indxcols=4; 
elif part6=0 then let indxcols=5; 
elif part7=0 then let indxcols=6; 
elif part8=0 then let indxcols=7; 
elif part9=0 then let indxcols=8; 
elif part10=0 then let indxcols=9; 
elif part11=0 then let indxcols=10; 
elif part12=0 then let indxcols=11; 
elif part13=0 then let indxcols=12; 
elif part14=0 then let indxcols=13; 
elif part15=0 then let indxcols=14;
elif part16=0 then let indxcols=15; 
else  let indxcols=16; 
end if;
if indxcols = tablecols then return 'True';
else return 'False';
end if;
end procedure;

-- DBPROPVAL_IN_DISALLOWNULL if primary key, else DBPROPVAL_IN_IGNORENULL
create procedure informix.oledbidxnulls(isprimary varchar(5))
returning int;
if isprimary = 'True' then return 1;
else return 2;
end if;
end procedure;

-- DB_COLLATION_DESC if desc, else DB_COLLATION_ASC
create procedure informix.oledbidxcoll( part1 int, part2 int, part3 int, part4
int, part5 int, part6 int, part7 int, part8 int, part9 int, part10 int,
part11 int, part12 int, part13 int, part14 int, part15 int, part16 int, val
int )
returning int;
define part int;
let part = 0;
if abs(part1) = val then let part = part1;
elif abs(part2) = val then let part = part2;
elif abs(part3) = val then let part = part3;
elif abs(part4) = val then let part = part4;
elif abs(part5) = val then let part = part5;
elif abs(part6) = val then let part = part6;
elif abs(part7) = val then let part = part7;
elif abs(part8) = val then let part = part8;
elif abs(part9) = val then let part = part9;
elif abs(part10) = val then let part = part10;
elif abs(part11) = val then let part = part11;
elif abs(part12) = val then let part = part12;
elif abs(part13) = val then let part = part13;
elif abs(part14) = val then let part = part14;
elif abs(part15) = val then let part = part15;
elif abs(part16) = val then let part = part16;
else return null;
end if;
if part < 0  then return 2; else return 1; end if;
end procedure;

-- True if index is unique, else False
create procedure informix.oledbidxunique(code char(1))
returning varchar(5);
if code = 'U' then return 'True';
else return 'False';
end if;
end procedure;

-- make character convertible to boolean from number or OLE boolean string
create procedure informix.oledbmakebool( v varchar(5) )
returning char(1);
if v is null then return null;
elif v = '1' or v = 'True' or v = 't' then return 't';
elif v = '0' or v = 'False' or v = 'f' then return 'f';
else raise exception -1260;
end if;
end procedure;

-- concatenate 16 check texts
create procedure informix.oledbconcatcheck(
	v1 char(32), v2 char(32), v3 char(32), v4 char(32),
	v5 char(32), v6 char(32), v7 char(32), v8 char(32),
	v9 char(32), v10 char(32), v11 char(32), v12 char(32),
	v13 char(32), v14 char(32), v15 char(32), v16 char(32))
returning char(512);
define s char(512);
if v1 is null then let v1 = ''; end if;
if v2 is null then let v2 = ''; end if;
if v3 is null then let v3 = ''; end if;
if v4 is null then let v4 = ''; end if;
if v5 is null then let v5 = ''; end if;
if v6 is null then let v6 = ''; end if;
if v7 is null then let v7 = ''; end if;
if v8 is null then let v8 = ''; end if;
if v9 is null then let v9 = ''; end if;
if v10 is null then let v10 = ''; end if;
if v11 is null then let v11 = ''; end if;
if v12 is null then let v12 = ''; end if;
if v13 is null then let v13 = ''; end if;
if v14 is null then let v14 = ''; end if;
if v15 is null then let v15 = ''; end if;
if v16 is null then let v16 = ''; end if;
let s = v1 || v2 || v3 || v4 || v5 || v6 || v7 || v8 || v9
	|| v10 || v11 || v12 || v13 || v14 || v15 || v16;
return s;
end procedure;

-- concatenate 16 view texts
create procedure informix.oledbconcatview(
	v1 char(64), v2 char(64), v3 char(64), v4 char(64),
	v5 char(64), v6 char(64), v7 char(64), v8 char(64),
	v9 char(64), v10 char(64), v11 char(64), v12 char(64),
	v13 char(64), v14 char(64), v15 char(64), v16 char(64))
returning char(1024);
define s char(1024);
if v1 is null then let v1 = ''; end if;
if v2 is null then let v2 = ''; end if;
if v3 is null then let v3 = ''; end if;
if v4 is null then let v4 = ''; end if;
if v5 is null then let v5 = ''; end if;
if v6 is null then let v6 = ''; end if;
if v7 is null then let v7 = ''; end if;
if v8 is null then let v8 = ''; end if;
if v9 is null then let v9 = ''; end if;
if v10 is null then let v10 = ''; end if;
if v11 is null then let v11 = ''; end if;
if v12 is null then let v12 = ''; end if;
if v13 is null then let v13 = ''; end if;
if v14 is null then let v14 = ''; end if;
if v15 is null then let v15 = ''; end if;
if v16 is null then let v16 = ''; end if;
let s = v1 || v2 || v3 || v4 || v5 || v6 || v7 || v8 || v9
	|| v10 || v11 || v12 || v13 || v14 || v15 || v16;
return s;
end procedure;

-- concatenate 16 procedure texts
create procedure informix.oledbconcatproc(
	v1 char(256), v2 char(256), v3 char(256), v4 char(256),
	v5 char(256), v6 char(256), v7 char(256), v8 char(256),
	v9 char(256), v10 char(256), v11 char(256), v12 char(256),
	v13 char(256), v14 char(256), v15 char(256), v16 char(256))
returning char(4096);
define s char(4096);
if v1 is null then let v1 = ''; end if;
if v2 is null then let v2 = ''; end if;
if v3 is null then let v3 = ''; end if;
if v4 is null then let v4 = ''; end if;
if v5 is null then let v5 = ''; end if;
if v6 is null then let v6 = ''; end if;
if v7 is null then let v7 = ''; end if;
if v8 is null then let v8 = ''; end if;
if v9 is null then let v9 = ''; end if;
if v10 is null then let v10 = ''; end if;
if v11 is null then let v11 = ''; end if;
if v12 is null then let v12 = ''; end if;
if v13 is null then let v13 = ''; end if;
if v14 is null then let v14 = ''; end if;
if v15 is null then let v15 = ''; end if;
if v16 is null then let v16 = ''; end if;
let s = v1 || v2 || v3 || v4 || v5 || v6 || v7 || v8 || v9
	|| v10 || v11 || v12 || v13 || v14 || v15 || v16;
return s;
end procedure;
