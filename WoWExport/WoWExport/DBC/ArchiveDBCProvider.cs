using DBCD.Providers;
using System.IO;

namespace WoWExport.DBC
{
    class ArchiveDBCProvider : IDBCProvider
    {
        public Stream StreamForTableName(string tableName, string build)
        {
            if(Managers.ArchiveManager.FileExists("dbfilesclient/" + tableName + ".db2"))
            {
                return Managers.ArchiveManager.ReadThisFile("dbfilesclient/" + tableName + ".db2");
            }
            else
            {
                throw new FileNotFoundException("DBC " + tableName + " not found in the archive!");
            }
        }
    }
}
