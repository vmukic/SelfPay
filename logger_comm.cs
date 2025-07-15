using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO .Ports;
using System.Windows.Forms;
using System.Threading;
namespace SelfPay
{
  public  class logger_comm
    {
      public System.Timers.Timer logger_timer;
      public SerialPort logger_port;
      public delegate void LoggerCheckedHandler(object myObject, logger_checked_args logger_checked_sent_Args);
      public event LoggerCheckedHandler logger_checked;
      public Boolean charging = false;
      public int charge_price = 0;
      public class logger_checked_args : EventArgs
      {
          private Boolean loggerOK;

          public logger_checked_args(Boolean _loggerOK)
          {
              loggerOK = _loggerOK;
          }

          public Boolean LoggerOK
          {
              get
              {
                  return loggerOK;
              }
          }
      }

      public delegate void LoggerCashHandler(object myObject, logger_cash_args logger_cash_sent_Args);
      public event LoggerCashHandler logger_cash;

      public class logger_cash_args : EventArgs
      {
          private int loggercash;

          public logger_cash_args(int _loggercash)
          {
              loggercash = _loggercash ;
          }

          public int  LoggerCash
          {
              get
              {
                  return loggercash ;
              }
          }
      }

      public delegate void LoggerCreditHandler(object myObject, logger_credit_args logger_credit_sent_Args);
      public event LoggerCreditHandler logger_credit;

      public class logger_credit_args : EventArgs
      {
          private string  loggercredit;

          public logger_credit_args(string  _loggercredit)
          {
              loggercredit = _loggercredit;
          }

          public string  LoggerCredit
          {
              get
              {
                  return loggercredit;
              }
              set
              {
                  loggercredit = value;
              }
          }
      }

      public delegate void LoggerReportHandler(object myObject, logger_report_args logger_report_sent_Args);
      public event LoggerReportHandler logger_report;

      public class logger_report_args : EventArgs
      {
          private byte[] loggerreport = new byte[19];
          public int logger_report_count;
          public logger_report_args(byte[] _loggerreport, int _logger_report_count)
          {
              loggerreport = _loggerreport;
              logger_report_count = _logger_report_count;
          }

          public byte[] LoggerReport
          {
              get
              {
                  return loggerreport;
              }
              set
              {
                  loggerreport = value;
              }
          }

          public int LoggerReportCount
          {
              get
              {
                  return logger_report_count;
              }
              set
              {
                  logger_report_count = value;
              }
          }
      }
      public delegate void LoggerErrorHandler(object myObject, logger_error_args logger_error_sent_Args);
      public event LoggerErrorHandler logger_error;

      public class logger_error_args : EventArgs
      {
          private int loggererror;

          public logger_error_args(int _loggererror)
          {
              loggererror = _loggererror;
          }

          public int LoggerError
          {
              get
              {
                  return loggererror;
              }
              set
              {
                  loggererror = value;
              }
          }
      }
      public logger_comm()
      {

          logger_timer = new System.Timers.Timer();
          logger_port = new SerialPort();
          logger_port.Parity = Parity.Even;
          logger_port.BaudRate = 115200;
          logger_port.DataBits = 8;
          logger_port.StopBits = StopBits.One;
          logger_port.PortName = FindLoggerPort ();
          logger_timer.Interval = 1000;
         
          logger_timer.Elapsed += new System.Timers.ElapsedEventHandler(logger_timer_Elapsed);
      }

   public    void logger_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
      {
          Boolean portOK = true;
          byte byte_02 = 0x00;
          byte byte_03 = 0x00;
          byte byte_04 = 0x00;
          byte byte_05 = 0x00;
          byte byte_06 = 0x00;
          byte byte_07 = 0x00;
          byte byte_08 = 0x00;
          if (!charging)
          {
              byte_02 = (byte)0;
              byte_03 = (byte)Convert.ToInt32(Convert.ToString(DateTime.Now.Year).Substring(2));
              byte_04 = (byte)DateTime.Now.Month;
              byte_05 = (byte)DateTime.Now.Day;
              byte_06 = (byte)DateTime.Now.Hour;
              byte_07 = (byte)DateTime.Now.Minute;
              byte_08 = (byte)DateTime.Now.Second;
          }
          else
          {
              byte_02 = (byte)1;
              byte_03 = (byte)(charge_price >> 8 & 0xff); ;
              byte_04 = (byte)(charge_price & 0xff);
              charging = false;

          }
          if (logger_port.PortName != "COM0")
          {
             if(!logger_port .IsOpen ){
                 try
                 {
                     logger_port.Open();
                 }
                 catch (Exception ){
                     portOK = false;
                 }
             }
              if(logger_port .IsOpen ){
                 
                 
                  GC.SuppressFinalize(logger_port.BaseStream);
                  logger_port.Write(new byte[] 
                    { (byte ) 1 , byte_02  ,byte_03  , byte_04 ,
                        byte_05  ,  byte_06   ,
                         byte_07  , byte_08  ,  0x00,
                          0x00  ,  0x00 ,
                       0x00 ,0x00  , 0x00  , 0x00   , 0x00,
                     0x00,0x00 , 0x00,0x00 , 0x00 ,
                0x00 ,0x00 , 0x00 ,0x00  , 0x00 , 
               0x00,0x00, 0x00,0x00  , 0x00,
                 0x00,0x00, 0x00, 0x00, 0x00, 0x00,0x00, 0x00, 0x00, 0x00,0x00, 0x00,0x00 ,0x00 , 0x00 ,
                    0x00}, 0, 47);

                  Thread.Sleep(500);
                  try
                  {
                      int bytes = logger_port.BytesToRead;
                      if (bytes == 21)
                      {
                          byte[] comBuffer = new byte[bytes];
                          try
                          {
                              
                              logger_port.Read(comBuffer, 0, bytes);
                            
                             logger_credit_args _args_cr = new logger_credit_args("");
                             logger_error_args _args_err = new logger_error_args(0);
                             byte[] logger_bytes = new byte[19];
                             logger_report_args _args_rep = new logger_report_args(logger_bytes, 0);
                              switch (Convert .ToInt32 (comBuffer [1])){
                                  case 1:
                                      logger_cash_args _args_f = new logger_cash_args(256 * Convert.ToInt32(comBuffer[2]) + Convert.ToInt32(comBuffer[3]));
                                      logger_cash(this, _args_f);
                                
                                      break;
                                  
                                  case 11:
                                      _args_err.LoggerError = 1;
                                      logger_error(this ,_args_err );
                                      break;
                                  case 12:
                                      _args_err.LoggerError = 2;
                                      logger_error(this, _args_err);
                                      break;
                                  default :
                                      if(Convert .ToInt32 (comBuffer [1])>=2 && Convert .ToInt32 (comBuffer [1]) <=10 ){
                                          _args_cr.LoggerCredit = ReadLoggerCardResponse(comBuffer);
                                          logger_credit(this, _args_cr);
                                      }
                                      if (Convert.ToInt32(comBuffer[1]) >= 20 && Convert.ToInt32(comBuffer[1])<=39)
                                      {
                                          _args_rep.logger_report_count = Convert.ToInt32(comBuffer[1] - 20);
                                          _args_rep.LoggerReport[0] = comBuffer[2];
                                          _args_rep.LoggerReport[1] = comBuffer[3];
                                          _args_rep.LoggerReport[2] = comBuffer[4];
                                          _args_rep.LoggerReport[3] = comBuffer[5];
                                          _args_rep.LoggerReport[4] = comBuffer[6];
                                          _args_rep.LoggerReport[5] = comBuffer[7];
                                          _args_rep.LoggerReport[6] = comBuffer[8];
                                          _args_rep.LoggerReport[7] = comBuffer[9];
                                          _args_rep.LoggerReport[8] = comBuffer[10];
                                          _args_rep.LoggerReport[9] = comBuffer[11];
                                          _args_rep.LoggerReport[10] = comBuffer[12];
                                          _args_rep.LoggerReport[11] = comBuffer[13];
                                          _args_rep.LoggerReport[12] = comBuffer[14];
                                          _args_rep.LoggerReport[13] = comBuffer[15];
                                          _args_rep.LoggerReport[14] = comBuffer[16];
                                          _args_rep.LoggerReport[15] = comBuffer[17];
                                          _args_rep.LoggerReport[16] = comBuffer[18];
                                          _args_rep.LoggerReport[17] = comBuffer[19];
                                          _args_rep.LoggerReport[18] = comBuffer[20];
                                          logger_report(this, _args_rep);
                                      }
                                      break;
                              }
                          }
                          catch (Exception )
                          {
                              portOK = false;
                          }
                        

                      }
                      else
                      {
                          portOK = false;
                          try
                          {
                              logger_port.DiscardInBuffer();
                              logger_port.DiscardOutBuffer();
                              logger_port.Close();
                              
                              GC.SuppressFinalize(logger_port.BaseStream);

                          }
                          catch (Exception)
                          {

                          }
                      }
                  }
                  catch (Exception)
                  {
                      portOK = false;
                  }
              }
          }
          else
          {
              portOK = false;
              logger_timer.Stop();
              logger_port.PortName = FindLoggerPort();
              logger_timer.Start();
          }
          logger_checked_args _args_c = new logger_checked_args(portOK );
          logger_checked(this ,_args_c  );
      }
      public void Start(){
          logger_timer.Start();
      }
      public void Stop()
      {
          logger_timer.Stop();
      }
      public string ReadLoggerCardResponse(byte []inp)
      {
          string res = "";
          for (int i = 2; i < inp.Length;i++ )
          {
              res += (char)inp[i];
          }
          return res ;
      }
      public string FindLoggerPort()
      {
          string res = "COM0";
          SerialPort test_port = new SerialPort();
          test_port.Parity = Parity.Even;
          test_port.BaudRate = 115200;
          test_port.DataBits = 8;
          test_port.StopBits = StopBits.One;
          string[] ports = SerialPort.GetPortNames();
          for(int i=0;i< ports .Length ;i++){
              try
              {
                  test_port.PortName = ports[i];
                  try
                  {
                      test_port .Open ();
                  }
                  catch (Exception ){
                  }
                  if (test_port.IsOpen)
                  {

                      test_port.Write(new byte[] 
                    { (byte ) 1 , 0x00 ,0x00 , 0x00,
                        0x00 ,  0x00  ,
                         0x00 , 0x00 ,  0x00,
                          0x00  ,  0x00 ,
                       0x00 ,0x00  , 0x00  , 0x00   , 0x00,
                     0x00,0x00 , 0x00,0x00 , 0x00 ,
                0x00 ,0x00 , 0x00 ,0x00  , 0x00 , 
               0x00,0x00, 0x00,0x00  , 0x00,
                 0x00,0x00, 0x00, 0x00, 0x00, 0x00,0x00, 0x00, 0x00, 0x00,0x00, 0x00,0x00 ,0x00 , 0x00 ,
                    0x00}, 0, 47);

                      Thread.Sleep(500);
                      try
                      {
                          int bytes = test_port.BytesToRead;
                          if (bytes == 21)
                          {

                              byte[] comBuffer = new byte[bytes];
                              try
                              {
                                  test_port.Read(comBuffer, 0, bytes);
                              }
                              catch (Exception )
                              {
                                 
                              }
                              if(Convert .ToInt32 (comBuffer [0])==1){
                              res = test_port.PortName;
                              test_port.Close();
                              break;
                              }
                              //    }

                              //}
                              //catch (Exception)
                              //{

                              //}

                          }
                          else
                          {
                              try
                              {
                                  test_port.DiscardInBuffer();
                                  test_port.DiscardOutBuffer();
                                  test_port.Close();
                                  test_port.Open();
                                  GC.SuppressFinalize(test_port.BaseStream);
                                 
                              }
                              catch (Exception)
                              {
                                  
                              }
                          }
                      }
                      catch (Exception)
                      {

                      }
                      test_port.DiscardInBuffer();
                      test_port.DiscardOutBuffer();
                      test_port.Close();
                  }
              }
              catch (Exception ){
              }
          }
          return res;

      }

    }
}
