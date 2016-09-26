

using Apache.NMS.Util;

namespace Apache.NMS
{
	public static class SessionExtensions
	{
		/// <summary>
		/// Extension function to create a text message from an object.  The object must be serializable to XML.
		/// </summary>
		public static ITextMessage CreateXmlMessage(this ISession session, object obj) => NMSConvert.SerializeObjToMessage(session.CreateTextMessage(), obj);

	    /// <summary>
		/// Extension function to get the destination by parsing the embedded type prefix.  Default is Queue if no prefix is
		/// embedded in the destinationName.
		/// </summary>
		public static IDestination GetDestination(this ISession session, string destinationName) => SessionUtil.GetDestination(session, destinationName);

	    /// <summary>
		/// Extension function to get the destination by parsing the embedded type prefix.
		/// </summary>
		public static IDestination GetDestination(this ISession session, string destinationName, DestinationType defaultType) => SessionUtil.GetDestination(session, destinationName, defaultType);

	    /// <summary>
		/// Extension function to get the destination by parsing the embedded type prefix.
		/// </summary>
		public static IQueue GetQueue(this ISession session, string queueName) => SessionUtil.GetQueue(session, queueName);

	    /// <summary>
		/// Extension function to get the destination by parsing the embedded type prefix.
		/// </summary>
		public static ITopic GetTopic(this ISession session, string topicName) => SessionUtil.GetTopic(session, topicName);

	    /// <summary>
		/// Extension function to delete the named destination by parsing the embedded type prefix.  Default is Queue if no prefix is
		/// embedded in the destinationName.
		/// </summary>
		public static void DeleteDestination(this ISession session, string destinationName) => SessionUtil.DeleteDestination(session, destinationName);

	    /// <summary>
		/// Extension function to delete the named destination by parsing the embedded type prefix.
		/// </summary>
		public static void DeleteDestination(this ISession session, string destinationName, DestinationType defaultType) => SessionUtil.DeleteDestination(session, destinationName, defaultType);

	    /// <summary>
		/// Extension function to delete the named destination by parsing the embedded type prefix.
		/// </summary>
		public static void DeleteQueue(this ISession session, string queueName) => SessionUtil.DeleteDestination(session, queueName);

	    /// <summary>
		/// Extension function to delete the named destination by parsing the embedded type prefix.
		/// </summary>
		public static void DeleteTopic(this ISession session, string topicName) => SessionUtil.DeleteDestination(session, topicName);
	}
}