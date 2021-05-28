using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IcpcResolver.Utils.EventFeed
{
    public class EventFeedRequest
    {
        private readonly HttpWebRequest _request;
        private HttpWebResponse _response;
        public EventFeedRequest(string uri, string username, string password)
        {
            uri += "?stream=false";

            // check arguments
            var success = Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var uriResult);
            success = success && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!success)
            {
                throw new ArgumentException("Invalid uri: " + uri);
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Empty username or password");
            }
            
            // create request
            _request = WebRequest.Create(uri) as HttpWebRequest ?? throw new Exception("Can not create web request");

            // add authorization header
            var encodedId = Convert.ToBase64String(
                Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password)
            );
            _request.Headers.Add("Authorization", "Basic " + encodedId);
            _request.Accept = "application/x-ndjson";
        }

        public async Task<HttpStatusCode> Validate()
        {
            try
            {
                _response = await _request.GetResponseAsync() as HttpWebResponse ?? throw new Exception("NoResponse");
            }
            catch (WebException exception)
            {
                if (exception.Response is not HttpWebResponse exceptionResponse) throw;

                switch (exceptionResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return HttpStatusCode.NotFound;
                    case HttpStatusCode.Unauthorized:
                        return HttpStatusCode.Unauthorized;
                }
                throw;
            }

            return HttpStatusCode.OK;
        }

        public async Task<string> Download()
        {
            var reader = new StreamReader(_response.GetResponseStream());
            var responseStr = await reader.ReadToEndAsync();
            return responseStr;
        }
    }
}