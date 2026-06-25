using Xunit;

namespace TodoApi.Tests.Infrastructure
{
    [CollectionDefinition("Integration")]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFactory>
    {
    }
}
