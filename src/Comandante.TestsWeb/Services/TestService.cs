using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comandante.TestsWeb.Services
{
    public interface ITestService
    {
        object Test(int id, string test, DateTime? date);
    }

    public class TestService : ITestService
    {
        private readonly IOptions<Db> db;

        public TestService(IOptions<Db> db)
        {
            this.db = db;
        }
        public object NotImplementedExceptionTest(int id, string test, DateTime? date)
        {
            throw new NotImplementedException();
        }

        public object Test(int id, string test, DateTime? date)
        {
            return new
            {
                Id = id,
                Test = test,
                Date = date
            };
        }

        public async Task TestAsync(string id)
        {
            await Task.CompletedTask;
        }
    }

}
