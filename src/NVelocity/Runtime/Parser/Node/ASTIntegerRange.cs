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

namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;
	using System.Collections;
				using System.Collections.Generic;

				public class ASTIntegerRange : SimpleNode
	{
		public ASTIntegerRange(int id) : base(id)
		{
		}

		public ASTIntegerRange(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>
		/// Accept the visitor.
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		/// <summary>
		/// does the real work.  Creates an Vector of Integers with the
		/// right value range
		/// </summary>
		/// <param name="context">app context used if Left or Right of .. is a ref</param>
		/// <returns>object array of Integers</returns>
		public override object Value(IInternalContextAdapter context)
		{
			// get the two range ends
			object left = GetChild(0).Value(context);
			object right = GetChild(1).Value(context);

			// if either is null, lets log and bail
			if (left == null || right == null)
			{
				runtimeServices.Error(
					string.Format(
						"{0} side of range operator [n..m] has null value. Operation not possible. {1} [line {2}, column {3}]",
						(left == null ? "Left" : "Right"), context.CurrentTemplateName, Line, Column));
				return null;
			}

			// if not an Integer, not much we can do either
			if (left is not int || right is not int)
			{
				runtimeServices.Error(
					string.Format(
						"{0} side of range operator is not a valid type. Currently only integers (1,2,3...) and Integer type is supported. {1} [line {2}, column {3}]",
						(left is not int ? "Left" : "Right"), context.CurrentTemplateName, Line, Column));

				return null;
			}

			// get the two integer values of the ends of the range
			int l = ((int)left);
			int r = ((int)right);

			// find out how many there are
			int num = Math.Abs(l - r);
			num += 1;

			// see if your increment is Pos or Neg
			int delta = (l >= r) ? -1 : 1;

			// make the vector and fill it
			List<object> foo = new(num);
			int val = l;

			for (int i = 0; i < num; i++)
			{
				foo.Add(val);
				val += delta;
			}

			return foo;
		}
	}
}