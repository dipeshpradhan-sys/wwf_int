namespace wwfpp.Models
{
    public class ModalViewModel
    {
        public string? ModalId { get; set; }          // e.g. "holidayModal"
        public string? DialogClass { get; set; }      // e.g. "modal-lg", "modal-xl"
        public string? CustomClass { get; set; }      // e.g. "modal-lg", "modal-xl"
        public string? Title { get; set; }            // e.g. "Add New Fiscal Year"
        public string? BodyId { get; set; }           // e.g. "HolidayModalBody"
        public string? SaveButtonId { get; set; }     // e.g. "HolidaySaveBtn"
        //public string UploadButtonId { get; set; }     // e.g. "HolidaySaveBtn"
        public string CancelButtonText { get; set; } = "Cancel";
        public string SaveButtonText { get; set; } = "Save";

        // New property
        public string? ShowEditButton { get; set; }
    }
}
