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

	public class ASTModNode : SimpleNode
	{
		public ASTModNode(int id) : base(id)
		{
		}

		public ASTModNode(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>
		/// Accept the visitor.
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		public override object Value(IInternalContextAdapter context)
		{
			// get the two args
			object left = GetChild(0).Value(context);
			object right = GetChild(1).Value(context);

			// if either is null, lets log and bail
			if (left == null || right == null)
			{
				runtimeServices.Error(
					string.Format(
						"{0} side ({1}) of modulus operation has null value. Operation not possible. {2} [line {3}, column {4}]",
						(left == null ? "Left" : "Right"), GetChild((left == null ? 0 : 1)).Literal, context.CurrentTemplateName, Line,
						Column));
				return null;
			}

			// if not an Integer, not much we can do either
			//			if (!(left is Int32) || !(right is Int32))
			//			{
			//				runtimeServices.Error((!(left is Int32) ? "Left" : "Right") + " side of modulus operation is not a valid type. " + "Currently only integers (1,2,3...) and Integer type is supported. " + context.CurrentTemplateName + " [line " + Line + ", column " + Column + "]");
			//
			//				return null;
			//			}

			Type maxType = MathUtil.ToMaxType(left.GetType(), right.GetType());

			if (maxType == null)
			{
				return null;
			}

			return MathUtil.Mod(maxType, left, right);


			// check for divide by 0
			//			if (((Int32) right) == 0)
			//			{
			//				runtimeServices.Error("Right side of modulus operation is zero. Must be non-zero. " + context.CurrentTemplateName + " [line " + Line + ", column " + Column + "]");
			//
			//				return null;
			//			}
			//
			//			return ((Int32) left)%((Int32) right);
		}
	}
}