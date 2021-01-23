using DBCD.Providers;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace WoWExport.DBC
{
    class LocalDBCDProvider : IDBDProvider
    {
        public Stream StreamForTableName(string tableName, string build = null)
        {
            if(File.Exists(Managers.ConfigurationManager.LocalDBCDefinitionLoc + "/" + tableName + ".dbd"))
            {
                return File.OpenRead(Managers.ConfigurationManager.LocalDBCDefinitionLoc + "/" + tableName + ".dbd");
            }
            else
            {
                //throw new FileNotFoundException("Definition " + tableName + " not found in " + Managers.ConfigurationManager.LocalDBCDefinitionLoc);
                try
                {
                    using (var client = new WebClient())
                    using (var stream = new MemoryStream())
                    {
                        client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                        var responseStream = new GZipStream(client.OpenRead($"https://raw.githubusercontent.com/wowdev/WoWDBDefs/master/definitions/{tableName}.dbd"), CompressionMode.Decompress);
                        responseStream.CopyTo(stream);

                        if (!Directory.Exists(Managers.ConfigurationManager.LocalDBCDefinitionLoc))
                        {
                            Directory.CreateDirectory(Managers.ConfigurationManager.LocalDBCDefinitionLoc);
                        }
                        File.WriteAllBytes(Path.Combine(Managers.ConfigurationManager.LocalDBCDefinitionLoc + "/" + tableName + ".dbd"), stream.ToArray());

                        responseStream.Close();
                        responseStream.Dispose();
                    }
                    return File.OpenRead(Managers.ConfigurationManager.LocalDBCDefinitionLoc + "/" + tableName + ".dbd");
                }
                catch
                {
                    throw new FileNotFoundException("Definition " + tableName + " not found in " + Managers.ConfigurationManager.LocalDBCDefinitionLoc + " and it couldn't be downloaded either");
                }
            }
        }
    }
}
