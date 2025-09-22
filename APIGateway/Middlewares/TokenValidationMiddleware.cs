using APIGateway.config;
using APIGateway.Singleton;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _identityServiceUrl;
    private readonly Store _store;
    private readonly IHostEnvironment _environment;

    public TokenValidationMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, IOptions<IdentityServiceOptions> identityServiceOptions, Store store, IHostEnvironment environment)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _identityServiceUrl = identityServiceOptions.Value.IdentityServiceUrl ?? throw new ArgumentNullException(nameof(identityServiceOptions.Value.IdentityServiceUrl), "IdentityServiceUrl is missing in configuration.");
        _store = store;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_store.noAuthroutes.Contains(context.Request.Path))
        {
            // This route does not need authorization or authentication, simply move on
            await _next(context);
            return;
        }
        var authorizationHeader = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            // Set content type to HTML
            context.Response.ContentType = "text/html";

            // HTML content for the unauthorized access message
            var htmlContent = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Invalid access</title>
    <style>
        body {
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100vh;
            margin: 0;
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
        }

        .redirect-container {
            text-align: center;
        }

        .redirect-container h1 {
            color: #333;
            font-size: 24px;
        }

        .redirect-container p {
            color: #666;
            font-size: 16px;
        }
    </style>
</head>
<body>
    <div class=""redirect-container"">
        <h1>Authorization Required. Contact site for more information.</h1>
        <p>You are being redirected. If the page doesn't automatically redirect, <a href=""https://dev.convos.site"">click here</a>.</p>
    </div>
</body>
</html>";

            // Write the HTML content to the response
            await context.Response.WriteAsync(htmlContent);
            return;
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_identityServiceUrl);  

        Console.WriteLine("Identity service url:"+_identityServiceUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/validate");
        request.Headers.Add("Authorization", authorizationHeader);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token validation failed.");
            return;
        }

        await _next(context);
    }
}
