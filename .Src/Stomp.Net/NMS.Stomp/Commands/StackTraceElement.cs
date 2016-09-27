#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public struct StackTraceElement
    {
        public String ClassName;
        public String FileName;
        public String MethodName;
        public Int32 LineNumber;
    }
}