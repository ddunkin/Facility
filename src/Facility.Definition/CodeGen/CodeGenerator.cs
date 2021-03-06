﻿using System;
using System.IO;

namespace Facility.Definition.CodeGen
{
	/// <summary>
	/// Base class for code generators.
	/// </summary>
	public abstract class CodeGenerator
	{
		/// <summary>
		/// The name of the generator (for comments).
		/// </summary>
		public string GeneratorName { get; set; }

		/// <summary>
		/// The text to use for each indent (null for tab).
		/// </summary>
		public string IndentText { get; set; }

		/// <summary>
		/// The text to use for each new line (null for default).
		/// </summary>
		public string NewLine { get; set; }

		/// <summary>
		/// Generates output for the specified service.
		/// </summary>
		public CodeGenOutput GenerateOutput(ServiceInfo service) => GenerateOutputCore(service);

		/// <summary>
		/// Generates output for the specified service.
		/// </summary>
		protected abstract CodeGenOutput GenerateOutputCore(ServiceInfo service);

		/// <summary>
		/// Creates a text source from a name and code writer.
		/// </summary>
		protected NamedText CreateNamedText(string name, Action<CodeWriter> writeTo)
		{
			using (var stringWriter = new StringWriter())
			{
				if (NewLine != null)
					stringWriter.NewLine = NewLine;

				var code = new CodeWriter(stringWriter);

				if (IndentText != null)
					code.IndentText = IndentText;

				writeTo(code);

				return new NamedText(name, stringWriter.ToString());
			}
		}
	}
}
