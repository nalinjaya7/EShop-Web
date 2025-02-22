namespace EShopWeb.Models
{
    public class ReportModel
    {
        public ReportModel(string reportName, string title)
        {
            this.ReportName = reportName;
            this.Title = title;
        }

        public string ReportName { get; set; }
        
        public string Title { get; set; }
    }
}