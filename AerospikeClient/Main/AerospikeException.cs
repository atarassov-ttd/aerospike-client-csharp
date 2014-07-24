/*******************************************************************************
 * Copyright 2012-2014 by Aerospike.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 ******************************************************************************/
using System;
using System.Text;

namespace Aerospike.Client
{
	/// <summary>
	/// Aerospike exceptions that can be thrown from the client.
	/// </summary>
	public class AerospikeException : Exception
	{
		private const long serialVersionUID = 1L;

		private int resultCode;

        public AerospikeException(int resultCode, string message) 
            : base(message)
		{
            this.resultCode = resultCode;
		}

        public AerospikeException(int resultCode, Exception e) 
            : base(e.Message, e)
		{
            this.resultCode = resultCode;
		}

        public AerospikeException(int resultCode) 
            : base("")
		{
            this.resultCode = resultCode;
		}

		public AerospikeException(string message, Exception e) 
            : base(message, e)
		{
		}

		public AerospikeException(string message) 
            : base(message)
		{
		}

        public AerospikeException(Exception e)
            : base(e.Message, e)
		{
		}

		public AerospikeException()
			: base("")
		{
		}

		/// <summary>
		/// Return error message string.
		/// </summary>
		public override string Message
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				string message = base.Message;
    
				if (resultCode != 0)
				{
					sb.Append("Error Code ");
					sb.Append(resultCode);
					sb.Append(": ");
    
					if (message != null && message.Length > 0)
					{
						sb.Append(message);
					}
					else
					{
						sb.Append(ResultCode.GetResultString(resultCode));
					}
				}
				else
				{
					if (message != null)
					{
						sb.Append(message);
					}
					else
					{
						sb.Append(this.GetType().FullName);
					}
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// Should connection be put back into pool.
		/// </summary>
		public bool KeepConnection()
		{
			return ResultCode.KeepConnection(resultCode);
		}

		/// <summary>
		/// Get integer result code.
		/// </summary>
		public int Result
		{
			get
			{
				return resultCode;
			}
		}

		/// <summary>
		/// Exception thrown when database request expires before completing.
		/// </summary>
		public sealed class Timeout : AerospikeException
		{
			/// <summary>
			/// Specified timeout in milliseconds.
			/// </summary>
			public int timeout;

			/// <summary>
			/// Number of attempts before failing.
			/// </summary>
			public int iterations;

			/// <summary>
			/// Number of times when no nodes could be accessed.
			/// </summary>
			public int failedNodes;

			/// <summary>
			/// Number of times a connection could not be retrieved from a connection pool.
			/// </summary>
			public int failedConns;

			/// <summary>
			/// Create timeout exception.
			/// </summary>
			public Timeout()
				: base(ResultCode.TIMEOUT)
			{
				this.timeout = -1;
			}

			/// <summary>
			/// Create timeout exception with statistics.
			/// </summary>
			public Timeout(int timeout, int iterations, int failedNodes, int failedConns)
				: base(ResultCode.TIMEOUT)
			{
				this.timeout = timeout;
				this.iterations = iterations;
				this.failedNodes = failedNodes;
				this.failedConns = failedConns;
			}

			/// <summary>
			/// Get timeout message with statistics.
			/// </summary>
			public override string Message
			{
				get
				{
					if (timeout == -1)
					{
						return base.Message;
					}
					return "Client timeout: timeout=" + timeout + " iterations=" + iterations + 
						" failedNodes=" + failedNodes + " failedConns=" + failedConns;
				}
			}
		}

		/// <summary>
		/// Exception thrown when Java serialization error occurs.
		/// </summary>
		public sealed class Serialize : AerospikeException
		{
			/// <summary>
			/// Create serialize exception.
			/// </summary>
			public Serialize(Exception e) : base(ResultCode.SERIALIZE_ERROR, e)
			{
			}

			/// <summary>
			/// Create serialize exception with additional string message.
			/// </summary>
			public Serialize(string message)
				: base(ResultCode.SERIALIZE_ERROR, message)
			{
			}
		}

		/// <summary>
		/// Exception thrown when client can't parse data returned from server.
		/// </summary>
		public sealed class Parse : AerospikeException
		{
			/// <summary>
			/// Create parse exception.
			/// </summary>
			public Parse(string message) : base(ResultCode.PARSE_ERROR, message)
			{
			}
		}

		/// <summary>
		/// Exception thrown when client can't connect to the server.
		/// </summary>
		public sealed class Connection : AerospikeException
		{
			/// <summary>
			/// Create connection exception with string message.
			/// </summary>
			public Connection(string message)
				: base(ResultCode.SERVER_NOT_AVAILABLE, message)
			{
			}

			/// <summary>
			/// Create connection exception with underlying exception.
			/// </summary>
			public Connection(Exception e)
				: base(ResultCode.SERVER_NOT_AVAILABLE, e)
			{
			}
		}

		/// <summary>
		/// Exception thrown when chosen node is not active.
		/// </summary>
		public sealed class InvalidNode : AerospikeException
		{
			/// <summary>
			/// Create invalid node exception.
			/// </summary>
			public InvalidNode() : base(ResultCode.INVALID_NODE_ERROR)
			{
			}
		}

		/// <summary>
		/// Exception thrown when scan was terminated prematurely.
		/// </summary>
		public sealed class ScanTerminated : AerospikeException
		{
			/// <summary>
			/// Create scan terminated exception.
			/// </summary>
			public ScanTerminated()
				: base(ResultCode.SCAN_TERMINATED)
			{
			}

			/// <summary>
			/// Create scan terminated exception with underlying exception.
			/// </summary>
			public ScanTerminated(Exception e)
				: base(ResultCode.SCAN_TERMINATED, e)
			{
			}
		}

		/// <summary>
		/// Exception thrown when query was terminated prematurely.
		/// </summary>
		public sealed class QueryTerminated : AerospikeException
		{
			/// <summary>
			/// Create query terminated exception.
			/// </summary>
			public QueryTerminated()
				: base(ResultCode.QUERY_TERMINATED)
			{
			}

			/// <summary>
			/// Create query terminated exception with underlying exception.
			/// </summary>
			public QueryTerminated(Exception e)
				: base(ResultCode.QUERY_TERMINATED, e)
			{
			}
		}

		/// <summary>
		/// Exception thrown when asynchronous command was rejected because the 
		/// max concurrent database commands have been exceeded.
		/// </summary>
		public sealed class CommandRejected : AerospikeException
		{
			/// <summary>
			/// Create command rejected exception.
			/// </summary>
			public CommandRejected() : base(ResultCode.COMMAND_REJECTED)
			{
			}
		}
	}
}
