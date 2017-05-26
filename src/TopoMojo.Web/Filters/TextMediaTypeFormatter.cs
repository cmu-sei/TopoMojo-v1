using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace TopoMojo.Web
{
    public class TextMediaTypeFormatter : IInputFormatter
    {
        public bool CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (contentType == null || contentType == "text/plain")
                return true;
            return false;
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var request = context.HttpContext.Request;
            if (request.ContentLength == 0)
            {
                if (context.ModelType.GetTypeInfo().IsValueType)
                    return InputFormatterResult.SuccessAsync(Activator.CreateInstance(context.ModelType));
                else return InputFormatterResult.SuccessAsync(null);
            }

            using (var reader = new StreamReader(context.HttpContext.Request.Body))
            {
                var model = reader.ReadToEnd();
                return InputFormatterResult.SuccessAsync(model);
            }
        }
    }
}