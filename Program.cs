using System.Buffers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestClient;

// 准备好所需的Http客户端和必要的Header
var httpClient = new HttpClient();
// 这些是假数据
httpClient.DefaultRequestHeaders.Add("X-AppId", "");
httpClient.DefaultRequestHeaders.Add("X-AppKey", "");
httpClient.DefaultRequestHeaders.Add("X-UserId", "");
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    WriteIndented = false
};

// 解析typora调用的参数，然后挨个上传、输出
var files = args;
var works = files.Select(async i => await UploadSingleFile(i));
var result = await Task.WhenAll(works);

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

    // 计算文件的SHA256，最大化利用已存在的文件
    using var fileStream = File.OpenRead(filePath);
    using var sha256 = SHA256.Create();
    var digest = await sha256.ComputeHashAsync(fileStream);

    // 创建文件句柄
    var createEntryDto = new CreateFileEntryRequest
    {
        Path = "/typora/shared_storage",
        FileNameWithExt = $"{Guid.NewGuid():N}{Path.GetExtension(filePath).ToLower()}",
        FileSize = fileInfo.Length,
        SHA256 = Convert.ToHexString(digest).ToLower(),
        MimeType = GetMimeType(fileInfo.Name),
        DeadLine = DateTimeOffset.Now.AddYears(10)
    };
    using var createEntryRequest = new HttpRequestMessage(HttpMethod.Post, "https://cos.kevinc.ltd/file/createFileEntry");
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
        Console.WriteLine("Duplicated file reused.");
        return $"https://cos.kevinc.ltd/file/download?fileId={fileMetaData.Id}";
    }

    // 将文件变成1MB的大小的分块上传
    // 这是一个非常耗费IO的任务，如果我们用数组做buffer的话，难免会出现大量内存分配，引发GC问题
    using var memoryHolder = MemoryPool<byte>.Shared.Rent(1048576);
    for (long i = 0; i < fileMetaData.Frames; i++)
    {
        var offset = i * 1048576;
        fileStream.Seek(offset, SeekOrigin.Begin);
        var contentLength = await fileStream.ReadAsync(memoryHolder.Memory);

        // 组装一个Http请求
        using var request = new HttpRequestMessage(HttpMethod.Put, $"https://tcp-cos.kevinc.ltd:8080/file/upload?fileId={fileMetaData.Id}&seqNumber={i + 1}");
        request.Content = new ReadOnlyMemoryContent(memoryHolder.Memory[..contentLength]);
        using var resp = await httpClient.SendAsync(request);

        if (!resp.IsSuccessStatusCode)
        {
            return "Error!";
        }
        else
        {
            // 输出上传分包的进度
            Console.WriteLine($"Writing file pack {i + 1} out of {fileMetaData.Frames}");
        }
    }

    return $"https://cos.kevinc.ltd/file/download?fileId={fileMetaData.Id}";
}

// 根据文件后缀名匹配mime-type，这里的并不完善，也不安全
string GetMimeType(string fileName)
{
    var ext = Path.GetExtension(fileName).ToLower();
    return ext switch
    {
        ".mp4" => "video/mp4",
        ".mp3" => "audio/mp3",
        ".aac" => "audio/aac",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream",
    };
}