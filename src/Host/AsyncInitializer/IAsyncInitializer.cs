using System.Threading.Tasks;

namespace tanka.graphql.samples.Host.AsyncInitializer
{
    public interface IAsyncInitializer
    {
        Task InitializeAsync();
    }
}