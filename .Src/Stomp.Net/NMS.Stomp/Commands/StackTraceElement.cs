#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public struct StackTraceElement
    {
        public String ClassName { get; set; }
        public String FileName { get; set; }
        public String MethodName { get; set; }
        public Int32 LineNumber { get; set; }
    }
}