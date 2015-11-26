﻿using Dapper;
using Mendham.Infrastructure.Dapper.Test.Fixtures;
using Mendham.Infrastructure.Dapper.Test.Helpers;
using Mendham.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mendham.Infrastructure.Dapper.Test
{
    public class ConnectionWithSetTest : BaseUnitTest<DatabaseFixture>
    {
        public ConnectionWithSetTest(DatabaseFixture fixture) : base(fixture)
        {
        }


        [Fact]
        public async Task ConnectionWithSet_IntDefaultMapping_AllSelectedValues()
        {
            var mapping = DefaultConnectionWithSetMapping.Get<int>();
            using (var conn = new ConnectionWithSet<int>(Fixture.CreateSut(), mapping))
            {
                await conn.OpenAsync(Fixture.KnownInts);

                var q = await conn.QueryAsync<int>(@"
                    SELECT Id
                    FROM IntTable it
                        INNER JOIN #Items items ON it.Id = items.Value
                ");

                var result = q.ToList();

                Assert.NotEmpty(result);
                Assert.Equal(Fixture.KnownInts.Count(), result.Count());
                Assert.Equal(Fixture.KnownInts.OrderBy(a => a), result.OrderBy(a => a));
            }
        }

        [Fact]
        public async Task ConnectionWithSet_GuidDefaultMapping_AllSelectedValues()
        {
            var mapping = DefaultConnectionWithSetMapping.Get<Guid>();
            using (var conn = new ConnectionWithSet<Guid>(Fixture.CreateSut(), mapping))
            {
                await conn.OpenAsync(Fixture.KnownGuids);

                var q = await conn.QueryAsync<Guid>(@"
                    SELECT Id
                    FROM GuidTable gt
                        INNER JOIN #Items items ON gt.Id = items.Value
                ");

                var result = q.ToList();

                Assert.NotEmpty(result);
                Assert.Equal(Fixture.KnownGuids.Count(), result.Count());
                Assert.Equal(Fixture.KnownGuids.OrderBy(a => a), result.OrderBy(a => a));
            }
        }

        [Fact]
        public async Task ConnectionWithSet_StringDefaultMapping_AllSelectedValues()
        {
            var mapping = DefaultConnectionWithSetMapping.Get<string>();
            using (var conn = new ConnectionWithSet<string>(Fixture.CreateSut(), mapping))
            {
                await conn.OpenAsync(Fixture.KnownStrings);

                var q = await conn.QueryAsync<string>(@"
                    SELECT Id
                    FROM StrTable st
                        INNER JOIN #Items items ON st.Id = items.Value
                ");

                var result = q.ToList();

                Assert.NotEmpty(result);
                Assert.Equal(Fixture.KnownStrings.Count(), result.Count());
                Assert.Equal(Fixture.KnownStrings.OrderBy(a => a), result.OrderBy(a => a));
            }
        }

        [Fact]
        public async Task ConnectionWithSet_CompositeIdMapping_AllSelectedValues()
        {
            var mapping = Fixture.GetCompositeIdMapping();
            using (var conn = new ConnectionWithSet<CompositeId>(Fixture.CreateSut(), mapping))
            {
                await conn.OpenAsync(Fixture.KnownCompositeIds);

                var q = await conn.QueryAsync<CompositeId>(@"
                    SELECT tcit.GuidVal, tcit.IntVal
                    FROM CompositeIdTable tcit
                        INNER JOIN #TestCompositeIdSet items ON tcit.GuidVal = items.GuidVal
                            AND tcit.IntVal= items.IntVal
                ");

                var result = q.ToList();

                Assert.NotEmpty(result);
                Assert.Equal(Fixture.KnownCompositeIds.Count(), result.Count());
                Assert.Equal(Fixture.KnownCompositeIds.OrderBy(a => a.GuidVal), result.OrderBy(a => a.GuidVal));
            }
        }
    }
}
