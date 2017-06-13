namespace LinqToQuerystring.IntegrationTests.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using LinqToQuerystring.WebApi;

    public class DataController : ApiController
    {
        [LinqToQueryable]
        public IQueryable<DataClass> Get()
        {
            return
                new List<DataClass>
                {
                    new DataClass("Peter", 29, true),
                    new DataClass("Kathryn", 26, false)
                }.AsQueryable();
        }
    }
}
