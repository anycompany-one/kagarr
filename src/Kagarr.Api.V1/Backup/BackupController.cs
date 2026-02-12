using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.Backup;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Backup
{
    [V1ApiController("backup")]
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;

        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpGet]
        public ActionResult<List<BackupResource>> GetAll()
        {
            return _backupService.GetBackups().Select(BackupResource.FromModel).ToList();
        }

        [HttpPost]
        public ActionResult<BackupResource> Create()
        {
            var backup = _backupService.CreateBackup();
            return BackupResource.FromModel(backup);
        }
    }

    public class BackupResource
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string Time { get; set; }

        public static BackupResource FromModel(BackupInfo model)
        {
            return new BackupResource
            {
                Name = model.Name,
                Size = model.Size,
                Time = model.Time.ToString("o", System.Globalization.CultureInfo.InvariantCulture)
            };
        }
    }
}
