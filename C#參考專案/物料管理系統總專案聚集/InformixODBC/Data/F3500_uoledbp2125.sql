-- uoledbp2125.sql - upgrades OLE DB Provider support from 2.10 to 2.50.
-- DBA runs this file (against sysmaster only) to upgrade provider support.
-- Changes are backwards-compatible, that is 2.10 provider can use 2.50
-- support (but 2.50 provider cannot use 2.10 support).

update informix.oledbversion set oledbver = 2;

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

-- DBCOLUMNFLAGS
drop procedure informix.oledbcolflags;
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
