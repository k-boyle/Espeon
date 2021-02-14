using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Espeon {
    public static class Methods {
        public static bool IsOverriden(this MethodInfo methodInfo) {
            return methodInfo == methodInfo?.GetBaseDefinition();
        }

        public static string Inspect(this object obj) {
            var type = obj.GetType();
            var overridenToString = type.GetMethod("ToString").IsOverriden();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name).ToArray();

            var sb = new StringBuilder("```css\n");

            //doesnt work
            sb.AppendLine($"{{{type}: '{(overridenToString ? obj.ToString() : "")}'}}");
            sb.AppendLine();

            var maxLength = props.Max(x => x.Name.Length);

            foreach (var prop in props) {
                sb.Append($"#{prop.Name.PadRight(maxLength, ' ')} - ");

                object value = null;

                try {
                    value = prop.GetValue(obj);
                } catch (TargetInvocationException) { } //c# bad

                static string DateTimeNicefy(DateTimeOffset dateTime) {
                    return $"{dateTime.Day}/{dateTime.Month}/{dateTime.Year} {dateTime.TimeOfDay.ToString("g").Split('.').First()}";
                }

                static string NicefyNamespace(Type inType) {
                    var str = inType.ToString();

                    return str.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
                }

                type = value?.GetType();
                if (type is null) {
                    continue;
                }

                overridenToString = type.GetMethods()
                    .First(method => method.GetParameters().Length == 0 && method.Name == "ToString")
                    .IsOverriden();

                switch (value) {
                    case IEnumerable collection when !(value is string):
                        var count = collection.Cast<object>().Count();
                        sb.AppendLine($"[{count} item{(count == 1 ? "" : "s")}]");
                        break;
                    
                    // case Enum @enum:
                    //     sb.AppendLine($"[{@enum.Humanize()}]");
                    //     break;

                    case string str:
                        sb.AppendLine($"[\"{str}\"]");
                        break;

                    case Task task:
                        var returnT = type.GetGenericArguments();
                        sb.AppendLine(returnT.Length > 0
                            ? $"[Task<{string.Join(", ", returnT.Select(NicefyNamespace))}>]"
                            : "[Task]");
                        break;

                    case DateTime dt:
                        sb.AppendLine($"[{DateTimeNicefy(dt)}]");
                        break;

                    case DateTimeOffset dto:
                        sb.AppendLine($"[{DateTimeNicefy(dto)}]");
                        break;

                    default:
                        if (overridenToString) {
                            sb.AppendLine($"[{value}]");
                        } else {
                            if (type?.IsValueType == false) {
                                var niceName = NicefyNamespace(type);
                                sb.AppendLine($"[{niceName}]");
                            } else {
                                sb.AppendLine($"[{value}]");
                            }
                        }
                        break;
                }
            }

            sb.AppendLine("```");

            return sb.ToString();
        }

        // Batch method written by https://github.com/Emzi0767
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> enumerable, int size) {
            if (enumerable == null) {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (size <= 0) {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");
            }

            return InternalBatch(enumerable, size);
        }

        private static IEnumerable<IEnumerable<T>> InternalBatch<T>(IEnumerable<T> enumerable, int size) {
            using var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext()) {
                yield return InternalSingleBatch(enumerator, size);
            }
        }

        private static IEnumerable<T> InternalSingleBatch<T>(IEnumerator<T> enumerator, int size) {
            do {
                yield return enumerator.Current;
            } while (--size > 0 && enumerator.MoveNext());
        }
    }
}