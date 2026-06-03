using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace AMR.Financeiro.API.Helpers;

public static class CsvExportHelper
{
    /// <summary>Serializa uma lista de registros em CSV e retorna o FileStreamResult.</summary>
    public static FileStreamResult Export<T>(IEnumerable<T> records, string fileName)
    {
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("pt-BR"))
        {
            Delimiter = ";",
            HasHeaderRecord = true,
        };

        var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(records);
        }
        ms.Position = 0;

        return new FileStreamResult(ms, "text/csv")
        {
            FileDownloadName = $"{fileName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv",
        };
    }
}
