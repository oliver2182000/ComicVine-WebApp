﻿using System.Threading.Tasks;
using Xunit;

namespace ComicVineApi.Tests.Integration
{
    public class FilterTests
    {
        public class TheIncludeMethod
        {
            [Fact]
            public async Task ReturnsCorrectData()
            {
                // arrange
                var client = new ComicVineClient(Settings.ApiKey, Settings.UserAgent);

                // act
                var series = await client.Series.Filter()
                    .IncludeField(s => s.Id)
                    .FirstOrDefaultAsync();

                // assert
                Assert.Equal(1, series.Id);
                Assert.Null(series.Name);
            }
        }
    }
}
