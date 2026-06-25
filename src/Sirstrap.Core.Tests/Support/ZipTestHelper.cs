namespace Sirstrap.Core.Tests.Support
{
    public static class ZipTestHelper
    {
        public static byte[] CreateZip(params (string Name, string Content)[] entries)
        {
            using MemoryStream stream = new();

            using (var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (name, content) in entries)
                {
                    var entry = archive.CreateEntry(name);

                    using var writer = new StreamWriter(entry.Open());

                    writer.Write(content);
                }
            }

            return stream.ToArray();
        }

        public static Dictionary<string, string> ReadZip(string path)
        {
            Dictionary<string, string> result = new(StringComparer.Ordinal);

            using var archive = System.IO.Compression.ZipFile.OpenRead(path);

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                using var reader = new StreamReader(entry.Open());

                result[entry.FullName] = reader.ReadToEnd();
            }

            return result;
        }
    }
}
