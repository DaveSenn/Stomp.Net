

using System;

namespace Apache.NMS
{
    /// <summary>
    ///     Provides information describing the NMS IConnection instance.
    /// </summary>
    public interface IConnectionMetaData
    {
        #region Properties

        /// <value>
        ///     Get the Major version number of the NMS API this Provider supports.
        /// </value>
        Int32 NMSMajorVersion { get; }

        /// <value>
        ///     Get the Minor version number of the NMS API this Provider supports.
        /// </value>
        Int32 NMSMinorVersion { get; }

        /// <value>
        ///     Get the name of this NMS Provider.
        /// </value>
        String NMSProviderName { get; }

        /// <value>
        ///     Gets a formatted string detailing the NMS API version this Provider supports.
        /// </value>
        String NMSVersion { get; }

        /// <value>
        ///     Gets a String array of all the NMSX property names this NMS Provider supports.
        /// </value>
        String[] NMSXPropertyNames { get; }

        /// <value>
        ///     Gets the Providers Major version number.
        /// </value>
        Int32 ProviderMajorVersion { get; }

        /// <value>
        ///     Gets the Providers Minor version number.
        /// </value>
        Int32 ProviderMinorVersion { get; }

        /// <value>
        ///     Gets a formatted string detailing the version of this NMS Provider.
        /// </value>
        String ProviderVersion { get; }

        #endregion
    }
}