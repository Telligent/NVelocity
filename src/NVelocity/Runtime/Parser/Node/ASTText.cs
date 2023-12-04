namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;
	using System.IO;

	public class ASTText : SimpleNode
	{
		private string text;

		public ASTText(int id) : base(id)
		{
		}

		public ASTText(Parser p, int id) : base(p, id)
		{
		}

		public string Text
		{
			get { return text; }
		}

		/// <summary>
		/// Accept the visitor.
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		public override object Init(IInternalContextAdapter context, object data)
		{
			Token t = FirstToken;

			text = NodeUtils.tokenLiteral(t);

			return data;
		}

		public override bool Render(IInternalContextAdapter context, TextWriter writer)
		{
			writer.Write(text);
			return true;
		}
	}
}