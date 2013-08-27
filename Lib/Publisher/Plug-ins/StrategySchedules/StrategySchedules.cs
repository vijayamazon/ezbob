using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using PublisherLib;
using PublisherLib.Configuration;
using PublisherLib.SrvPatron;
using PublisherPlugin;
using Aspose.Cells;
using Scorto.StrategySchedule;
using log4net;

namespace StrategySchedules
{
    public class StrategyScheduleUploadInfo
    {
        public enum Columns
        {
            Id = 0,
            Path,
            Filename,
        }

        public int Id { get; set; }

        public string Path { get; set; }

        public string FileName { get; set; }

        public string FullPath
        {
            get { return System.IO.Path.Combine(Path, FileName); }
        }

        public int StrategyId { get; set; }
    }

    public class StrategySchedules : IPublisherPlugin
    {
        static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int UploadPriority
        {
            get { return (int) Common.UploadOrderPlugin.StrategySchedules; }
            set { throw new NotImplementedException(); }
        }

        public int RollbackPriority
        {
            get { return (int) Common.RollbackOrderPlugin.StrategySchedules; }
            set { throw new NotImplementedException(); }
        }

        public int BackupPriority
        {
            get { return (int) Common.BackupOrderPlugin.StrategySchedules; }
            set { throw new NotImplementedException(); }
        }

        public int DeletePriority
        {
            get { return (int) Common.DeleteOrderPlugin.StrategySchedules; }
            set { throw new NotImplementedException(); }
        }

        bool StrategySchedulesUploadError { get; set; }

        public void Validate()
        {
            if (StrategySchedulesUploadError)
            {
                throw new Exception("Some strategy schedules haven't been published correctly.");
            }
        }

        public void Delete()
        {
            Logger.InfoFormat("{0}. Strategy schedules", DeletePriority);

            var service = ServiceFactory.CreatePatronService();

            var strategySchedules = service.GetStrategyScheduleItems();

            foreach (var strategySchedule in strategySchedules)
            {
                Logger.InfoFormat("Deleting strategy schedule {0}[id = {1}]", strategySchedule.Name, strategySchedule.Id);

                service.DeleteStrategyScheduleItem(strategySchedule.Id);

                Logger.InfoFormat("Strategy schedule has been deleted");
            }
        }

        readonly List<StrategyScheduleUploadInfo> StrategySchedulesUploadInfo = new List<StrategyScheduleUploadInfo>();

        const int StartRowIndex = 1;

        public void ReadEntity(Worksheet worksheet)
        {
            for (int i = 0 + StartRowIndex; i < worksheet.Cells.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(worksheet.Cells[i, (int)StrategyScheduleUploadInfo.Columns.Id].StringValue)) {
                    break;
                }

                var uploadInfo = new StrategyScheduleUploadInfo {
                    Id = Convert.ToInt32(worksheet.Cells[i, (int)StrategyScheduleUploadInfo.Columns.Id].StringValue),
                    Path = worksheet.Cells[i, (int)StrategyScheduleUploadInfo.Columns.Path].StringValue,
                    FileName = worksheet.Cells[i, (int)StrategyScheduleUploadInfo.Columns.Filename].StringValue,
                };

                StrategySchedulesUploadInfo.Add(uploadInfo);
            }
        }

        public void Upload()
        {
            Logger.InfoFormat("{0}. Strategy schedules to upload (Count = {1})", UploadPriority, StrategySchedulesUploadInfo.Count);

            var patronService = ServiceFactory.CreatePatronService();
            var existingSchedules = patronService.GetStrategyScheduleItems().ToList();

            foreach (var uploadInfo in StrategySchedulesUploadInfo)
            {
                var scheduleUploaded = UploadStrategySchedule(uploadInfo, existingSchedules);
                if (scheduleUploaded == null) {
                    return;
                }

                existingSchedules.Add(scheduleUploaded);
            }

            Logger.InfoFormat("All strategy schedules have been uploaded");
        }

        private StrategyScheduleItemDto UploadStrategySchedule(StrategyScheduleUploadInfo uploadInfo, IList<StrategyScheduleItemDto> existingSchedules)
        {
            try
            {
                Logger.DebugFormat("[Number: {0}] Uploading strategy schedule {1}", uploadInfo.Id, uploadInfo.FullPath);

                var patronService = ServiceFactory.CreatePatronService();

                string str = System.IO.File.ReadAllText(uploadInfo.FullPath);

                var strategySchedule = StrategyScheduleItemDto.DeserializeFromXML(str);

                var alreadyExists = existingSchedules.Any(x => x.Name == strategySchedule.Name);
                if (alreadyExists) {
                    throw new Exception("The strategy schedule with the same name as being uploaded already exists");
                }

                var strategies = patronService.GetStrategyDetails(strategySchedule.StrategyName);
                var strategy = strategies.SingleOrDefault(x => x.Name == strategySchedule.StrategyUniqueName)
                    ?? strategies.SingleOrDefault(x => x.TerminationDate == null);
    
                if (strategy == null) {
                    throw new Exception("There is no strategy in the database with the name specified in the file");
                }

                if (strategy.StrategyType != strategySchedule.StrategyType) {
                    throw new Exception("The current version of the Master Application does not support the strategy type specified in the file. The strategy schedule will not be opened");
                }

                if (strategy.Name != strategySchedule.StrategyUniqueName
                    && !CheckStrategyScheduleParameters(strategy.Name, strategySchedule))
                {
                    throw new Exception("The strategy does not contain input strategy parameters with the attributes and values specified in the file. The strategy schedule will not be opened.");
                }
    
                strategySchedule.StrategyId = Convert.ToInt32(strategy.Id);
                
                ScheduleRunWrapper runWrapper = ScheduleRunWrapper.GetWrapper((int)strategySchedule.ScheduleType, strategySchedule.Mask);
                strategySchedule.NextRun = runWrapper.GetNextRun(DateTime.UtcNow);

                uploadInfo.StrategyId = (int)patronService.InsertStrategyScheduleItemDto(strategySchedule);

                foreach (var strategyScheduledInput in strategySchedule.ChildHistoryProxy.Cast<StrategyScheduledInput>())
                {
                    strategyScheduledInput.ScheduleId = uploadInfo.StrategyId;

                    var result = strategyScheduledInput.Serialize();
                    var sign = DigitalSign.GetSign(result);

                    patronService.SaveScheduledStrategyParameters(strategyScheduledInput.Name, strategyScheduledInput.Description, result, sign);
                }

                Logger.InfoFormat("Strategy schedule has been uploaded");

                return strategySchedule;
            }
            catch (Exception ex)
            {
                StrategySchedulesUploadError = true;

                Logger.ErrorFormat("{0} failed to upload. {1}", uploadInfo.FullPath, ex);

                return null;
            }
        }

        Boolean CheckStrategyScheduleParameters(String strategyUniqueName, StrategyScheduleItemDto schedule)
        {
            var patronService = ServiceFactory.CreatePatronService();

            var str = patronService.GetScheduledStrategyParametersByStrategyName(strategyUniqueName);
            var strategyScheduledInput = StrategyScheduledInput.Deserialize(str);

            foreach (var input in schedule.ChildHistoryProxy.Cast<StrategyScheduledInput>())
            {
                if (input.StrategyScheduledInputParameters.Length != strategyScheduledInput.StrategyScheduledInputParameters.Length)
                {
                    return false;
                }

                foreach (var parameter in input.StrategyScheduledInputParameters)
                {
                    if (strategyScheduledInput.StrategyScheduledInputParameters.All(
                        x => x.Name != parameter.Name && x.TypeName != parameter.TypeName && x.Constraint != parameter.Constraint
                        ))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private const string RollbackSql = "DELETE FROM Strategy_ScheduleParam WHERE StrategyScheduleId = {0}; DELETE FROM Strategy_Schedule WHERE Id = {0}";

        public void Rollback()
        {
            Logger.Info("Rolling back strategy schedules publishing");

            foreach (var uploadInfo in StrategySchedulesUploadInfo)
            {
                Logger.InfoFormat("Rolling back strategy schedule [id = '{0}', file name = '{1}', path = '{2}']", uploadInfo.Id, uploadInfo.FileName, uploadInfo.FullPath);

                var sqls = RollbackSql.Split(';').Select(x => String.Format(x, uploadInfo.StrategyId).Trim());

                foreach (var sql in sqls)
                {
                    if (PublisherConfiguration.Instance.LogRollbackQuery)
                    {
                        Logger.InfoFormat("Query: {0}", sql);
                    }

                    RollbackUpload.Rollback(sql);
                }
            }
        }

        public string GetWorkSheetName()
        {
            return "StrategySchedules";
        }

        const String BaseFolderName = "StrategySchedules";

        public void Backup()
        {
            Logger.Info("Reading strategy schedules data ...");

            var backupPath = Path.Combine(
               Path.Combine(
                   PublisherConfiguration.Instance.BackupPath,
                   PublisherConfiguration.Instance.CommonBackupFolder
               ),
               BaseFolderName
            );

            var service = ServiceFactory.CreatePatronService();
            var strategySchedules = service.GetStrategyScheduleItems();

            foreach (var strategySchedule in strategySchedules.OrderBy(x => x.Id))
            {
                var strategyScheduleUploadInfo = new StrategyScheduleUploadInfo {
                    Id = (int)strategySchedule.Id,
                    Path = backupPath,
                    FileName = String.Format("{0}.{1}", strategySchedule.Name, "sch")
                };

                StrategySchedulesUploadInfo.Add(strategyScheduleUploadInfo);

                Save(strategySchedule, strategyScheduleUploadInfo);
            }

            Logger.Info("All strategy schedules have been saved.");
        }

        void Save(StrategyScheduleItemDto strategySchedule, StrategyScheduleUploadInfo uploadInfo)
        {
            Logger.Info("Saving strategy schedule data to file system ...");

            var exists = Directory.Exists(uploadInfo.Path);

            Logger.InfoFormat("Directory '{0}' exists? = {1}", uploadInfo.Path, exists);

            if (!exists)
            {
                Logger.InfoFormat("Create directory '{0}'", uploadInfo.Path);
                Directory.CreateDirectory(uploadInfo.Path);
            }

            using (var fs = new FileStream(uploadInfo.FullPath, FileMode.Create))
            {
                var str = strategySchedule.SerializeToXML();
                var bytes = Encoding.UTF8.GetBytes(str);
                fs.Write(bytes, 0, bytes.Length);
            }

            Logger.InfoFormat("File '{0}' has been saved into '{1}'", uploadInfo.FileName, uploadInfo.Path);
        }

        public void SaveToXls(Workbook workbook)
        {
            var worksheet = workbook.Worksheets.Add(GetWorkSheetName());
            CreateXlsHeader(worksheet);

            for (int index = 0; index < StrategySchedulesUploadInfo.Count; index++)
            {
                var uploadInfo = StrategySchedulesUploadInfo[index];
                int row = index + StartRowIndex;

                worksheet.Cells[row, (int)StrategyScheduleUploadInfo.Columns.Id].PutValue(uploadInfo.Id);
                worksheet.Cells[row, (int)StrategyScheduleUploadInfo.Columns.Path].PutValue(uploadInfo.Path);
                worksheet.Cells[row, (int)StrategyScheduleUploadInfo.Columns.Filename].PutValue(uploadInfo.FileName);

                SetCellStyle(worksheet, row, (int)StrategyScheduleUploadInfo.Columns.Id, false);
                SetCellStyle(worksheet, row, (int)StrategyScheduleUploadInfo.Columns.Path, false);
                SetCellStyle(worksheet, row, (int)StrategyScheduleUploadInfo.Columns.Filename, false);
            }
        }

        public void CreateXlsHeader(Worksheet worksheet)
        {
            worksheet.Cells[0, 0].PutValue(StrategyScheduleUploadInfo.Columns.Id.ToString());
            SetCellStyle(worksheet, 0, 0, true);

            worksheet.Cells[0, 1].PutValue(StrategyScheduleUploadInfo.Columns.Path.ToString());
            SetCellStyle(worksheet, 0, 1, true);

            worksheet.Cells[0, 2].PutValue(StrategyScheduleUploadInfo.Columns.Filename.ToString());
            SetCellStyle(worksheet, 0, 2, true);
        }

        private static void SetCellStyle(Worksheet worksheet, int row, int column, bool isBold)
        {
            worksheet.Cells[row, column].Style.Font.Size = 11;
            worksheet.Cells[row, column].Style.Font.Name = "Calibri";
            worksheet.Cells[row, column].Style.Font.IsBold = isBold;
            worksheet.Cells[row, column].Style.ForegroundColor = Color.Black;
            worksheet.Cells[row, column].Style.HorizontalAlignment = TextAlignmentType.Left;
            worksheet.Cells[row, column].Style.ShrinkToFit = true;

            worksheet.AutoFitRows();
            worksheet.AutoFitColumns();
        }
    }
}