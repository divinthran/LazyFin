using MediaBrowser.Model.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Jellyfin.Plugin.MultiSourceScraper
{
    [Route("/MultiSourceStream/Stream", "GET", Summary = "Intercepts STRM requests and redirects to VidKing")]
    public class StreamRequest : IReturn<string>
    {
        public string? ImdbId { get; set; }
        public string? TmdbId { get; set; }
    }

    public class StreamController : IService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StreamController> _logger;

        public StreamController(IHttpClientFactory httpClientFactory, ILogger<StreamController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<object> Get(StreamRequest request)
        {
            string? targetId = !string.IsNullOrEmpty(request.ImdbId) ? request.ImdbId : request.TmdbId;

            if (string.IsNullOrEmpty(targetId))
            {
                return HttpError.BadRequest("Missing both IMDb and TMDb ID parameters.");
            }
            
            try
            {
                string apiKey = Plugin.Instance?.Configuration.VidkingApiKey ?? string.Empty;
                string vidkingUrl = $"https://api.vidking.com/v1/src/{targetId}?apiKey={apiKey}";
                
                var apiResponse = await _httpClient.GetFromJsonAsync<VidkingResponse>(vidkingUrl);

                if (apiResponse?.Streams != null && apiResponse.Streams.Count > 0)
                {
                    // Sort order: Match 4K first, then 1080p, then fall back to highest internal bitrate
                    var bestStream = apiResponse.Streams
                        .OrderByDescending(s => s.Quality == "4K")
                        .ThenByDescending(s => s.Quality == "1080p")
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();

                    if (bestStream != null && !string.IsNullOrEmpty(bestStream.Url))
                    {
                        // A 302 Redirect lets Jellyfin handle playback, buffer speeds, and saving progress tracking natively
                        return HttpResult.Redirect(bestStream.Url);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error extracting high-speed stream from VidKing: {Message}", ex.Message);
            }

            return HttpError.NotFound("No compatible streaming sources could be pulled from the API provider.");
        }
    }

    public class VidkingResponse 
    { 
        public List<VidkingStream>? Streams { get; set; } 
    }

    public class VidkingStream 
    { 
        public string Url { get; set; } = string.Empty; 
        public string Quality { get; set; } = string.Empty; 
        public int Bitrate { get; set; }
    }
}