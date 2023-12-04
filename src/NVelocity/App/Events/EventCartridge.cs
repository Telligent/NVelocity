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

namespace NVelocity.App.Events
{
	using Context;
	using Exception;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// 'Package' of event handlers...
	/// </summary>
	public class EventCartridge
	{
		public event EventHandler<ReferenceInsertionEventArgs> ReferenceInsertion;
		public event NullSetEventHandler NullSet;
		public event EventHandler<MethodExceptionEventArgs> MethodExceptionEvent;

		/// <summary>
		/// Called during Velocity merge before a reference value will
		/// be inserted into the output stream.
		/// </summary>
		/// <param name="referenceStack">the stack of objects used to reach this reference</param>
		/// <param name="reference">reference from template about to be inserted</param>
		/// <param name="value"> value about to be inserted (after toString() )</param>
		/// <returns>
		/// object on which toString() should be called for output.
		/// </returns>
		internal object ReferenceInsert(Stack<object> referenceStack, string reference, object value)
		{
			if (ReferenceInsertion != null)
			{
				ReferenceInsertionEventArgs args = new(referenceStack, reference, value);
				ReferenceInsertion(this, args);
				value = args.NewValue;
			}

			return value;
		}

		/// <summary>
		/// Called during Velocity merge to determine if when
		/// a #set() results in a null assignment, a warning
		/// is logged.
		/// </summary>
		/// <returns>true if to be logged, false otherwise</returns>
		internal bool ShouldLogOnNullSet(string lhs, string rhs)
		{
			if (NullSet == null)
			{
				return true;
			}

			NullSetEventArgs e = new(lhs, rhs);
			NullSet(this, e);

			return e.ShouldLog;
		}

		/// <summary>
		/// Called during Velocity merge if a reference is null
		/// </summary>
		/// <param name="type">Class that is causing the exception</param>
		/// <param name="method">method called that causes the exception</param>
		/// <param name="e">Exception thrown by the method</param>
		/// <returns>object to return as method result</returns>
		/// <exception cref="Exception">exception to be wrapped and propagated to app</exception>
		internal object HandleMethodException(Type type, string method, Exception e)
		{
			// if we don't have a handler, just throw what we were handed
			if (MethodExceptionEvent == null)
			{
				throw new VelocityException(e.Message, e);
			}

			MethodExceptionEventArgs mea = new(type, method, e);
			MethodExceptionEvent(this, mea);

			if (mea.ValueToRender == null)
			{
				throw new VelocityException(e.Message, e);
			}
			else
			{
				return mea.ValueToRender;
			}
		}

		/// <summary>
		/// Attached the EventCartridge to the context
		/// </summary>
		/// <param name="context">context to attach to</param>
		/// <returns>true if successful, false otherwise</returns>
		public bool AttachToContext(IContext context)
		{
			if (context is not IInternalEventContext internalEventContext)
			{
				return false;
			}
			else
			{
				internalEventContext.AttachEventCartridge(this);
				return true;
			}
		}
	}
}