namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.ProcessSteps
{
    public class ProcessStepCreateViewModel
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
