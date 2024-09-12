from django.urls import path
from . import views  #引用這個資料夾中的views檔案

urlpatterns = [
    path('overview/', views.overview, name = "Index"),
    path('hello/', views.hello_world, name='hello_world'),
    path('members/', views.members, name='members'),
    path('index/', views.index, name='index')
]