namespace wwfpp.Models.General
{
    public class DocumentTemplatesViewModel
    {
        public string id { get; set; }     //nvarchar 50 NOT NULL
        public string? document_title { get; set; } // nvarchar 250
        public string? document_version { get; set; } //nvarchar 250 
        public string? document_desc { get; set; } //ntext
        public string? upload_file { get; set; }    //nvarchar 250
        public DateTime? upload_date { get; set; }  //
        public string? status { get; set; }// varchar(1)
    }
}
