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

namespace NVelocity.Runtime.Visitor
{
	using Context;
	using NVelocity.Runtime.Parser.Node;
	using System;
	using System.IO;

	/// <summary> This is the base class for all visitors.
	/// For each AST node, this class will provide
	/// a bare-bones method for traversal.
	/// *
	/// </summary>
	/// <author> <a href="mailto:jvanzyl@apache.org">Jason van Zyl</a>
	/// </author>
	/// <author> <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
	/// </author>
	/// <version> $Id: BaseVisitor.cs,v 1.3 2003/10/27 13:54:11 corts Exp $
	///
	/// </version>
	public abstract class BaseVisitor : IParserVisitor
	{
		/// <summary>
		/// Context used during traversal
		/// </summary>
		protected internal IInternalContextAdapter context;

		/// <summary>
		/// Writer used as the output sink
		/// </summary>
		protected internal StreamWriter writer;

		public virtual object Visit(SimpleNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTprocess node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTExpression node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTAssignment node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTOrNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTAndNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTEQNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTNENode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTLTNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTGTNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTLENode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTGENode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTAddNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTSubtractNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTMulNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTDivNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTModNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTNotNode node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTNumberLiteral node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTStringLiteral node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTIdentifier node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTMethod node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTReference node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTTrue node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTFalse node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTBlock node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTText node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTIfStatement node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTElseStatement node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTElseIfStatement node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTComment node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTObjectArray node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTMap node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTWord node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTSetDirective node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}

		public virtual object Visit(ASTDirective node, object data)
		{
			data = node.ChildrenAccept(this, data);
			return data;
		}
	}
}