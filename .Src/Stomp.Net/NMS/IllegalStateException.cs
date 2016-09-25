/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
*  http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied.  See the License for the
* specific language governing permissions and limitations
* under the License.
*/

#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Apache.NMS
{
    [Serializable]
    public class IllegalStateException : NMSException
    {
        #region Ctor

        public IllegalStateException()
        {
        }

        public IllegalStateException( String message )
            : base( message )
        {
        }

        public IllegalStateException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        public IllegalStateException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        public IllegalStateException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }

        #region ISerializable interface implementation

#if !NETCF

        /// <summary>
        ///     Initializes a new instance of the IllegalStateException class with serialized data.
        ///     Throws System.ArgumentNullException if the info parameter is null.
        ///     Throws System.Runtime.Serialization.SerializationException if the class name is null or System.Exception.HResult is
        ///     zero (0).
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected IllegalStateException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

#endif

        #endregion

        #endregion
    }
}