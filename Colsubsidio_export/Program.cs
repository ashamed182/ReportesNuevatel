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
        private static bool localMachine = true;

        static void Main(string[] args)
        {
            string reporte12 = "ReportDialerNoContactado_{fecha}.csv";

            Program.StroredProcedureReportDialerNoContactadoSFTP(reporte12);

        }

        static void StroredProcedureReportDialerNoContactadoSFTP(String reporte12)
        {
            try
            {
                Console.WriteLine("inicio - Reporte Dialer No Contactados SFTP");
                GuardarLog("----------inicio - Reporte Dialer No Contactados SFTP-----------");

                string bd = ConfigurationManager.AppSettings["bd"];
                string cmd_rutaarchivo = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string host = ConfigurationManager.AppSettings["sftp"];
                int port = int.Parse(ConfigurationManager.AppSettings["port"]);
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                string ruta_reportes = ConfigurationManager.AppSettings["cmd_rutaarchivo"];
                string remoteFileName = ConfigurationManager.AppSettings["ruta_archivossftp"];
                string fecha_estatico = ConfigurationManager.AppSettings["fecha_estatico"];
                string nomFileDownload = "";
                string dateExecute = ValidateDateExecute();

                GuardarLog("----------Inicia conexion a la base de datos-----------");

                using (SqlConnection con = new SqlConnection(bd))
                {
                    
                    GuardarLog("----------inicio - Reporte Dialer No Contactados SFTP-----------");

                        using (SqlCommand cmd = new SqlCommand("ReportDialerNoContactadoSFTP", con))
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

                                GuardarLog("----------Se ejecuto SP - Report Dialer No Contactado SFTP-----------");


                                StringBuilder sb = new StringBuilder();

                                foreach (DataRow row in dt.Rows)
                                {
                                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                                    sb.AppendLine(string.Join(",", fields));
                                }

                                string csv = sb.ToString();

                                string dateWithGuion = dateExecute.Replace("-", "_");
                                string reporte12_1 = reporte12.Replace("{fecha}", dateWithGuion);

                                StreamWriter outputFileTest = new StreamWriter(ruta_reportes + @"/" + reporte12_1, false, new UTF8Encoding(true));
                                outputFileTest.Write(csv);
                                outputFileTest.Close();

                                GuardarLog("---------- Se creo archivo:  " + ruta_reportes + "-----------");

                                string rutad = ruta_reportes;
                                string rutad_copiar = ruta_reportes + dateExecute;

                                //tiempo para que genere el reporte de sql a la ruta del servidor
                                Thread.Sleep(10000);
                                Console.WriteLine("fin_copias - Report Dialer No Contactado SFTP");

                                //File.Copy(rutad, rutad_copiar);
                                GuardarLog("Archivo copiado a raiz - fin_copias - Report Dialer No Contactado SFTP: " + rutad_copiar);

                                if (localMachine == false)
                                {
                                    Console.WriteLine("inicio de sftp - Report Dialer No Contactado SFTP");
                                    
                                    SessionOptions sessionOptions = new SessionOptions
                                    {
                                        Protocol = Protocol.Sftp,
                                        UserName = username,
                                        Password = password,
                                        HostName = host,
                                        PortNumber = port,
                                    };
                                using (Session session = new Session())
                                {
                                    Console.WriteLine("-----Entro SFTP - Report Dialer No Contactado SFTP ------");
                                    GuardarLog("-----Login SFTP - Report Dialer No Contactado SFTP ------");
                                    // Connect
                                    session.Open(sessionOptions);
                                    GuardarLog("-----Sesión abierta SFTP - Report Dialer No Contactado SFTP ------");
                                    // Your code
                                    TransferOptions transferOptions = new TransferOptions();
                                    transferOptions.TransferMode = TransferMode.Binary;


                                    TransferOperationResult transferResult4;
                                    transferResult4 = session.PutFiles(ruta_reportes + reporte12_1, remoteFileName + "/", false, transferOptions);
                                    // Throw on any error
                                    transferResult4.Check();

                                    GuardarLog("------ Report Dialer No Contactado SFTP transferido SFTP------");


                                    // Print results
                                    foreach (TransferEventArgs transfer in transferResult4.Transfers)
                                    {
                                        GuardarLog("Download of {0} succeeded" + transfer.FileName);
                                        Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                        nomFileDownload = transfer.FileName;
                                    }
                                }

                                GuardarLog("-----Fin SFTP - Report Dialer No Contactado SFTP ------");

                                Console.WriteLine("Elimando archivo: " + reporte12_1);

                                GuardarLog("-----Inicio borrado archivo CSV : " + reporte12_1);

                                File.Delete(reporte12_1);
                                File.Delete(ruta_reportes + reporte12_1);

                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Carpeta: " + ruta_reportes + dateExecute);
                                GuardarLog("-----Inicio borrado carpeta que contiene archivo CSV Prueba Archivo: " + ruta_reportes + reporte12_1);
                                //System.IO.Directory.Delete(ruta_reportes + dateExecute);
                                GuardarLog("-----Archivos CSV eliminados------");
                                Thread.Sleep(10000);

                                }
                            }
                        }
                    }
                Console.WriteLine("Fin - Report Dialer No Contactado SFTP");
            }            
            
            catch (Exception ex)
            {
                GuardarLog("-----Error------");
                string dateExecute = ValidateDateExecute();
                string dateWithGuion = dateExecute.Replace("-", "_");
                string reporte12_1 = reporte12.Replace("{fecha}", dateWithGuion);

                File.Delete(reporte12_1);
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
