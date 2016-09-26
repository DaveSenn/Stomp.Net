// /*
//  * Licensed to the Apache Software Foundation (ASF) under one or more
//  * contributor license agreements.  See the NOTICE file distributed with
//  * this work for additional information regarding copyright ownership.
//  * The ASF licenses this file to You under the Apache License, Version 2.0
//  * (the "License"); you may not use this file except in compliance with
//  * the License.  You may obtain a copy of the License at
//  *
//  *     http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
// 

#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Protocol
{
    /// <summary>
    ///     Interface for a utility class used to marshal an IPrimitiveMap instance
    ///     to/from an String.
    /// </summary>
    public interface IPrimitiveMapMarshaler
    {
        #region Properties

        /// <summary>
        ///     Retreives the Name of this Marshaler.
        /// </summary>
        String Name { get; }

        #endregion

        /// <summary>
        ///     Marshals a PrimitiveMap instance to an serialized byte array.
        /// </summary>
        Byte[] Marshal( IPrimitiveMap map );

        /// <summary>
        ///     Un-marshals an IPrimitiveMap instance from a String object.
        /// </summary>
        IPrimitiveMap Unmarshal( Byte[] mapContent );
    }
}