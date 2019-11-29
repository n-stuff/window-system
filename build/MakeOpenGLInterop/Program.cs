using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace NStuff.MakeOpenGLInterop
{
    class Program
    {
        static void Main(string[] arguments)
        {
            Console.WriteLine("OpenGL interop source files generation...");
            if (arguments.Length < 1)
            {
                Console.WriteLine("Usage: dotnet run -p build/MakeOpenGLInterop <config-file>");
                return;
            }

            var fileInfo = new FileInfo(arguments[^1]);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("**** Cannot find " + fileInfo.FullName);
                return;
            }

            var configuration = XDocument.Load(fileInfo.FullName);
            var namePrefix = typeof(Program).Namespace + ".";
            var registry = XDocument.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "gl.xml"));
            var registryOverride = XDocument.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "gl_override.xml"));
            var destinationFolder = fileInfo.DirectoryName;

            var glinterop = configuration.Element("glinterop");
            var api = glinterop.Element("api");
            var apiName = api.Attribute("name").Value;
            var major = int.Parse(api.Element("version").Attribute("major").Value);
            var minor = int.Parse(api.Element("version").Attribute("minor").Value);

            //
            // All enums / commands for the requested version and extensions.
            //

            var allEnumNames = new HashSet<string>();
            var allCommandNames = new HashSet<string>();

            foreach (var e in registry.Element("registry").Elements("feature").Where(p => p.Attribute("api").Value == apiName))
            {
                var name = e.Attribute("name").Value;
                var featureMajor = name[^3] - '0';
                var featureMinor = name[^1] - '0';
                if (featureMajor < major || (featureMajor == major && featureMinor <= minor))
                {
                    foreach (var child in e.Elements().Where(p => p.Name.LocalName == "require" || p.Name.LocalName == "remove"))
                    {
                        RegisterFeatures(child, allEnumNames, allCommandNames);
                    }
                }
            }

            var extensions = new HashSet<string>();
            foreach (var e in glinterop.Element("extensions").Elements("extension"))
            {
                extensions.Add(e.Attribute("name").Value);
            }
            foreach (var e in registry
                .Element("registry")
                .Element("extensions")
                .Elements("extension").Where(p => extensions.Contains(p.Attribute("name").Value)))
            {
                foreach (var child in e.Elements())
                {
                    RegisterFeatures(child, allEnumNames, allCommandNames);
                }
            }

            //
            // Requested commands.
            //

            var commandNames = new HashSet<string>();
            var lazyCommandNames = new HashSet<string>();
            foreach (var e in glinterop.Element("commands").Elements("command"))
            {
                commandNames.Add(e.Attribute("name").Value);
                if (e.Attribute("binding")?.Value == "lazy")
                {
                    lazyCommandNames.Add(e.Attribute("name").Value);
                }
            }

            //
            // Command parameter types from gl_override.xml.
            //

            var groupNames = new HashSet<string> {
                "Boolean"
            };

            var groupTypes = new Dictionary<string, string> {
                ["Boolean"] = "GLboolean",
            };
            var usedTypes = new HashSet<string> {
                "GLboolean",
            };

            if (glinterop.Element("groups") != null)
            {
                foreach (var e in glinterop.Element("groups").Elements("group"))
                {
                    var name = e.Attribute("name").Value;
                    var type = e.Attribute("type").Value;
                    groupNames.Add(name);
                    groupTypes[name] = type;
                    usedTypes.Add(type);
                }
            }

            var commandOverrides = new Dictionary<string, Dictionary<string, string>>();
            foreach (var e in registryOverride.Element("registry").Element("commands").Elements("command"))
            {
                var name = e.Element("proto").Element("name").Value;
                if (!commandNames.Contains(name))
                {
                    continue;
                }
                var parameters = new Dictionary<string, string>();
                commandOverrides[name] = parameters;

                var group = e.Element("proto").Attribute("group");
                if (group != null)
                {
                    parameters[name] = group.Value;
                    groupNames.Add(group.Value);
                }
                foreach (var p in e.Elements("param"))
                {
                    group = p.Attribute("group");
                    parameters[p.Element("name").Value] = group.Value;
                    groupNames.Add(group.Value);
                }
            }

            //
            // Enum names from gl_override.xml.
            //

            var enumNames = new HashSet<string>();

            foreach (var g in registryOverride.Element("registry").Element("groups").Elements("group"))
            {
                if (!groupNames.Contains(g.Attribute("name").Value))
                {
                    continue;
                }
                foreach (var e in g.Elements("enum"))
                {
                    enumNames.Add(e.Attribute("name").Value);
                }
            }

            //
            // Delegate signatures.
            //

            var delegateSignatures = new Dictionary<string, string>();
            var delegateReturnTypes = new Dictionary<string, string>();

            foreach (var e in registry.Element("registry").Element("commands").Elements("command"))
            {
                var proto = e.Element("proto");
                var name = proto.Element("name").Value;
                if (!commandNames.Contains(name))
                {
                    continue;
                }
                delegateReturnTypes[name] = GetParameterType(name, proto, commandOverrides, groupTypes, usedTypes);

                var builder = new StringBuilder();
                builder.Append('(');
                var first = true;
                foreach (var p in e.Elements("param"))
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                    builder.Append(GetParameterType(name, p, commandOverrides, groupTypes, usedTypes));
                    builder.Append(' ');
                    var pname = p.Element("name").Value;
                    switch (pname)
                    {
                        case "params":
                        case "ref":
                        case "string":
                            builder.Append('@');
                            break;
                        default:
                            break;
                    }
                    builder.Append(pname);
                }
                builder.Append(')');
                delegateSignatures[name] = builder.ToString();
            }

            //
            // Enum values from gl.xml.
            //

            var enumValues = new Dictionary<string, string>();

            foreach (var e in registry.Element("registry").Elements("enums").SelectMany(p => p.Elements())
                .Where(p => p.Name.LocalName == "enum"))
            {
                var name = e.Attribute("name").Value;
                if (enumNames.Contains(name))
                {
                    enumValues[name] = e.Attribute("value").Value;
                }
            }

            //
            // Collect enum groups
            //

            var groups = new Dictionary<string, List<string>>();
            foreach (var g in registryOverride.Element("registry").Element("groups").Elements("group"))
            {
                var name = g.Attribute("name").Value;
                if (!groupNames.Contains(name))
                {
                    continue;
                }
                var names = new List<string>();
                groups[name] = names;
                foreach (var e in g.Elements("enum"))
                {
                    names.Add(e.Attribute("name").Value);
                }
            }

            //
            // Write enums.
            //

            var files = glinterop.Element("files");
            var ns = files.Attribute("namespace").Value;
            var visibility = files.Attribute("visibility").Value;
            var enumsFilePath = Path.Combine(destinationFolder, files.Element("enums").Attribute("name").Value);

            Console.WriteLine("  Writing " + enumsFilePath + " ...");
            using (var writer = new StreamWriter(File.Create(enumsFilePath)))
            {
                writer.WriteLine($"// Generated by 'dotnet run -p build/MakeOpenGLInterop {arguments[0]}'\n//\n");
                foreach (var t in groupTypes.Values.Distinct().OrderBy(p => p))
                {
                    WriteUsingClause(writer, t);
                }
                writer.WriteLine();
                writer.Write("namespace ");
                writer.Write(ns);
                writer.WriteLine();
                writer.WriteLine("{");

                foreach (var name in groups.Keys)
                {
                    if (!groupNames.Contains(name))
                    {
                        continue;
                    }
                    writer.Write("    ");
                    writer.Write(visibility);
                    writer.Write(" enum ");
                    writer.Write(name);
                    writer.Write(" : ");
                    writer.WriteLine(groupTypes[name]);
                    writer.WriteLine("    {");
                    GetEnumMemberBounds(groups[name], out var start, out var end);
                    foreach (var enumName in groups[name])
                    {
                        writer.Write("        ");
                        writer.Write(GetEnumMemberName(enumName, start, end));
                        writer.Write(" = ");
                        writer.Write(enumValues[enumName]);
                        writer.WriteLine(',');
                    }
                    writer.WriteLine("    }");
                    writer.WriteLine();
                }

                writer.WriteLine("}");
            }

            //
            // Write delegates.
            //
            var delegatesFilePath = Path.Combine(destinationFolder, files.Element("delegates").Attribute("name").Value);

            Console.WriteLine("  Writing " + delegatesFilePath + " ...");
            using (var writer = new StreamWriter(File.Create(delegatesFilePath)))
            {
                writer.WriteLine($"// Generated by 'dotnet run -p build/MakeOpenGLInterop {arguments[0]}'\n//\n");
                writer.WriteLine("using System;");
                writer.WriteLine();
                foreach (var t in usedTypes.OrderBy(p => p))
                {
                    WriteUsingClause(writer, t);
                }
                writer.WriteLine();
                writer.Write("namespace ");
                writer.Write(ns);
                writer.WriteLine();
                writer.WriteLine("{");

                foreach (var c in commandNames)
                {
                    writer.Write("    ");
                    writer.Write(visibility);
                    writer.Write(' ');
                    var returnType = delegateReturnTypes[c];
                    var signature = delegateSignatures[c];
                    if (returnType[^1] == '*' || signature.IndexOf('*') > 0)
                    {
                        writer.Write("unsafe ");
                    }
                    writer.Write("delegate ");
                    writer.Write(returnType);
                    writer.Write(' ');
                    writer.Write(c.Substring(2));
                    writer.Write("Delegate");
                    writer.Write(signature);
                    writer.WriteLine(';');
                }

                writer.WriteLine("}");
            }

            //
            // Write commands.
            //
            var commandsFilePath = Path.Combine(destinationFolder, files.Element("commands").Attribute("name").Value);

            Console.WriteLine("  Writing " + commandsFilePath + " ...");
            using (var writer = new StreamWriter(File.Create(commandsFilePath)))
            {
                writer.WriteLine($"// Generated by 'dotnet run -p build/MakeOpenGLInterop {arguments[0]}'\n//\n");
                writer.Write("namespace ");
                writer.Write(ns);
                writer.WriteLine();
                writer.WriteLine("{");
                writer.Write("    partial class ");
                writer.Write(files.Element("commands").Attribute("class").Value);
                writer.WriteLine();
                writer.WriteLine("    {");

                if (lazyCommandNames.Count > 0)
                {
                    foreach (var c in lazyCommandNames)
                    {
                        writer.Write("        private ");
                        writer.Write(c.Substring(2));
                        writer.Write("Delegate");
                        writer.Write(' ');
                        writer.Write(c);
                        writer.WriteLine(';');
                    }
                    writer.WriteLine();
                }

                foreach (var c in commandNames)
                {
                    writer.Write("        ");
                    writer.Write(visibility);
                    writer.Write(' ');
                    writer.Write(c.Substring(2));
                    writer.Write("Delegate");
                    writer.Write(' ');
                    writer.Write(c.Substring(2));
                    if (lazyCommandNames.Contains(c))
                    {
                        writer.WriteLine(" {");
                        writer.WriteLine("            get {");
                        writer.Write("                if (");
                        writer.Write(c);
                        writer.WriteLine(" == null)");
                        writer.WriteLine("                {");
                        writer.Write("                    ");
                        writer.Write(c);
                        writer.Write(" = GetOpenGLEntryPoint<");
                        writer.Write(c.Substring(2));
                        writer.Write("Delegate");
                        writer.Write(">(\"");
                        writer.Write(c);
                        writer.WriteLine("\");");
                        writer.WriteLine("                }");
                        writer.Write("                return ");
                        writer.Write(c);
                        writer.WriteLine(';');
                        writer.WriteLine("            }");
                        writer.WriteLine("        }");
                    }
                    else
                    {
                        writer.WriteLine(" { get; private set; }");
                    }
                }

                writer.WriteLine();
                writer.WriteLine("        private void Initialize()");
                writer.WriteLine("        {");
                foreach (var c in commandNames.Except(lazyCommandNames))
                {
                    writer.Write("            ");
                    writer.Write(c.Substring(2));
                    writer.Write(" = GetOpenGLEntryPoint<");
                    writer.Write(c.Substring(2));
                    writer.Write("Delegate");
                    writer.Write(">(\"");
                    writer.Write(c);
                    writer.WriteLine("\");");
                }
                writer.WriteLine("        }");

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            Console.WriteLine("OpenGL interop source files generation done.");
        }

        private static void RegisterFeatures(XElement parent, HashSet<string> enums, HashSet<string> commands)
        {
            var add = parent.Name.LocalName == "require";
            foreach (var e in parent.Elements())
            {
                HashSet<string>? set = null;
                switch (e.Name.LocalName)
                {
                    case "enum":
                        set = enums;
                        break;
                    case "command":
                        set = commands;
                        break;
                    case "type":
                        break;
                    default:
                        throw new Exception("Unexpected element: " + e.Name);
                }
                if (set == null)
                {
                    continue;
                }
                if (add)
                {
                    set.Add(e.Attribute("name").Value);
                }
                else
                {
                    set.Remove(e.Attribute("name").Value);
                }
            }
        }

        private static string GetParameterType(string command, XElement p, Dictionary<string, Dictionary<string, string>> commandOverrides,
            Dictionary<string, string> groupTypes, HashSet<string> usedTypes)
        {
            string? result = null;
            var ptype = p.Element("ptype");
            var pname = p.Element("name").Value;
            var ptrCount = p.Value.Count(c => c == '*');
            if (commandOverrides.TryGetValue(command, out var overrides))
            {
                if (overrides.TryGetValue(pname, out result))
                {
                    groupTypes[result] = (ptype == null) ? "GLenum" : ptype.Value;
                    if (ptype == null)
                    {
                        usedTypes.Add("GLenum");
                    }
                }
            }
            if (result == null)
            {
                if (ptype == null)
                {
                    result = (ptrCount > 0) ? "IntPtr" : ((command == pname) ? "void" : "IntPtr");
                }
                else
                {
                    var t = ptype.Value;
                    result = (t == "GLboolean") ? "Boolean" : t;
                    if (t != "GLboolean")
                    {
                        usedTypes.Add(t);
                    }
                }
            }
            if (ptype != null)
            {
                for (int i = 0; i < ptrCount; i++)
                {
                    result += '*';
                }
            }
            return result;
        }

        private static void WriteUsingClause(StreamWriter writer, string type)
        {
            writer.Write("using ");
            writer.Write(type);
            writer.Write(" = System.");
            switch (type)
            {
                case "GLboolean":
                case "GLchar":
                case "GLubyte":
                    writer.Write("Byte");
                    break;
                case "GLbitfield":
                case "GLenum":
                case "GLuint":
                    writer.Write("UInt32");
                    break;
                case "GLfloat":
                    writer.Write("Single");
                    break;
                case "GLint":
                case "GLsizei":
                    writer.Write("Int32");
                    break;
                case "GLintptr":
                case "GLsizeiptr":
                    writer.Write("IntPtr");
                    break;
                default:
                    throw new Exception("Unexpected type: " + type);
            }
            writer.WriteLine(";");
        }

        private static void GetEnumMemberBounds(List<string> names, out int start, out int end)
        {
            start = 3;
            end = 0;
            if (names.Count <= 1)
            {
                return;
            }
            if (names[0].StartsWith("GL_UNSIGNED"))
            {
                return;
            }
            char c = '\0';
            int i = start;
            var previous = 0;
            var done = false;
            do
            {
                for (int n = 0; n < names.Count; n++)
                {
                    var s = names[n];
                    if (i >= s.Length)
                    {
                        done = true;
                        break;
                    }
                    if (start == i && char.IsNumber(s[i]))
                    {
                        start = previous;
                        done = true;
                        break;
                    }
                    if (n == 0)
                    {
                        c = s[i];
                    }
                    else if (c != s[i])
                    {
                        done = true;
                        break;
                    }
                    else if (n == names.Count - 1)
                    {
                        i++;
                        if (c == '_')
                        {
                            previous = start;
                            start = i;
                        }
                    }
                }
            }
            while (!done);

            if (names[0].EndsWith("_LOD"))
            {
                return;
            }
            done = false;
            i = 0;
            do
            {
                for (int n = 0; n < names.Count; n++)
                {
                    var s = names[n];
                    if (s.Length - 1 - i < 3)
                    {
                        done = true;
                        break;
                    }
                    if (n == 0)
                    {
                        c = s[s.Length - 1 - i];
                    }
                    else if (c != s[s.Length - 1 - i])
                    {
                        done = true;
                        break;
                    }
                    if (n == names.Count - 1)
                    {
                        i++;
                        if (c == '_')
                        {
                            end = i;
                        }
                    }
                }
            }
            while (!done);
        }

        private static string GetEnumMemberName(string name, int start, int end)
        {
            var builder = new StringBuilder();
            bool capitalize = true;
            for (int i = start; i < name.Length - end; i++)
            {
                if (name[i] == '_')
                {
                    capitalize = true;
                    continue;
                }
                if (capitalize)
                {
                    builder.Append(name[i]);
                    capitalize = false;
                }
                else
                {
                    builder.Append(char.ToLower(name[i]));
                }
            }
            return builder.ToString();
        }
    }
}
