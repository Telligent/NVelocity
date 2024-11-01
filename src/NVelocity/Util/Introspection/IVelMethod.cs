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

	/// <summary>  Method used for regular method invocation
	/// *
	/// $foo.bar()
	/// *
	/// *
	/// </summary>
	/// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
	/// </author>
	/// <version>  $Id: VelMethod.cs,v 1.1 2004/12/27 05:55:08 corts Exp $
	/// 
	/// </version>
	public interface IVelMethod
	{
		/// <summary>  specifies if this VelMethod is cacheable and able to be
		/// reused for this class of object it was returned for
		/// *
		/// </summary>
		/// <returns> true if can be reused for this class, false if not
		/// 
		/// </returns>
		bool Cacheable { get; }

		/// <summary>  returns the method name used
		/// </summary>
		string MethodName { get; }

		/// <summary>  returns the return type of the method invoked
		/// </summary>
		Type ReturnType { get; }

		/// <summary>  invocation method - called when the method invocation should be
		/// performed and a value returned
		/// </summary>
		object Invoke(object o, object[] paramsRenamed);
	}
}