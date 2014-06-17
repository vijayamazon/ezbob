using System;
using System.IO;

namespace EchoSoap.secure.echosign.com
{
    public partial class FileInfo
    {
        public FileInfo() { }
        public FileInfo(String libraryDocumentKey)
        {
            this.libraryDocumentKey = libraryDocumentKey;
        }
        public FileInfo(String fileName, String mimeType, String data)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            this.file = encoding.GetBytes(data);
            this.fileName = fileName;
            this.mimeType = mimeType;
        }
        public FileInfo(String fileName, String mimeType, FileStream file)
        {
            this.fileName = fileName;
            this.mimeType = mimeType;
            this.file = new Byte[file.Length];
            file.Read(this.file, 0, this.file.Length);
        }
    }

    public partial class LibraryDocumentCreationInfo
    {
        public LibraryDocumentCreationInfo() { }
        public LibraryDocumentCreationInfo(string name, FileInfo[] fileInfos, SignatureType signatureType, SignatureFlow signatureFlow, LibrarySharingMode librarySharingMode, System.Nullable<LibraryTemplateType>[] libraryTemplateTypes)
        {
            this.name = name;
            this.fileInfos = fileInfos;
            this.signatureType = signatureType;
            this.signatureFlow = signatureFlow;
            this.librarySharingMode = librarySharingMode;
            this.libraryTemplateTypes = libraryTemplateTypes;
        }
    }

    public partial class DocumentCreationInfo
    {
        public DocumentCreationInfo() { }
        public DocumentCreationInfo(string[] tos, string name, string message, FileInfo[] fileInfos, SignatureType signatureType, SignatureFlow signatureFlow)
        {
            this.tos = tos;
            this.name = name;
            this.message = message;
            this.fileInfos = fileInfos;
            this.signatureTypeSpecified = true;
            this.signatureType = signatureType;
            this.signatureFlowSpecified = true;
            this.signatureFlow = signatureFlow;
        }
    }

    public partial class SenderInfo
    {
        public SenderInfo() { }
        public SenderInfo(string email, string password, string userKey)
        {
            this.email = email;
            this.password = password;
            this.userKey = userKey;
        }
    }
}