namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// AST Node for creating a map / dictionary.
	/// This class was originally generated from Parset.jjt.
	/// </summary>
	/// <version>$Id: ASTMap.cs,v 1.2 2004/12/27 05:50:11 corts Exp $</version>
	public class ASTMap : SimpleNode
	{
		public ASTMap(int id) : base(id)
		{
		}

		public ASTMap(Parser p, int id) : base(p, id)
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
		/// Evaluate the node.
		/// </summary>
		public override object Value(IInternalContextAdapter context)
		{
			int size = ChildrenCount;

			var objectMap = new Dictionary<object, object>();

			for (int i = 0; i < size; i += 2)
			{
				SimpleNode keyNode = (SimpleNode)GetChild(i);
				SimpleNode valueNode = (SimpleNode)GetChild(i + 1);

				object key = (keyNode?.Value(context));
				object value = (valueNode?.Value(context));

				objectMap.Add(key, value);
			}

			return objectMap;
		}
	}
}