using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubExtensions.Reports
{
    public static class ReportHelper
    {
        public static System.IO.Stream GetReportDefinition(string reportName)
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(ReportHelper));
            var reportPath = $"{nameof(ClubExtensions)}.{nameof(ClubExtensions.Reports)}.{reportName}.rdlc";

            return assembly.GetManifestResourceStream(reportPath);
        }

        public static byte[] RenderReport(string reportName, string dataSourceName, object dataSource, string format, out string mimeType)
        {
            Warning[] warnings;
            string[] streamids;
            string encoding;
            string filenameExtension;
          
            var reportViewer = new ReportViewer();
            var ds = new ReportDataSource(dataSourceName);

            ds.Value = dataSource;

            reportViewer.LocalReport.DataSources.Add(ds);

            using (var reportDef = ClubExtensions.Reports.ReportHelper.GetReportDefinition(reportName))
            {
                reportViewer.LocalReport.LoadReportDefinition(reportDef);

                return reportViewer.LocalReport.Render(format, null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
            }
        }
    }
    
}
