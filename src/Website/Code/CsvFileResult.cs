using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Website.Extensions
{
    public static partial class ControllerExtensions
    {
        /// <summary>
        /// Writes the contents of a <see cref="System.Data.IDataReader"/> to a comma delimited text file.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="fileName"></param>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public static ActionResult CsvFileResult(this Controller controller, string fileName, IDataReader dataSource)
        {
            var writer = new Csg.IO.TextData.CsvWriter(controller.Response.Body);

            writer.Delimiter = ',';

            try
            {
                controller.Response.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);
                controller.Response.ContentType = "text/csv";

                using (dataSource)
                {
                    var header = new List<string>();
                    for (int x = 0; x < dataSource.FieldCount; x++)
                    {
                        header.Add(dataSource.GetName(x));
                    }

                    //writer.HeaderNames = header.ToArray();
                    writer.Fields = header.ToArray();

                    while (dataSource.Read())
                    {
                        writer.WriteLine(dataSource);
                    }
                }

                writer.Flush();

                return new Microsoft.AspNetCore.Mvc.StatusCodeResult(200);
            }
            finally
            {
                writer.Flush();
            }
        }
    }
}
