using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Common.Instrumentation;
using NLog;

namespace Kagarr.Core.RemotePathMappings
{
    public interface IRemotePathMappingService
    {
        List<RemotePathMapping> All();
        RemotePathMapping Get(int id);
        RemotePathMapping Add(RemotePathMapping mapping);
        RemotePathMapping Update(RemotePathMapping mapping);
        void Delete(int id);
        string RemapRemoteToLocal(string host, string remotePath);
    }

    public class RemotePathMappingService : IRemotePathMappingService
    {
        private readonly IRemotePathMappingRepository _repository;
        private readonly Logger _logger;

        public RemotePathMappingService(IRemotePathMappingRepository repository)
        {
            _repository = repository;
            _logger = KagarrLogger.GetLogger(this);
        }

        public List<RemotePathMapping> All()
        {
            return _repository.All().ToList();
        }

        public RemotePathMapping Get(int id)
        {
            return _repository.Get(id);
        }

        public RemotePathMapping Add(RemotePathMapping mapping)
        {
            // Ensure paths end with separator for reliable prefix matching
            mapping.RemotePath = EnsureTrailingSeparator(mapping.RemotePath);
            mapping.LocalPath = EnsureTrailingSeparator(mapping.LocalPath);

            return _repository.Insert(mapping);
        }

        public RemotePathMapping Update(RemotePathMapping mapping)
        {
            mapping.RemotePath = EnsureTrailingSeparator(mapping.RemotePath);
            mapping.LocalPath = EnsureTrailingSeparator(mapping.LocalPath);

            return _repository.Update(mapping);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public string RemapRemoteToLocal(string host, string remotePath)
        {
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remotePath))
            {
                return remotePath;
            }

            var mappings = _repository.All()
                .Where(m => string.Equals(m.Host, host, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var mapping in mappings)
            {
                if (remotePath.StartsWith(mapping.RemotePath, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = remotePath.Substring(mapping.RemotePath.Length);
                    var localPath = mapping.LocalPath + relativePath;
                    _logger.Debug("Remapped '{0}' on host '{1}' to '{2}'", remotePath, host, localPath);
                    return localPath;
                }
            }

            return remotePath;
        }

        private static string EnsureTrailingSeparator(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            // Normalize: ensure it ends with a path separator
            if (!path.EndsWith('/') &&
                !path.EndsWith('\\'))
            {
                return path + "/";
            }

            return path;
        }
    }
}
