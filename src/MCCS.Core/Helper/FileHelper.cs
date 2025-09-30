using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace MCCS.Core.Helper
{
    /// <summary>
    /// 高性能文件读写库
    /// </summary>
    public static class FileHelper
    {
        // 默认配置
        private const int DefaultBufferSize = 65536; // 64KB
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None
        };

        #region 同步文本操作

        /// <summary>
        /// 同步写入文本文件
        /// </summary>
        public static void WriteText(string filePath, string content, bool append = false, int? bufferSize = null, Encoding encoding = null)
        {
            EnsureDirectoryExists(filePath);

            var buffer = bufferSize ?? DefaultBufferSize;
            var enc = encoding ?? DefaultEncoding;

            using var stream = new FileStream(
                filePath,
                append ? FileMode.Append : FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                buffer,
                FileOptions.SequentialScan);
            using var writer = new StreamWriter(stream, enc, buffer);
            writer.Write(content);
        }

        /// <summary>
        /// 同步读取文本文件
        /// </summary>
        public static string ReadText(string filePath, int? bufferSize = null, Encoding encoding = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var buffer = bufferSize ?? DefaultBufferSize;
            var enc = encoding ?? DefaultEncoding;

            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                buffer,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, enc, true, buffer);
            return reader.ReadToEnd();
        }

        #endregion

        #region 异步文本操作

        /// <summary>
        /// 异步写入文本文件
        /// </summary>
        public static async Task WriteTextAsync(string filePath, string content, bool append = false, int? bufferSize = null, Encoding encoding = null)
        {
            EnsureDirectoryExists(filePath);

            var buffer = bufferSize ?? DefaultBufferSize;
            var enc = encoding ?? DefaultEncoding;

            await using var stream = new FileStream(
                filePath,
                append ? FileMode.Append : FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                buffer,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using var writer = new StreamWriter(stream, enc, buffer);
            await writer.WriteAsync(content).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步读取文本文件
        /// </summary>
        public static async Task<string> ReadTextAsync(string filePath, int? bufferSize = null, Encoding encoding = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var buffer = bufferSize ?? DefaultBufferSize;
            var enc = encoding ?? DefaultEncoding;

            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                buffer,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, enc, true, buffer);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        #endregion

        #region 同步 JSON 操作

        /// <summary>
        /// 同步序列化对象并写入 JSON 文件
        /// </summary>
        public static void WriteJson<T>(string filePath, T obj, Formatting formatting = Formatting.None, int? bufferSize = null, Encoding encoding = null)
        {
            EnsureDirectoryExists(filePath);

            var buffer = bufferSize ?? DefaultBufferSize;
            var enc = encoding ?? DefaultEncoding;

            var settings = new JsonSerializerSettings
            {
                Formatting = formatting,
                NullValueHandling = DefaultJsonSettings.NullValueHandling,
                ReferenceLoopHandling = DefaultJsonSettings.ReferenceLoopHandling,
                TypeNameHandling = DefaultJsonSettings.TypeNameHandling
            };

            using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                buffer,
                FileOptions.SequentialScan);
            using var writer = new StreamWriter(stream, enc, buffer);
            using var jsonWriter = new JsonTextWriter(writer);
            var serializer = JsonSerializer.Create(settings);
            serializer.Serialize(jsonWriter, obj);
        }

        /// <summary>
        /// 同步读取 JSON 文件并反序列化
        /// </summary>
        public static T? ReadJson<T>(string filePath, int? bufferSize = null, Encoding encoding = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var buffer = bufferSize ?? DefaultBufferSize;
            var enc = encoding ?? DefaultEncoding;

            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                buffer,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, enc, true, buffer);
            using var jsonReader = new JsonTextReader(reader);
            var serializer = JsonSerializer.Create(DefaultJsonSettings);
            return serializer.Deserialize<T>(jsonReader);
        }

        #endregion

        #region 异步 JSON 操作

        /// <summary>
        /// 异步序列化对象并写入 JSON 文件
        /// </summary>
        public static async Task WriteJsonAsync<T>(string filePath, T obj, Formatting formatting = Formatting.None, int? bufferSize = null, Encoding encoding = null)
        {
            EnsureDirectoryExists(filePath);

            var settings = new JsonSerializerSettings
            {
                Formatting = formatting,
                NullValueHandling = DefaultJsonSettings.NullValueHandling,
                ReferenceLoopHandling = DefaultJsonSettings.ReferenceLoopHandling,
                TypeNameHandling = DefaultJsonSettings.TypeNameHandling
            };

            var json = await Task.Run(() => JsonConvert.SerializeObject(obj, settings))
                .ConfigureAwait(false);

            await WriteTextAsync(filePath, json, false, bufferSize, encoding).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步读取 JSON 文件并反序列化
        /// </summary>
        public static async Task<T?> ReadJsonAsync<T>(string filePath, int? bufferSize = null, Encoding encoding = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var json = await ReadTextAsync(filePath, bufferSize, encoding).ConfigureAwait(false);

            return await Task.Run(() => JsonConvert.DeserializeObject<T>(json, DefaultJsonSettings))
                .ConfigureAwait(false);
        }

        #endregion

        #region 同步二进制操作

        /// <summary>
        /// 同步写入二进制文件
        /// </summary>
        public static void WriteBytes(string filePath, byte[] data, int? bufferSize = null)
        {
            EnsureDirectoryExists(filePath);

            var buffer = bufferSize ?? DefaultBufferSize;

            using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                buffer,
                FileOptions.SequentialScan);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 同步读取二进制文件
        /// </summary>
        public static byte[] ReadBytes(string filePath, int? bufferSize = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var buffer = bufferSize ?? DefaultBufferSize;

            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                buffer,
                FileOptions.SequentialScan);
            var result = new byte[stream.Length];
            stream.ReadExactly(result, 0, result.Length);
            return result;
        }

        #endregion

        #region 异步二进制操作

        /// <summary>
        /// 异步写入二进制文件
        /// </summary>
        public static async Task WriteBytesAsync(string filePath, byte[] data, int? bufferSize = null)
        {
            EnsureDirectoryExists(filePath);

            var buffer = bufferSize ?? DefaultBufferSize;

            await using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                buffer,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步读取二进制文件
        /// </summary>
        public static async Task<byte[]> ReadBytesAsync(string filePath, int? bufferSize = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var buffer = bufferSize ?? DefaultBufferSize;

            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                buffer,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var result = new byte[stream.Length];
            var readAsync = await stream.ReadAsync(result, 0, result.Length).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 确保目录存在
        /// </summary>
        private static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public static bool FileExists(string filePath) => File.Exists(filePath);

        /// <summary>
        /// 删除文件
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// 获取文件大小（字节）
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            return new FileInfo(filePath).Length;
        }

        #endregion
    }
}
