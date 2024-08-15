using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using System.Diagnostics;
using System.Net;

namespace TestContainersTTLIntegration
{
    [TestClass]
    public class UnitTest1
    {
        private IContainer _Container1;
        private IContainer _MeshSandBoxContainer;
        public UnitTest1()
        {

            _Container1 = new ContainerBuilder()
            // Set the image for the container to "testcontainers/helloworld:1.1.0".
            .WithImage("testcontainers/helloworld:1.1.0")
            // Bind port 8080 of the container to a random port on the host.
            .WithPortBinding(8080, true)
            // Wait until the HTTP endpoint of the container is available.
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080)))
            // Build the container configuration.
            .Build();



            _MeshSandBoxContainer = new ContainerBuilder()
            // Set the image for the container to "testcontainers/helloworld:1.1.0".
            .WithImage("mesh-sandbox:integrationTest")
            .WithEnvironment("SHARED_KEY","TestKey")
            .WithEnvironment("SSL","no")
            //.WithResourceMapping("./src/mesh_sandbox/store/data/mailboxes.jsonl", "/app/mesh_sandbox/store/data/mailboxes.jsonl")
            //.WithResourceMapping("./src/mesh_sandbox/test_plugin", "/app/mesh_sandbox/plugins")
            // Bind port 8080 of the container to a random port on the host.
            .WithPortBinding(80, true)
            // Wait until the HTTP endpoint of the container is available.
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => 
                r.ForPath("/health")
                   .ForPort(80)))
            // Build the container configuration.
            .Build();


        }

        [TestInitialize]
        public async Task StartupContainers()
        {

            // Start the container.
            await _Container1.StartAsync()
                .ConfigureAwait(false);

            await _MeshSandBoxContainer.StartAsync()
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task CheckTestContainer()
        {
            var httpClient = new HttpClient();

            // Construct the request URI by specifying the scheme, hostname, assigned random host port, and the endpoint "uuid".
            var requestUri = new UriBuilder(Uri.UriSchemeHttp, _Container1.Hostname, _Container1.GetMappedPublicPort(8080), "uuid").Uri;

            // Send an HTTP GET request to the specified URI and retrieve the response as a string.
            var guid = await httpClient.GetStringAsync(requestUri)
              .ConfigureAwait(false);

            // Ensure that the retrieved UUID is a valid GUID.
            Debug.Assert(Guid.TryParse(guid, out _));
        }


        [TestMethod]
        public async Task CheckMeshSandBox()
        {
            var httpClient = new HttpClient();

            // Construct the request URI by specifying the scheme, hostname, assigned random host port, and the endpoint "uuid".
            var requestUri = new UriBuilder(Uri.UriSchemeHttp, _MeshSandBoxContainer.Hostname, _Container1.GetMappedPublicPort(8080), "health").Uri;

            // Send an HTTP GET request to the specified URI and retrieve the response as a string.
            var result = await httpClient.GetAsync(requestUri);

            // Ensure that the retrieved UUID is a valid GUID.
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }


    }
}