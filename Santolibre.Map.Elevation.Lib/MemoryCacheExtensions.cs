using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Santolibre.Map.Elevation.Lib
{
    public static class MemoryCacheExtensions
    {
        public static long GetSize(this MemoryCache memoryCache)
        {
            var sizeProperty = typeof(MemoryCache).GetProperty("Size", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sizeProperty != null)
            {
                var size = sizeProperty.GetValue(memoryCache);
                if (size != null)
                {
                    return (long)size;
                }
            }
            return 0;
        }

        public static long GetMaxSize(this MemoryCache memoryCache)
        {
            var optionsField = typeof(MemoryCache).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);
            if (optionsField != null)
            {
                var options = optionsField.GetValue(memoryCache) as MemoryCacheOptions;
                if (options != null)
                {
                    return (long)options.SizeLimit!;
                }
            }
            return 0;
        }

        public static List<(string Key, long Size)> GetEntryInfos(this MemoryCache memoryCache)
        {
            var info = new List<(string Key, long Size)>();

            var coherentStateField = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
            if (coherentStateField != null)
            {
                var coherentState = coherentStateField.GetValue(memoryCache);
                if (coherentState != null)
                {
                    var entriesCollectionProperty = coherentState.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (entriesCollectionProperty != null)
                    {
                        var entriesCollection = entriesCollectionProperty.GetValue(coherentState) as ICollection;
                        if (entriesCollection != null)
                        {
                            var entriesCollectionEnumerator = entriesCollection.GetEnumerator();
                            while (entriesCollectionEnumerator.MoveNext())
                            {
                                var current = entriesCollectionEnumerator.Current;
                                var keyProperty = current.GetType().GetProperty("Key");
                                var valueProperty = current.GetType().GetProperty("Value");
                                if (keyProperty != null && valueProperty != null)
                                {
                                    var key = keyProperty.GetValue(current);
                                    var value = valueProperty.GetValue(current);
                                    if (key != null && value != null)
                                    {
                                        var sizeProperty = value.GetType().GetProperty("Size", BindingFlags.NonPublic | BindingFlags.Instance);
                                        if (sizeProperty != null)
                                        {
                                            var size = sizeProperty.GetValue(value);
                                            if (size != null)
                                            {
                                                info.Add(((string)key, (long)size));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return info.OrderBy(x => x.Key).ToList();
        }
    }
}
