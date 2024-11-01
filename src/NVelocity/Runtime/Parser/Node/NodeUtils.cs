namespace NVelocity.Runtime.Parser.Node
{
	using Context;
	using System;
	using System.Text;

	/// <summary> Utilities for dealing with the AST node structure.
	/// *
	/// </summary>
	/// <author> <a href="mailto:jvanzyl@apache.org">Jason van Zyl</a>
	/// </author>
	/// <author> <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
	/// </author>
	/// <version> $Id: NodeUtils.cs,v 1.4 2003/10/27 13:54:10 corts Exp $
	///
	/// </version>
	public class NodeUtils
	{
		/// <summary> Collect all the &lt;SPECIAL_TOKEN&gt;s that
		/// are carried along with a token. Special
		/// tokens do not participate in parsing but
		/// can still trigger certain lexical actions.
		/// In some cases you may want to retrieve these
		/// special tokens, this is simply a way to
		/// extract them.
		/// </summary>
		public static string specialText(Token t)
		{
			string specialText = string.Empty;

			if (t.SpecialToken == null || t.SpecialToken.Image.StartsWith("##"))
			{
				return specialText;
			}

			Token specialToken = t.SpecialToken;

			while (specialToken.SpecialToken != null)
			{
				specialToken = specialToken.SpecialToken;
			}

			while (specialToken != null)
			{
				string st = specialToken.Image;

				StringBuilder sb = new();

				for (int i = 0; i < st.Length; i++)
				{
					char c = st[i];

					if (c == '#' || c == '$')
					{
						sb.Append(c);
					}

					/*
				*  more dreaded MORE hack :)
				* 
				*  looking for ("\\")*"$" sequences
				*/

					if (c == '\\')
					{
						bool ok;
						bool term = false;

						int j = i;
						for (ok = true; ok && j < st.Length; j++)
						{
							char cc = st[j];

							if (cc == '\\')
							{
								/*
				*  if we see a \, keep going
				*/
								continue;
							}
							else if (cc == '$')
							{
								/*
				*  a $ ends it correctly
				*/
								term = true;
								ok = false;
							}
							else
							{
								/*
				*  nah...
				*/
								ok = false;
							}
						}

						if (term)
						{
							string foo = st[i..j];
							sb.Append(foo);
							i = j;
						}
					}
				}

				specialText += sb.ToString();

				specialToken = specialToken.Next;
			}

			return specialText;
		}

		/// <summary>  complete node literal
		/// *
		/// </summary>
		public static string tokenLiteral(Token t)
		{
			return specialText(t) + t.Image;
		}

		/// <summary> Utility method to interpolate context variables
		/// into string literals. So that the following will
		/// work:
		/// *
		/// #set $name = "candy"
		/// $image.getURI("${name}.jpg")
		/// *
		/// And the string literal argument will
		/// be transformed into "candy.jpg" before
		/// the method is executed.
		/// </summary>
		public static string interpolate(string argStr, IContext vars)
		{
			StringBuilder argBuf = new();

			for (int cIdx = 0; cIdx < argStr.Length;)
			{
				char ch = argStr[cIdx];

				switch (ch)
				{
					case '$':
						StringBuilder nameBuf = new();
						for (++cIdx; cIdx < argStr.Length; ++cIdx)
						{
							ch = argStr[cIdx];
							if (ch == '_' || ch == '-' || char.IsLetterOrDigit(ch))
							{
								nameBuf.Append(ch);
							}
							else if (ch == '{' || ch == '}')
							{
								continue;
							}
							else
							{
								break;
							}
						}

						if (nameBuf.Length > 0)
						{
							object value = vars.Get(nameBuf.ToString());

							if (value == null)
							{
								argBuf.Append('$').Append(nameBuf.ToString());
							}
							else
							{
								//UPGRADE_TODO: The equivalent in .NET for method 'java.object.toString' may return a different value. 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="jlca1043"'
								argBuf.Append(value.ToString());
							}
						}
						break;


					default:
						argBuf.Append(ch);
						++cIdx;
						break;
				}
			}

			return argBuf.ToString();
		}
	}
}