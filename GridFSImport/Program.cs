using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GridFSImport
{
    class Program
    {
        private bool _recursive;
        private string _dir;
        private string _server;
        private string _user;
        private string _password;
        private string _database;
        private string _root;
        private bool _unixStyle;
        private bool _winStyle;



        public Program(IEnumerable<string> args)
        {
            var enumerator = args.GetEnumerator();

            while (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case "-r":
                        _recursive = true;
                        break;
                    case "-unix":
                        _unixStyle = true;
                        break;
                    case "-win":
                        _winStyle = true;
                        break;
                    case "-s":
                        enumerator.MoveNext();
                        _server = enumerator.Current;
                        break;
                    case "-d":
                        enumerator.MoveNext();
                        _dir = enumerator.Current;
                        break;
                    case "-p":
                        enumerator.MoveNext();
                        _password = enumerator.Current;
                        break;
                    case "-u":
                        enumerator.MoveNext();
                        _user = enumerator.Current;
                        break;
                    case "-db":
                        enumerator.MoveNext();
                        _database = enumerator.Current;
                        break;
                    case "-root":
                        enumerator.MoveNext();
                        _root = enumerator.Current;
                        break;
                }
            }
        }
        public void Run()
        {
            string connectionString = "mongodb://localhost/?safe=true";

            if (!string.IsNullOrEmpty(_server))
            {
                connectionString = string.Format("mongodb://{0}/?safe=true", _server);
            }

            var server = MongoServer.Create(connectionString);
            var db = server.GetDatabase(_database);
            var gridFs = new MongoGridFS(db);
            gridFs.EnsureIndexes();

            var dirInfo = new DirectoryInfo(_dir);
            ImportDirectory(gridFs, dirInfo, _root, _recursive);
        }

        private void ImportDirectory(MongoGridFS fs, DirectoryInfo dir, string root, bool includeSubdir)
        {
            ImportFiles(fs, dir.GetFiles(), _root);

            if (includeSubdir)
            {
                var subDirs = dir.GetDirectories();

                foreach (var subDir in subDirs)
                {
                    ImportDirectory(fs, subDir, root, includeSubdir);
                }
            }
        }

        private void ImportFiles(MongoGridFS fs, IEnumerable<FileInfo> files, string root)
        {
            foreach (var file in files)
            {
                var name = file.FullName;
                if (!string.IsNullOrEmpty(root))
                {
                    name = name.Replace(root, string.Empty);
                }

                if(_unixStyle)
                    name = name.Replace('\\', '/');

                if (_winStyle)
                    name = name.Replace('/', '\\');

                Stream stream;

                if (fs.Exists(name))
                    stream = fs.Open(name, FileMode.Truncate, FileAccess.Write);
                else
                    stream = fs.Create(name);

                var fileStream = file.Open(FileMode.Open, FileAccess.Read);

                fileStream.CopyTo(stream);
                fileStream.Close();
                stream.Close();                
            }
        }

        static void Main(string[] args)
        {
            new Program(args).Run();
        }
    }
}
