using System;
using System.Collections.Generic;

namespace DataModelMigration.Model.ClassModel
{
    public class BulkUploadRequest : BulkUploadRequestData
    {
        public int Index { get; set; }

        public string File { get; set; }
    }

    public class BulkUploadResult
    {
        public BulkUploadRequestData RequestFile { get; set; }
        public Guid DirectoryId { get; set; }
        public Guid FileId { get; set; }
        public string Name { get; set; }
        public string ItemType { get; set; }
        public string FullPath { get; set; }
        public string Extention { get; set; }
        public string S3Filename { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsUploaded { get; set; }
    }

    public class BulkUploadRequestData
    {
        public string ClientId { get; set; }
        public string PolicyVersionId { get; set; }
        public string QuoteId { get; set; }
        public string CommunicationType { get; set; }

        public string Path { get; set; }
        public IDictionary<string, string> Tags { get; set; }
    }

    public class Result
    {
        public IList<BulkUploadResult> BulkFiles { get; set; }
    }
}
