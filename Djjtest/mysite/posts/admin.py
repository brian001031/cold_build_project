from django.contrib import admin
from .models import Post

class AuthorAdmin(admin.ModelAdmin):
    pass

admin.site.register(Post)