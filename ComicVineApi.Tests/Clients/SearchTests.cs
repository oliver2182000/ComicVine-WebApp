﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComicVineApi.Clients;
using ComicVineApi.Http;
using ComicVineApi.Models;
using NSubstitute;
using Xunit;

namespace ComicVineApi.Tests.Clients
{
    public class SearchTests
    {
        public class SearchOptionsObject
        {
            [Fact]
            public void CloneCopiesEverything()
            {
                // arrange
                var original = new SearchOptions("age")
                {
                    FieldList = { "id", "name" },
                    Limit = 33,
                    Offset = 44,
                    Resources = { "character" },
                };

                // act
                var clone = new SearchOptions(original);

                // assert
                Assert.Equal(new[] { "id", "name" }, clone.FieldList);
                Assert.Equal(33, clone.Limit);
                Assert.Equal(44, clone.Offset);
                Assert.Equal("age", clone.Query);
                Assert.Equal(new[] { "character" }, clone.Resources);
            }
        }

        public class TheCountAsyncMethod
        {
            [Fact]
            public async Task OverwritesAllParameters()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var model = await client.Search("anything")
                    .Take(7)
                    .CountAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o =>
                        o.Count == 4 &&
                        o["query"].Equals("anything") &&
                        o["offset"].Equals(0) &&
                        o["limit"].Equals(1) &&
                        o["field_list"].Equals("id")));
            }
        }

        public class TheFirstOrDefaultAsyncMethod
        {
            [Fact]
            public async Task OverwritesLimitParameter()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var model = await client.Search("anything")
                    .Take(7)
                    .FirstOrDefaultAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["offset"].Equals(0) && o["limit"].Equals(1)));
            }

            [Fact]
            public async Task PreservesOffsetParameter()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var model = await client.Search("anything")
                    .Skip(7)
                    .FirstOrDefaultAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["offset"].Equals(7) && o["limit"].Equals(1)));
            }
        }

        public class SearchOptionsTests
        {
            [Fact]
            public async Task DefaultValuesAreCorrect()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var models = await client.Search("anything")
                    .ToListAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Is<Uri>(u => u.ToString() == "search"),
                    Arg.Is<Dictionary<string, object>>(o =>
                        o.Count == 3 &&
                        o["query"].Equals("anything") &&
                        o["offset"].Equals(0) &&
                        o["limit"].Equals(10)));
            }
        }

        public class TheIncludeFieldMethod
        {
            [Fact]
            public async Task NoIncludeFieldDoesNotHaveFieldListParameter()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var model = await client.Search("anything")
                    .FirstOrDefaultAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => !o.ContainsKey("field_list")));
            }

            [Fact]
            public async Task SpecifiesCorrectParameters()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var model = await client.Search("anything")
                    .IncludeField<Character>(c => c.Id)
                    .IncludeField<Character>(c => c.Origin)
                    .IncludeField<Issue>(c => c.CoverDate)
                    .FirstOrDefaultAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["field_list"].Equals("id,origin,cover_date")));
            }

            [Fact]
            public async Task SupportsSplittingRequests()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var Search = client.Search("anything")
                    .IncludeField<Character>(c => c.Id);
                var SearchA = Search
                    .IncludeField<Character>(c => c.Origin);
                var SearchB = Search
                    .IncludeField<Issue>(c => c.CoverDate);

                // assert
                var model = await Search.FirstOrDefaultAsync();
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["field_list"].Equals("id")));

                var modelA = await SearchA.FirstOrDefaultAsync();
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["field_list"].Equals("id,origin")));

                var modelB = await SearchB.FirstOrDefaultAsync();
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["field_list"].Equals("id,cover_date")));
            }
        }

        public class TheIncludeResourceMethod
        {
            [Fact]
            public async Task NoIncludeResourceDoesNotHaveResourcesParameter()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var model = await client.Search("anything")
                    .FirstOrDefaultAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => !o.ContainsKey("resources")));
            }

            [Fact]
            public async Task SpecifiesCorrectParameters()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var model = await client.Search("anything")
                    .IncludeResource(SearchResource.Character)
                    .IncludeResource(SearchResource.Issue)
                    .FirstOrDefaultAsync();

                // assert
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["resources"].Equals("character,issue")));
            }

            [Fact]
            public async Task SupportsSplittingRequests()
            {
                // arrange
                var httpConnection = Substitute.For<IHttpConnection>();
                httpConnection
                    .SearchAsync(Arg.Any<Uri>(), Arg.Any<Dictionary<string, object>>())
                    .Returns(Task.FromResult(new SearchResult { Results = Array.Empty<ComicVineObject>() }));
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var Search = client.Search("anything")
                    .IncludeResource(SearchResource.Character);
                var SearchA = Search
                    .IncludeResource(SearchResource.Origin);
                var SearchB = Search
                    .IncludeResource(SearchResource.Issue);

                // assert
                var model = await Search.FirstOrDefaultAsync();
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["resources"].Equals("character")));

                var modelA = await SearchA.FirstOrDefaultAsync();
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["resources"].Equals("character,origin")));

                var modelB = await SearchB.FirstOrDefaultAsync();
                _ = httpConnection.Received().SearchAsync(
                    Arg.Any<Uri>(),
                    Arg.Is<Dictionary<string, object>>(o => o["resources"].Equals("character,issue")));
            }
        }

        public class TheToAsyncEnumerableMethod
        {
            [Theory]
            [InlineData(0, 0)]
            [InlineData(5, 5)]
            [InlineData(10, 10)]
            [InlineData(15, 15)]
            [InlineData(20, 20)]
            [InlineData(25, 20)]
            [InlineData(100, 20)]
            public async Task FetchesTheCorrectAmount(int desiredSize, int actualSize)
            {
                // arrange
                var actual = Enumerable.Range(1, actualSize).ToList();
                var httpConnection = Substitute.For<IHttpConnection>();
                AddCall(httpConnection, 0, 10);
                AddCall(httpConnection, 10, 10);
                AddEmptyCall(httpConnection, 10);
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var enumerable = client.Search("anything")
                    .Take(desiredSize)
                    .ToAsyncEnumerable();
                var results = new List<int>();
                await foreach (var res in enumerable)
                    results.Add(res.Id!.Value);

                // assert
                Assert.Equal(actual, results);
            }

            [Theory]
            [InlineData(0, 0)]
            [InlineData(5, 5)]
            [InlineData(10, 10)]
            [InlineData(15, 15)]
            [InlineData(20, 20)]
            [InlineData(25, 24)]
            [InlineData(100, 24)]
            public async Task FetchesTheCorrectAmountWithPartialPage(int desiredSize, int actualSize)
            {
                // arrange
                var actual = Enumerable.Range(1, actualSize).ToList();
                var httpConnection = Substitute.For<IHttpConnection>();
                AddCall(httpConnection, 0, 10);
                AddCall(httpConnection, 10, 10);
                AddCall(httpConnection, 20, 4);
                AddEmptyCall(httpConnection, 20);
                var apiConnection = new ApiConnection(httpConnection);
                var client = new SearchClient(apiConnection);

                // act
                var enumerable = client.Search("anything")
                    .Take(desiredSize)
                    .ToAsyncEnumerable();
                var results = new List<int>();
                await foreach (var res in enumerable)
                    results.Add(res.Id!.Value);

                // assert
                Assert.Equal(actual, results);
            }

            private static void AddCall(IHttpConnection httpConnection, int offset, int count)
            {
                httpConnection
                    .SearchAsync(
                        Arg.Any<Uri>(),
                        Arg.Is<Dictionary<string, object>>(o => o["offset"].Equals(offset)))
                    .Returns(Task.FromResult(new SearchResult
                    {
                        Results = Enumerable.Range(offset + 1, count)
                            .Select(i => new TestModel { Id = i })
                            .ToArray()
                    }));
            }

            private static void AddEmptyCall(IHttpConnection httpConnection, int startOffset)
            {
                httpConnection
                    .SearchAsync(
                        Arg.Any<Uri>(),
                        Arg.Is<Dictionary<string, object>>(o => (int)o["offset"] > startOffset))
                    .Returns(Task.FromResult(new SearchResult
                    {
                        Results = Array.Empty<TestModel>()

                    }));
            }
        }
    }
}
