using System.Text.Json.Serialization;

namespace TestClient
{
    public class CreateFileEntryRequest
    {
        /// <summary>
        /// 文件的路径，以 / 开头，按照unix风格，例如 /home/kcos/pictures/test/
        /// 结尾的 / 可有可无，后端会自动处理
        /// 文件长度比较玄学，跟虚拟机、数据库的支持有关系
        /// 建议每个“段”不要超过255个字符（建议而已，就算超过了我也不知道会发生什么）
        /// </summary>
        public string Path { get; set; } = "/";

        /// <summary>
        /// 带后缀的文件名
        /// 文件名中不要含有特殊字符，例如空格，/，emoji之类的，建议全程ASCII
        /// 文件名（含后缀名）不要超过255字节（注意，中文算3字节）
        /// </summary>
        public string FileNameWithExt { get; set; } = "file.unknown";

        public ulong FileSize { get; set; } = 0UL;

        /// <summary>
        /// 后端为了减轻储存压力，会把那些已经存在的数据进行重复引用
        /// SHA256就是一个很重要的判据
        /// 当后端发现，文件的SHA256与文件大小完全一致的时候，会自动重复引用
        /// </summary>
        [JsonPropertyName("sha256")]
        public string SHA256 { get; set; } = null!;

        public string MimeType { get; set; } = "application/octet-stream";

        /// <summary>
        /// 以下两个字段看文档解释
        /// </summary>
        public int Protection { get; set; } = 0;
        public string SecurityPayload { get; set; } = string.Empty;

        public DateTimeOffset DeadLine { get; set; } = DateTimeOffset.MaxValue;
    }

    public class CreateFileEntryReply
    {
        /// <summary>
        /// 以后下载用ID来下就可以了
        /// </summary>
        public long Id { get; set; } = 0L;

        public long FileSize { get; set; } = 0L;

        public long Frames { get; set; } = 0L;

        /// <summary>
        /// 如果这个字段为0，表示根本不需要进行传送了，已经有可以复用的了
        /// </summary>
        public long NextRequestedFrame { get; set; } = 1L;

        public string[] PathHierarchy { get; set; } = Array.Empty<string>();

        public string FileNameWithExt { get; set; } = "file.unknown";

        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset DeadLine { get; set; } = DateTimeOffset.MaxValue;
    }

    public class UploadFileReply
    {
        public ulong NextRequestedFrame { get; set; } = 0UL;
    }
}
