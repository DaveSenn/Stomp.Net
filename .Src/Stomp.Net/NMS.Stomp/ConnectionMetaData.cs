#region Usings

using System;
using System.Reflection;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Implements the Connection Meta-Data feature for Apache.NMS.ActiveMQ
    /// </summary>
    public class ConnectionMetaData : IConnectionMetaData
    {
        #region Ctor

        public ConnectionMetaData()
        {
            var self = Assembly.GetExecutingAssembly();
            var asmName = self.GetName();

            NMSProviderName = asmName.Name;
            ProviderMajorVersion = asmName.Version.Major;
            ProviderMinorVersion = asmName.Version.Minor;
            ProviderVersion = asmName.Version.ToString();

            NMSXPropertyNames =
                new[] { "NMSXGroupID", "NMSXGroupSeq", "NMSXDeliveryCount", "NMSXProducerTXID" };

#if NETCF
            this.nmsMajorVersion = 0;
            this.nmsMinorVersion = 0;
            this.nmsVersion = "Unknown";

            return;
#else
            foreach ( var name in self.GetReferencedAssemblies() )
                if ( 0 == String.Compare( name.Name, "Apache.NMS", true ) )
                {
                    NMSMajorVersion = name.Version.Major;
                    NMSMinorVersion = name.Version.Minor;
                    NMSVersion = name.Version.ToString();

                    return;
                }

            throw new NMSException( "Could not find a reference to the Apache.NMS Assembly." );
#endif
        }

        #endregion

        public Int32 NMSMajorVersion { get; }

        public Int32 NMSMinorVersion { get; }

        public String NMSProviderName { get; }

        public String NMSVersion { get; }

        public String[] NMSXPropertyNames { get; }

        public Int32 ProviderMajorVersion { get; }

        public Int32 ProviderMinorVersion { get; }

        public String ProviderVersion { get; }
    }
}