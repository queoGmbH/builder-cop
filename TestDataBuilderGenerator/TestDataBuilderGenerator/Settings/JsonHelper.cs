using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TestDataBuilderGenerator.Settings {
    internal static class JsonHelper {
        internal static ImmutableArray<TItem> GetImmutableStringArrayFromToken<TItem>(this JToken token, string childTokenPath) {
            if (token == null || !token.HasValues) {
                return ImmutableArray<TItem>.Empty;
            }

            JToken childToken = token.SelectToken(childTokenPath, false);
            if (childToken != null) {
                return childToken.GetImmutableStringArrayFromToken<TItem>();
            } else {
                return ImmutableArray<TItem>.Empty;
            }
        }

        internal static ImmutableArray<TItem> GetImmutableStringArrayFromToken<TItem>(this JToken token) {
            if (token == null || !token.HasValues) {
                return ImmutableArray<TItem>.Empty;
            } else {
                return ImmutableArray.Create(token.Values<TItem>().ToArray());
            }
        }

        internal static TValue GetValueFromToken<TValue>(this JToken token, TValue defaultValue = default(TValue)) {
            if (token == null || !token.HasValues) {
                return defaultValue;
            } else {
                return token.Value<TValue>();
            }
        }

        internal static TValue GetValueFromToken<TValue>(this JToken token, TValue defaultValue, string childTokenPath) {
            if (token == null || !token.HasValues) {
                return default(TValue);
            }

            JToken childToken = token.SelectToken(childTokenPath, false);
            if (childToken != null) {
                return childToken.GetValueFromToken<TValue>(defaultValue);
            } else {
                return defaultValue;
            }
        }
    }
}