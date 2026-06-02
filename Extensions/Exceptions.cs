using Microsoft.AspNetCore.Mvc;

namespace Tasqana.Extensions
{
    public class HttpException : Exception
    {
        public ActionResult Result;
        protected HttpException(ActionResult result) { Result = result; }
    }

    public class NotFoundException : HttpException
    {
        public NotFoundException() : base(new NotFoundObjectResult(null)) { }
    }

}
