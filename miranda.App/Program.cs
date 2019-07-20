using System;
using System.Collections.Generic;
using miranda.runTimeUtils;
namespace miranda.App
{
    class Program
    {
        static void Main(string[] args)
        {
            List<object> obj=new List<object>();
            obj.Add("test1");
            obj.Add("test2");
            obj.Add("test3");
            
            ClassInfo inf=new ClassInfo("miranda.App","test",obj);
            var a=inf.ExecMethod("GetMsg",null);
        }
    }
    public class test
    {
        public test(string arg1,string arg2,string arg3)
        {

        }
        public string GetMsg()
        {
         return   "Hello";
        }
    }
}
