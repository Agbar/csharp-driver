﻿//
//      Copyright (C) 2012-2014 DataStax Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

namespace Cassandra
{
    internal class PrepareRequest : IRequest
    {
        public const byte OpCode = 0x09;
        /// <summary>
        /// The CQL string to be prepared
        /// </summary>
        public string Query { get; set; }

        public int ProtocolVersion { get; set; }

        public PrepareRequest(int protocolVersion, string cqlQuery)
        {
            ProtocolVersion = protocolVersion;
            Query = cqlQuery;
        }

        public RequestFrame GetFrame(short streamId)
        {
            var wb = new BEBinaryWriter();
            wb.WriteFrameHeader((byte)ProtocolVersion, 0x00, streamId, OpCode);
            wb.WriteLongString(Query);
            return wb.GetFrame();
        }
    }
}
