using System;
using System.IO;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public interface ICaisReportsHistoryRepository : IRepository<CaisReportsHistory>
    {
    }

    public class CaisReportsHistoryRepository : NHibernateRepositoryBase<CaisReportsHistory>,
                                                ICaisReportsHistoryRepository
    {
        public CaisReportsHistoryRepository(ISession session) : base(session)
        {
        }

        public void AddFile(string fileData, string name, string folderName, int type, int ofItems, int goodUsers,
                            int defaults)
        {
            Save(new CaisReportsHistory
                {
                    FileName = name,
                    DirName = folderName,
                    Date = DateTime.UtcNow,
                    Defaults = defaults,
                    GoodUsers = goodUsers,
                    OfItems = ofItems,
                    Type = (CaisType) type,
                    UploadStatus = CaisUploadStatus.Generated,
                    FileData = fileData
                });
        }

        public void UpdateFile(string fileData, int id)
        {
            var model = Get(id);
            if (model == null) throw new FileNotFoundException();
            model.FileData = fileData;
            Update(model);
        }

        public CaisReportsHistory GetByPathAndName(string dirName, string name)
        {
            return GetAll().FirstOrDefault(x => x.FileName == name && x.DirName == dirName);
        }
    }
}