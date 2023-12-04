namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;
	using System.IO;

	/// <summary> This class is responsible for handling Escapes
	/// in VTL.
	///
	/// Please look at the Parser.jjt file which is
	/// what controls the generation of this class.
	/// *
	/// </summary>
	public class ASTEscape : SimpleNode
	{
		private string text = string.Empty;

		public ASTEscape(int id) : base(id)
		{
		}

		public ASTEscape(Parser p, int id) : base(p, id)
		{
		}

		/// <summary>Accept the visitor. *
		/// </summary>
		public override object Accept(IParserVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}

		public override object Init(IInternalContextAdapter context, object data)
		{
			text = FirstToken.Image;
			return data;
		}

		public override bool Render(IInternalContextAdapter context, TextWriter writer)
		{
			writer.Write(text);
			return true;
		}
	}
}