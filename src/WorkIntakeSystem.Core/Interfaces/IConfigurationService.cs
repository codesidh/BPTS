using System.Threading.Tasks;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IConfigurationService
    {
        Task<string?> GetValueAsync(string key, int? businessVerticalId = null, int? version = null);
        Task<T?> GetValueAsync<T>(string key, int? businessVerticalId = null, int? version = null);
    }
} 