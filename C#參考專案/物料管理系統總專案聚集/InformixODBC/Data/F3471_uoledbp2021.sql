-- uoledbp2021.sql - upgrades OLE DB Provider support from 2.0 to 2.10.
-- DBA runs this file (against sysmaster only) to upgrade provider support.
-- Changes are backwards-compatible, that is 2.0 provider can use 2.10
-- support (but 2.10 provider cannot use 2.0 support).

update informix.oledbversion set oledbver = 1;

update informix.oledbtypes set columnsize = 32676 where xstid = 0;

drop procedure informix.oledbcoldef;
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

drop procedure informix.oledbnvl;
create procedure informix.oledbnvl(v varchar(127), a varchar(127))
returning varchar(127);
if (v is null) then return a;
else return v;
end if;
end procedure;

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
