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
    }
}
