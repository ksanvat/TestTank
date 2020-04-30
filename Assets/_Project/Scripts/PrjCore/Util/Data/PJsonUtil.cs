using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace PrjCore.Util.Data {
    public static class PJsonUtil {
        public static string ToJson(object obj) {
            try {
                return SerializeObject(obj);
            } catch (Exception e) {
                Debug.LogError("Cannot serialize json cause of exception");
                Debug.LogException(e);
                Debug.LogErrorFormat("Data: {0}", obj);
                return null;
            }
        }

        public static T FromJson<T>(string data) where T : class {
            try {
                return DeserializeObject<T>(data);
            } catch (Exception e) {
                Debug.LogError("Cannot deserialize json cause of exception");
                Debug.LogException(e);
                Debug.LogErrorFormat("Json: {0}", data);
                return null;
            }
        }

        public static T ConvertObject<T>(object source) where T : class {
            try {
                return (T) ConvertObject(source, typeof(T));
            } catch (Exception e) {
                Debug.LogError("Cannot covert object cause of exception");
                Debug.LogException(e);
                Debug.LogErrorFormat("Data: {0}", source);
                return null;
            }
        }

        public static object ConvertObject(object source, Type targetType) {
            var settings = CreateDefaultSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            var jsonSerializer = JsonSerializer.CreateDefault(settings);
            
            var sb = new StringBuilder(256);
            using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture)) {
                using (var jsonWriter = new JsonTextWriter(sw)) {
                    jsonSerializer.Serialize(jsonWriter, source);
                }
            }
            
            using (var sr = new StringReader(sb.ToString())) {
                using (var jsonReader = new JsonTextReader(sr)) {
                    return jsonSerializer.Deserialize(jsonReader, targetType);
                }
            }
        }

        private static string SerializeObject(object value) {
            var sb = new StringBuilder(256);
            var jsonSerializer = JsonSerializer.CreateDefault(CreateDefaultSettings());

            using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture)) {
                using (var jsonWriter = new JsonTextWriter(sw)) {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.IndentChar = ' ';
                    jsonWriter.Indentation = 4;

                    jsonSerializer.Serialize(jsonWriter, value);
                }
            }

            return sb.ToString();
        }

        private static T DeserializeObject<T>(string value) {
            using (var sr = new StringReader(value)) {
                var jsonSerializer = JsonSerializer.CreateDefault(CreateDefaultSettings());
                using (var jsonReader = new JsonTextReader(sr)) {
                    return jsonSerializer.Deserialize<T>(jsonReader);
                }
            }
        }

        private static JsonSerializerSettings CreateDefaultSettings() {
            return new JsonSerializerSettings {
                ContractResolver = new ContractResolver(),
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.Indented
            };
        }

        private class ContractResolver : DefaultContractResolver {
            
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
                if (member.MemberType != MemberTypes.Field) {
                    return null;
                }

                return base.CreateProperty(member, memberSerialization);
            }
            
        }
        
    }
}