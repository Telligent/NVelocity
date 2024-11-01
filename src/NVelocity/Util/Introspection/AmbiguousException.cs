// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace NVelocity.Util.Introspection
{
	using System;

	/// <summary>  
	/// Simple distinguishable exception, used when
	/// we run across ambiguous overloading
	/// </summary>
	[Serializable]
	public class AmbiguousException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AmbiguousException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public AmbiguousException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AmbiguousException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public AmbiguousException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}