// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
namespace WebQuark.Core.Interfaces
{
    public interface ISessionHandler
    {
        void SetString(string key, string value);
        string GetString(string key);
        void Set<T>(string key, T value);
        T Get<T>(string key, T defaultValue = default);
        bool HasKey(string key);
        void Remove(string key);
        void Clear();
    }
}
