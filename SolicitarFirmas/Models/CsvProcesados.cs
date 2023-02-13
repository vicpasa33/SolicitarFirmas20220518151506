using Microsoft.WindowsAzure.Storage.Table;
using SolicitarFirmas.Models;
using System.Collections.Concurrent;

namespace SolicitarFirmas.Models
{
    public class CsvProcesados : TableEntity
    {
        public CsvProcesados(string nombre, string estado) : base(nombre, estado) { }
        public CsvProcesados()
        {
            PartitionKey = "";
            RowKey = "";
        }
    }
}