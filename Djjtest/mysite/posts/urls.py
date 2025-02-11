from django.urls import path
from . import views  #引用這個資料夾中的views檔案

urlpatterns = [
    path('overview/', views.overview, name = "Index"),
    path('hello/', views.hello_world, name='hello_world'),
    path('member_list/', views.member_list, name='member_list'), # 顯示會員頁面
    path('add_member/', views.add_member, name='add_member'),  # 新增會員資料
    path('delete_member/<int:sporterid>/', views.delete_member, name='delete_member'), # 刪除會員資料 
    path('index/', views.index, name='index')    
]