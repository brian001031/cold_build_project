from django.db import connections
from django.db import models
from django.core.exceptions import ValidationError

class Member(models.Model):
  firstname = models.CharField(max_length=255)
  lastname = models.CharField(max_length=255)
  phone = models.IntegerField(null=True)
  joined_date = models.DateField(null=True)

  def __str__(self) :
      return '{} {} {} {}'.format(self.firstname ,self.lastname, self.phone,self.joined_date)

class Post(models.Model):
    title = models.CharField(max_length=100)
    content = models.TextField(blank=True)
    photo = models.URLField(blank=True)
    location = models.CharField(max_length=100)
    created_at = models.DateTimeField(auto_now_add=True)

class DjDB_SID(models.Model):
     pknum = models.IntegerField(null=True)
     county_city = models.CharField(max_length=255)
     aqi = models.IntegerField(null=True)
     status = models.CharField(max_length=25)
     so2 = models.FloatField(null=True)
     co = models.FloatField(null=True)
     o3 = models.FloatField(null=True)
     o3_8hr = models.FloatField(null=True)
     pm10 = models.FloatField(null=True)

     class meta:
         db_table = "sid"