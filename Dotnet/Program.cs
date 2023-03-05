using System.Buffers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestClient;

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("X-AppId", "");
httpClient.DefaultRequestHeaders.Add("X-AppKey", "");
httpClient.DefaultRequestHeaders.Add("X-UserId", "");
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    WriteIndented = false
};

if (args.Length == 0)
{
    Console.WriteLine("Finished!");
    return;
}

var isNumber = int.TryParse(args[0], out var maybeDDL);
var ddl = isNumber ? DateTimeOffset.Now.AddDays(maybeDDL) : DateTimeOffset.Now.AddYears(5);

var files = isNumber ? args[1..] : args;
var works = files.Select(async i => await UploadSingleFile(i));
var result = await Task.WhenAll(works);

Console.WriteLine("Upload Success:");
foreach (var i in result)
{
    Console.WriteLine(i);
}

async ValueTask<string> UploadSingleFile(string filePath)
{
    var fileInfo = new FileInfo(filePath);
    if (!fileInfo.Exists)
    {
        throw new FileNotFoundException(filePath);
    }

    if (fileInfo.Length > 10485760L)
    {
        return "File is too large, make sure it's less than 10MB!";
    }

    using var fileStream = File.OpenRead(filePath);
    using var sha256 = SHA256.Create();
    var digest = await sha256.ComputeHashAsync(fileStream);

    var createEntryDto = new CreateFileEntryRequest
    {
        Path = "/typora/shared_storage",
        FileNameWithExt = $"{Guid.NewGuid():N}{Path.GetExtension(filePath)}",
        FileSize = Convert.ToUInt64(fileInfo.Length),
        SHA256 = Convert.ToHexString(digest).ToLower(),
        MimeType = GetMimeType(fileInfo.Name),
        DeadLine = ddl
    };
    using var createEntryRequest = new HttpRequestMessage(HttpMethod.Post, "https://tcp-cos.kevinc.ltd:8080/file/createFileEntry");
    createEntryRequest.Content = JsonContent.Create(createEntryDto, MediaTypeHeaderValue.Parse("application/json"), jsonOptions);
    using var createEntryResponse = await httpClient.SendAsync(createEntryRequest);
    if (!createEntryResponse.IsSuccessStatusCode)
    {
        return "Error!";
    }
    var fileMetaData = await createEntryResponse.Content.ReadFromJsonAsync<CreateFileEntryReply>(jsonOptions);

    if (fileMetaData!.NextRequestedFrame == 0)
    {
        // 已经出现过的文件，不需要再传了
        return $"https://cos.kevinc.ltd/file/download?fileId={fileMetaData.Id}";
    }

    using var memoryHolder = MemoryPool<byte>.Shared.Rent(1048576);
    for (long i = 0; i < fileMetaData.Frames; i++)
    {
        var offset = i * 1048576;
        fileStream.Seek(offset, SeekOrigin.Begin);
        var contentLength = await fileStream.ReadAsync(memoryHolder.Memory);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"https://tcp-cos.kevinc.ltd:8080/file/upload?fileId={fileMetaData.Id}&seqNumber={i + 1}");
        request.Content = new ReadOnlyMemoryContent(memoryHolder.Memory[..contentLength]);
        using var resp = await httpClient.SendAsync(request);

        if (!resp.IsSuccessStatusCode)
        {
            return "Error!";
        }
    }

    return $"https://cos.kevinc.ltd/file/download?fileId={fileMetaData.Id}";
}

string GetMimeType(string fileName)
{
    var ext = Path.GetExtension(fileName).ToLower();
    return ext switch
    {
        ".mp4" => "video/mp4",
        ".mov" => "video/quicktime",
        ".mp3" => "audio/mp3",
        ".aac" => "audio/aac",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream",
    };
}