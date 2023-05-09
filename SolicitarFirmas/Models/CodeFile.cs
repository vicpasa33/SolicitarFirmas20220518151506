using Azure.Identity;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using FluentFTP;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using MimeKit;
using Newtonsoft.Json;
using Org.BouncyCastle.Cms;
using Renci.SshNet;
using Serilog;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Net.Mail;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using static System.Net.WebRequestMethods;
using System.IO;

namespace SolicitarFirmas.Models
{
    public class CodeFile
    {
        public Createenvelope MontarJson(int rCnt, int fCnt, string CsvDades, string linecapçalera, string line, string templateId, DateTime llancament, string entorn)
        {
            Models.Createenvelope? Dcreateenvelope = new();
            var values = line.Split(';');
            int numcol = values.Count();
            string wbody = "";
            var valueslinecapçalera = linecapçalera.Split(';');
            string strdiaexpiracio = "14/"; //Huy dos digits
            int diaexpiracio = 14;
            DateTime Avuii = DateTime.Today;
            DateTime Avui = DateTime.Now;
            DateTime expires = DateTime.Today;
            string expiresString, format, output;
            String date;
            CultureInfo provider = CultureInfo.InvariantCulture;
            if (entorn == "azure")
            {
                provider = CultureInfo.CreateSpecificCulture("en-US");
                date = DateTime.Now.ToString("M/dd/yyyy", provider);
                Avui = DateTime.ParseExact(date, "M/dd/yyyy", provider);
                format = "M/dd/yyyy";
                expiresString = (Avui.Month + "/" + strdiaexpiracio + Avui.Year);
                expires = DateTime.ParseExact(expiresString, format, provider);
                if (Avui.Day > diaexpiracio)
                {
                    output = Avui.AddMonths(1).ToString(provider);
                    expires = Convert.ToDateTime(output);
                    expires = Convert.ToDateTime(expires.Month + "/" + strdiaexpiracio + expires.Year);
                    expiresString = (expires.Month + "/" + strdiaexpiracio + expires.Year);
                }
            }
            else
            {
                provider = CultureInfo.CreateSpecificCulture("es-ES");
                date = DateTime.Now.ToString("dd/M/yyyy", provider);
                Avui = DateTime.ParseExact(date, "dd/M/yyyy", provider);
                format = "dd/M/yyyy";
                expiresString = (strdiaexpiracio + Avui.Month + "/" + Avui.Year);
                expires = DateTime.ParseExact(expiresString, format, provider);
                if (Avui.Day > diaexpiracio)
                {
                    output = Avui.AddMonths(1).ToString(provider);
                    expires = Convert.ToDateTime(output);
                    expires = Convert.ToDateTime(strdiaexpiracio + expires.Month + "/" + expires.Year);
                    expiresString = (strdiaexpiracio + expires.Month + "/" + expires.Year);
                }
            }
            int expireAfter = (expires - Avui).Days;
            int Warn = 1;
            int Delay = 0;
            int Frequency = 0;
            int Repitions = 0;
            Reminder? DReminder = new();
            DReminder.expireAfter = expireAfter;
            DReminder.expireWarn = Warn;
            DReminder.reminderDelay = Delay;
            DReminder.reminderFrequency = Frequency;
            DReminder.numberOfRepitions = Repitions;
            Dcreateenvelope.reminders = DReminder;
            Signer[] DSigner = new Signer[1];
            DSigner[0] = new Signer();
            DSigner[0].name = values[0];
            string referencia = templateId + " C " + fCnt + " " + llancament + " E " + expiresString + " A " + DReminder.expireAfter + " W " + Warn + " D " + Delay + " F " + Frequency + " R " + Repitions;
            //if (CsvDades == "NOVACION")
            //{
            //    DSigner[0].lastName = values[21];
            //    DSigner[0].documentNumber = values[22];
            //}
            //else
            //{
            //}
            DSigner[0].lastName = values[21];
            DSigner[0].documentNumber = values[22];
            DSigner[0].email = values[1];
            DSigner[0].order = 1;
            DSigner[0].clientReference = "ee4de95a-f804-11ea-adc1-0242ac120002";
            DSigner[0].role = "SIGNER";
            DSigner[0].signMode = "EMAIL";
            DSigner[0].authenticationMethod = "NONE";
            DSigner[0].id = "33c09020-9b47-4afa-8612-0dcf17bd86e6";
            DSigner[0].mobile = "";
            Dcreateenvelope.signers = DSigner;
            wbody = "Aplicar DocuSign a: " + CsvDades;
            //deixar com estaba sino funciona
            Dcreateenvelope.emailSubject = wbody;
            //Dcreateenvelope.emailSubject = referencia;
            FormField[] DFormField = new FormField[numcol];
            for (int i = 0; i < numcol; i++)
            {
                DFormField[i] = new FormField();
                DFormField[i].name = valueslinecapçalera[i];
                DFormField[i].value = values[i];
                DFormField[i].label = valueslinecapçalera[i];
            }
            Dcreateenvelope.clientReference = "ee4de95a-f804-11ea-adc1-0242ac120002";
            Dcreateenvelope.autoClose = true;
            Dcreateenvelope.formFields = DFormField;
            Dcreateenvelope.templateId = templateId;
            Customfield[] DCustomfield = new Customfield[1];
            DCustomfield[0] = new Customfield
            {
                name = "policyNumber",
                value = "04241166"
            };
            Dcreateenvelope.customFields = DCustomfield;
            return Dcreateenvelope;
        }
        public async Task<string> SendEmailAsync(HttpResponseMessage cos, string ErrField, string SubjectField, string PlantillaBranddoc, int NoLineaCsv, string CsvDades, string tenantId, string AppclientId, string AppclientSecret)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            ClientSecretCredential credential = new(tenantId, AppclientId, AppclientSecret);
            //ClientSecretCredential credential = new("be99d803-d172-408b-8aba-61957eaad52b", "2b3bbd22-beba-428b-b8b0-8924c2ceaa61", "b_e8Q~FIT.__OwUmqnO5OpO71Nkk1YAZkQvcTbL7");
            GraphServiceClient graphClient = new(credential, scopes);
            var bodybuilder = "<p>" + ErrField + "</p>";
            if (CsvDades != "") bodybuilder = "<p>" + ErrField + "</p><p>CSV " + CsvDades + " Linea " + NoLineaCsv + "</p>";
            if (PlantillaBranddoc != "") bodybuilder = "<p>" + cos.StatusCode + " " + cos.Headers.Date + "</p><p>" + ErrField + "</p><p>CSV " + CsvDades + " Linea " + NoLineaCsv + "</p>";
            if (CsvDades == "") bodybuilder = "<p>" + cos.StatusCode + " " + cos.Headers.Date + "</p><p>" + ErrField + "</p>";
            Message message = new()
            {
                Subject = SubjectField,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = bodybuilder
                },
                ToRecipients = new List<Recipient>()
                {
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = "vsp@nextret.net"
                    }
                }
            }
            };
            if (PlantillaBranddoc != "")
            {
                MessageAttachmentsCollectionPage attachments = new MessageAttachmentsCollectionPage();
                byte[] contentBytes = System.IO.File.ReadAllBytes("JsonInvocoPlantilla.json");
                string contentType = "json";
                attachments.Add(new FileAttachment
                {
                    ODataType = "#microsoft.graph.fileAttachment",
                    ContentBytes = contentBytes,
                    ContentType = contentType,
                    ContentId = "testing",
                    Name = "JsonInvocoPlantilla.json"
                });
                message.Attachments = attachments;
                message.Subject = SubjectField + PlantillaBranddoc;
            }
            bool saveToSentItems = true;
            await graphClient.Users["comunicaciones@kintegrations.onmicrosoft.com"]
             .SendMail(message, saveToSentItems)
             .Request()
             .PostAsync();
            return "Ok";
        }
        public bool UploadFile(string ftpip, string ftpuser, string ftppasw, string directori, string fitxer)
        {
            SftpClient clientsftp = new SftpClient(ftpip, 22, ftpuser, ftppasw);
            clientsftp.Connect();
            clientsftp.ChangeDirectory("/Integracion DocuSign-Meta4/" + directori);
            FileStream uplfileStream = System.IO.File.OpenRead("downloadedCsv.csv");
            long n = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            fitxer += n.ToString() + ".csv";
            clientsftp.UploadFile(uplfileStream, fitxer, true);
            uplfileStream.Close();
            return true;
        }
        public bool insertOperation(string StorageConnectionString, string taule, TrustCloudFileIds contingut)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(taule);
            _ = table.CreateIfNotExistsAsync();
            //CsvProcesados entity = new();
            TableOperation insertOperation = TableOperation.Insert(contingut);
            _ = table.ExecuteAsync(insertOperation);
            return true;
        }
    }
}
