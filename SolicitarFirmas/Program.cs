// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure;
using Serilog;
using System.Net;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Configuration;
using System;
using System.Collections;
using SolicitarFirmas.Models;
using Azure.Core;
using FluentFTP;
//using Org.BouncyCastle.Asn1.Ocsp;
using FluentFTP.Helpers;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using System.Collections.Concurrent;
using System.Globalization;
using MailKit.Search;
using MimeKit;
using MailKit.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.Graph.ExternalConnectors;
using Renci.SshNet;
using System.Net.Http;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Logging;
using Renci.SshNet.Sftp;
try
{
    DateTime llancament = DateTime.Now;
    string CsvDades = "";
    string wCsvDades = "";
    string VaultU = "";
    string entorn = "devolopment";
    string PathAzure = "";
    IConfiguration config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
   .Build();
    foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
    {
        if (Convert.ToString(de.Key) == "WEBSITE_SITE_NAME") entorn = "azure";
    }
    if (entorn == "devolopment")
    {
        IConfiguration config1 = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
         .Build();
        Credentials settings = config1.GetRequiredSection("Settings").Get<Credentials>();
        VaultU = settings.VaultUri;
        PathAzure = "";
    }
    if (entorn == "azure")
    {
        VaultU = Environment.GetEnvironmentVariable("VaultUri");
        PathAzure = System.Environment.GetEnvironmentVariable("WEBROOT_PATH") + @"\";
    }

    Uri keyVaultUrl = new(VaultU);
    var clientsecret = new SecretClient(keyVaultUrl, new DefaultAzureCredential());
    //AsyncPageable<SecretProperties> allSecrets = clientsecret.GetPropertiesOfSecretsAsync();
    KeyVaultSecret client_secret = clientsecret.GetSecret("clientsecret");
    KeyVaultSecret grant_type = clientsecret.GetSecret("granttype");
    KeyVaultSecret client_id = clientsecret.GetSecret("clientid");
    KeyVaultSecret ftpip = clientsecret.GetSecret("ftpip");
    KeyVaultSecret ftpuser = clientsecret.GetSecret("ftpuser");
    KeyVaultSecret ftppasw = clientsecret.GetSecret("ftppasw");
    string Nomlog = DateTime.Now.ToString("yyMMdd");
    KeyVaultSecret StorageConnectionString = clientsecret.GetSecret("StorageConnectionString");
    KeyVaultSecret tenantId = clientsecret.GetSecret("tenantId");
    KeyVaultSecret AppclientId = clientsecret.GetSecret("AppclientId");
    KeyVaultSecret AppclientSecret = clientsecret.GetSecret("AppclientSecret");
    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
       .WriteTo.File(PathAzure + "Log_" + Nomlog + ".txt")
       .WriteTo.Console()
       .CreateLogger();
    //Log.Debug("creat log " + PathAzure + "Log_" + Nomlog + ".txt");
    var linecapçalera = "";
    int rCnt = 0;
    int tCnt = 0;
    int fCnt = 0;
    int eCnt = 0;
    int epCnt = 0;
    string dolents = "";
    string missatge = "";
    string ErrField = "";
    string templateId = "";
    HttpClient client = new();
    bool procesar = false;
    bool resultat = true;
    var form = new Dictionary<string, string>
                {
                    {"grant_type", grant_type.Value},
                    {"client_id", client_id.Value},
                    {"client_secret", client_secret.Value},
                };
    HttpResponseMessage result = new(HttpStatusCode.MultiStatus);
    CodeFile DCodeFile = new();
    string baseAddress = @"https://identityserver-pre.trustcloud.solutions/IdentityServer/connect/token";
    string baseAddressAma = @"https://14ldz2qnw0.execute-api.eu-west-1.amazonaws.com/pre/api/v1/sign/useCase/";
    Uri u = new(baseAddressAma + client_id.Value + "/create");
    result = await client.PostAsync(baseAddress, new FormUrlEncodedContent(form));
    bool success = ((int)result.StatusCode) >= 200 && ((int)result.StatusCode) < 300;
    if (success)
    {
        var json = result.Content.ReadAsStringAsync();
        Token DToken = new();
        DToken = JsonConvert.DeserializeObject<Token>(json.Result);
        string tToken = DToken.Token_Type + " " + DToken.Access_Token;
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", tToken);
        var clientsftp = new SftpClient(ftpip.Value, 22, ftpuser.Value, ftppasw.Value);
        clientsftp.Connect();
        foreach (var item in clientsftp.ListDirectory("/Integracion DocuSign-Meta4/"))
        {
            if (!item.Attributes.IsDirectory)
            {
                if (rCnt-1 == epCnt && rCnt != 0) resultat = DCodeFile.UploadFile(ftpip.Value, ftpuser.Value, ftppasw.Value, "Procesados", wCsvDades);
                if (rCnt-1 != epCnt && rCnt != 0) resultat = DCodeFile.UploadFile(ftpip.Value, ftpuser.Value, ftppasw.Value, "Errores", wCsvDades);
                CsvDades = item.Name;
                procesar = false;
                linecapçalera = "";
                if (CsvDades.ToUpper() == "ACTUALIZACIÓN.CSV" || CsvDades.ToUpper() == "ACTUALIZACION.CSV" || CsvDades.ToUpper() == "NOVACIÓN.CSV" || CsvDades.ToUpper() == "NOVACION.CSV")
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString.Value);
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable table = tableClient.GetTableReference("CsvProcesados");
                    TableOperation retrieveOperation = TableOperation.Retrieve<CsvProcesados>(CsvDades, "Procesado");
                    TableResult query = await table.ExecuteAsync(retrieveOperation);
                    if (query.Result == null) procesar = true;
                }
                if (CsvDades.ToUpper() == "ACTUALIZACION.CSV")
                {
                    if (System.IO.File.Exists("downloadedCsv.csv")) System.IO.File.Delete("downloadedCsv.csv");
                    FileStream stream = System.IO.File.OpenWrite("downloadedCsv.csv");
                    clientsftp.DownloadFile("/Integracion DocuSign-Meta4/" + CsvDades, stream);
                    stream.Flush();
                    stream.Close();
                    rCnt = 0;
                    epCnt = 0;
                    using StreamReader streamReader = new(stream.Name);
                    while (!streamReader.EndOfStream)
                    {
                        rCnt++;
                        if (rCnt == 1)
                        {
                            TrustCloudFileIds entity = new()
                            {
                                RowKey = "Procesado",
                                NoLineaCsv = null,
                                Csv = null,
                                PartitionKey = CsvDades
                            };
                            resultat = DCodeFile.insertOperation(StorageConnectionString.Value, "CsvProcesados", entity);
                            fCnt++;
                            linecapçalera = streamReader.ReadLine();
                        }
                        if (rCnt > 1)
                        {
                            templateId = "";
                            var line = streamReader.ReadLine();
                            string[] source = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int wordCount = 0;
                            wordCount = source.Length;
                            if (wordCount != 0)
                            {
                                var values = line.Split(';');
                                int numcol = values.Length - 1;
                                bool PPP = false;
                                if (CsvDades.ToUpper() == "ACTUALIZACIÓN.CSV" || CsvDades.ToUpper() == "ACTUALIZACION.CSV") CsvDades = "ACTUALIZACION";
                                if (CsvDades.ToUpper() == "NOVACIÓN.CSV" || CsvDades.ToUpper() == "NOVACION.CSV") CsvDades = "NOVACION";
                                if (values[numcol] == "EXCLU" || values[numcol] == "EXCLU_12P" || values[numcol] == "01RENO") PPP = true;
                                if (CsvDades == "NOVACION" && PPP && (values[20] == "A62733126" || values[20] == "B06787618" || values[20] == "A79787354")) templateId = "52d8073c-93bb-423c-91a2-33f5c050d221";
                                if (CsvDades == "NOVACION" && !PPP && (values[20] == "A62733126" || values[20] == "B06787618" || values[20] == "A79787354")) templateId = "9ec2c3f1-da0f-4cb8-a91b-9885e027deae";
                                if (CsvDades == "ACTUALIZACION" && PPP && (values[20] == "A62733126" || values[20] == "B06787618" || values[20] == "A79787354")) templateId = "25ac4256-501c-4649-8165-0b1916c1c1e3";
                                tCnt++;
                                wCsvDades = CsvDades;
                                if (templateId != "")
                                {
                                    string jsonStringSerialized = "";
                                    Createenvelope? Dcreateenvelope = DCodeFile.MontarJson(rCnt, fCnt, CsvDades, linecapçalera, line, templateId, llancament, entorn);
                                    jsonStringSerialized = JsonConvert.SerializeObject(Dcreateenvelope);
                                    StringContent content = new(jsonStringSerialized, System.Text.Encoding.UTF8, "application/json");
                                    System.IO.File.WriteAllText("JsonInvocoPlantilla.json", jsonStringSerialized);
                                    result = await client.PostAsync(u, content);
                                    Thread.Sleep(33000);
                                    var jsonresult = result.Content.ReadAsStringAsync();
                                    string newString = jsonresult.Result;
                                    string trustCloudFileId = "";
                                    if (newString.Length == 38)
                                    {
                                        trustCloudFileId = newString.Replace("\"", "");
                                    }
                                    success = ((int)result.StatusCode) >= 200 && ((int)result.StatusCode) < 300;
                                    if (success)
                                    {
                                        eCnt++;
                                        epCnt++;
                                        TrustCloudFileIds entity = new()
                                        {
                                            RowKey = "Procesado",
                                            NoLineaCsv = rCnt,
                                            Csv = CsvDades,
                                            PartitionKey = trustCloudFileId
                                        };
                                        resultat = DCodeFile.insertOperation(StorageConnectionString.Value, "TrustCloudFileIds", entity);
                                    }
                                    if (!success)
                                    {
                                        if (newString.Length == 38)
                                        {
                                            trustCloudFileId = newString.Replace("\"", "");
                                        }
                                        else
                                        {
                                            ErrField = newString;
                                        }
                                        if (trustCloudFileId != "")
                                        {
                                            Uri ue = new(baseAddressAma + client_id.Value + "/trustCloudFile/" + trustCloudFileId + "/clientReference/" + Dcreateenvelope.clientReference + "/search");
                                            SignSearch? DSignSearch = new();
                                            Customfield[] DCustomfield = new Customfield[1];
                                            DCustomfield[0] = new Customfield();
                                            DCustomfield[0] = Dcreateenvelope.customFields[0];
                                            DSignSearch.customFields = DCustomfield;
                                            jsonStringSerialized = JsonConvert.SerializeObject(DSignSearch);
                                            content = new StringContent(jsonStringSerialized, System.Text.Encoding.UTF8, "application/json");
                                            result = await client.PostAsync(ue, content);
                                            json = result.Content.ReadAsStringAsync();
                                            string newString1 = json.Result;
                                            if (newString1.Substring(0, 1) == "[") newString1 = newString1.Substring(1, newString1.Length - 2);
                                            System.IO.File.WriteAllText("JsonSearch.json", newString1);
                                            SolicitarFirmas.Models.Search? DSearch = new();
                                            DSearch = JsonConvert.DeserializeObject<Search>(newString1);
                                            ErrField = (string)DSearch.status;
                                        }
                                        await DCodeFile.SendEmailAsync(result, ErrField, "Error en la llamada a Branddocs, plantilla: ", templateId, rCnt, CsvDades, tenantId.Value, AppclientId.Value, AppclientSecret.Value);
                                        Console.WriteLine("\t" + CsvDades + " Linea " + rCnt + "\t" + ErrField);
                                    }
                                }
                                else
                                {
                                    ErrField = "No enviado por filtro CIF o ID CONVENIO";
                                    await DCodeFile.SendEmailAsync(result, ErrField, "Linea no Enviada", "", rCnt, CsvDades, tenantId.Value, AppclientId.Value, AppclientSecret.Value);
                                    Console.WriteLine("\t" + CsvDades + " Linea " + rCnt + "\t" + ErrField);
                                }
                            }
                        }
                    }
                }
            }
        }
        if (rCnt-1 == epCnt && rCnt != 0) resultat = DCodeFile.UploadFile(ftpip.Value, ftpuser.Value, ftppasw.Value, "Procesados", wCsvDades);
        if (rCnt-1 != epCnt && rCnt != 0) resultat = DCodeFile.UploadFile(ftpip.Value, ftpuser.Value, ftppasw.Value, "Errores", wCsvDades);
        dolents = Convert.ToString(tCnt - eCnt);
        if (tCnt != 0 && dolents == "0") missatge = "Envio finalizado con exito, numero de envios totales : " + tCnt;
        if (tCnt == 0) missatge = "No hay envios para este dia";
        if (dolents != "0") missatge = "Numero de envios previstos totales : " + tCnt + " envios filtrados : " + dolents + " consultar e-mail";
        Log.Information(missatge);
    }
    else
    {
        ErrField = "Error al obtener Token " + result.StatusCode;
        await DCodeFile.SendEmailAsync(result, ErrField, "Error en la llamada a Branddocs", "", 0, "", tenantId.Value, AppclientId.Value, AppclientSecret.Value);
        Console.WriteLine(ErrField);
    }
}
catch (IOException ioEx)
{
    Log.Error("IOException " + ioEx);
}
catch (Exception ex)
{
    Log.Error("Exception " + ex);
}
finally
{
}
