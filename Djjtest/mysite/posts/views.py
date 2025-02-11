"""
Connects to a SQL database using pyodbc
"""
from django.shortcuts import render ,get_object_or_404,redirect
from django.http import HttpResponse ,JsonResponse
from django.template import loader
from datetime import datetime
from . import models
import pyodbc
import pymysql
import pandas as pd
from flask import Flask ,render_template ,g
import sqlite3
import mysql.connector
import string ,array
import webbrowser
import numpy as np
from posts.models import DjDB_SID ,Member
import datetime ,time 
import calendar
from turtle import *


# Create your views here.
def overview(request):
    return HttpResponse("My First Django POST App.")

app = Flask(__name__) # __name__ 代表目前執行的模組

SQLITE_DB_PATH = 'db.db'
SQLITE_DB_SCHEMA = 'db.sqlite3'

#depond current setting status
SERVER = '<server-address>'
DATABASE = '<database-name>'
USERNAME = '<username>'
PASSWORD = '<password>'

plist=[]

def test_pyodbc_connect():
    connectionString = f'DRIVER={{ODBC Driver 18 for SQL Server}};SERVER={SERVER};DATABASE={DATABASE};UID={USERNAME};PWD={PASSWORD}'
    conn = pyodbc.connect(connectionString)
    SQL_QUERY = "Select * from [Northwind].[dbo].[Customers]"
    cursor = conn.cursor()
    cursor.execute(SQL_QUERY)
    row = cursor.fetchone()
    if row:
     return row
    else:
       raise ValueError ('There seems no operator')
    

def build_db():
  with open(SQLITE_DB_SCHEMA) as f:
    create_db_sql = f.read()
    db = sqlite3.connect(SQLITE_DB_PATH)
    with db:
     db.executescript(create_db_sql)
    with db:
     db.execute("PRAGMA foreign_keys = ON")
     db.execute(
        'INSERT INTO members (account, password) VALUES ("user", "0000")'
     )


def get_db():
     db = getattr(g, '_database', None)
     if db is None:
         db = g._database = sqlite3.connect(SQLITE_DB_PATH)
         if db is None:
             db = g._database = sqlite3.connect(SQLITE_DB_SCHEMA)
         # Enable foreign key check
         db.execute("PRAGMA foreign_keys = ON")
     return db


@app.teardown_appcontext
def close_db(exception):
    db = getattr(g, '_database', None)
    if db is not None:
        db.close()

def member_list(request):
    
    #先刪除所有的運動球員明星數據
    # Member.objects.all().delete()

    # #先展示預設範例格式以下球員
    # new_member1 = Member(
    #     firstname='Curry ',
    #     lastname='Stephen',
    #     phone=000000,
    #     joined_date='1988-03-14',
    # )

    # new_member2 = Member(
    #     firstname='LeBron ',
    #     lastname='Raymone James',
    #     phone=111111,
    #     joined_date='1984-12-30',
    # )

    # new_member3 = Member(
    #     firstname='Shohei ',
    #     lastname='Ohtani',
    #     phone=222222,
    #     joined_date='1994-07-05',
    # )

    # members_list = [new_member1, new_member2, new_member3]
    # for sqlallmember in members_list:
    #   sqlallmember.save()

    sqlallmember = Member.objects.all().values()
    template = loader.get_template('sqlite_members.html')
    context = {
    'sqlallmember': sqlallmember,
  }
    return render(request, 'sqlite_members.html', context)
    #原先使用以下做http傳輸
    # return HttpResponse(template.render(context, request))

  #method2 使用render指向html
  #  template = loader.get_template('myfirst.html')
  #  return HttpResponse(template.render())

  #method1 透過HttpResponse
  #  return HttpResponse("Hello world!")

#用來新增會員資料

def add_member(request):
  if request.method == "POST":
    # 從前端獲取新增的資料
        firstname = request.POST.get('firstname')
        lastname = request.POST.get('lastname')
        phone = request.POST.get('phone')
        joined_date = request.POST.get('joined_date')

 
        # 新增到資料庫
        new_member = Member(
            firstname=firstname,
            lastname=lastname,
            phone=phone,
            joined_date=joined_date
        )
        new_member.save()  # 儲存新會員

        # 返回新增成功的訊息
        return JsonResponse({"message": "New Sporter info added successfully!"}, status=208)
  else:
        return JsonResponse({"error": "Invalid request method."}, status=400)

def delete_member(request, sporterid):
   if request.method == 'DELETE':        
      try:
         #方法1 Member.Objects.get
        member = Member.objects.get(id=sporterid)
        # 刪除會員
        member.delete()  # Delete the member

         #方法2 使用get_object_or_404
        #  # 嘗試查找並刪除指定的 Member
        # sporterid = request.GET.get('id')
        # member = get_object_or_404(Member, id=sporterid)
        # member.delete()  # 刪除該會員
        # 返回刪除成功的訊息
        return JsonResponse({"message": "Select Sporter deleted successfully!"}, status=240)
        # return redirect('some_success_page')  # Redirect to a success page or home
      except Member.DoesNotExist:       
        return redirect('some_error_page')  # Redirect to an error page       
   else:
        return JsonResponse({'error': 'Invalid deleted request method.'}, status=400)
def hello_world(request):
    return render(request, 'myfirst.html', {
        'current_time': str(datetime.datetime.now()),
    })


def test_mysql_db_sid(request):
    mydbconn = mysql.connector.connect(
      host = "localhost",
      user="root",
      password="K@admin123456",
      database="djdb"
    )

    if mydbconn:
      print ("Connected Successfully")
      mycursor = mydbconn.cursor()
      mycursor.execute("SELECT * FROM sid")
      # The fetchone() method will return the first row of the result
      #myresult = mycursor.fetchone()

      # all records from the "t1" table, and display the result
      sid_result = mycursor.fetchall()

      for row in sid_result:
         plist.append(str(row))

      # html_table_template = """<table>
      # {}
      # </table>""" 
      
      # row_template = "<tr><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td><td>{}</td></tr>"
      
      # col_names = "[county_city]","[aqi]","[status]","[so2]","[co]","[o3]","[o3_8hr]","[pm10]"
      # html_rows = [row_template.format(*col_names)]

      # for record in myt1_result:
      #     html_rows.append(row_template.format(*record))

      # html_table = html_table_template.format('\n  '.join(html_rows))
      template = loader.get_template('mysql_db_member.html')
      context = {
        #'html_table': html_table,
         'sid_result': sid_result,
      }
      return HttpResponse(template.render(context, request))
    else:
      print ("Connection Not Established")

def Showsid(request):
   resultDisplay = DjDB_SID.objects.all()
   templatehtml = loader.get_template('mysql_db_member.html')
   return render(request,templatehtml,{'DjDB_SID':resultDisplay})

def checkDB_Connection():
   try:
    mydbconn = mysql.connector.connect(
      host = "localhost",
      user="root",
      password="K@admin123456",
      database="djdb"
    )
    return True
   except mysql.connector.errors as e:
      return False

def index(request):
    isconnect = checkDB_Connection()
    if isconnect:
      message = 'Sucessful Connected'
      sqldbconn = mysql.connector.connect(
      host = "localhost",
      user="root",
      password="K@admin123456",
      database="djdb"
    )
      
      mycursor = sqldbconn.cursor()
      sql = "select * from sid"
      mycursor.execute(sql)
      # myres 是個陣列，該陣列會讀入所有的資料 (fetchall())
      mysqlres = mycursor.fetchall()
      # mysqlres = [ (80, 'kaohsiung city', 36, 'good', 0.0, 0.11, 28.3, 25.5, 35.0),
      #   (81, 'Tainan City', 23, 'good', 0.1, 0.1, 26.0, 25.3, 24.0),
      #   (82, 'Pingtung County', 56, 'normal', 0.1, 0.1, 25.6, 26.3, 57.0),
      #            ]
    else:
       message = 'Connected NG'

    templatehtml = loader.get_template('index.html')
    now = datetime.datetime.now()
    dttotal = now.strftime("%Y-%m-%d %H:%M:%S")

    context = {
         'message': message,
         'mysqlres':mysqlres,
         'dttotal':dttotal
      }
    return HttpResponse(templatehtml.render(context, request))

