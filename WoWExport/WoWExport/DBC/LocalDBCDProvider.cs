using DBCD.Providers;
using System.IO;

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
                throw new FileNotFoundException("Definition " + tableName + " not found in " + Managers.ConfigurationManager.LocalDBCDefinitionLoc);
            }
        }
    }
}
