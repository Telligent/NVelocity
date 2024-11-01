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
	using Context;
	using NVelocity.Runtime.Parser.Node;
	using Parser;
	using System;
	using System.Collections;
	using System.IO;
	using System.Text;
	using System.Collections.Generic;

	/// <summary>
	/// Macro implements the macro definition directive of VTL.
	///
	/// example :
	///
	/// #macro( isnull $i )
	/// #if( $i )
	/// $i
	/// #end
	/// #end
	///
	/// This object is used at parse time to mainly process and register the
	/// macro.  It is used inline in the parser when processing a directive.
	///
	/// </summary>
	/// <author> <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a></author>
	/// <version> $Id: Macro.cs,v 1.3 2003/10/27 13:54:10 corts Exp $</version>
	public class Macro : Directive
	{
		public override string Name
		{
			get { return "macro"; }
			set { throw new NotSupportedException(); }
		}

		public override DirectiveType Type
		{
			get { return DirectiveType.BLOCK; }
		}

		/// <summary> Return name of this directive.
		/// </summary>
		/// <summary> Return type of this directive.
		/// </summary>
		/// <summary>   render() doesn't do anything in the final output rendering.
		/// There is no output from a #macro() directive.
		/// </summary>
		public override bool Render(IInternalContextAdapter context, TextWriter writer, INode node)
		{
			// do nothing : We never render.  The VelocimacroProxy object does that
			return true;
		}

		/// <summary>
		/// Used by Parser.java to process VMs within the parsing process
		///
		/// processAndRegister() doesn't actually render the macro to the output
		/// Processes the macro body into the internal representation used by the
		/// VelocimacroProxy objects, and if not currently used, adds it
		/// to the macro Factory
		/// </summary>
		public static void processAndRegister(IRuntimeServices rs, INode node, string sourceTemplate)
		{
			// There must be at least one arg to  #macro,
			// the name of the VM.  Note that 0 following 
			// args is ok for naming blocks of HTML
			int numArgs = node.ChildrenCount;

			// this number is the # of args + 1.  The + 1
			// is for the block tree
			if (numArgs < 2)
			{
				// error - they didn't name the macro or
				// define a block
				rs.Error("#macro error : Velocimacro must have name as 1st argument to #macro()");

				return;
			}

			// get the arguments to the use of the VM
			string[] argArray = getArgArray(node);

			// now, try and eat the code block. Pass the root.
			var macroArray = getASTAsStringArray(node.GetChild(numArgs - 1));

			// make a big string out of our macro
			StringBuilder temp = new();

			foreach (var v in macroArray)
			{
				temp.Append(v);
			}

			string macroBody = temp.ToString();

			// now, try to add it.  The Factory controls permissions, 
			// so just give it a whack...
			rs.AddVelocimacro(argArray[0], macroBody, argArray, sourceTemplate);

			return;
		}

		/// <summary>  creates an array containing the literal
		/// strings in the macro argument
		/// </summary>
		private static string[] getArgArray(INode node)
		{
			// remember : this includes the block tree
			int numArgs = node.ChildrenCount;

			numArgs--; // avoid the block tree...

			string[] argArray = new string[numArgs];

			int i = 0;

			//  eat the args
			while (i < numArgs)
			{
				argArray[i] = node.GetChild(i).FirstToken.Image;

				// trim off the leading $ for the args after the macro name.
				// saves everyone else from having to do it

				if (i > 0)
				{
					if (argArray[i].StartsWith("$"))
					{
						argArray[i] = argArray[i][1..];
					}
				}

				i++;
			}

			//			if (debugMode)
			//			{
			//				Console.Out.WriteLine("Macro.getArgArray() : #args = " + numArgs);
			//				Console.Out.Write(argArray[0] + "(");
			//
			//				for (i = 1; i < numArgs; i++)
			//					Console.Out.Write(" " + argArray[i]);
			//
			//				Console.Out.WriteLine(" )");
			//			}

			return argArray;
		}

		/// <summary>  Returns an array of the literal rep of the AST
		/// </summary>
		private static List<string> getASTAsStringArray(INode rootNode)
		{
			// this assumes that we are passed in the root 
			// node of the code block
			//Token t = rootNode.FirstToken;
			Token tLast = rootNode.LastToken;

			// now, run down the part of the tree bounded by
			// our first and last tokens
			List<string> list = new();

			Token t = rootNode.FirstToken;

			while (t != tLast)
			{
				list.Add(NodeUtils.tokenLiteral(t));
				t = t.Next;
			}

			// make sure we get the last one...
			list.Add(NodeUtils.tokenLiteral(t));

			return list;
		}
	}
}