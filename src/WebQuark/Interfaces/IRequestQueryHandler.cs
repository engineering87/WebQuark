// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System.Collections.Generic;

namespace WebQuark.Core.Interfaces
{
    public interface IRequestQueryHandler
    {
        string Get(string key, string defaultValue = null);
        void Set(string key, string value);
        bool HasKey(string key);
        void Remove(string key);
        T Get<T>(string key, T defaultValue = default);
        void Set<T>(string key, T value);
        IEnumerable<string> AllKeys();
        Dictionary<string, string> ToDictionary();
        string ToQueryString();
        string ToEncodedString();
    }
}
