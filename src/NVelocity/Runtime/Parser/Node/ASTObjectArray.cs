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
			int size = ChildrenCount;

			List<object> objectArray = new(size);

			for (int i = 0; i < size; i++)
				objectArray.Add(GetChild(i).Value(context));

			return objectArray;
		}
	}
}