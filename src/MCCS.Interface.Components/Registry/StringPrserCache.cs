using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCCS.Interface.Components.Registry
{
    public static class StringPrserCache
    {
        private static readonly ConcurrentDictionary<Type, Func<string, object?>> _stringParsers = new();

        private static readonly JsonSerializerSettings _jsonSettings = new()
        { 
            NullValueHandling = NullValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateParseHandling = DateParseHandling.DateTime, // 你也可以设 None
        };

        private static object? ConvertStringToTarget(string text, Type targetType)
        {
            var parser = _stringParsers.GetOrAdd(targetType, BuildStringParserNewtonsoft);
            return parser(text);
        }

        private static Func<string, object?> BuildStringParserNewtonsoft(Type targetType)
        {
            // Nullable<T>
            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying != null)
            {
                var inner = BuildStringParserNewtonsoft(underlying);
                return s =>
                {
                    if (string.IsNullOrWhiteSpace(s) || s.Equals("null", StringComparison.OrdinalIgnoreCase))
                        return null;
                    return inner(s);
                };
            }

            // string
            if (targetType == typeof(string))
                return s => s;

            // enum
            if (targetType.IsEnum)
                return s => Enum.Parse(targetType, s, ignoreCase: true);

            // TryParse(string, out T)
            var tryParse = targetType.GetMethod(
                "TryParse",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(string), targetType.MakeByRefType() },
                modifiers: null);

            if (tryParse != null && tryParse.ReturnType == typeof(bool))
            {
                return s =>
                {
                    var args = new object?[] { s, null };
                    var ok = (bool)tryParse.Invoke(null, args)!;
                    if (!ok) throw new FormatException($"无法将字符串转换为 {targetType.Name}");
                    return args[1];
                };
            }

            // Parse(string)
            var parse = targetType.GetMethod(
                "Parse",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            if (parse != null && parse.ReturnType == targetType)
                return s => parse.Invoke(null, new object[] { s })!;

            // TypeConverter
            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
                return s => converter.ConvertFromInvariantString(s);

            // JSON（对象/数组都能吃）
            return s =>
            {
                // 小优化：如果是 JToken 文字（json array/object），直接 Deserialize；否则也一样能处理
                return JsonConvert.DeserializeObject(s, targetType, _jsonSettings);
            };
        }

        /// <summary>
        /// string 转换为目标类型
        /// </summary>
        /// <param name="input"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static object? ConvertToTarget(object? input, Type targetType)
        {
            if (input == null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    throw new InvalidOperationException($"目标类型 {targetType.Name} 不接受 null");
                return null;
            }

            // 已经是目标类型
            if (targetType.IsInstanceOfType(input))
                return input;

            // string -> 强类型（含 JSON object/array）
            if (input is string s)
                return ConvertStringToTarget(s, targetType);

            // 如果 input 是 JToken（有些上层可能已经 parse 过），直接 ToObject
            if (input is JToken token)
                return token.ToObject(targetType, JsonSerializer.Create(_jsonSettings));

            // 集合参数：input 是 IEnumerable（注意 string 之前已处理）
            if (input is IEnumerable enumerable && TryGetEnumerableElementType(targetType, out var elemType))
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType))!;

                foreach (var item in enumerable)
                {
                    // item 可能是 string(元素序列化) / 可能已是对象
                    var converted = ConvertToTarget(item, elemType);
                    list.Add(converted);
                }

                // 目标是数组
                if (targetType.IsArray)
                {
                    var array = Array.CreateInstance(elemType, list.Count);
                    list.CopyTo(array, 0);
                    return array;
                }

                // 目标是各种 IEnumerable<T>/IReadOnlyList<T>/ICollection<T> 等，List<T> 通常可赋值
                if (targetType.IsInstanceOfType(list))
                    return list;

                // 目标是具体集合类型（ObservableCollection<T> 等）：
                // 优先找 IEnumerable<T> 构造
                var ctor = targetType.GetConstructor([typeof(IEnumerable<>).MakeGenericType(elemType)]);
                if (ctor != null)
                    return ctor.Invoke([list]);

                // 或者无参构造 + Add(T)
                var instance = Activator.CreateInstance(targetType);
                if (instance != null)
                {
                    var add = targetType.GetMethod("Add", [elemType]);
                    if (add != null)
                    {
                        foreach (var x in list) add.Invoke(instance, [x]);
                        return instance;
                    }
                }

                // 兜底：给 List<T>
                return list;
            }

            // 兜底桥接：把任意对象转成 JSON 再转目标类型（通用但慢点）
            var json = JsonConvert.SerializeObject(input, _jsonSettings);
            return JsonConvert.DeserializeObject(json, targetType, _jsonSettings);
        }

        private static bool TryGetEnumerableElementType(Type targetType, out Type elementType)
        {
            // 数组
            if (targetType.IsArray)
            {
                elementType = targetType.GetElementType()!;
                return true;
            }

            // 常见泛型集合/接口
            if (targetType.IsGenericType)
            {
                var genDef = targetType.GetGenericTypeDefinition();
                if (genDef == typeof(IEnumerable<>) ||
                    genDef == typeof(ICollection<>) ||
                    genDef == typeof(IReadOnlyCollection<>) ||
                    genDef == typeof(IReadOnlyList<>) ||
                    genDef == typeof(List<>))
                {
                    elementType = targetType.GetGenericArguments()[0];
                    return true;
                }
            }

            // 实现了 IEnumerable<T>
            var ienumT = targetType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (ienumT != null)
            {
                elementType = ienumT.GetGenericArguments()[0];
                return true;
            }

            elementType = typeof(object);
            return false;
        }

    }
}
