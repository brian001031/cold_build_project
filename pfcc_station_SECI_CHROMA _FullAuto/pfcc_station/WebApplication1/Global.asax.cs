using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace WebApplication1
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 應用程式啟動時執行的程式碼
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // 获取当前请求的查询字符串
            string queryString = HttpContext.Current.Request.QueryString.ToString();

            // 检查是否包含 flag=PF_CC12_Default 参数
            if (queryString.Contains("flag=PF_CC12_Default"))
            {
                // 构建新的查询字符串
                string newQueryString = queryString.Replace("flag=PF_CC12_Default", "side=PF_CC12_Default");
                
                // 重定向到新的 URL
                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsolutePath + "?" + newQueryString);
            }
        }
    }
}