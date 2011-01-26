using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Vsdoc_Generator {

	public static class Extensions {

		public static void AddRange<T, U>(this IDictionary<T, U> D, IEnumerable<KeyValuePair<T, U>> V) {
			foreach (var kvp in V) {
				if (D.ContainsKey(kvp.Key)) {
					throw new ArgumentException("An item with the same key has already been added.");
				}
				D.Add(kvp);
			}
		}
	}
	
	class Program {

		static XmlNodeList categories = null;
		static List<XmlNode> staticMethods = new List<XmlNode>();
		static List<XmlNode> instanceMethods = new List<XmlNode>();
		static List<XmlNode> jQuery = new List<XmlNode>();
		static Dictionary<String, String> reservedWords;

		static void Main(string[] args) {

			InitReservedWords();

			String apiXml = String.Empty;
			XmlDocument xml = new XmlDocument();

			using (WebClient client = new WebClient()) {
				apiXml = client.DownloadString("http://api.jquery.com/api");
			}

			xml.LoadXml(apiXml);

			categories = xml.SelectNodes("api/categories");

			foreach (XmlNode method in xml.SelectNodes(@"api/entries/entry[@type='method']")) {
				String name = method.Attributes["name"].Value;

				if (name == "jQuery") {
					jQuery.Add(method);
				}
				else if (name.StartsWith("jQuery.")) {
					staticMethods.Add(method);
				}
				else {
					if (!name.StartsWith("event.")) { // ignore the event.* "methods" that are in the docs. these should really be properties.
						instanceMethods.Add(method);
					}
				}

			}

			using (FileStream fs = new FileStream("jquery-1.5-vsdoc.js", FileMode.Create, FileAccess.ReadWrite)) {
				using (StreamWriter sw = new StreamWriter(fs)) {
					sw.Write(Render());
				}
			}

		}

		static String Render() {

			StringBuilder sb = new StringBuilder();
			String output = String.Empty;
			List<String> methodOutput = new List<string>();

			//&#10; = \n
			//&#09; - tab

			foreach (XmlElement method in instanceMethods) {
				methodOutput.Add(RenderMethod(method, true));
			}

			sb.Append(RenderJQuery());
			sb.Append("$.fn = $.prototype = {\n");
			sb.Append(String.Join(", ", methodOutput.ToArray()));
			sb.Append("\n};\n");

			foreach (XmlElement method in staticMethods) {
				sb.Append(RenderMethod(method, false));
			}

			return sb.ToString();
		}

		static String RenderMethod(XmlElement method, Boolean instance) {

			StringBuilder sb = new StringBuilder();
			String name = method.Attributes["name"].Value;
			String summary = RenderSummary(method, name);
			String returns = RenderReturn(method);
			String @params = RenderParams(method);
			String arguments = String.Empty;
			String opening, closing = String.Empty;
			XmlNodeList signatures = method.SelectNodes("signature");

			if (signatures.Count > 0) {
				XmlNode signature = signatures[0];
				arguments = BuildArguments(signature, false);
			}

			if (instance) {
				opening = String.Concat("\n\t", name, ": function(", arguments, "){\n");
				closing = "\t}";
			}
			else {
				opening = String.Concat("\n", name, " = function(", arguments, "){\n");
				closing = "};";
			}

			sb.Append(opening);
			sb.Append(summary);
			sb.Append(@params);
			sb.Append(returns);
			sb.Append(closing);

			return sb.ToString();
		}

		static String RenderSummary(XmlElement method, String name) {

			StringBuilder sb = new StringBuilder();
			XmlNode desc = method.SelectSingleNode("desc");
			XmlNodeList signatures = method.SelectNodes("signature");
			String methodName = name.StartsWith("jQuery") ? name : String.Concat(".", name);

			if (desc == null) {
				desc = signatures[0].SelectSingleNode("desc");
			}

			sb.Append("\t\t/// <summary>\n\t\t/// \t");
			sb.Append(desc.InnerText);
			sb.Append("\n");

			if (signatures.Count > 1) {
				sb.Append("\t\t/// \t&#10;Additional Signatures:\n");

				for (int i = 1; i < signatures.Count; i++) {
					XmlNode signature = signatures[i];
					String arguments = BuildArguments(signature, true);

					sb.Append(String.Format("\t\t/// \t&#10;&#09;{0}. {1}( {2} )\n", i.ToString(), methodName, arguments));
				}

			}

			sb.Append(String.Concat("\t\t/// \t&#10;&#10;API Reference: http://api.jquery.com/", name));
			sb.Append("\n\t\t/// </summary>\n");

			return sb.ToString();

		}

		static String RenderParams(XmlElement method) {

			StringBuilder sb = new StringBuilder();
			XmlNodeList signatures = method.SelectNodes("signature");

			if (signatures.Count == 0) {
				return String.Empty;
			}

			XmlNode signature = signatures[0];
			XmlNodeList args = signature.SelectNodes("argument");
			int dupe = 1;
			List<String> arugmentNames = new List<string>();

			foreach (XmlElement arg in args) {
				String desc = arg.SelectSingleNode("desc").InnerText;
				String type = arg.HasAttribute("type") ? arg.Attributes["type"].Value : String.Empty;
				String name = arg.Attributes["name"].Value;
				String optional = arg.HasAttribute("optional") ? " optional=\"true\"" : String.Empty;
				String integer = type.ToLower() == "integer" ? " integer=\"true\"" : String.Empty;

				//if (name == "function") {
				//  name = "method";
				//}
				name = ReplaceReservedWord(name);

				if (type == "Function" && name.Contains("(")) {
					if (name.Contains("handler")) {
						name = "handler";
					}
					else {
						name = "method";
					}
				}

				if (arugmentNames.Contains(name)) {
					name = String.Concat(name, dupe.ToString());
					dupe++;
				}

				arugmentNames.Add(name);

				String param = String.Format("\t\t///	<param name=\"{0}\" type=\"{1}\"{2}{3}>\n\t\t/// \t{4}\n\t\t/// </param>\n", name, ResolveType(type), optional, integer, desc);

				sb.Append(param);
			}

			return sb.ToString();
		}

		static String RenderReturn(XmlElement method) {
			String returnType = String.Empty;

			if (method.HasAttribute("return")) {
				returnType = method.Attributes["return"].Value;
			}

			if (String.IsNullOrEmpty(returnType)) {
				return String.Empty;
			}

			return String.Concat("\t\t/// <returns type=\"", ResolveType(returnType), "\" />\n");
		}

		static String RenderJQuery() {

			XmlElement baseNode = jQuery[0] as XmlElement;
			String arguments = BuildArguments(baseNode.SelectNodes("signature")[0], false);
			StringBuilder sb = new StringBuilder(String.Concat("var jQuery = $ = function(", arguments, "){\n"));

			// for some reason the xml documentation has the jQuery method's signatures split between three entries (for 1.4.4)
			// combine them into one entry for processing.
			for (int i = 1; i < jQuery.Count; i++) {
				foreach (XmlNode signature in jQuery[i].SelectNodes("signature")) {
					baseNode.AppendChild(signature);
				}
			}

			String summary = RenderSummary(baseNode, "jQuery");
			String returns = RenderReturn(baseNode);
			String @params = RenderParams(baseNode);

			sb.Append(summary);
			sb.Append(@params);
			sb.Append(returns);
			sb.Append("};\n");

			return sb.ToString();
		}

		static String ResolveType(String type) {

			if (type.Contains(",")) {
				return type.Split(',').Last().Trim();
			}

			if (type.Contains("/")) {
				return type.Split('/').Last().Trim();
			}

			if (type == "Callback") {
				return "Function";
			}

			if ((new[] { "Options", "Map", "Any" }).Contains(type)) {
				return "Object";
			}

			if (type.Equals("selector", StringComparison.OrdinalIgnoreCase) || type.Equals("HTML", StringComparison.OrdinalIgnoreCase)) {
				return "String";
			}

			if (type == "Integer") {
				return "Number";
			}

			if (type == "Elements") {
				return "Array";
			}

			if (type == "boolean") { // normalize mismatched cases
				return "Boolean";
			}

			return type;

		}

		static String BuildArguments(XmlNode signature, Boolean wrapOptional) {

			List<String> arguments = new List<string>();
			XmlNodeList args = signature.SelectNodes("argument");
			int dupe = 1;

			foreach (XmlElement arg in args) {
				String name = arg.Attributes["name"].Value;
				String type = arg.HasAttribute("type") ? ResolveType(arg.Attributes["type"].Value) : String.Empty;

				if (name == "function") {
					name = "method";
				}

				if (type == "Function" && name.Contains("(")) {
					if (name.Contains("handler")) {
						name = "handler";
					}
					else {
						name = "method";
					}
				}

				if (arguments.Contains(name)) {
					name = String.Concat(name, dupe.ToString());
					dupe++;
				}

				if (arg.HasAttribute("optional") && wrapOptional) {
					name = String.Concat("[", name, "]");
				}

				arguments.Add(name);
			}

			return String.Join(", ", arguments.ToArray());
		}

		static void InitReservedWords() {

			reservedWords = new Dictionary<String, String>() {
				{ "break", String.Empty },
				{ "case", "kase" },
				{ "catch", String.Empty },
				{ "continue", String.Empty },
				{ "debugger", String.Empty },
				{ "default", String.Empty },
				{ "delete", String.Empty },
				{ "do", String.Empty },
				{ "else", String.Empty },
				{ "finally", String.Empty },
				{ "for", "_for" },
				{ "function", "method" },
				{ "if", String.Empty },
				{ "in", String.Empty },
				{ "instanceof", String.Empty },
				{ "new", String.Empty },
				{ "return", String.Empty },
				{ "switch", String.Empty },
				{ "this", String.Empty },
				{ "throw", String.Empty },
				{ "try", String.Empty },
				{ "typeof", String.Empty },
				{ "var", String.Empty },
				{ "void", String.Empty },
				{ "while", String.Empty },
				{ "with", String.Empty },
				{ "class", "klass" },
				{ "enum", String.Empty },
				{ "export", String.Empty },
				{ "extends", String.Empty },
				{ "import", String.Empty },
				{ "super", String.Empty },
				{ "implements", String.Empty },
				{ "interface", String.Empty },
				{ "let", String.Empty },
				{ "package", String.Empty },
				{ "private", String.Empty },
				{ "protected", String.Empty },
				{ "public", String.Empty },
				{ "static", String.Empty },
				{ "yield", String.Empty }
			};

		}

		/// <summary>
		/// Resolves any conflicts with reserved words.
		/// The jQuery docs aren't exactly consistent, many functions in the docs use reserve words for parameter names.
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		static String ReplaceReservedWord(String word) {

			String result = word;

			if (reservedWords.ContainsKey(word)) {
				String replacement = reservedWords[word];

				if (String.IsNullOrEmpty(replacement)) {
					replacement = String.Concat("_", word);
				}

				result = replacement;
			}

			return result;

		}
	}

}
