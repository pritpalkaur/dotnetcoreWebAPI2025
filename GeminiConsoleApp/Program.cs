using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GeminiConsoleApp
{
    // --- Request and Response Data Transfer Objects (DTOs) ---

    // For requesting content generation
    public class GenerateContentRequest
    {
        [JsonPropertyName("contents")]
        public Content[] Contents { get; set; } = [];

        [JsonPropertyName("tools")]
        public Tool[] Tools { get; set; } = [];
    }

    public class Content
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; } = [];
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class Tool
    {
        // Using "google_search": {} to enable grounding
        [JsonPropertyName("google_search")]
        public object GoogleSearch { get; set; } = new { };
    }


    // For parsing the response
    public class GenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[] Candidates { get; set; } = [];
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }

        [JsonPropertyName("groundingMetadata")]
        public GroundingMetadata? GroundingMetadata { get; set; }
    }

    public class GroundingMetadata
    {
        [JsonPropertyName("groundingAttributions")]
        public GroundingAttribution[] GroundingAttributions { get; set; } = [];
    }

    public class GroundingAttribution
    {
        [JsonPropertyName("web")]
        public WebSource? Web { get; set; }
    }

    public class WebSource
    {
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }


    public class Program
    {
        // Use the preview model that supports tools/grounding
        private const string ModelName = "gemini-2.5-flash-preview-09-2025";
        private const string ApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Gemini LLM Console Application ---");
                
           // string GEMINI_API_KEY= "AIzaSyALRNon7-42zuKJAbOwHmJNHxCEazOaaXU";
            // 1. Get API Key from environment variable
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FATAL ERROR: GEMINI_API_KEY environment variable not set.");
                Console.WriteLine("Please set the variable with your Google AI API key.");
                Console.ResetColor();
                return;
            }

            // 2. Define the user's query and enable Google Search grounding
           // var prompt = "What are the latest developments in .NET Core 8.0 and C# 12?";
            var prompt = "tell me todays temprature in singapore ";
            Console.WriteLine($"\nQuerying Gemini ({ModelName})...\n");
            Console.WriteLine($"PROMPT: {prompt}\n");

            // 3. Prepare the request payload
            var requestPayload = new GenerateContentRequest
            {
                Contents =
                [
                    new Content
                    {
                        Parts =
                        [
                            new Part { Text = prompt }
                        ]
                    }
                ],
                Tools =
                [
                    // Enable Google Search grounding
                    new Tool { GoogleSearch = new { } }
                ]
            };

            // 4. Construct the URL
            var apiUrl = $"{ApiBaseUrl}{ModelName}:generateContent?key={apiKey}";

            // 5. Make the API call with exponential backoff for resilience
            GenerateContentResponse? response = null;
            using (var httpClient = new HttpClient())
            {
                int maxRetries = 5;
                int delay = 1000; // 1 second

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var httpResponse = await httpClient.PostAsJsonAsync(apiUrl, requestPayload);

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            var jsonString = await httpResponse.Content.ReadAsStringAsync();
                            response = JsonSerializer.Deserialize<GenerateContentResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            break; // Success, exit retry loop
                        }
                        else
                        {
                            // Handle API errors (e.g., 4xx or 5xx that are not retriable 500/503)
                            var errorContent = await httpResponse.Content.ReadAsStringAsync();
                            if (httpResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                                httpResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                            {
                                if (attempt < maxRetries)
                                {
                                    await Task.Delay(delay);
                                    delay *= 2; // Exponential backoff
                                    continue;
                                }
                            }
                            throw new HttpRequestException($"API request failed with status code {httpResponse.StatusCode}.\nContent: {errorContent}");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (attempt == maxRetries)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\nFatal error after {maxRetries} attempts: {ex.Message}");
                            Console.ResetColor();
                            return;
                        }
                        await Task.Delay(delay);
                        delay *= 2; // Exponential backoff
                    }
                }
            }

            // 6. Process the response
            if (response?.Candidates?.Length > 0 && response.Candidates[0].Content?.Parts?.Length > 0)
            {
                var generatedText = response.Candidates[0].Content.Parts[0].Text;
                var metadata = response.Candidates[0].GroundingMetadata;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("--- GENERATED RESPONSE ---\n");
                Console.ResetColor();
                Console.WriteLine(generatedText);

                // 7. Display grounding sources (citations)
                if (metadata != null && metadata.GroundingAttributions.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n--- GROUNDING SOURCES (Cited) ---");
                    Console.ResetColor();
                    for (int i = 0; i < metadata.GroundingAttributions.Length; i++)
                    {
                        var source = metadata.GroundingAttributions[i].Web;
                        if (source != null)
                        {
                            Console.WriteLine($"- Source {i + 1}: {source.Title}");
                            Console.WriteLine($"  URI: {source.Uri}");
                        }
                    }
                }
                Console.WriteLine("\n----------------------------------");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nCould not retrieve a valid response from the model.");
                Console.ResetColor();
            }
        }
    }
}