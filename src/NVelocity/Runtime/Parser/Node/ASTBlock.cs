namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;
	using System.IO;

	public class ASTBlock : SimpleNode
	{
		public ASTBlock(int id) : base(id)
		{
		}

		public ASTBlock(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>
		/// Accept the visitor.
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		public override bool Render(IInternalContextAdapter context, TextWriter writer)
		{
			if (children != null)
				foreach (var node in children)
					node.Render(context, writer);

			return true;
		}
	}
}