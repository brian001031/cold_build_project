













drop table regress_ado;






create table regress_ado
(
    c1_char char(60),
    c2_nchar nchar(60),
    c3_vchar varchar(60),
    c4_nvchar nvarchar(60),
    c5_dec decimal(29,3),
    c6_int int,
    c7_serial serial,
    c8_smlint smallint,
    c9_money money(29,3),
    c10_float float(14),
    c11_numeric numeric, -- default precision and scale
    c12_smallfloat smallfloat, 
    c13_date date,
    c14_datetime datetime year to fraction(5),
    c15_date_interv interval day(9) to second


















);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv











)
values
(
        "char value",     
        "nchar value",     
        "varchar value",  
        "nvarchar value",  
        1.1,              
        1,                
        1,                
        1.1,                
        1.1,              
        1.1,              
        1.1,              
        '02/13/1967',     
        '1998-02-01 12:42:06.999',  
        '1 3:9:5'	  











);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv







)
values
(
        "Max numbers of characters that can be stored in this column.", 
        "Max numbers of characters that can be stored in this column.", 
        "Max numbers of characters that can be stored in this column.", 
        "Max numbers of characters that can be stored in this column.", 
        99.999,          
        2147483647,      
        32767,           
        1.1, -- 99.09,          
        1e124,
        3.33333,      
        3.33333e38,      
        '12/31/9999',	 
        '9999-12-01 00:00:00.000', 
        '1 23:59:59'    







);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        "",          
        "",          
        "", 
        "", 
        0,
        0,
        0,
        1.1, --0,
        0, 
        0,
        0,
        0,			   
        '1899-12-31 00:00:00.000',	   
        '1 00:00:00'	   



);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv







)
values
(
        "negtive data, NA", 
        "negtive data, NA", 
        "negtive data, NA", 
        "negtive data, NA", 
        -99.999,
        -2147483647,
        -32767,
        1.1,  -- 99.00,
        -1e124,
        -3.33333,
        -3.33333e38,
        '01/01/1899',
        '1899-01-01 00:00:00.000',	   
        '-1 23:59:59'







);


insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        null, 
        null, 
        null, 
        null, 
        null,
        null,
        null,
        1.1, -- null,
        null,
        null,
        null,
        null,
        null,
        null



);


insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        "c", 
        null,  
        "c", 
        null,  
        1.1,
        null,
        100,
        1.1, -- 10.1,
        1.1, 
        -3.33333,
        -3.33333e38,
        '02/13/1967',			   
        '1908-02-01 12:42:06.999',	   
        '100 9:9:9'	   



);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        null, 
        null, 
        "48 characters are stored in this column.", 
        "48 characters are stored in this column.", 
        1.01,
        2647,
        3267,
        1.1, -- 9.998,
        0.69999e120,
        4.8899,
        4.8899e31,
        '12/31/9999',			   
        '9999-12-01 00:00:00.000',	   
        null



);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        "also, two tab chars			are stored.", 
        "1200004387415230202704752702027220", 
        "also, two tab chars			are stored.", 
        "1200004387415230202704752702027220", 
        1.01,
        7,
        37,
        9.1,
        1e2, 
        4.801,
        4.801e31,
        '12/31/9999',			   
        '9999-12-01 00:00:00.000',	   
        '0 00:00:00'	   



);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        "Followed are two tab chars,		", 
        "11\\18\\1998", 
        "Followed are two tab chars,		", 
        "11\\18\\1998", 
        0.01,
        70,
        -1,
        98.1,
        1e-129,
        81,
        81e-31,
        '12/31/1976',			   
        '1979-12-01 00:00:00.000',	   
        '100 14:00:45'	   



);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        "1\1\1998", 
        "NULL", 
        "1\1\1998", 
        "NULL", 
        0.001,
        70,
        -3,
        94.1,
        1e-3,
        81,
        81e-1,
        '12/31/1976',			   
        '1979-12-01 00:00:00.000',	   
        '100 13:19:11'	   



);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        "2\1\1998", 
        "NULL", 
        "3\1\1998", 
        "NULL", 
        0.002,
        71,
        -4,
        95.1,
        2e-3,
        82,
        82e-1,
        '12/31/1977',			   
        '1979-12-02 00:00:00.000',	   
        '100 13:18:11'	   



);

insert into regress_ado 
(
      c1_char,
      c2_nchar,
      c3_vchar,
      c4_nvchar,
      c5_dec,
      c6_int,
      c8_smlint,
      c9_money,
      c10_float,
      c11_numeric,
      c12_smallfloat,
      c13_date,
      c14_datetime,
      c15_date_interv



)
values
(
        "3\1\1998", 
        "NULL", 
        "4\1\1998", 
        "NULL", 
        0.012,
        69,
        -1,
        94.1,
        3e-3,
        79,
        72e-1,
        '11/27/1977',			   
        '1978-12-02 00:00:00.000',	   
        '99 13:18:11'	   



);

create view regress_ado_empty
as select * from regress_ado where 1 = 2;
