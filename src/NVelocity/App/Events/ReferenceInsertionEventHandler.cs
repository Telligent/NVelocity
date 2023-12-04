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
	using System;
	using System.Collections.Generic;

	public class ReferenceInsertionEventArgs : EventArgs
	{
		private readonly Stack<object> referenceStack;
		private readonly object originalValue;
		private object newValue;
		private readonly string rootString;

		public ReferenceInsertionEventArgs(Stack<object> referenceStack, string rootString, object value)
		{
			this.rootString = rootString;
			this.referenceStack = referenceStack;
			originalValue = newValue = value;
		}

		public Stack<object> GetCopyOfReferenceStack()
		{
			var a = new object[referenceStack.Count];
			referenceStack.CopyTo(a, 0);
			Array.Reverse(a);
			return new(a);
		}

		public string RootString
		{
			get { return rootString; }
		}

		public object OriginalValue
		{
			get { return originalValue; }
		}

		public object NewValue
		{
			get { return newValue; }
			set { newValue = value; }
		}
	}

	///// <summary>
	///// Reference 'Stream insertion' event handler.  Called with object
	///// that will be inserted into stream via value.toString().
	///// Make sure you return an object that will toString() without throwing an exception.
	///// </summary>
	//public delegate void ReferenceInsertionEventHandler(object sender, ReferenceInsertionEventArgs e);
}