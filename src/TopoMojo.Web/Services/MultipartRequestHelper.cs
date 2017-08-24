using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace TopoMojo.Services
{
    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec says 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && string.IsNullOrEmpty(contentDisposition.FileName)
                   && string.IsNullOrEmpty(contentDisposition.FileNameStar);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!string.IsNullOrEmpty(contentDisposition.FileName)
                       || !string.IsNullOrEmpty(contentDisposition.FileNameStar));
        }

        public static NameValueCollection FileProperties(string querystring)
        {
            string input = HeaderUtilities.RemoveQuotes(querystring);
            NameValueCollection props = new NameValueCollection();
            string[] fields = input.Split('&');
            foreach (string field in fields)
            {
                string[] prop = field.Split('=');
                string key = prop[0].Trim();
                string val = (prop.Length > 1) ? prop[1].Trim() : "";
                if (!String.IsNullOrEmpty(key))
                    props.Add(key, val);
            }
            return props;
        }

    }

    public class MultipartRequestHandler
    {

        public MultipartRequestHandler()
        {
             _formOptions = new FormOptions();
        }
        FormOptions _formOptions;

        public async Task Process(
            HttpRequest request,
            Action<FormOptions> optionsAction,
            Action<NameValueCollection> dispositionAction
        )
        {
            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
            {
                throw new InvalidOperationException($"Expected a multipart request, but got {request.ContentType}");
            }

            optionsAction.Invoke(_formOptions);

            string boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(request.ContentType),
                _formOptions.MultipartBoundaryLengthLimit);

            NameValueCollection metadata = new NameValueCollection();
            MultipartReader reader = new MultipartReader(boundary, request.Body);
            MultipartSection section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                bool hasContentDispositionHeader = ContentDispositionHeaderValue
                    .TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        //add form values to data collection
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            string value = await streamReader.ReadToEndAsync();
                            if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = String.Empty;
                            }
                            metadata.Add(MultipartRequestHelper.FileProperties(value));
                        }

                    }

                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        metadata.Add("rawFilename", contentDisposition.FileName);
                        dispositionAction.Invoke(metadata);

                        // handleAction.Invoke()
                        // Log("uploading", null, filename);
                        // string dest = DestinationPath(filename, key, scope);

                        // using (var targetStream = System.IO.File.Create(dest))
                        // {
                        //     await Save(section.Body, targetStream, size, pkey);
                        // }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
        }

        private Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }

    public class MultipartFileSaveOptions
    {
        public string DestinationPath { get; set; }
        public string ProgressKey { get; set; }
        public long size { get; set; }
        public Stream SourceStream { get; set; }
    }
}