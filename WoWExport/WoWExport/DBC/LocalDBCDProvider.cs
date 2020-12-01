using DBCD.Providers;
using System.IO;

namespace WoWExport.DBC
{
    class LocalDBCDProvider : IDBDProvider
    {
        string localDefinitionPath = @"D:/test/definitions"; //HARDCODED FOR THE MOMENT

        public Stream StreamForTableName(string tableName, string build = null)
        {
            if(File.Exists(localDefinitionPath + "/" + tableName + ".dbd"))
            {
                return File.OpenRead(localDefinitionPath + "/" + tableName + ".dbd");
            }
            else
            {
                throw new FileNotFoundException("Definition " + tableName + " not found in " + localDefinitionPath);
            }
        }
    }
}
