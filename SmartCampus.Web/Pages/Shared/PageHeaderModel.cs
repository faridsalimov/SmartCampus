namespace SmartCampus.Web.Pages.Shared
{
    public class PageHeaderModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string IconClass { get; set; }
        public string IconColor { get; set; } = "#0066ff";
        public string GradientStart { get; set; } = "rgba(0, 102, 255, 0.05)";
        public string GradientEnd { get; set; } = "rgba(94, 92, 230, 0.05)";
        public string BorderColor { get; set; } = "rgba(0, 102, 255, 0.1)";
        public List<PageHeaderAction> Actions { get; set; } = new();
    }

    public class PageHeaderAction
    {
        public string Label { get; set; }
        public string Url { get; set; }
        public string CssClass { get; set; } = "btn-primary";
        public string IconClass { get; set; }
    }
}
