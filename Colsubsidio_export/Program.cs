using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WinSCP;
using System.Web;

namespace Colsubsidio_export
{
    class Program
    {
        static void Main(string[] args)
        {

            string reporte4 = "Virgin_Respuestas_{fecha}.csv";
            string reporte5 = "Virgin_Marcacion_{fecha}_{campaing}.csv";

            string reporte6 = "Virgin_RespuestasBackEnvio_{fecha}_{campaing}.csv";
            string reporte7 = "Virgin_RespuestasBackPorta_{fecha}_{campaing}.csv";
            string reporte8 = "Virgin_RespuestasEncuesta_{fecha}_{campaing}.csv";
            string reporte9 = "Virgin_RespuestasLeads_{fecha}_{campaing}.csv";
            string reporte10 = "Virgin_RespuestasPortout_{fecha}_{campaing}.csv";
            string reporte11 = "Virgin_RespuestasPostventa1_{fecha}_{campaing}.csv";
            string reporte12 = "ReportDialerNoContactado_{fecha}_{campaing}.csv";

            Program.StoredProcedureMarcacion(reporte5);
            Program.StoredProcedureRespuestas(reporte4);
            Program.StoredProcedureRespuestasBackEnvio(reporte6);
            Program.StoredProcedureRespuestasBackPorta(reporte7);
            Program.StoredProcedureRespuestasEncuesta(reporte8);
            Program.StoredProcedureRespuestasLeads(reporte9);
            Program.StoredProcedureRespuestasPortout(reporte10);
            Program.StoredProcedureRespuestasPosventa1(reporte11);
            Program.StroredProcedureReportDialerNoContactadoSFTP(reporte12);

        }

        static void StoredProcedureRespuestas(String reporte4)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Respuestas");
                GuardarLog("----------inicio - Reporte Respuestas-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    using (SqlCommand cmd = new SqlCommand("RpteLlamadasFTP", con))
                    {
                        cmd.CommandTimeout = 500;
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                            sda.SelectCommand = cmd;
                            DataTable dt = new DataTable();
                            sda.Fill(dt);
                            string header = string.Empty;

                            GuardarLog("----------Se ejecuto SP - Reporte Respuestas-----------");

                            StringBuilder sb = new StringBuilder();
                            foreach (DataRow row in dt.Rows)
                            {
                                string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                sb.AppendLine(string.Join("|", fields));
                            }

                            string csv = sb.ToString();

                            string dateWithGuion = dateExecute.Replace("-", "_");
                            string reporte4_1 = reporte4.Replace("{fecha}", dateWithGuion);

                            StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte4_1, false, new UTF8Encoding(true));
                            outputFileTest.Write(csv);
                            outputFileTest.Close();

                            GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte4_1 + "-----------");

                            string rutad = ruta_reportes + reporte4_1;
                            string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte4_1;

                            //tiempo para que genere el reporte de sql a la ruta del servidor
                            Thread.Sleep(10000);
                            Console.WriteLine("fin_copias - Reporte Respuestas");

                            //File.Copy(rutad, rutad_copiar);
                            GuardarLog("Archivo copiado a raiz - Reporte Respuestas: " + rutad_copiar);
                            
                            Console.WriteLine("inicio de ftp - Reporte Respuestas");

                            SessionOptions sessionOptions = new SessionOptions
                            {
                                Protocol = Protocol.Ftp,
                                UserName = username,
                                Password = password,
                                HostName = host,
                                PortNumber = port,
                            };

                            using (Session session = new Session())
                            {
                                Console.WriteLine("-----Entro FTP 1 - Reporte Respuestas ------");
                                GuardarLog("-----Login FTP - Reporte Marcacion ------");
                                // Connect
                                session.Open(sessionOptions);
                                GuardarLog("-----Sesión abierta FTP - Reporte Respuestas ------");
                                // Your code
                                TransferOptions transferOptions = new TransferOptions();
                                transferOptions.TransferMode = TransferMode.Binary;


                                TransferOperationResult transferResult4;
                                transferResult4 = session.PutFiles(ruta_reportes + reporte4_1, remoteFileName + "/", false, transferOptions);
                                // Throw on any error
                                transferResult4.Check();

                                GuardarLog("------ Reporte Respuestas transferido FTP------");


                                // Print results
                                foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                {
                                    GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                    Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                    nomFileDownload = transfer.FileName;
                                }
                            }

                            GuardarLog("-----Fin FTP - Reporte Respuestas ------");

                            Console.WriteLine("Elimando archivo: " + reporte4_1);

                            GuardarLog("-----Inicio borrado archivo CSV : " + reporte4_1);

                            File.Delete(reporte4_1);
                            File.Delete(ruta_reportes + reporte4_1);

                            GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                            GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte4_1);
                            //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                            GuardarLog("-----Archivos CSV eliminados------");
                            Thread.Sleep(10000);
                            
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Respuestas");
            }
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte4);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }

        static void StoredProcedureMarcacion(String reporte5) 
        {
            try 
            {
                Console.WriteLine("inicio - Reporte Marcacion");
                GuardarLog("----------inicio - Reporte Marcacion-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    //List<string> campaings = new List<string> { "virgibmobilerecargadigital", "virginmobileantiplan12k", "virginmobileantiplan22k", "virginmobileantiplan5k" , "virginmobilebolsavoz60", "virginmobilefvd", "virginmobileposventaweb", "virginmobileretail", "virginmobilescam11marzosegmento2",
                    //"virginmobilesegmentacion2", "virginmobilesegmentacion20k", "virginmobilesegmentacion30k", "virginmobilesegmento1", "virginmobilevbrechazos", "virginoutcpmigracion", "virginoutcposventa1", "virginoutcposventa9", "virginoutcrechazos", "virginvoiceblastedistri", "virginvoiceblaster1", "virginvoiceblaster2", "virginvoiceblaster3" };

                    string[] resultArray = ArrayCampaing("campanias");

                    for (int i = 0; i <= resultArray.Length; i++)
                    {
                        GuardarLog("----------inicio - Reporte Campaña " + resultArray[i] + "-----------");

                        using (SqlCommand cmd = new SqlCommand("DetailBatchCallsVirginFTP", con))
                        {
                            cmd.CommandTimeout = 500;
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = resultArray[i];
                                cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                                sda.SelectCommand = cmd;
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                string header = string.Empty;

                                GuardarLog("----------Se ejecuto SP - Reporte Campaña " + resultArray[i] + "-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte5_1 = reporte5.Replace("{fecha}", dateWithGuion);
                                string reporte5_2 = reporte5_1.Replace("{campaing}", resultArray[i]);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte5_2, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte5_2 + "-----------");

                                string rutad = ruta_reportes + reporte5_2;
                                string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte5_2;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Reporte Marcacion");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - Reporte Marcacion: " + rutad_copiar);
                                
                                Console.WriteLine("inicio de ftp - Reporte Marcacion");

                                SessionOptions sessionOptions = new SessionOptions
                                {
                                    Protocol = Protocol.Ftp,
                                    UserName = username,
                                    Password = password,
                                    HostName = host,
                                    PortNumber = port,
                                };

                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro FTP 1 - Reporte Marcacion ------");
                                    GuardarLog("-----Login FTP - Reporte Marcacion ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta FTP - Reporte Marcacion ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte5_2, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Reporte Marcacion transferido FTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin FTP - Reporte Marcacion ------");

                                Console.WriteLine("Elimando archivo: " + reporte5_2);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte5_2);

                                File.Delete(reporte5_2);
                                File.Delete(ruta_reportes + reporte5_2);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte5_2);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);
                                
                            }
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Marcacion");
            } 
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte5);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }

        static void StoredProcedureRespuestasBackEnvio(String reporte6)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Repuestas BackEnvio");
                GuardarLog("----------inicio - Reporte Repuestas BackEnvio-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    //List<string> campaings = new List<string> { "virgibmobilerecargadigital", "virginmobileantiplan12k", "virginmobileantiplan22k", "virginmobileantiplan5k" , "virginmobilebolsavoz60", "virginmobilefvd", "virginmobileposventaweb", "virginmobileretail", "virginmobilescam11marzosegmento2",
                    //"virginmobilesegmentacion2", "virginmobilesegmentacion20k", "virginmobilesegmentacion30k", "virginmobilesegmento1", "virginmobilevbrechazos", "virginoutcpmigracion", "virginoutcposventa1", "virginoutcposventa9", "virginoutcrechazos", "virginvoiceblastedistri", "virginvoiceblaster1", "virginvoiceblaster2", "virginvoiceblaster3" };

                    string[] resultArray = ArrayCampaing("campaniasbackenvio");

                    for (int i = 0; i <= resultArray.Length; i++)
                    {
                        GuardarLog("----------inicio - Reporte Repuestas BackEnvio Campaña " + resultArray[i] + "-----------");

                        using (SqlCommand cmd = new SqlCommand("RpteLlamadasBackEnvioFTP", con))
                        {
                            cmd.CommandTimeout = 500;
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = resultArray[i];
                                cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                                sda.SelectCommand = cmd;
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                string header = string.Empty;

                                GuardarLog("----------Se ejecuto SP - Reporte Repuestas BackEnvio Campaña " + resultArray[i] + "-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte6_1 = reporte6.Replace("{fecha}", dateWithGuion);
                                string reporte6_2 = reporte6_1.Replace("{campaing}", resultArray[i]);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte6_2, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte6_2 + "-----------");

                                string rutad = ruta_reportes + reporte6_2;
                                string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte6_2;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Reporte Repuestas BackEnvio");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - Reporte Repuestas BackEnvio: " + rutad_copiar);

                                Console.WriteLine("inicio de ftp - Reporte Repuestas BackEnvio");

                                SessionOptions sessionOptions = new SessionOptions
                                {
                                    Protocol = Protocol.Ftp,
                                    UserName = username,
                                    Password = password,
                                    HostName = host,
                                    PortNumber = port,
                                };

                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro FTP 1 - Reporte Repuestas BackEnvio ------");
                                    GuardarLog("-----Login FTP - Reporte Repuestas BackEnvio ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta FTP - Reporte Repuestas BackEnvio ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte6_2, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Reporte Repuestas BackEnvio transferido FTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin FTP - Reporte Repuestas BackEnvio ------");

                                Console.WriteLine("Elimando archivo: " + reporte6_2);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte6_2);

                                File.Delete(reporte6_2);
                                File.Delete(ruta_reportes + reporte6_2);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte6_2);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);
                                
                            }
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Repuestas BackEnvio");
            }
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte6);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }

        static void StoredProcedureRespuestasBackPorta(String reporte7)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Repuestas BackPorta");
                GuardarLog("----------inicio - Reporte Repuestas BackPorta-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    //List<string> campaings = new List<string> { "virgibmobilerecargadigital", "virginmobileantiplan12k", "virginmobileantiplan22k", "virginmobileantiplan5k" , "virginmobilebolsavoz60", "virginmobilefvd", "virginmobileposventaweb", "virginmobileretail", "virginmobilescam11marzosegmento2",
                    //"virginmobilesegmentacion2", "virginmobilesegmentacion20k", "virginmobilesegmentacion30k", "virginmobilesegmento1", "virginmobilevbrechazos", "virginoutcpmigracion", "virginoutcposventa1", "virginoutcposventa9", "virginoutcrechazos", "virginvoiceblastedistri", "virginvoiceblaster1", "virginvoiceblaster2", "virginvoiceblaster3" };

                    string[] resultArray = ArrayCampaing("campaniasbackporta");

                    for (int i = 0; i <= resultArray.Length; i++)
                    {
                        GuardarLog("----------inicio - Reporte Repuestas BackPorta Campaña " + resultArray[i] + "-----------");

                        using (SqlCommand cmd = new SqlCommand("RpteLlamadasBackPortaFTP", con))
                        {
                            cmd.CommandTimeout = 500;
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = resultArray[i];
                                cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                                sda.SelectCommand = cmd;
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                string header = string.Empty;

                                GuardarLog("----------Se ejecuto SP - Reporte Repuestas BackPorta Campaña " + resultArray[i] + "-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte7_1 = reporte7.Replace("{fecha}", dateWithGuion);
                                string reporte7_2 = reporte7_1.Replace("{campaing}", resultArray[i]);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte7_1, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte7_1 + "-----------");

                                string rutad = ruta_reportes + reporte7_1;
                                string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte7_1;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Reporte Repuestas BackPorta");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - Reporte Repuestas BackPorta: " + rutad_copiar);

                                Console.WriteLine("inicio de ftp - Reporte Repuestas BackPorta");

                                SessionOptions sessionOptions = new SessionOptions
                                {
                                    Protocol = Protocol.Ftp,
                                    UserName = username,
                                    Password = password,
                                    HostName = host,
                                    PortNumber = port,
                                };

                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro FTP 1 - Reporte Repuestas BackPorta ------");
                                    GuardarLog("-----Login FTP - Reporte Repuestas BackPorta ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta FTP - Reporte Repuestas BackPorta ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte7_1, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Reporte Repuestas BackPorta transferido FTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin FTP - Reporte Repuestas BackPorta ------");

                                Console.WriteLine("Elimando archivo: " + reporte7_1);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte7_1);

                                File.Delete(reporte7_1);
                                File.Delete(ruta_reportes + reporte7_1);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte7_1);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);
                                
                            }
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Repuestas BackEnvio");
            }
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte7);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }
        static void StoredProcedureRespuestasEncuesta(String reporte8)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Repuestas Encuesta");
                GuardarLog("----------inicio - Reporte Repuestas Encuesta-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    //List<string> campaings = new List<string> { "virgibmobilerecargadigital", "virginmobileantiplan12k", "virginmobileantiplan22k", "virginmobileantiplan5k" , "virginmobilebolsavoz60", "virginmobilefvd", "virginmobileposventaweb", "virginmobileretail", "virginmobilescam11marzosegmento2",
                    //"virginmobilesegmentacion2", "virginmobilesegmentacion20k", "virginmobilesegmentacion30k", "virginmobilesegmento1", "virginmobilevbrechazos", "virginoutcpmigracion", "virginoutcposventa1", "virginoutcposventa9", "virginoutcrechazos", "virginvoiceblastedistri", "virginvoiceblaster1", "virginvoiceblaster2", "virginvoiceblaster3" };

                    string[] resultArray = ArrayCampaing("campaniasencuesta");

                    for (int i = 0; i <= resultArray.Length; i++)
                    {
                        GuardarLog("----------inicio - Reporte Repuestas Encuesta Campaña " + resultArray[i] + "-----------");

                        using (SqlCommand cmd = new SqlCommand("RpteLlamadasEncuestaFTP", con))
                        {
                            cmd.CommandTimeout = 500;
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = resultArray[i];
                                cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                                sda.SelectCommand = cmd;
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                string header = string.Empty;

                                GuardarLog("----------Se ejecuto SP - Reporte Repuestas Encuesta Campaña " + resultArray[i] + "-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte8_1 = reporte8.Replace("{fecha}", dateWithGuion);
                                string reporte8_2 = reporte8_1.Replace("{campaing}", resultArray[i]);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte8_2, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte8_2 + "-----------");

                                string rutad = ruta_reportes + reporte8_2;
                                string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte8_2;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Reporte Repuestas Encuesta");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - Reporte Repuestas Encuesta: " + rutad_copiar);

                                Console.WriteLine("inicio de ftp - Reporte Repuestas Encuesta");

                                SessionOptions sessionOptions = new SessionOptions
                                {
                                    Protocol = Protocol.Ftp,
                                    UserName = username,
                                    Password = password,
                                    HostName = host,
                                    PortNumber = port,
                                };

                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro FTP 1 - Reporte Repuestas Encuesta ------");
                                    GuardarLog("-----Login FTP - Reporte Repuestas Encuesta ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta FTP - Reporte Repuestas Encuesta ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte8_2, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Reporte Repuestas Encuesta transferido FTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin FTP - Reporte Repuestas Encuesta ------");

                                Console.WriteLine("Elimando archivo: " + reporte8_2);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte8_2);

                                File.Delete(reporte8_2);
                                File.Delete(ruta_reportes + reporte8_2);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte8_2);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);
                                
                            }
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Repuestas Encuesta");
            }
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte8);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }

        static void StoredProcedureRespuestasLeads(String reporte9)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Repuestas Leads");
                GuardarLog("----------inicio - Reporte Repuestas Leads-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    //List<string> campaings = new List<string> { "virgibmobilerecargadigital", "virginmobileantiplan12k", "virginmobileantiplan22k", "virginmobileantiplan5k" , "virginmobilebolsavoz60", "virginmobilefvd", "virginmobileposventaweb", "virginmobileretail", "virginmobilescam11marzosegmento2",
                    //"virginmobilesegmentacion2", "virginmobilesegmentacion20k", "virginmobilesegmentacion30k", "virginmobilesegmento1", "virginmobilevbrechazos", "virginoutcpmigracion", "virginoutcposventa1", "virginoutcposventa9", "virginoutcrechazos", "virginvoiceblastedistri", "virginvoiceblaster1", "virginvoiceblaster2", "virginvoiceblaster3" };

                    string[] resultArray = ArrayCampaing("campaniasleads");

                    for (int i = 0; i <= resultArray.Length; i++)
                    {
                        GuardarLog("----------inicio - Reporte Repuestas Leads Campaña " + resultArray[i] + "-----------");

                        using (SqlCommand cmd = new SqlCommand("RpteLlamadasLeadsFTP", con))
                        {
                            cmd.CommandTimeout = 500;
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = resultArray[i];
                                cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                                sda.SelectCommand = cmd;
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                string header = string.Empty;

                                GuardarLog("----------Se ejecuto SP - Reporte Repuestas Leads Campaña " + resultArray[i] + "-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte9_1 = reporte9.Replace("{fecha}", dateWithGuion);
                                string reporte9_2 = reporte9_1.Replace("{campaing}", resultArray[i]);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte9_2, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte9_2 + "-----------");

                                string rutad = ruta_reportes + reporte9_2;
                                string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte9_2;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Reporte Repuestas Leads");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - Reporte Repuestas Leads: " + rutad_copiar);

                                Console.WriteLine("inicio de ftp - Reporte Repuestas Leads");

                                SessionOptions sessionOptions = new SessionOptions
                                {
                                    Protocol = Protocol.Ftp,
                                    UserName = username,
                                    Password = password,
                                    HostName = host,
                                    PortNumber = port,
                                };

                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro FTP 1 - Reporte Repuestas Leads ------");
                                    GuardarLog("-----Login FTP - Reporte Repuestas Leads ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta FTP - Reporte Repuestas Leads ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte9_2, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Reporte Repuestas Leads transferido FTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin FTP - Reporte Repuestas Leads ------");

                                Console.WriteLine("Elimando archivo: " + reporte9_2);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte9_2);

                                File.Delete(reporte9_2);
                                File.Delete(ruta_reportes + reporte9_2);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte9_2);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);
                                
                            }
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Repuestas Leads");
            }
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte9);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }

        static void StroredProcedureReportDialerNoContactadoSFTP(String reporte12) { 
        
        }

        static void StoredProcedureRespuestasPortout(String reporte10)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Repuestas Portout");
                GuardarLog("----------inicio - Reporte Repuestas Portout-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    //List<string> campaings = new List<string> { "virgibmobilerecargadigital", "virginmobileantiplan12k", "virginmobileantiplan22k", "virginmobileantiplan5k" , "virginmobilebolsavoz60", "virginmobilefvd", "virginmobileposventaweb", "virginmobileretail", "virginmobilescam11marzosegmento2",
                    //"virginmobilesegmentacion2", "virginmobilesegmentacion20k", "virginmobilesegmentacion30k", "virginmobilesegmento1", "virginmobilevbrechazos", "virginoutcpmigracion", "virginoutcposventa1", "virginoutcposventa9", "virginoutcrechazos", "virginvoiceblastedistri", "virginvoiceblaster1", "virginvoiceblaster2", "virginvoiceblaster3" };

                    string[] resultArray = ArrayCampaing("campaniasportout");

                    for (int i = 0; i <= resultArray.Length; i++)
                    {
                        GuardarLog("----------inicio - Reporte Repuestas Portout Campaña " + resultArray[i] + "-----------");

                        using (SqlCommand cmd = new SqlCommand("RpteLlamadasPortoutFTP", con))
                        {
                            cmd.CommandTimeout = 500;
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = resultArray[i];
                                cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                                sda.SelectCommand = cmd;
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                string header = string.Empty;

                                GuardarLog("----------Se ejecuto SP - Reporte Repuestas Portout Campaña " + resultArray[i] + "-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte10_1 = reporte10.Replace("{fecha}", dateWithGuion);
                                string reporte10_2 = reporte10_1.Replace("{campaing}", resultArray[i]);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte10_2, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte10_2 + "-----------");

                                string rutad = ruta_reportes + reporte10_2;
                                string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte10_2;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Reporte Repuestas Portout");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - Reporte Repuestas Portout: " + rutad_copiar);

                                Console.WriteLine("inicio de ftp - Reporte Repuestas Portout");

                                SessionOptions sessionOptions = new SessionOptions
                                {
                                    Protocol = Protocol.Ftp,
                                    UserName = username,
                                    Password = password,
                                    HostName = host,
                                    PortNumber = port,
                                };

                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro FTP 1 - Reporte Repuestas Portout ------");
                                    GuardarLog("-----Login FTP - Reporte Repuestas Portout ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta FTP - Reporte Repuestas Portout ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte10_2, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Reporte Repuestas Portout transferido FTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin FTP - Reporte Repuestas Portout ------");

                                Console.WriteLine("Elimando archivo: " + reporte10_2);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte10_2);

                                File.Delete(reporte10_2);
                                File.Delete(ruta_reportes + reporte10_2);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte10_2);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);
                                
                            }
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Repuestas Portout");
            }
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte10);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }

        static void StoredProcedureRespuestasPosventa1(String reporte11)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Repuestas Posventa1");
                GuardarLog("----------inicio - Reporte Repuestas Posventa1-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["ftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivoftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    //List<string> campaings = new List<string> { "virgibmobilerecargadigital", "virginmobileantiplan12k", "virginmobileantiplan22k", "virginmobileantiplan5k" , "virginmobilebolsavoz60", "virginmobilefvd", "virginmobileposventaweb", "virginmobileretail", "virginmobilescam11marzosegmento2",
                    //"virginmobilesegmentacion2", "virginmobilesegmentacion20k", "virginmobilesegmentacion30k", "virginmobilesegmento1", "virginmobilevbrechazos", "virginoutcpmigracion", "virginoutcposventa1", "virginoutcposventa9", "virginoutcrechazos", "virginvoiceblastedistri", "virginvoiceblaster1", "virginvoiceblaster2", "virginvoiceblaster3" };

                    string[] resultArray = ArrayCampaing("campaniasposventa1");

                    for (int i = 0; i <= resultArray.Length; i++)
                    {
                        GuardarLog("----------inicio - Reporte Repuestas Posventa1 Campaña " + resultArray[i] + "-----------");

                        using (SqlCommand cmd = new SqlCommand("RpteLlamadasPostventa1FTP", con))
                        {
                            cmd.CommandTimeout = 500;
                            using (SqlDataAdapter sda = new SqlDataAdapter())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = resultArray[i];
                                cmd.Parameters.Add("@StartDateTime", SqlDbType.Date).Value = Convert.ToDateTime(dateExecute);


                                sda.SelectCommand = cmd;
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                string header = string.Empty;

                                GuardarLog("----------Se ejecuto SP - Reporte Repuestas Posventa1 Campaña " + resultArray[i] + "-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte11_1 = reporte11.Replace("{fecha}", dateWithGuion);
                                string reporte11_2 = reporte11_1.Replace("{campaing}", resultArray[i]);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte11_2, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + @"/" + reporte11_2 + "-----------");

                                string rutad = ruta_reportes + reporte11_2;
                                string rutad_copiar = ruta_reportes + dateExecute + @"\" + reporte11_2;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Reporte Repuestas Posventa1");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - Reporte Repuestas Posventa1: " + rutad_copiar);

                                Console.WriteLine("inicio de ftp - Reporte Repuestas Posventa1");

                                SessionOptions sessionOptions = new SessionOptions
                                {
                                    Protocol = Protocol.Ftp,
                                    UserName = username,
                                    Password = password,
                                    HostName = host,
                                    PortNumber = port,
                                };

                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro FTP 1 - Reporte Repuestas Posventa1 ------");
                                    GuardarLog("-----Login FTP - Reporte Repuestas Posventa1 ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta FTP - Reporte Repuestas Posventa1 ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte11_2, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Reporte Repuestas Posventa1 transferido FTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin FTP - Reporte Repuestas Posventa1 ------");

                                Console.WriteLine("Elimando archivo: " + reporte11_2);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte11_2);

                                File.Delete(reporte11_2);
                                File.Delete(ruta_reportes + reporte11_2);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte11_2);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);
                                
                            }
                        }
                    }
                }

                Console.WriteLine("Fin - Reporte Repuestas Posventa1");
            }
            catch (Exception ex)
            {
                GuardarLog("-----Error------");

                File.Delete(reporte11);
                GuardarLog("-----Archivos CSV eliminados------");

                GuardarLog("Motivo del Error: " + ex.Message);
                Console.WriteLine(ex.Message);
                Thread.Sleep(10000);
            }

        }

        static string ValidateDateExecute() 
        {
            string nameDate = "";
            string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];

            if (fecha_estatico == "")
            {
                Console.WriteLine("-----If Fecha Hoy------");
                nameDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            else
            {
                Console.WriteLine("-----If Fecha Estatica------");
                nameDate = fecha_estatico;
            }

            return nameDate;
        }

        static string[] ArrayCampaing(string campaniasproperty) 
        {
            string StringCampaing = ConfigurationManager.AppSettings[campaniasproperty];
            string[] arrayCampaing = StringCampaing.Split(',');

            return arrayCampaing;
        }

        static int GuardarLog(string msj)
        {
            string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];

            string fecha = System.DateTime.Now.ToString("yyyy-MM-dd");
            string hora = System.DateTime.Now.ToString("HH:mm:ss");
            string path = cmd_rutaarchivo + "\\log_import.txt";

            StreamWriter sw = new StreamWriter(path, true);

            StackTrace stacktrace = new StackTrace();
            sw.WriteLine(fecha + " " + hora + "  " + msj);

            sw.Flush();
            sw.Close();

            return 0;
        }


    }
}
