using System.Threading.Tasks;

namespace fugu.graphql.samples.Host.AsyncInitializer
{
    public interface IAsyncInitializer
    {
        Task InitializeAsync();
    }
}