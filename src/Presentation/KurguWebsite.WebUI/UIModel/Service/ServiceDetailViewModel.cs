namespace KurguWebsite.WebUI.UIModel.Service
{
    public class ServiceDetailViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Description { get; set; }
        public string? FullDescription { get; set; }     // <-- added
        public string? CoverImageUrl { get; set; }
        public bool IsActive { get; set; }

        public List<ServiceFeatureVm> Features { get; set; } = new();

        public List<OtherServiceVm> OtherServices { get; set; } = new();  // <-- added
 
    }
    public class ServiceFeatureVm
    {
        public string Name { get; set; } = "";
        public string? IconcClass { get; set; }
        public string? Summary { get; set; }
    }
    public class OtherServiceVm
    {
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
    }
}
