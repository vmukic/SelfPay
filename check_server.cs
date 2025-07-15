using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
namespace SelfPay
{
  public   class check_server
    {
      public System.Timers.Timer check_server_timer;
      public delegate void ServerCheckedHandler(object myObject, server_checked_args server_checked_sent_Args);
      public event ServerCheckedHandler server_checked;
      public class server_checked_args : EventArgs
      {
          private Boolean  serverOK;

          public server_checked_args(Boolean _serverOK)
          {
              serverOK = _serverOK;
          }

          public Boolean  ServerOK
          {
              get
              {
                  return serverOK;
              }
          }
      }

      public delegate void ServerFoundHandler(object myObject, server_found_args server_found_sent_Args);
      public event ServerFoundHandler server_found;
      public class server_found_args : EventArgs
      {
          private string  serverFound;
          public string serverName;
          public server_found_args(string _server_found,string _server_name)
          {
              serverFound = _server_found;
              serverName = _server_name;
          }

          public string ServerFound
          {
              get
              {
                  return serverFound;
              }
          }
          public string ServerName
          {
              get
              {
                  return serverName;
              }
          }

      }
      public check_server()
     {
         check_server_timer = new System.Timers.Timer();
         check_server_timer.Interval = 1000;
         check_server_timer.Elapsed += new System.Timers.ElapsedEventHandler(check_server_timer_Elapsed);
         
         
     }
      public Boolean  IsServerConnected(string chk)
      {
          Boolean result = false;
          using (SqlConnection connection = new SqlConnection(chk))
          {
              try
              {
                  connection.Open();
                  
                      DataTable databases = connection.GetSchema("Databases");
                      foreach (DataRow database in databases.Rows)
                      {
                          if (database.Field<String>("database_name")==Properties .Resources .datatase_remote )
                          {
                              result = true;
                          }
                         
                      }
                  
                  
              }
              catch (SqlException )
              {
                 
                  result = false;
              }
              catch (Exception ){
                  result = false;
              }
              
          }
          return result;
      }
      void check_server_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
      {

          if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
          {
              string found_server_address = "0.0.0.0";
              if (IsServerConnected(frmMainForm.connectionstring_remote))
              {
                  server_checked_args _args = new server_checked_args(true);
                  server_checked(this, _args);
              }
              else
              {
                 
                  server_checked_args _args = new server_checked_args(false);
                  server_checked(this, _args);
                  Stop();
                  Process netUtility = new Process();
                  netUtility.StartInfo.FileName = "net.exe";
                  netUtility.StartInfo.CreateNoWindow = true;
                  netUtility.StartInfo.Arguments = "view";
                  netUtility.StartInfo.RedirectStandardOutput = true;
                  netUtility.StartInfo.UseShellExecute = false;
                  netUtility.StartInfo.RedirectStandardError = true;
                  netUtility.Start();
                  
                  StreamReader streamReader = new StreamReader(netUtility.StandardOutput.BaseStream, netUtility.StandardOutput.CurrentEncoding);


                  string line = "";
                  List<string> lines = new List<string>();
                  while ((line = streamReader.ReadLine()) != null)
                  {

                      if (line.StartsWith("\\"))
                      {
                          lines.Add(line );
                          //try
                          //{
                          //    line = line.Replace("\\", "").Substring(0, line.IndexOf(" ")).ToUpper();
                          //    if (line != Dns.GetHostName().ToUpper())
                          //    {
                          //        IPAddress[] addresses = Dns.GetHostAddresses(line);
                          //        for (int i = 0; i < addresses.Length; i++)
                          //        {
                          //            if (IsServerConnected(frmMainForm.connectionstring_remote.Replace(frmMainForm.remote_server_address, addresses[i].ToString())))
                          //            {
                          //                found_server_address = addresses[i].ToString();
                          //                break;
                          //            }
                          //        }
                          //    }
                          //}
                          //catch (Exception)
                          //{
                          //}
                      }
                      //if (found_server_address != "0.0.0.0")
                      //{

                      //    break;
                      //}
                  }

                  netUtility.WaitForExit(1000);
                  //server_found_args _args_f = new server_found_args(found_server_address, line);
                  //server_found(this, _args_f);
                  streamReader.Close();
                  string server_name = "";
                  for (int j = 0; j < lines.Count;j++ )
                  {
                      
                      try
                      {
                          
                          if (lines[j].Replace("\\", "").Substring(0, lines[j].IndexOf(" ")).ToUpper() != Dns.GetHostName().ToUpper())
                          {
                              IPAddress[] addresses = Dns.GetHostAddresses(lines[j].Replace("\\", "").Substring(0, lines[j].IndexOf(" ")).ToUpper());
                              for (int i = 0; i < addresses.Length; i++)
                              {
                                     if (IsServerConnected(frmMainForm.connectionstring_remote.Replace(frmMainForm.remote_server_address, addresses[i].ToString())))
                                      {
                                          found_server_address = addresses[i].ToString();
                                          server_name = lines[j].Replace("\\", "").Substring(0, lines[j].IndexOf(" ")).ToUpper();
                                          break;
                                      }
                                 
                              }
                          }
                      }
                      catch (Exception)
                      {
                      } 
                  }
                  if(found_server_address !="0.0.0.0"){
                  server_found_args _args_f = new server_found_args(found_server_address, server_name  );
                  server_found(this, _args_f);
                  }
                  Start();

              }
          }
          else
          {
              server_checked_args _args = new server_checked_args(false);
              server_checked(this, _args);
          }
      }
      public void Start()
      {
          check_server_timer.Start();
      }
      public void Stop()
      {
          check_server_timer.Stop();
      }

    }
}
