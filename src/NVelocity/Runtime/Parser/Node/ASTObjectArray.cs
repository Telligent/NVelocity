namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class ASTObjectArray : SimpleNode
	{
		public ASTObjectArray(int id) : base(id)
		{
		}

		public ASTObjectArray(Parser p, int id) : base(p, id)
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
			List<object> objectArray = new(ChildrenCount);

			if (children != null)
				foreach (var node in children)
					objectArray.Add(node.Value(context));

			return objectArray;
		}
	}
}