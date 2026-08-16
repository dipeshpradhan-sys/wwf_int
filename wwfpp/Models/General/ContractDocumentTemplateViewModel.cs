using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.General
{
    public class ContractDocumentTemplateViewModel
    {
        public int contract_document_id { get; set; }
        public string document_subject { get; set; } //nvarchar 255
        public string document_desc { get; set; } //ntext
    }

}