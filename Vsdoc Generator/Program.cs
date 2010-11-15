using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Vsdoc_Generator {
	class Program {

		static XmlNodeList categories = null;
		static List<XmlNode> staticMethods = new List<XmlNode>();
		static List<XmlNode> instanceMethods = new List<XmlNode>();
		static List<XmlNode> jQuery = new List<XmlNode>();

		static void Main(string[] args) {

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

			using (FileStream fs = new FileStream("jquery-1.4.4-vsdoc.js", FileMode.Create, FileAccess.ReadWrite)) {
				using (StreamWriter sw = new StreamWriter(fs)) {
					sw.Write(Render());
				}
			}

		}

		static String Render() {

			StringBuilder sb = new StringBuilder("var jQuery = $ = function(){\n");
			String output = String.Empty;
			List<String> methodOutput = new List<string>();

			//&#09;

			foreach (XmlElement method in jQuery) {
				//sb.Append(RenderMethod(method));
			}

			foreach (XmlElement method in instanceMethods) {
				methodOutput.Add(RenderMethod(method, true));
			}

			
			sb.Append("\n};\n");
			sb.Append("$.prototype = {\n");
			sb.Append(String.Join(", ", methodOutput.ToArray()));
			sb.Append("\n};\n");

			foreach (XmlElement method in staticMethods) {
				sb.Append(RenderMethod(method, false));
			}

			return sb.ToString();
		}

		static String RenderJQuery() {

			return String.Empty;
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

				if (type == "Function" && name.Contains("(")) {
					if (name.Contains("handler")) {
						name = "handler";
					}
					else {
						name = "function";
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

				if (type == "Function" && name.Contains("(")) {
					if (name.Contains("handler")) {
						name = "handler";
					}
					else{
						name = "function";
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

	}
}
