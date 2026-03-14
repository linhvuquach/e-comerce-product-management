using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProductManagement.Integration.Tests.Fixtures;

namespace ProductManagement.Integration.Tests.Products;

/// <summary>
/// Integration tests for /api/v1/products endpoints.
/// Each test class shares one <see cref="ProductApiFactory"/> instance (containers + migrations run once).
/// </summary>
public sealed class ProductsEndpointTests(ProductApiFactory factory)
    : IClassFixture<ProductApiFactory>
{
    // Well-known IDs from the seed migration
    private static readonly Guid _seededActiveProductId = Guid.Parse("22222222-0001-0000-0000-000000000000");
    private static readonly Guid _seededDraftProductId  = Guid.Parse("22222222-0017-0000-0000-000000000000");
    private static readonly Guid _seededCategoryId      = Guid.Parse("11111111-0004-0000-0000-000000000000");
    private static readonly Guid _unknownId             = Guid.NewGuid();

    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    // ── GET /api/v1/products ────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_NoFilters_Returns200WithPagedResult()
    {
        var response = await _client.GetAsync("/api/v1/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResultJson>(_jsonOpts);
        body.Should().NotBeNull();
        body!.Data.Should().NotBeEmpty();
        body.TotalItems.Should().BeGreaterThan(0);
        body.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsCorrectPage()
    {
        var response = await _client.GetAsync("/api/v1/products?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResultJson>(_jsonOpts);
        body!.Data.Should().HaveCount(5);
        body.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetProducts_FilterByStatus_ReturnsMatchingProducts()
    {
        var response = await _client.GetAsync("/api/v1/products?status=Active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResultJson>(_jsonOpts);
        body!.Data.Should().AllSatisfy(p =>
            p.GetProperty("status").GetString().Should().Be("Active"));
    }

    // ── GET /api/v1/products/{id} ───────────────────────────────────────────

    [Fact]
    public async Task GetProductById_ExistingId_Returns200WithETag()
    {
        var response = await _client.GetAsync($"/api/v1/products/{_seededActiveProductId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.ETag.Should().NotBeNull("ETag must be set for optimistic concurrency");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOpts);
        body.GetProperty("id").GetGuid().Should().Be(_seededActiveProductId);
        body.GetProperty("name").GetString().Should().Be("Classic White Tee");
    }

    [Fact]
    public async Task GetProductById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/products/{_unknownId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/v1/products ───────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_ValidRequest_Returns201WithLocation()
    {
        var payload = new
        {
            name        = "Integration Test Jacket",
            brand       = "TestBrand",
            categoryId  = _seededCategoryId,
            basePrice   = 49.99m,
            currency    = "USD",
            description = "Created in integration test",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var id = await response.Content.ReadFromJsonAsync<Guid>(_jsonOpts);
        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_EmptyName_Returns400()
    {
        var payload = new
        {
            name       = "",
            brand      = "TestBrand",
            categoryId = _seededCategoryId,
            basePrice  = 10m,
            currency   = "USD",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_NegativePrice_Returns400()
    {
        var payload = new
        {
            name       = "Bad Price Product",
            brand      = "TestBrand",
            categoryId = _seededCategoryId,
            basePrice  = -5m,
            currency   = "USD",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT /api/v1/products/{id} ───────────────────────────────────────────

    [Fact]
    public async Task UpdateProduct_MissingIfMatchHeader_Returns428()
    {
        var payload = new
        {
            name       = "Updated Name",
            brand      = "Updated Brand",
            categoryId = _seededCategoryId,
            basePrice  = 55m,
            currency   = "USD",
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/products/{_seededActiveProductId}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.PreconditionRequired);
    }

    [Fact]
    public async Task UpdateProduct_WithValidETag_Returns204()
    {
        // First create a product so we have full control over its ETag
        var createPayload = new
        {
            name       = "ETag Test Product",
            brand      = "ETagBrand",
            categoryId = _seededCategoryId,
            basePrice  = 30m,
            currency   = "USD",
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createPayload);
        var newId = await createResponse.Content.ReadFromJsonAsync<Guid>(_jsonOpts);

        // GET to obtain the ETag
        var getResponse = await _client.GetAsync($"/api/v1/products/{newId}");
        var etag = getResponse.Headers.ETag!.Tag;

        // PUT with If-Match
        var updatePayload = new
        {
            name       = "ETag Test Product Updated",
            brand      = "ETagBrand",
            categoryId = _seededCategoryId,
            basePrice  = 35m,
            currency   = "USD",
        };
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/products/{newId}")
        {
            Content = JsonContent.Create(updatePayload),
            Headers = { IfMatch = { new System.Net.Http.Headers.EntityTagHeaderValue(etag) } }
        };

        var updateResponse = await _client.SendAsync(request);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── PATCH /api/v1/products/{id}/status ─────────────────────────────────

    [Fact]
    public async Task PatchProductStatus_DraftToActive_Returns204()
    {
        // Use the seeded Draft product
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{_seededDraftProductId}/status",
            new { status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PatchProductStatus_ArchivedToActive_Returns422()
    {
        // First create and archive a fresh product
        var created = await _client.PostAsJsonAsync("/api/v1/products", new
        {
            name       = "To Be Archived",
            brand      = "Brand",
            categoryId = _seededCategoryId,
            basePrice  = 10m,
            currency   = "USD",
        });
        var id = await created.Content.ReadFromJsonAsync<Guid>(_jsonOpts);

        await _client.PatchAsJsonAsync($"/api/v1/products/{id}/status", new { status = "Archived" });

        // Now try to re-activate → should be 422
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{id}/status",
            new { status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PatchProductStatus_UnknownProduct_Returns404()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{_unknownId}/status",
            new { status = "Active" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/v1/products/{id} ────────────────────────────────────────

    [Fact]
    public async Task DeleteProduct_ExistingProduct_Returns204()
    {
        var created = await _client.PostAsJsonAsync("/api/v1/products", new
        {
            name       = "To Be Deleted",
            brand      = "Brand",
            categoryId = _seededCategoryId,
            basePrice  = 10m,
            currency   = "USD",
        });
        var id = await created.Content.ReadFromJsonAsync<Guid>(_jsonOpts);

        var response = await _client.DeleteAsync($"/api/v1/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteProduct_UnknownId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/v1/products/{_unknownId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    // Minimal projection for deserializing paged results
    private sealed record PagedResultJson(
        JsonElement[] Data,
        int TotalItems,
        int Page,
        int PageSize);
}
