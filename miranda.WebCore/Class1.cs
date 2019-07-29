using System;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading.Tasks;

namespace miranda.WebCore
{
    public class CustRqHandler
    {
        private readonly RequestDelegate _next;
        public CustRqHandler(RequestDelegate next)
        {
                _next=next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
           if(context.Request.Method.ToUpper()=="GET")
           {
               
           }
           else
           {

           }
           await _next(context);
        }
    }
}
