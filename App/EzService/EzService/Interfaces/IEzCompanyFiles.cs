using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces
{
    using System.ServiceModel;

    public interface IEzCompanyFiles
    {
        [OperationContract]
        ActionMetaData UploadCompanyFiles(int customerId, string fileName, byte[] fileContent, string fileContentType);

        [OperationContract]
        byte[] GetCompanyFile(int userId, int companyFileId);
    }
}
