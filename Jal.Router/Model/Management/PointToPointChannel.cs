using System;

namespace Jal.Router.Model.Management
{
    public class PointToPointChannel
    {
        public PointToPointChannel(string path)
        {
            Path = path;
        }
        public string Path { get; set; }

        public string ConnectionString { get; set; }

        public Type ConnectionStringExtractorType { get; set; }

        public object ConnectionStringExtractor { get; set; }
    }
}