using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

internal static class Program
{
    private static readonly Dictionary<string, string> TypeAliases = new()
    {
        ["System.Void"] = "void",
        ["System.String"] = "string",
        ["System.Object"] = "object",
        ["System.Boolean"] = "bool",
        ["System.Byte"] = "byte",
        ["System.SByte"] = "sbyte",
        ["System.Int16"] = "short",
        ["System.UInt16"] = "ushort",
        ["System.Int32"] = "int",
        ["System.UInt32"] = "uint",
        ["System.Int64"] = "long",
        ["System.UInt64"] = "ulong",
        ["System.Single"] = "float",
        ["System.Double"] = "double",
        ["System.Decimal"] = "decimal",
        ["System.Char"] = "char",
        ["System.DateTime"] = "DateTime"
    };

    private static NullabilityInfoContext? _nullabilityContext;

    private static int Main(string[] args)
    {
        _nullabilityContext = TryCreateNullabilityContext();

        if (!TryParseArgs(args, out var options))
            return 1;
        if (options.ShowHelp)
            return 0;

        string root = FindSolutionRoot(Directory.GetCurrentDirectory());
        string outFile = options.OutputPath ??
                         Path.Combine(root, "docs", "uml", "EasySave-full.puml");
        string outDir = Path.GetDirectoryName(outFile) ?? root;
        Directory.CreateDirectory(outDir);

        var projects = DiscoverProjects(root, options.IncludeTests);
        if (projects.Count == 0)
        {
            Console.Error.WriteLine("No solution projects found.");
            return 1;
        }

        var assemblyPaths = ResolveAssemblyPaths(root, options.Configuration, projects);
        if (assemblyPaths.Count == 0)
        {
            Console.Error.WriteLine(
                "No assemblies found for the selected projects. Build first (e.g., dotnet build EasySave.sln -c " +
                options.Configuration + ").");
            return 1;
        }

        var assemblies = assemblyPaths
            .Select(Assembly.LoadFrom)
            .ToList();
        var namespaceRoots = BuildNamespaceRoots(projects);

        var types = new List<Type>();
        foreach (Assembly asm in assemblies)
        {
            try
            {
                types.AddRange(asm.GetTypes());
            }
            catch (ReflectionTypeLoadException ex)
            {
                types.AddRange(ex.Types.Where(t => t != null)!);
            }
        }

        types = types
            .Where(t => t != null && IsRelevantType(t, namespaceRoots))
            .ToList();

        var typeSet = types.ToDictionary(t => t.AssemblyQualifiedName!, t => t);

        var aliasMap = new Dictionary<string, string>(StringComparer.Ordinal);
        int aliasIndex = 1;
        foreach (Type t in types.OrderBy(t => t.FullName))
        {
            string key = t.AssemblyQualifiedName ?? t.FullName ?? t.Name;
            if (!aliasMap.ContainsKey(key))
            {
                aliasMap[key] = $"T{aliasIndex:D4}";
                aliasIndex++;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("skinparam classAttributeIconSize 0");
        sb.AppendLine("skinparam linetype ortho");
        sb.AppendLine("skinparam ranksep 80");
        sb.AppendLine("skinparam nodesep 50");
        sb.AppendLine("left to right direction");
        sb.AppendLine("set namespaceSeparator .");
        sb.AppendLine();

        foreach (IGrouping<string, Type> group in types
                     .OrderBy(t => t.Namespace)
                     .ThenBy(t => t.Name)
                     .GroupBy(t => t.Namespace ?? "<global>"))
        {
            sb.AppendLine($"package \"{group.Key}\" {{");
            foreach (Type t in group)
            {
                string umlName = GetDisplayName(t);
                string alias = GetAlias(t, aliasMap);
                if (t.IsEnum)
                {
                    sb.AppendLine($"  enum \"{umlName}\" as {alias} {{");
                    foreach (string name in Enum.GetNames(t))
                        sb.AppendLine($"    {name}");
                    sb.AppendLine("  }");
                    sb.AppendLine();
                    continue;
                }

                string kind = "class";
                if (t.IsInterface) kind = "interface";
                else if (t.IsAbstract && !t.IsSealed) kind = "abstract class";

                string header = kind + " \"" + umlName + "\" as " + alias;
                if (t.IsValueType && !t.IsEnum) header += " <<struct>>";
                sb.AppendLine("  " + header + " {");

                const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static |
                                             BindingFlags.Public | BindingFlags.NonPublic |
                                             BindingFlags.DeclaredOnly;

                foreach (FieldInfo f in t.GetFields(binding))
                {
                    if (f.IsDefined(typeof(CompilerGeneratedAttribute), false) || IsGeneratedMemberName(f.Name))
                        continue;
                    if (!TryGetMemberType(() => f.FieldType, out Type fieldType))
                        continue;
                    string vis = GetVisibilitySymbol(f);
                    string mods = "";
                    if (f.IsStatic) mods += "{static} ";
                    if (f.IsInitOnly) mods += "{readonly} ";
                    if (f.IsLiteral) mods += "{const} ";
                    sb.AppendLine($"    {vis} {mods}{f.Name}: {GetSimpleTypeName(fieldType)}");
                }

                foreach (PropertyInfo p in t.GetProperties(binding))
                {
                    if (IsGeneratedMemberName(p.Name))
                        continue;
                    if (!TryGetMemberType(() => p.PropertyType, out Type propertyType))
                        continue;
                    string vis = GetVisibilitySymbol(p);
                    string mods = "";
                    MethodInfo? accGet = p.GetGetMethod(true);
                    MethodInfo? accSet = p.GetSetMethod(true);
                    if ((accGet != null && accGet.IsStatic) || (accSet != null && accSet.IsStatic))
                        mods += "{static} ";

                    var accessors = new List<string>();
                    if (accGet != null) accessors.Add("get");
                    if (accSet != null) accessors.Add("set");
                    string accText = accessors.Count > 0 ? " { " + string.Join("; ", accessors) + "; }" : "";

                    sb.AppendLine($"    {vis} {mods}{p.Name}: {GetSimpleTypeName(propertyType)}{accText}");
                }

                foreach (EventInfo e in t.GetEvents(binding))
                {
                    if (IsGeneratedMemberName(e.Name))
                        continue;
                    if (!TryGetMemberType(() => e.EventHandlerType!, out Type eventType))
                        continue;
                    MethodInfo acc = e.AddMethod!;
                    string vis = GetVisibilitySymbol(acc);
                    string mods = acc.IsStatic ? "{static} " : "";
                    sb.AppendLine($"    {vis} {mods}event {e.Name}: {GetSimpleTypeName(eventType)}");
                }

                foreach (ConstructorInfo ctor in t.GetConstructors(binding))
                {
                    if (ctor.IsStatic)
                        continue;
                    string vis = GetVisibilitySymbol(ctor);
                    string mods = "";
                    string parameters = string.Join(", ", GetSafeParameters(ctor).Select(FormatParameter));
                    sb.AppendLine($"    {vis} {mods}{GetSimpleTypeName(t)}({parameters})");
                }

                foreach (MethodInfo m in t.GetMethods(binding))
                {
                    if (m.IsSpecialName ||
                        m.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
                        IsGeneratedMemberName(m.Name))
                        continue;
                    if (!TryGetMemberType(() => m.ReturnType, out Type returnType))
                        continue;
                    string vis = GetVisibilitySymbol(m);
                    string mods = "";
                    if (m.IsStatic) mods += "{static} ";
                    if (m.IsAbstract) mods += "{abstract} ";
                    string parameters = string.Join(", ", GetSafeParameters(m).Select(FormatParameter));
                    sb.AppendLine($"    {vis} {mods}{m.Name}({parameters}): {GetSimpleTypeName(returnType)}");
                }

                sb.AppendLine("  }");
                sb.AppendLine();
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        var relations = new Dictionary<string, Relation>(StringComparer.Ordinal);

        foreach (Type t in types)
        {
            string from = GetAlias(t, aliasMap);

            Type? baseType = GetSafeBaseType(t);
            Type? resolvedBaseType = baseType == null ? null : ResolveModelType(baseType, typeSet);
            if (resolvedBaseType != null)
            {
                string to = GetAlias(resolvedBaseType, aliasMap);
                AddRelation(relations, RelationKind.Inheritance, from, to);
            }

            Type[] interfaces = GetSafeInterfaces(t);
            if (baseType != null)
            {
                Type[] baseIfaces = GetSafeInterfaces(baseType);
                interfaces = interfaces.Where(i => !baseIfaces.Contains(i)).ToArray();
            }

            foreach (Type i in interfaces)
            {
                Type? resolvedInterface = ResolveModelType(i, typeSet);
                if (resolvedInterface == null) continue;
                string to = GetAlias(resolvedInterface, aliasMap);
                AddRelation(relations,
                    t.IsInterface ? RelationKind.Inheritance : RelationKind.Realization,
                    from, to);
            }

            const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static |
                                         BindingFlags.Public | BindingFlags.NonPublic |
                                         BindingFlags.DeclaredOnly;

            if (t.IsEnum)
                continue;

            foreach (FieldInfo f in t.GetFields(binding))
            {
                if (f.IsDefined(typeof(CompilerGeneratedAttribute), false) || IsGeneratedMemberName(f.Name))
                    continue;
                if (f.IsStatic || f.IsLiteral)
                    continue;
                if (!TryGetMemberType(() => f.FieldType, out Type fieldType))
                    continue;
                foreach ((Type target, string mult) in GetAssociationTargets(fieldType, f))
                {
                    Type? resolved = ResolveModelType(target, typeSet);
                    if (resolved == null) continue;
                    string to = GetAlias(resolved, aliasMap);
                    AddRelation(relations, RelationKind.Aggregation, from, to, mult);
                }
            }

            foreach (PropertyInfo p in t.GetProperties(binding))
            {
                if (IsGeneratedMemberName(p.Name))
                    continue;
                if (IsStaticProperty(p))
                    continue;
                if (!TryGetMemberType(() => p.PropertyType, out Type propertyType))
                    continue;
                foreach ((Type target, string mult) in GetAssociationTargets(propertyType, p))
                {
                    Type? resolved = ResolveModelType(target, typeSet);
                    if (resolved == null) continue;
                    string to = GetAlias(resolved, aliasMap);
                    AddRelation(relations, RelationKind.Aggregation, from, to, mult);
                }
            }
        }

        EnsureOrphansAreLinked(types, aliasMap, typeSet, relations);

        sb.AppendLine();
        foreach (Relation rel in relations.Values.OrderBy(r => r.From).ThenBy(r => r.To).ThenBy(r => r.Kind))
            sb.AppendLine(rel.ToPlantUml());
        sb.AppendLine();
        sb.AppendLine("@enduml");

        File.WriteAllText(outFile, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine(outFile);
        return 0;
    }

    private sealed record Options(
        string Configuration,
        string? OutputPath,
        bool IncludeTests,
        bool ShowHelp);

    private sealed record ProjectInfo(
        string Name,
        string RelativePath,
        string AssemblyName);

    private static readonly HashSet<string> ExcludedProjectNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "EasySave.Translation"
    };

    private static bool TryParseArgs(string[] args, out Options options)
    {
        string configuration = "Debug";
        string? output = null;
        bool includeTests = false;
        bool showHelp = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-c":
                case "--config":
                    if (i + 1 >= args.Length)
                    {
                        PrintUsage();
                        options = new Options(configuration, output, includeTests, showHelp);
                        return false;
                    }

                    configuration = args[++i];
                    break;
                case "--out":
                    if (i + 1 >= args.Length)
                    {
                        PrintUsage();
                        options = new Options(configuration, output, includeTests, showHelp);
                        return false;
                    }

                    output = args[++i];
                    break;
                case "--include-tests":
                    includeTests = true;
                    break;
                case "-h":
                case "--help":
                    showHelp = true;
                    break;
                default:
                    Console.Error.WriteLine("Unknown argument: " + arg);
                    PrintUsage();
                    options = new Options(configuration, output, includeTests, showHelp);
                    return false;
            }
        }

        if (showHelp)
            PrintUsage();

        options = new Options(configuration, output, includeTests, showHelp);
        return true;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(
            "  dotnet run --project tools/UmlGenerator/UmlGenerator.csproj -- --config <CONFIG> [--out <FILE>] [--include-tests]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -c, --config   Build configuration to scan (default: Debug)");
        Console.WriteLine("  --out          Output .puml file (default: docs/uml/EasySave-full.puml)");
        Console.WriteLine("  --include-tests Include test projects in the diagram");
    }

    private static List<ProjectInfo> DiscoverProjects(string root, bool includeTests)
    {
        string solutionPath = Path.Combine(root, "EasySave.sln");
        if (!File.Exists(solutionPath))
            return new List<ProjectInfo>();

        var projectRegex = new Regex(
            "^Project\\(\"[^\"]+\"\\) = \"([^\"]+)\", \"([^\"]+\\.csproj)\",",
            RegexOptions.Compiled);

        var projects = new List<ProjectInfo>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string line in File.ReadLines(solutionPath))
        {
            Match match = projectRegex.Match(line);
            if (!match.Success)
                continue;

            string name = match.Groups[1].Value.Trim();
            string relativePath = match.Groups[2].Value.Trim()
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            if (IsToolProject(relativePath))
                continue;

            if (!includeTests && IsTestProject(name, relativePath))
                continue;

            if (ExcludedProjectNames.Contains(name))
                continue;

            string fullPath = Path.Combine(root, relativePath);
            if (!File.Exists(fullPath))
                continue;

            if (!seen.Add(fullPath))
                continue;

            string assemblyName = Path.GetFileNameWithoutExtension(fullPath);
            projects.Add(new ProjectInfo(name, relativePath, assemblyName));
        }

        return projects;
    }

    private static bool IsTestProject(string projectName, string relativePath)
    {
        return projectName.Contains("Test", StringComparison.OrdinalIgnoreCase) ||
               relativePath.Contains("Test", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsToolProject(string relativePath)
    {
        return relativePath.StartsWith("tools" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static HashSet<string> BuildNamespaceRoots(IEnumerable<ProjectInfo> projects)
    {
        var roots = new HashSet<string>(StringComparer.Ordinal);
        foreach (ProjectInfo project in projects)
        {
            string assemblyName = project.AssemblyName;
            if (string.IsNullOrWhiteSpace(assemblyName))
                continue;

            string root = assemblyName.Split('.')[0];
            if (!string.IsNullOrWhiteSpace(root))
                roots.Add(root);
        }

        return roots;
    }

    private static List<string> ResolveAssemblyPaths(
        string root,
        string configuration,
        IEnumerable<ProjectInfo> projects)
    {
        var results = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (ProjectInfo project in projects)
        {
            string? projectDir = Path.GetDirectoryName(project.RelativePath);
            string binDir = Path.Combine(root, projectDir ?? string.Empty, "bin", configuration);
            if (!Directory.Exists(binDir))
                continue;

            var candidates = Directory.GetFiles(binDir, project.AssemblyName + ".dll", SearchOption.AllDirectories)
                .Where(path =>
                    !path.Contains(Path.DirectorySeparatorChar + "ref" + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (candidates.Length == 0)
                continue;

            string latest = candidates
                .Select(path => new FileInfo(path))
                .OrderByDescending(info => info.LastWriteTimeUtc)
                .First()
                .FullName;

            if (seen.Add(latest))
                results.Add(latest);
        }

        return results;
    }

    private static string FindSolutionRoot(string start)
    {
        string current = start;
        while (!File.Exists(Path.Combine(current, "EasySave.sln")))
        {
            DirectoryInfo? parent = Directory.GetParent(current);
            if (parent == null) break;
            current = parent.FullName;
        }

        return current;
    }

    private static NullabilityInfoContext? TryCreateNullabilityContext()
    {
        try
        {
            return new NullabilityInfoContext();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsRelevantType(Type type, HashSet<string> namespaceRoots)
    {
        if (type.AssemblyQualifiedName == null ||
            type.FullName == null ||
            type.Name == "AutoGeneratedProgram" ||
            type.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
            IsGeneratedTypeName(type.Name))
            return false;

        if (type.FullName.StartsWith("<", StringComparison.Ordinal))
            return false;

        string? ns = type.Namespace;
        if (string.IsNullOrWhiteSpace(ns))
            return false;

        if (ns.Contains(".__Internals", StringComparison.Ordinal))
            return false;

        return IsWithinNamespaceRoots(ns, namespaceRoots);
    }

    private static bool IsWithinNamespaceRoots(string ns, HashSet<string> namespaceRoots)
    {
        foreach (string root in namespaceRoots)
            if (ns.Equals(root, StringComparison.Ordinal) ||
                ns.StartsWith(root + ".", StringComparison.Ordinal))
                return true;
        return false;
    }

    private static bool IsGeneratedTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return true;

        return typeName.StartsWith("<", StringComparison.Ordinal) ||
               typeName.StartsWith("__", StringComparison.Ordinal) ||
               typeName.Contains("AnonymousType", StringComparison.Ordinal) ||
               typeName.Contains("DisplayClass", StringComparison.Ordinal);
    }

    private static bool IsGeneratedMemberName(string memberName)
    {
        if (string.IsNullOrWhiteSpace(memberName))
            return true;

        return memberName.StartsWith("<", StringComparison.Ordinal) ||
               memberName.StartsWith("!", StringComparison.Ordinal) ||
               memberName.Contains("k__BackingField", StringComparison.Ordinal) ||
               memberName.Contains("XamlIl", StringComparison.Ordinal);
    }

    private static Type? GetSafeBaseType(Type type)
    {
        try
        {
            return type.BaseType;
        }
        catch (Exception ex) when (IsTypeResolutionException(ex))
        {
            return null;
        }
    }

    private static Type[] GetSafeInterfaces(Type type)
    {
        try
        {
            return type.GetInterfaces();
        }
        catch (Exception ex) when (IsTypeResolutionException(ex))
        {
            return Array.Empty<Type>();
        }
    }

    private static bool TryGetMemberType(Func<Type> accessor, out Type type)
    {
        try
        {
            type = accessor();
            return true;
        }
        catch (Exception ex) when (IsTypeResolutionException(ex))
        {
            type = typeof(object);
            return false;
        }
    }

    private static ParameterInfo[] GetSafeParameters(MethodBase method)
    {
        try
        {
            return method.GetParameters();
        }
        catch (Exception ex) when (IsTypeResolutionException(ex))
        {
            return Array.Empty<ParameterInfo>();
        }
    }

    private static bool IsTypeResolutionException(Exception ex)
    {
        return ex is FileNotFoundException ||
               ex is FileLoadException ||
               ex is TypeLoadException ||
               ex is ReflectionTypeLoadException ||
               ex is NotSupportedException;
    }

    private static string GetVisibilitySymbol(MemberInfo member)
    {
        return member switch
        {
            MethodBase m => GetVisibilitySymbol(m),
            FieldInfo f => GetVisibilitySymbol(f),
            PropertyInfo p => GetVisibilitySymbol(p),
            _ => "~"
        };
    }

    private static string GetVisibilitySymbol(MethodBase m)
    {
        if (m.IsPublic) return "+";
        if (m.IsPrivate) return "-";
        if (m.IsFamily) return "#";
        if (m.IsAssembly) return "~";
        if (m.IsFamilyOrAssembly) return "#";
        if (m.IsFamilyAndAssembly) return "#";
        return "~";
    }

    private static string GetVisibilitySymbol(FieldInfo f)
    {
        if (f.IsPublic) return "+";
        if (f.IsPrivate) return "-";
        if (f.IsFamily) return "#";
        if (f.IsAssembly) return "~";
        if (f.IsFamilyOrAssembly) return "#";
        if (f.IsFamilyAndAssembly) return "#";
        return "~";
    }

    private static string GetVisibilitySymbol(PropertyInfo p)
    {
        MethodInfo? acc = p.GetGetMethod(true) ?? p.GetSetMethod(true);
        return acc == null ? "~" : GetVisibilitySymbol(acc);
    }

    private static string GetSimpleTypeName(Type t)
    {
        if (t.IsByRef) t = t.GetElementType()!;
        if (t.IsGenericParameter) return t.Name;
        if (t.IsArray) return GetSimpleTypeName(t.GetElementType()!) + "[]";
        if (t.IsGenericType && t.GetGenericTypeDefinition().FullName == "System.Nullable`1")
            return GetSimpleTypeName(t.GetGenericArguments()[0]) + "?";

        if (TypeAliases.TryGetValue(t.FullName ?? t.Name, out string? alias))
            return alias;

        if (t.IsGenericType)
        {
            string name = t.Name.Split('`')[0];
            string args = string.Join(", ", t.GetGenericArguments().Select(GetSimpleTypeName));
            return $"{name}<{args}>";
        }

        return t.Name;
    }

    private static string GetDisplayName(Type t)
    {
        string name = t.Name.Split('`')[0];
        if (t.IsGenericTypeDefinition)
        {
            string args = string.Join(", ", t.GetGenericArguments().Select(a => a.Name));
            name = $"{name}<{args}>";
        }

        return t.DeclaringType == null
            ? name
            : $"{GetDisplayName(t.DeclaringType)}.{name}";
    }

    private static string GetAlias(Type t, Dictionary<string, string> aliasMap)
    {
        string key = t.AssemblyQualifiedName ?? t.FullName ?? t.Name;
        if (aliasMap.TryGetValue(key, out string? alias))
            return alias;
        alias = $"T{aliasMap.Count + 1:D4}";
        aliasMap[key] = alias;
        return alias;
    }

    private static bool IsNullableMember(MemberInfo member, Type type)
    {
        if (type.IsValueType)
            return type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Nullable`1";

        if (_nullabilityContext != null)
        {
            try
            {
                NullabilityInfo? info = member switch
                {
                    FieldInfo f => _nullabilityContext.Create(f),
                    PropertyInfo p => _nullabilityContext.Create(p),
                    _ => null
                };

                return info != null && info.ReadState == NullabilityState.Nullable;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private static IEnumerable<(Type target, string mult)> GetAssociationTargets(Type type, MemberInfo member)
    {
        foreach (Type t in GetCollectionElementTypes(type))
            yield return (t, "0..*");

        if (IsNullableValueType(type))
        {
            yield return (type.GetGenericArguments()[0], "0..1");
            yield break;
        }

        if (!GetCollectionElementTypes(type).Any())
        {
            string mult = IsNullableMember(member, type) ? "0..1" : "1";
            yield return (type, mult);
        }
    }

    private static IEnumerable<Type> GetDependencyTargets(Type type)
    {
        foreach (Type t in GetCollectionElementTypes(type))
            yield return t;

        if (IsNullableValueType(type))
        {
            yield return type.GetGenericArguments()[0];
            yield break;
        }

        if (!GetCollectionElementTypes(type).Any())
            yield return type;
    }

    private static IEnumerable<Type> GetCollectionElementTypes(Type type)
    {
        try
        {
            if (type.IsByRef) type = type.GetElementType()!;
            if (type.IsArray) return new[] { type.GetElementType()! };
            if (!type.IsGenericType) return Array.Empty<Type>();

            Type def = type.GetGenericTypeDefinition();
            string defName = def.FullName ?? def.Name;

            if (defName is "System.Collections.Generic.Dictionary`2" or
                "System.Collections.Generic.IDictionary`2" or
                "System.Collections.Generic.IReadOnlyDictionary`2")
            {
                return type.GetGenericArguments();
            }

            IEnumerable<Type> ifaces = new[] { type }.Concat(GetSafeInterfaces(type));
            foreach (Type iface in ifaces)
            {
                if (iface.IsGenericType &&
                    iface.GetGenericTypeDefinition().FullName == "System.Collections.Generic.IEnumerable`1")
                {
                    return new[] { iface.GetGenericArguments()[0] };
                }
            }

            return Array.Empty<Type>();
        }
        catch (Exception ex) when (IsTypeResolutionException(ex))
        {
            return Array.Empty<Type>();
        }
    }

    private static bool IsNullableValueType(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Nullable`1";

    private static Type? ResolveModelType(Type type, Dictionary<string, Type> typeSet)
    {
        if (type.IsByRef) type = type.GetElementType()!;
        if (type.AssemblyQualifiedName != null &&
            typeSet.TryGetValue(type.AssemblyQualifiedName, out Type? direct))
            return direct;

        if (type.IsGenericType)
        {
            Type def = type.GetGenericTypeDefinition();
            if (def.AssemblyQualifiedName != null &&
                typeSet.TryGetValue(def.AssemblyQualifiedName, out Type? defType))
                return defType;
        }

        return null;
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        if (!TryGetMemberType(() => parameter.ParameterType, out Type type))
            return $"{parameter.Name ?? "param"}: <unresolved>";
        string prefix = "";
        if (parameter.IsOut) prefix = "out ";
        else if (type.IsByRef) prefix = "ref ";
        if (type.IsByRef) type = type.GetElementType()!;
        return $"{prefix}{parameter.Name ?? "param"}: {GetSimpleTypeName(type)}";
    }

    private static void EnsureOrphansAreLinked(
        List<Type> types,
        Dictionary<string, string> aliasMap,
        Dictionary<string, Type> typeSet,
        Dictionary<string, Relation> relations)
    {
        var degree = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (Type t in types)
        {
            string alias = GetAlias(t, aliasMap);
            degree[alias] = 0;
        }

        foreach (Relation rel in relations.Values)
        {
            if (degree.ContainsKey(rel.From)) degree[rel.From]++;
            if (degree.ContainsKey(rel.To)) degree[rel.To]++;
        }

        const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static |
                                     BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.DeclaredOnly;

        foreach (Type t in types)
        {
            string from = GetAlias(t, aliasMap);
            if (degree[from] > 0) continue;

            foreach (ConstructorInfo ctor in t.GetConstructors(binding))
            {
                if (ctor.IsStatic)
                    continue;
                foreach (ParameterInfo param in GetSafeParameters(ctor))
                {
                    if (!TryGetMemberType(() => param.ParameterType, out Type parameterType))
                        continue;

                    foreach (Type target in GetDependencyTargets(parameterType))
                    {
                        Type? resolved = ResolveModelType(target, typeSet);
                        if (resolved == null) continue;
                        string to = GetAlias(resolved, aliasMap);
                        AddRelation(relations, RelationKind.Dependency, from, to);
                        degree[from]++;
                        degree[to]++;
                        goto linked;
                    }
                }
            }

            foreach (MethodInfo m in t.GetMethods(binding))
            {
                if (m.IsSpecialName ||
                    m.IsDefined(typeof(CompilerGeneratedAttribute), false) ||
                    IsGeneratedMemberName(m.Name))
                    continue;

                if (TryGetMemberType(() => m.ReturnType, out Type returnType))
                {
                    foreach (Type target in GetDependencyTargets(returnType))
                    {
                        Type? resolved = ResolveModelType(target, typeSet);
                        if (resolved == null) continue;
                        string to = GetAlias(resolved, aliasMap);
                        AddRelation(relations, RelationKind.Dependency, from, to);
                        degree[from]++;
                        degree[to]++;
                        goto linked;
                    }
                }

                foreach (ParameterInfo param in GetSafeParameters(m))
                {
                    if (!TryGetMemberType(() => param.ParameterType, out Type parameterType))
                        continue;

                    foreach (Type target in GetDependencyTargets(parameterType))
                    {
                        Type? resolved = ResolveModelType(target, typeSet);
                        if (resolved == null) continue;
                        string to = GetAlias(resolved, aliasMap);
                        AddRelation(relations, RelationKind.Dependency, from, to);
                        degree[from]++;
                        degree[to]++;
                        goto linked;
                    }
                }
            }

        linked:
            continue;
        }
    }

    private static void AddRelation(
        Dictionary<string, Relation> relations,
        RelationKind kind,
        string from,
        string to,
        string? multiplicity = null)
    {
        if (string.Equals(from, to, StringComparison.Ordinal))
            return;

        string key = $"{from}|{to}";
        if (relations.TryGetValue(key, out Relation? existing))
        {
            if (existing.Kind <= kind)
            {
                if (existing.Kind == RelationKind.Aggregation && kind == RelationKind.Aggregation)
                {
                    string merged = MergeMultiplicity(existing.MultiplicityTo, multiplicity ?? "1");
                    relations[key] = existing with { MultiplicityTo = merged };
                }
                return;
            }
        }

        relations[key] = new Relation(from, to, kind, "1", multiplicity ?? "1");
    }

    private static string MergeMultiplicity(string current, string incoming)
    {
        if (current == "0..*") return current;
        if (incoming == "0..*") return incoming;
        if (current == "0..1" || incoming == "0..1") return "0..1";
        return "1";
    }

    private static bool IsStaticProperty(PropertyInfo property)
    {
        MethodInfo? getter = property.GetGetMethod(true);
        MethodInfo? setter = property.GetSetMethod(true);
        return (getter != null && getter.IsStatic) || (setter != null && setter.IsStatic);
    }
}

internal enum RelationKind
{
    Inheritance = 0,
    Realization = 1,
    Aggregation = 2,
    Dependency = 3
}

internal sealed record Relation(
    string From,
    string To,
    RelationKind Kind,
    string MultiplicityFrom,
    string MultiplicityTo)
{
    public string ToPlantUml()
    {
        return Kind switch
        {
            RelationKind.Inheritance => $"{From} --|> {To}",
            RelationKind.Realization => $"{From} ..|> {To}",
            RelationKind.Aggregation => $"{From} \"{MultiplicityFrom}\" o-- \"{MultiplicityTo}\" {To}",
            RelationKind.Dependency => $"{From} ..> {To}",
            _ => $"{From} ..> {To}"
        };
    }
}
