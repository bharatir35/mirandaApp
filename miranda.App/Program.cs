using System;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using miranda.runTimeUtils;
using System.Threading.Tasks;

namespace miranda.App
{
    class Program
    {
        static void Main(string[] args)
        {
           /* List<object> obj = new List<object>();
            obj.Add("test1");
            obj.Add("test2");
            obj.Add("test3");
            ClassInfo inf = new ClassInfo("miranda.App", "test", obj);
            var a = inf.ExecMethod("GetMsg", null);
            */
            Create(args).Build().Run();
        }
        public static IWebHostBuilder Create(string[] arg)=>
        WebHost.CreateDefaultBuilder(arg)
            .UseStartup<Startup>();
        
    }


    public class test
    {
        public test(string arg1, string arg2, string arg3)
        {

        }
        public string GetMsg()
        {
            return "Hello";
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ReqMiddleWare>();
            app.Run(async (ctx)=>{await ctx.Response.WriteAsync("This is test");} );
        }
        public void ConfigureServices(IServiceCollection srvc)
        {
        }
    }
    public class ReqMiddleWare
    {
        private readonly RequestDelegate nextDel;
        public ReqMiddleWare(RequestDelegate next)
        {
            nextDel = next;
        }
        public async Task InvokeAsync(HttpContext ctx)
        {
            await nextDel(ctx);
        }
    }
}
