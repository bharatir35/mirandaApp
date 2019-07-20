using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace miranda.runTimeUtils
{
    public delegate void delRuntimeEvents(object sender, RuntimeEventArg ev);
    public enum RET_CODE { RET_OK, RET_NO_DATE, RET_ERROR, RET_EXCEPTION, RET_UNKNOWN };
    public enum DATA_TYPE { DATA_CUSTOM, DATA_JSON, DATA_XML, DATA_DICT };
    public class RuntimeEventArg : System.EventArgs
    {
        RET_CODE retCode;
        Dictionary<string, string> EventDetails;
        String eventMessage;
        public RuntimeEventArg(RET_CODE ret, Dictionary<string, string> evDetails, string evMsg)
        {
            this.retCode = ret;
            this.eventMessage = evMsg;
            this.EventDetails = evDetails;
        }
        public RET_CODE ResultCode
        {
            get { return this.retCode; }
        }
        public Dictionary<string, string> EventDetail
        {
            get { return this.EventDetails; }
        }
        public string EventDescription
        {
            get { return this.eventMessage; }
        }
    }
    public class Response
    {
        RET_CODE retCode;
        DATA_TYPE dtType;
        Object data;
        public Response(RET_CODE retcode, DATA_TYPE dttype, object retData)
        {
            this.retCode = retcode;
            this.dtType = dttype;
            data = retData;
        }
    }

    public class Request
    {
        DATA_TYPE rqDataType;
        DATA_TYPE respDataType;
        Object rqData;
    }
    public class SsnContext
    {
        Dictionary<String, object> ctxData;
        public SsnContext()
        {
            ctxData = new Dictionary<string, object>();
        }
        public SsnContext(Dictionary<string, object> ssnData)
        {
            ctxData = ssnData;
        }
        public Object GetValue(string keyName)
        {
            return (ctxData == null || ctxData.Count == 0 || !ctxData.ContainsKey(keyName.ToUpper())) ? null : ctxData[keyName.ToUpper()];
        }
        public void SetValue(string keyName, object val)
        {

            if (ctxData == null) ctxData = new Dictionary<string, object>();
            if (ctxData.ContainsKey(keyName.ToUpper()))
            {
                lock (ctxData[keyName.ToUpper()])
                {
                    ctxData[keyName] = val;
                }
            }
            else
                ctxData.Add(keyName.ToUpper(), val);
        }

    }
    public class Globals
    {
        Dictionary<string, object> glValues;
        public Globals()
        {
            glValues = new Dictionary<string, object>();
        }
        public Globals(Dictionary<string, object> glItems)
        {
            glValues = glItems;
        }
        public bool RemoveAll()
        {
            if (this.glValues != null)
                this.glValues.Clear();
            return true;
        }
        public bool RemoveItem(string keyName)
        {
            if (glValues != null && glValues.ContainsKey(keyName.ToUpper()))
            {
                return glValues.Remove(keyName);
            }
            return false;
        }

        public void AddOrModify(string keyName, object val)
        {
            if (glValues == null)
                glValues = new Dictionary<string, object>();
            if (glValues.ContainsKey(keyName.ToUpper()))
            {
                glValues[keyName.ToUpper()] = val;
            }
            else
            {
                glValues.Add(keyName.ToUpper(), val);
            }
        }
    }
    public class ClassInfo
    {
        string assmName;
        string clsName;
        List<object> ctorArg;
        Object clsObj;
        public event delRuntimeEvents RuntimeExecutionEvent;
        public ClassInfo(string asmName, string cls, List<object> ctorParams)
        {
            this.assmName = asmName;
            this.clsName = cls;
            this.ctorArg = ctorParams;
            try
            {
                Assembly asm = Assembly.Load(this.assmName);
                if (asm == null)
                {
                    throw new Exception(string.Format("Assembly [{0}] could not be loaded", this.assmName));
                }
                System.Type clsType = asm.GetType(this.clsName);
                if (clsType == null)
                    clsType = asm.GetType(string.Format("{0}.{1}", this.assmName, this.clsName));
                if (clsType == null)
                    throw new Exception(string.Format("Type [{0}] could not be loaded", this.clsName));
                var ctr = clsType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Standard, GetTypesFromArg(this.ctorArg.ToArray()), null);
                if (ctr == null)
                {
                    throw new Exception(string.Format("Type [{0}] can not be initialized", this.clsName));

                }
                clsObj = ctr.Invoke(this.ctorArg.ToArray());
                if (clsObj == null)
                    throw new Exception(string.Format("Type [{0}] can not be initialized", this.clsName));

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static ClassInfo LoadFromFile(string asmPath, string cls, List<object> ctorParams)
        {
            //this.assmName = asm;
            if (!System.IO.File.Exists(asmPath))
                return null;
            try
            {
                Assembly asm = Assembly.LoadFile(asmPath);
                if (asm == null)
                {
                    throw new Exception(string.Format("Assembly [{0}] could not be loaded", asmPath));
                }
                ClassInfo inf = new ClassInfo(asm.GetName().Name, cls, ctorParams);
                return inf;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        protected Dictionary<String, string> CallerInfo([CallerMemberName] string methodName = "", [CallerFilePath] string srcFile = "", [CallerLineNumber] int codeLoc = 0)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            retVal.Add("Func", methodName);
            retVal.Add("Source", srcFile);
            retVal.Add("LineNo", string.Format("{0}", codeLoc));
            return retVal;
        }
        public Object ExecMethod(string fnName, List<object> args)
        {
            if (this.clsObj == null)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), "Invalid or Null Target Object"));
                }
                return null;
            }
            MethodInfo inf = this.clsObj.GetType().GetMethod(fnName);
            if (inf != null)
                return inf.Invoke(this.clsObj, null);
            else
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), string.Format("Invalid function:{0}", fnName)));
                }
            }
            return null;
        }
        public void SetPropertyValue(string propName, object propVal)
        {
            if (this.clsObj == null)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), string.Format("Class not initialized or null")));
                }
                return;
            }
            PropertyInfo inf = this.clsObj.GetType().GetProperty(propName);
            if (inf == null)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), string.Format("Invalid property:{0}", propName)));
                }
                return;
            }
            try
            {
                inf.SetValue(clsObj, propVal);
            }
            catch (Exception e)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), string.Format("{0}", e.Message)));
                }
                return;
            }
        }
        public object GetPropertyValue(string propName)
        {
            if (this.clsObj == null)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), string.Format("Class not initialized or null")));
                }
                return null;
            }
            PropertyInfo inf = this.clsObj.GetType().GetProperty(propName);
            if (inf == null)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), string.Format("Invalid property:{0}", propName)));
                }
                return null;
            }
            try
            {
                return inf.GetValue(clsObj);
            }
            catch (Exception e)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_ERROR, CallerInfo(), string.Format("{0}", e.Message)));
                }
                return null;
            }
        }
        protected Object[] GetObjectArrayFromList(List<Object> lstArr)
        {
            if (lstArr == null) return null;
            if (lstArr.Count == 0)
                return new object[] { null };
            return lstArr.ToArray();

        }
        protected Type[] GetTypesFromArg(object[] args)
        {
            if (args == null || args.Length == 0)
                return Type.EmptyTypes;
            List<Type> types = new List<Type>();

            foreach (object a in args)
            {
                types.Add(a.GetType());
            }
            return types.ToArray();
        }
        public System.Type ObjectType
        {
            get { return clsObj == null ? null : clsName.GetType(); }
        }
        public MemberInfo[] MemberExist(string memberName)
        {
            if (this.clsObj == null) return null;
            try
            {
                return this.clsObj.GetType().GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase);

            }
            catch (Exception e)
            {
                if (RuntimeExecutionEvent != null)
                {
                    RuntimeExecutionEvent(this, new RuntimeEventArg(RET_CODE.RET_EXCEPTION, CallerInfo(), e.Message));
                }
                return null;

            }
        }
    }
    public static class DynaCall
    {

        public static Response ExecMethod(object target, string method, Dictionary<string, object> fnArg)
        {
            if (target == null) return new Response(RET_CODE.RET_ERROR, DATA_TYPE.DATA_DICT, "");
            System.Type targetType = target.GetType();
            string[] methodArr = method.Split('.');
            MethodInfo mInf = null;
            if (methodArr.Length > 1)
            {
                FieldInfo fld = targetType.GetField(methodArr[0]);
                if (fld == null)
                {
                    return new Response(RET_CODE.RET_ERROR, DATA_TYPE.DATA_DICT, "");
                }
                mInf = fld.GetType().GetMethod(methodArr[1]);
            }
            else
            {
                mInf = targetType.GetMethod(methodArr[0]);
            }
            return null;
        }
        public static Response ExecMethod(string asm, string clsName, string method, Dictionary<string, object> fnArg)
        {
            ClassInfo inf = new ClassInfo(asm, clsName, null);
            if (inf != null)
            {
                inf.ExecMethod(method, null);
            }
            return null;
        }
    }
}
