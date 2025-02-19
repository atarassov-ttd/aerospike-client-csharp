/* 
 * Copyright 2012-2021 Aerospike, Inc.
 *
 * Portions may be licensed to Aerospike, Inc. under one or more contributor
 * license agreements.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
namespace Aerospike.Client
{
	public sealed class ServerCommand : MultiCommand
	{
		private readonly Statement statement;

		public ServerCommand(Cluster cluster, Node node, WritePolicy policy, Statement statement) 
			: base(cluster, policy, node, false, false)
		{
			this.statement = statement;
		}

		protected internal override bool IsWrite()
		{
			return true;
		}
		
		protected internal override void WriteBuffer()
		{
			SetQuery(policy, statement, true, null);
		}

		protected internal override void ParseRow(Key key)
		{
			// Server commands (Query/Execute UDF) should only send back a return code.
			// Keep parsing logic to empty socket buffer just in case server does
			// send records back.
			for (int i = 0 ; i < opCount; i++)
			{
				int opSize = ByteUtil.BytesToInt(dataBuffer, dataOffset);
				dataOffset += 7;
				byte nameSize = dataBuffer[dataOffset++];
				int particleBytesSize = (int)(opSize - (4 + nameSize));
				dataOffset += nameSize + particleBytesSize;
			}

			if (!valid)
			{
				throw new AerospikeException.QueryTerminated();
			}
		}		
	}
}
