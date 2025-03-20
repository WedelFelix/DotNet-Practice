using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;

var services = new ServiceCollection();

services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => new RefitSettings
    {
        AuthorizationHeaderValueGetter = async (x, y) => await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(c =>
        c.BaseAddress = new Uri("http://localhost:5236"));

var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

//var Movie = await moviesApi.GetMovieAsync("nick-the-greek-2022");

//Console.WriteLine(JsonSerializer.Serialize(Movie));

var request = new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 25
};

var movies = await moviesApi.GetMoviesAsync(request);

foreach (var item in movies.Items) Console.WriteLine(JsonSerializer.Serialize(item));