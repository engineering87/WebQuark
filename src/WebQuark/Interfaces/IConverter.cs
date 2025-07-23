// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
namespace WebQuark.Core.Interfaces
{
    public interface IConverter
    {
        T ConvertTo<T>(string input, T defaultValue = default);
        string ConvertFrom<T>(T value);
    }
}
