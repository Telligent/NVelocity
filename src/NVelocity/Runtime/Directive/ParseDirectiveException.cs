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

namespace NVelocity.Runtime.Directive
{
	using System;
	using System.Collections.Generic;

	/// <summary> Exception for #parse() problems
	/// *
	/// </summary>
	/// <author> <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
	/// </author>
	/// <version> $Id: ParseDirectiveException.cs,v 1.3 2003/10/27 13:54:10 corts Exp $
	///
	/// </version>
	[Serializable]
	public class ParseDirectiveException : Exception
	{
		private void InitBlock()
		{
			filenameStack = new Stack<string>();
		}

		public override string Message
		{
			get
			{
				string returnStr = string.Format("#parse() exception : depth = {0} -> {1}", depthCount, msg);

				returnStr += " File stack : ";

				try
				{
					while (!(filenameStack.Count == 0))
					{
						returnStr += (string)filenameStack.Pop();
						returnStr += " -> ";
					}
				}
				catch (Exception)
				{
				}

				return returnStr;
			}
		}

		//UPGRADE_NOTE: The initialization of  'filenameStack' was moved to method 'InitBlock'. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Stack<string> filenameStack;
		private readonly string msg = string.Empty;
		private readonly int depthCount = 0;

		/// <summary> Constructor
		/// </summary>
		internal ParseDirectiveException(string m, int i)
		{
			InitBlock();
			msg = m;
			depthCount = i;
		}

		/// <summary> Get a message.
		/// </summary>
		/// <summary> Add a file to the filename stack
		/// </summary>
		public void addFile(string s)
		{
			filenameStack.Push(s);
		}
	}
}