using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
namespace SelfPay
{
    public partial class frmMainForm : Form
    {
        public frmMainForm()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);
       
        delegate void SetControlTextCallback(Control c, string text);
        delegate void SetControlVisibleCallback(Control c,Boolean v);
        delegate void SetControlFocusCallback(Control c);
        delegate void HideTextBoxCaretCallback(TextBox t);
        delegate void SetLabelImageCallback(Label l, Image i);
        delegate void SetControlEnabledCallback(Control c, Boolean e);
        delegate void SetTextBoxSelectionCallback(TextBox t);
        public ControlsTexts ct_croatian = new ControlsTexts();
        public ControlsTexts ct_italian = new ControlsTexts();
        public ControlsTexts ct_german = new ControlsTexts();
        public ControlsTexts ct_english = new ControlsTexts();
        public ControlsTexts ct_default = new ControlsTexts ();
      public static   string  remote_server_address = "0.0.0.0";
      public static  string remote_server_name = "";
        public static string connectionstring_remote;
        
      public List<LicensePlate> LicensesPlateList = new List<LicensePlate>();
      public Image empty_license_plate_image;
      public int first_license_plate_ID = 0;
        public static   check_server _check_server= new check_server ();
        Thread check_server_thread = new Thread(_check_server.Start );
        public Boolean server_OK = false;
        public Boolean logger_OK = false;
      public List<Panel> Panels = new List<Panel>();
      public Boolean mainform_keypress = true;
      public int last_panel = 1;
      public static logger_comm _logger_comm = new logger_comm ();
     Thread logger_comm_thread = new Thread(_logger_comm .Start );
   public static   check_payment _check_payment = new check_payment();
     Thread check_payment_thread = new Thread(_check_payment.Start );
  public    Cjenik last_price_cj = new Cjenik(0,0.00m,0.00m,0.00m,0,0,0);
     public Cijena last_price ;
     public int last_sum_paid = 0;
     public List<string> credit_paid = new List<string>();
     public List<string> logger_report = new List<string>();
        public System.Timers.Timer licence_plate_panel_timer;
        public int lpp_count = 0;
        public int lbl_charge_paid_state=0;
        public int lbl_charge_message_state = 0;
        private void frmMainForm_Load(object sender, EventArgs e)
        {
            SetControlText(lblChargeEntrance,"");
            SetControlText(lblChargeExit ,"");
            SetControlText(lblChargePrice ,"");
            SetControlText(lblChargePaid ,"");
            SetControlText(lblChargeMessage ,"");
            HideCaret(tbxLicencePlate.Handle);
            if(File.Exists (@"C:\server.txt")){
                using (System.IO.StreamReader file = new System.IO.StreamReader(@"C:\server.txt"))
                {
                    try
                    {
                        remote_server_address = file.ReadLine();
                        
                            remote_server_name = string.Concat(@"\\", file .ReadLine (), @"\");
                            connectionstring_remote = string.Concat(@"Data Source=",
                             remote_server_address, "\\SQLEXPRESS,1433;Network Library=DBMSSOCN;Initial Catalog=",
                              Properties.Resources.datatase_remote, ";User ID=", Properties.Resources.user_name_remote,
                              ";Password=", Properties.Resources.password_remote);
                        
                    }
                    catch (Exception)
                    {
                    }
                }
            }
           if(remote_server_address ==""){
               remote_server_address = "0.0.0.0";
           }
          connectionstring_remote = string.Concat(@"Data Source=",
           remote_server_address , "\\SQLEXPRESS,1433;Network Library=DBMSSOCN;Initial Catalog=",
            Properties.Resources.datatase_remote, ";User ID=", Properties.Resources.user_name_remote,
            ";Password=", Properties.Resources.password_remote);
            Panels.Add(pnlSpinner );
            Panels.Add(pnlLicencePlate );
            Panels.Add(pnlSettings );
            Panels.Add(pnlCharge );
            empty_license_plate_image = lblLicencePlate01.Image;
            ct_croatian.lbl_licence_plate = "REGISTRACIJA";
            ct_italian.lbl_licence_plate = "TARGA";
            ct_german.lbl_licence_plate = "KENNZEICHEN";
            ct_english.lbl_licence_plate = "LICENCE PLATE";
            ct_croatian.lbl_charge_entrance = "ULAZ: "; 
            ct_italian.lbl_charge_entrance = "ULAZ (IT): ";
            ct_german.lbl_charge_entrance = "ULAZ (GE): ";
            ct_english.lbl_charge_entrance = "ULAZ (EN): ";
            ct_croatian.lbl_charge_exit = "IZLAZ: ";
            ct_italian.lbl_charge_exit = "IZLAZ (IT): ";
            ct_german.lbl_charge_exit = "IZLAZ (GE): ";
            ct_english.lbl_charge_exit = "IZLAZ (EN): ";
            ct_croatian.lbl_charge_price = "UKUPNO ZA PLATITI: ";
            ct_italian.lbl_charge_price = "UKUPNO ZA PLATITI (IT): ";
            ct_german.lbl_charge_price = "UKUPNO ZA PLATITI (GE): ";
            ct_english.lbl_charge_price = "UKUPNO ZA PLATITI (EN): ";
            ct_croatian.lbl_charge_paid_cash = "UBAČENO NOVACA: ";
            ct_italian.lbl_charge_paid_cash = "UBAČENO NOVACA (IT): ";
            ct_german.lbl_charge_paid_cash = "UBAČENO NOVACA (GE): ";
            ct_english.lbl_charge_paid_cash = "UBAČENO NOVACA (EN): ";
            ct_croatian.lbl_charge_paid_card = "PLAĆANJE KARTICOM U TIJEKU";
            ct_italian.lbl_charge_paid_card = "PLAĆANJE KARTICOM U TIJEKU (IT)";
            ct_german.lbl_charge_paid_card = "PLAĆANJE KARTICOM U TIJEKU (GE)";
            ct_english.lbl_charge_paid_card = "PLAĆANJE KARTICOM U TIJEKU (EN)";
            ct_croatian.lbl_message_card_error = "KARTICA JE ODBIJENA!";
            ct_italian.lbl_message_card_error = "CARTA RIFIUTATO!";
            ct_german.lbl_message_card_error = "KARTE ABGELEHNT!";
            ct_english.lbl_message_card_error = "CARD DECLINED!";
            ct_croatian.lbl_message_cash_error = "VELIKA NOVČANICA!";
            ct_italian.lbl_message_cash_error = "VELIKA NOVČANICA! (IT)";
            ct_german.lbl_message_cash_error = "VELIKA NOVČANICA! (GE)";
            ct_english.lbl_message_cash_error = "VELIKA NOVČANICA! (EN)";
            ct_croatian.cancel = "ODUSTANI";
            ct_italian.cancel = "CANCELLARE";
            ct_german.cancel = "STORNIEREN";
            ct_english.cancel = "CANCEL";
            SetLanguage(ct_croatian );
            ct_default = ct_croatian;
           _check_server .server_checked +=new check_server.ServerCheckedHandler(server_checked);  
            _check_server .server_found +=new check_server.ServerFoundHandler(server_found);
            check_server_thread.Start();
            SetControlText(tbxSettingsServer ,remote_server_address .ToString ());
            _logger_comm.logger_checked += new logger_comm.LoggerCheckedHandler(_logger_comm_logger_checked);
            _logger_comm.logger_cash += new logger_comm.LoggerCashHandler(_logger_comm_logger_cash);
            _logger_comm.logger_credit += new logger_comm.LoggerCreditHandler(_logger_comm_logger_credit);
            _logger_comm.logger_error += new logger_comm.LoggerErrorHandler(_logger_comm_logger_error);
            _logger_comm.logger_report += new logger_comm.LoggerReportHandler(_logger_comm_logger_report);
            logger_comm_thread.Start();
            _check_payment.payment_checked += new check_payment.PaymentCheckedHandler(_check_payment_payment_checked);
            check_payment_thread.Start();
            licence_plate_panel_timer = new System.Timers.Timer();
            licence_plate_panel_timer.Interval = 1000;
            licence_plate_panel_timer.Elapsed += new System.Timers.ElapsedEventHandler(licence_plate_panel_timer_Elapsed);
             last_price = new Cijena(0, last_price_cj,0,0,0 );
        }
        void _logger_comm_logger_report(object myObject, logger_comm.logger_report_args logger_report_sent_Args)
        {
            PrintDialog pd = new PrintDialog();
            pd.PrinterSettings = new PrinterSettings();
            Int32 dwCount = logger_report_sent_Args.LoggerReport.Length;
            IntPtr print = Marshal.AllocCoTaskMem((int)logger_report_sent_Args.LoggerReport.Length);
            Marshal.Copy(logger_report_sent_Args.LoggerReport, 0, print, (int)logger_report_sent_Args.LoggerReport.Length);
            RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

        }

        void _logger_comm_logger_error(object myObject, logger_comm.logger_error_args logger_error_sent_Args)
        {
            lbl_charge_message_state = logger_error_sent_Args.LoggerError;
            switch (logger_error_sent_Args .LoggerError ){
                case 1:
                    credit_paid.Clear();
                    SetControlText(lblChargeMessage ,ct_default .lbl_message_card_error );
                    break;
                case 2:
                    SetControlText(lblChargeMessage ,ct_default .lbl_message_cash_error );
                    break ;
            }
        }

        void licence_plate_panel_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lpp_count++;
            if(lpp_count ==5){
                licence_plate_panel_timer.Stop();
                SetControlText(tbxLicencePlate ,"");
                SetControlText(lblChargeEntrance ,"");
                SetControlText(lblChargeExit ,"");
                SetControlText(lblChargeLicensePlate ,"");
                SetControlText(lblChargeMessage ,"");
                SetControlText(lblChargePaid ,"");
                DisplayPanel(Panels ,pnlLicencePlate );
                last_panel = 1;
                lpp_count = 0;
            }
        }

        void _logger_comm_logger_credit(object myObject, logger_comm.logger_credit_args logger_credit_sent_Args)
        {
            lbl_charge_message_state = 0;
            SetControlText(lblChargeMessage, "");
            lbl_charge_paid_state = 2;
            SetControlText(lblChargePaid ,ct_default .lbl_charge_paid_card );
            _check_payment.checking_payment = false;
            _check_payment.check_payment_interval = 0;
            credit_paid.Add(logger_credit_sent_Args .LoggerCredit );
            if(credit_paid .Count ==9){
                PrintLoggerList(credit_paid );
                credit_paid.Clear();
                int slijedID = SelectMaxSlijedID() + 1;
                int racun = InsertRacun(slijedID, DateTime.Now, last_price, false, 'K');
                UpdateParkVehiclePaid(last_price .parkvehicleID ,DateTime .Now .AddMinutes (last_price .cjenik.minute_za_izlaz ));
                last_price.ukupno = 0;
                last_sum_paid = 0;
                lpp_count = 0;
                SetControlText(lblChargeMessage ,"ZAHVALJUJEMO NA UPLATI");
                licence_plate_panel_timer.Start();
                SetControlEnabled(btnChargeCancel, false);
            }
        }

        void _logger_comm_logger_cash(object myObject, logger_comm.logger_cash_args logger_cash_sent_Args)
        {
            
            last_sum_paid += logger_cash_sent_Args.LoggerCash;
            lbl_charge_paid_state = 1;
            SetControlText(lblChargePaid ,string .Concat (ct_default .lbl_charge_paid_cash ,last_sum_paid .ToString ()," KN"));
            lbl_charge_message_state = 0;
            SetControlText(lblChargeMessage, "");

            if(last_sum_paid >= Convert .ToInt32 ( last_price.ukupno  )){

                
                int slijedID = SelectMaxSlijedID() + 1;
                int racun = InsertRacun(slijedID, DateTime.Now, last_price ,  false, 'N');
                UpdateParkVehiclePaid(last_price .parkvehicleID ,DateTime .Now.AddMinutes (last_price .cjenik .minute_za_izlaz ));
                PrintRacun(racun );
                lpp_count = 0;
                SetControlText(lblChargeMessage, "ZAHVALJUJEMO NA UPLATI");
                licence_plate_panel_timer.Start();
                SetControlEnabled(btnChargeCancel ,false );
                last_sum_paid = 0;
                last_price.ukupno  = 0;
                _check_payment.checking_payment = false;
            }
            _check_payment.check_payment_interval = 0;
        }

        void _check_payment_payment_checked(object myObject, check_payment.payment_checked_args payment_checked_sent_Args)
        {
            credit_paid.Clear();
            last_price.ukupno  = 0;
            last_sum_paid = 0;
            _logger_comm.charge_price = 0;
            _logger_comm .charging =true   ;
            SetControlText(tbxLicencePlate ,"");
            DisplayPanel(Panels ,pnlLicencePlate );
            last_panel = 1;
        }

        void _logger_comm_logger_checked(object myObject, logger_comm.logger_checked_args logger_checked_sent_Args)
        {
           logger_OK = logger_checked_sent_Args.LoggerOK;
           
            if (logger_OK )
            {
                if (server_OK )
                {
                    SetControlText(lblSettingsServerError, "");
                   
                    switch (last_panel ){
                        case 1:
                            DisplayPanel(Panels ,pnlLicencePlate );
                            SetTextBoxSelection(tbxLicencePlate);
                            mainform_keypress = false;
                            HideTextBoxCaret(tbxLicencePlate );
                            break;
                        case 2:
                           
                            DisplayPanel(Panels ,pnlCharge );
                            mainform_keypress = false;
                            break;
                        case 3:
                            DisplayPanel(Panels ,pnlSettings );
                            break;
                    }
                }
            }
            else
            {
                
                if (last_panel !=3)
                {

                    mainform_keypress = true;
                    DisplayPanel(Panels, pnlSpinner);
                }
            }
        }

       
        public void SetLanguage(ControlsTexts ct)
        {
            SetControlText(lblLicencePlate ,ct.lbl_licence_plate );
            SetControlText(btnChargeCancel ,ct.cancel );
            if(pnlCharge .Visible && lblChargeEntrance .Text!=""){
                SetControlText(lblChargeEntrance ,string .Concat (ct.lbl_charge_entrance ,lblChargeEntrance .Text.Substring (lblChargeEntrance .Text.IndexOf (':')+2)));
            }
            if (pnlCharge.Visible && lblChargeExit.Text != "")
            {
                SetControlText(lblChargeExit, string.Concat(ct.lbl_charge_exit, lblChargeExit.Text.Substring(lblChargeExit.Text.IndexOf(':') + 2)));
            }

            if (pnlCharge.Visible && lblChargePrice.Text != "")
            {
                SetControlText(lblChargePrice, string.Concat(ct.lbl_charge_price, lblChargePrice.Text.Substring(lblChargePrice.Text.IndexOf(':') + 2)));
            }

            if (pnlCharge.Visible)
            {
                switch (lbl_charge_paid_state ){
                    case 0:
                        SetControlText(lblChargePaid ,"");
                        break;
                    case 1:
                SetControlText(lblChargePaid, string.Concat(ct.lbl_charge_paid_cash, lblChargePaid.Text.Substring(lblChargePaid.Text.IndexOf(':') + 2)));
                break;
                    case 2:
                SetControlText(lblChargePaid ,ct.lbl_charge_paid_card );
                break;
                }
                switch (lbl_charge_message_state ){
                    case 0:
                        SetControlText(lblChargeMessage ,"");
                        break;
                    case 1:
                        SetControlText(lblChargeMessage ,ct.lbl_message_card_error );
                        break;
                    case 2:
                        SetControlText(lblChargeMessage ,ct.lbl_message_cash_error );
                        break;
                }
            }
        }
      public   void server_checked(object myObject, check_server .server_checked_args _args)
        {
           
            server_OK = _args.ServerOK;
            if (server_OK)
            {
                if (logger_OK ){
                SetControlText(lblSettingsServerError ,"");
               switch (last_panel ){
                   case 1:
                       DisplayPanel( Panels , pnlLicencePlate );
                       SetTextBoxSelection(tbxLicencePlate);
                       HideTextBoxCaret(tbxLicencePlate);
                       break;
                   case 2:
                       DisplayPanel(Panels ,pnlCharge );
                       break;
                   case 3:
                       DisplayPanel(Panels ,pnlSettings );
                       break;
               }
                }
            }
            else
            {
                 SetControlText(lblSettingsServerError, "!!!");
                if (last_panel !=3)
                {
                    
                    mainform_keypress = true;
                    DisplayPanel(Panels, pnlSpinner);
                }
            }
            
            
        }
      public void server_found(object myObject, check_server.server_found_args _args)
      {
          
          
          if (_args.serverName != null)
          {
              remote_server_address = _args.ServerFound;
              
              remote_server_name = string.Concat(@"\\", _args.serverName.Trim(), @"\");
              connectionstring_remote = string.Concat(@"Data Source=",
               remote_server_address, "\\SQLEXPRESS,1433;Network Library=DBMSSOCN;Initial Catalog=",
                Properties.Resources.datatase_remote, ";User ID=", Properties.Resources.user_name_remote,
                ";Password=", Properties.Resources.password_remote);

              server_OK = true;

              SetControlText(tbxSettingsServer, remote_server_address.ToString());
              SetControlText(lblSettingsServerError, "");
              if (!pnlSettings.Visible)
              {
                  DisplayPanel(Panels, pnlLicencePlate);
                  last_panel = 1;
                  SetTextBoxSelection(tbxLicencePlate);
                  HideTextBoxCaret(tbxLicencePlate);
                  mainform_keypress = false;
                 // SetControlFocus(tbxLicencePlate);
              }

          }
          else
          {
              mainform_keypress = true;
          }
          if (!File.Exists(@"C:\server.txt"))
          {
              try
              {
                FileStream  f =File.Create(@"C:\server.txt");
                  f.Close();
              }
              catch (Exception ){
              }
          }
          if (File.Exists(@"C:\server.txt"))
          {
              try
              {
                  using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\server.txt"))
                  {
                      try
                      {
                          file.WriteLine(remote_server_address);
                          file.WriteLine(_args.serverName.Trim());

                      }
                      catch (Exception)
                      {
                      }
                  }
              }
              catch (Exception ){
              }
          }
      }
        public void DisplayPanel(List <Panel >p,Panel d)
        {
            foreach (Panel pnl in p)
            {
                if (pnl.Name == d.Name)
                {
                    SetControlVisible(pnl, true);
                }
                else
                {
                    SetControlVisible(pnl ,false );
                }
            }
        }
        public void UpdateParkVehiclePaid(int ID, DateTime dateto)
        {

            using (SqlConnection conn = new SqlConnection(connectionstring_remote))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateParkVehiclePaid", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", ID);
                    cmd.Parameters.AddWithValue("@DateTo", dateto);
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception)
                    {

                    }
                }
            }


        }
        public void DisplayLicensePlateCharge(int ID)
        {
            SetControlEnabled(btnChargeCancel ,true  );
            Cjenik c = SelectCjenik();
            last_price.cjenik = c;
            DateTime entrance = new DateTime();
            DateTime exit = DateTime.Now;
            using (SqlConnection conn = new SqlConnection(connectionstring_remote))
            {
                using (SqlCommand cmd = new SqlCommand("SelectParkVehicleById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", ID);
                    try
                    {
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                try
                                {

                                    entrance = Convert.ToDateTime(dr["DateFrom"]);
                                    SetLabelImage(lblChargeLicensePlate, Image.FromFile(string.Concat(@"\\", remote_server_name, @"\", Convert.ToString(dr["LicensePlateImageURI"]))));
                                    SetControlText(lblChargeEntrance, string.Concat(ct_default.lbl_charge_entrance, entrance.ToString("dd.MM.yyyy. HH:mm:ss")));
                                    SetControlText(lblChargeExit, string.Concat(ct_default.lbl_charge_exit, exit.ToString("dd.MM.yyyy. HH:mm:ss")));


                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            last_price = IzracunCijene(entrance, exit, c, ID);
            SetControlText(lblChargePrice, string.Concat(ct_default.lbl_charge_price, last_price.ukupno.ToString(), " KN"));
           if(last_price .ukupno >0){
            _logger_comm.charge_price = Convert .ToInt32 (last_price .ukupno );
            _logger_comm.charging = true;
          
            lbl_charge_paid_state = 0;
            SetControlText(lblChargePaid ,"");
            lbl_charge_message_state = 0;
            SetControlText(lblChargeMessage ,"");
           }
           else{
               SetControlText(lblChargeMessage ,"SLOBODAN IZLAZ");
               lpp_count = 0;
               licence_plate_panel_timer.Start();
               UpdateParkVehiclePaid(last_price.parkvehicleID, DateTime.Now.AddMinutes(last_price.cjenik.minute_za_izlaz));
               }
        }
        public void DisplayLicensePlates(int start)
        {
           if(LicensesPlateList .Count >0){
                SetLabelImage(lblLicencePlate01, LicensesPlateList[start].licence_plate_image);
          
            SetControlText(lblEntranceDateTime01, LicensesPlateList[start].date_time_from.ToString("dd.MM.yyyy. HH:mm:ss"));
            SetControlVisible(lblLicencePlate01, true);
            SetControlVisible(lblEntranceDateTime01, true);
            if (LicensesPlateList.Count > start + 1) {
                SetLabelImage(lblLicencePlate02, LicensesPlateList[start+1].licence_plate_image);
                SetControlText(lblEntranceDateTime02, LicensesPlateList[start+1].date_time_from.ToString("dd.MM.yyyy. HH:mm:ss"));
                SetControlVisible(lblLicencePlate02, true);
                SetControlVisible(lblEntranceDateTime02, true);
            }
            else
            {
                SetLabelImage(lblLicencePlate02, empty_license_plate_image);
                SetControlText(lblEntranceDateTime02 ,"");
                SetControlVisible(lblLicencePlate02, false );
                SetControlVisible(lblEntranceDateTime02, false );
            }
            if (LicensesPlateList.Count > start + 2)
            {
                SetLabelImage(lblLicencePlate03, LicensesPlateList[start + 2].licence_plate_image);
                SetControlText(lblEntranceDateTime03, LicensesPlateList[start+2].date_time_from.ToString("dd.MM.yyyy. HH:mm:ss"));
                SetControlVisible(lblLicencePlate03, true);
                SetControlVisible(lblEntranceDateTime03, true);

            }
            else
            {
                SetLabelImage(lblLicencePlate03, empty_license_plate_image);
                SetControlText(lblEntranceDateTime03 ,"");
                SetControlVisible(lblLicencePlate03, false );
                SetControlVisible(lblEntranceDateTime03, false );
            }

            if (start > 0 && LicensesPlateList.Count > 3)
            {
                SetControlEnabled(btnLicencePlateUp, true);
            }
            else
            {
                SetControlEnabled(btnLicencePlateUp ,false );
            }
            if (start + 3< LicensesPlateList.Count && LicensesPlateList.Count > 3)
            {
                SetControlEnabled(btnLicencePlateDown, true);
            }
            else
            {
                SetControlEnabled(btnLicencePlateDown ,false );
            }
           }
           // SetControlFocus(tbxLicencePlate );
        }
        public void SelectParkVehicleByLicensePlate(string license_plate)
        {


            using (SqlConnection conn = new SqlConnection(connectionstring_remote))
                {
                    using (SqlCommand cmd = new SqlCommand("SelectParkVehicleByLicensePlate", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@license_plate", license_plate );
                        try
                        {
                            conn.Open();
                            SqlDataReader dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    try
                                    {
                                      
                                        LicensesPlateList.Add(new LicensePlate(Convert.ToInt32(dr["Id"]), Image.FromFile(string .Concat ( @"\\",remote_server_name ,@"\",Convert .ToString (dr["LicensePlateImageURI"]))), Convert.ToString(dr["LicensePlate"]), Convert.ToDateTime(dr["DateFrom"])));
                                    }
                                    catch (Exception ){
                                    }
                                    }
                            }
                            conn.Close();
                        }
                        catch (Exception )
                        {
                           
                        }
                    }
                }
           
            
        }

        public int SelectMaxSlijedID()
        {
            int res = 0;
            using (SqlConnection conn = new SqlConnection(connectionstring_remote))
            {
                using (SqlCommand cmd = new SqlCommand("SelectMaxSlijedID", conn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PoslovniProstorID",Properties .Resources .poslovni_prostor );
                    cmd .Parameters .AddWithValue ("@NaplatniUredjajID",Properties .Resources.naplatni_uredjaj );
                    cmd.Parameters.AddWithValue("@year",DateTime.Now .Year );
                    try
                    {
                        conn.Open();

                        if(cmd.ExecuteScalar ()!=DBNull .Value ){
                        res = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        conn.Close();
                    }
                    catch (Exception )
                    {
                        
                        res = -1;

                    }
                }

            }
            return res;
        }
        public int InsertRacun(int slijedID, DateTime vrijeme, Cijena c,Boolean naknadnafiskalizacija,
            char nacinplacanja)
        {
            int res = 0;
            using (SqlConnection conn = new SqlConnection(connectionstring_remote))
            {
                using (SqlCommand cmd = new SqlCommand("InsertRacun", conn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@slijedID",slijedID );
                    cmd.Parameters.AddWithValue("@vrijeme", vrijeme);
                    cmd.Parameters.AddWithValue("@poslovniprostorID", Properties .Resources.poslovni_prostor );
                    cmd.Parameters.AddWithValue("@naplatniuredjajID", Properties .Resources.naplatni_uredjaj );
                    cmd.Parameters.AddWithValue("@cijeneID", c.cjenik .ID  );
                    cmd.Parameters.AddWithValue("@porezstopa", c.cjenik .PDV  );
                    cmd.Parameters.AddWithValue("@porezosnovica", c.porez_osnovica );
                    cmd.Parameters.AddWithValue("@poreziznos", c.porez_iznos );
                    cmd.Parameters.AddWithValue("@ukupno", c.ukupno  );
                    cmd.Parameters.AddWithValue("@naknadnafiskalizacija", naknadnafiskalizacija);
                    cmd.Parameters.AddWithValue("@nacinplacanja", nacinplacanja);
                    cmd.Parameters.AddWithValue("@parkvehicleID",c.parkvehicleID );
                    cmd.Parameters.AddWithValue("@sati", c.sati);
                    cmd.Parameters.AddWithValue("@dani", c.dani);
                    
                    try
                    {
                        conn.Open();
                       

                        res = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception ex )
                    {
                    
                        res = -1;

                    }
                }

            }
            return res;
        }
        public Cjenik SelectCjenik()
        {
            Cjenik res = new Cjenik(0, 0.00m, 0.00m, 0.00m, 0, 0,0);

            using (SqlConnection conn = new SqlConnection(connectionstring_remote))
            {
                using (SqlCommand cmd = new SqlCommand("SelectCjenik", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                try
                                {

                                    res.ID = Convert.ToInt32(dr["ID"]);
                                    res.PDV += Convert.ToDecimal(dr["PDV"]);
                                    res.cijena_sat += Convert.ToDecimal(dr["cijena_sat"]);
                                    res.cijena_dan += Convert.ToDecimal(dr["cijena_dan"]);
                                    res.besplatne_minute = Convert.ToInt32(dr["besplatne_minute"]);
                                    res.max_sati = Convert.ToInt32(dr["max_sati"]);
                                    res.minute_za_izlaz = Convert.ToInt32(dr["minute_za_izlaz"]);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return res;

        }

        public void PrintRacun(int racun_ID)
        {
            string slijedID = "";
            Boolean dataOK = true;
            string OIB = "";
            string adresa = "";
            string total = "";
            string tax = "";
            string vrijeme = "";
            string header_01 = "";
            string header_02 = "";
            string footer_01 = "";
            string footer_02 = "";
            int sati = 0;
            int dani = 0;
            decimal sat_cijena = 0.00m;
            decimal dan_cijena = 0.00m;
            string plate = "";
            using (SqlConnection conn = new SqlConnection(connectionstring_remote))
            {
                using (SqlCommand cmd = new SqlCommand("SelectPoslovniProstorByID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", Properties .Resources.poslovni_prostor );
                    try
                    {
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    adresa = string.Concat(Convert.ToString (dr["Ulica"]).Trim (), " ",
                                        Convert .ToString (dr["KucniBroj"]).Trim ()," ");
                                    if(  (dr["KucniBrojDodatak"])!=DBNull .Value ){
                                        adresa = string.Concat(adresa ,Convert .ToString (dr["KucniBrojDodatak"]).Trim ()," ");
                                    }
                                    adresa = string.Concat(adresa ,Convert.ToString (dr["PostanskiBroj"]).Trim ()," ",
                                        Convert .ToString (dr["Naselje"])).PadRight (45);
                                    OIB = Convert.ToString(dr["OIB"]);
                                }
                                catch (Exception  )
                                {
                                    
                                    dataOK = false;
                                }
                            }
                        }
                        else
                        {
                            dataOK = false;
                        }
                        conn.Close();
                       
                    }
                    catch (Exception )
                    {
                        dataOK = false;

                    }

                    //
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SelectRacunByID";
                    cmd.Parameters.AddWithValue("@ID",racun_ID );
                    try
                    {
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                   // specifikacija = Convert.ToString(dr["specifikacija"]);
                                    total = string.Concat("UKUPNO: ",Convert .ToString (dr["Ukupno"])," KN");
                                    tax = string.Concat("PDV          ",Convert.ToString (dr["PorezStopa"]).PadRight (8),
                                        Convert.ToString (dr["PorezOsnovica"]).PadRight (13),Convert .ToString (dr["PorezIznos"]));
                                    vrijeme = string.Concat("RACUN IZDAN: ", Convert.ToDateTime(dr["Vrijeme"]).ToString("dd.MM.yyyy HH:mm:ss"));
                                    sati = Convert.ToInt32(dr["sati"]);
                                    dani = Convert.ToInt32(dr["dani"]);
                                    sat_cijena = Convert.ToDecimal(dr["cijena_sat"]);
                                    dan_cijena = Convert.ToDecimal(dr["cijena_dan"]);
                                    plate = string.Concat("ID: ",Convert .ToString (dr["ParkVehicleID"])," ", Convert .ToString (dr["LicensePlate"])).PadRight (45);
                                    slijedID = Convert.ToString(dr["slijedID"]);
                                }
                                catch (Exception)
                                {

                                    dataOK = false;
                                }
                            }
                        }
                        else
                        {
                            dataOK = false;
                        }
                        conn.Close();

                    }
                    catch (Exception)
                    {
                        dataOK = false;

                    }
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SelectRacunTekstByNM";
                    cmd.Parameters.AddWithValue("@PoslovniProstorID",Properties .Resources.poslovni_prostor );
                    cmd.Parameters.AddWithValue("@NaplatniUredjajID",Properties .Resources.naplatni_uredjaj );
                    try
                    {
                        conn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    if (dr["Header01"] != DBNull.Value)
                                    {
                                       header_01 = Convert.ToString(dr["Header01"]);
                                   }
                                   if (dr["Header02"] != DBNull.Value)
                                   {
                                       header_02 = Convert.ToString(dr["Header02"]);
                                   }
                                   if (dr["Footer01"] != DBNull.Value)
                                   {
                                       footer_01 = Convert.ToString(dr["Footer01"]);
                                   }
                                   if (dr["Footer02"] != DBNull.Value)
                                   {
                                       footer_02 = Convert.ToString(dr["Footer02"]);
                                   }
                                }
                                catch (Exception)
                                {

                                    dataOK = false;
                                }
                            }
                        }
                        
                        conn.Close();

                    }
                    catch (Exception)
                    {
                        dataOK = false;

                    }
                }
            }
            if(dataOK ){
                byte[] header_01_bytes = new byte[45];
                byte[] header_02_bytes = new byte[45];
                byte[] footer_01_bytes = new byte[45];
                byte[] footer_02_bytes = new byte[45];
                byte[] sati_bytes = new byte[45];
                byte[] dani_bytes = new byte[45];
                byte[] plate_bytes = new byte[45];
                for (int i = 0; i < header_01_bytes.Length; i++)
                {
                    header_01_bytes[i] = 0x00;
                    header_02_bytes[i] = 0x00;
                    footer_01_bytes[i] = 0x00;
                    footer_02_bytes[i] = 0x00;
                    sati_bytes[i] = 0x00;
                    dani_bytes[i] = 0x00;
                }
                if(header_01 !=""){
                    header_01_bytes = Encoding.ASCII.GetBytes(header_01 .PadRight (45));
                    header_01_bytes[42] = 0x0a;
                    header_01_bytes[43] = 0x0d;
                    header_01_bytes[44] = 0x0a;
                }
                if (header_02 != "")
                {
                    header_02_bytes = Encoding.ASCII.GetBytes(header_02.PadRight(45));
                    header_02_bytes[42] = 0x0a;
                    header_02_bytes[43] = 0x0d;
                    header_02_bytes[44] = 0x0a;
                }
                if (footer_01 != "")
                {
                    footer_01_bytes = Encoding.ASCII.GetBytes(footer_01.PadRight(45));
                    footer_01_bytes[42] = 0x0a;
                    footer_01_bytes[43] = 0x0d;
                    footer_01_bytes[44] = 0x0a;
                }
                if (footer_02 != "")
                {
                    footer_02_bytes = Encoding.ASCII.GetBytes(footer_02.PadRight(45));
                    footer_02_bytes[42] = 0x0a;
                    footer_02_bytes[43] = 0x0d;
                    footer_02_bytes[44] = 0x0a;
                }
                if(sati>0){
                    sati_bytes = Encoding.ASCII.GetBytes(string .Concat ("SATI         ", sati.ToString ().PadRight (8),sat_cijena .ToString ().PadRight (13), Convert .ToString (sat_cijena * sati )).PadRight (45));
                    sati_bytes[42] = 0x0a;
                    sati_bytes[43] = 0x0d;
                    sati_bytes[44] = 0x0a;
                }
                if (dani > 0)
                {
                    dani_bytes = Encoding.ASCII.GetBytes(string.Concat("DANI         ", dani.ToString().PadRight(8), dan_cijena.ToString().PadRight(13), Convert.ToString(dan_cijena * dani)).PadRight(45));
                    dani_bytes[42] = 0x0a;
                    dani_bytes[43] = 0x0d;
                    dani_bytes[44] = 0x0a;
                }
                plate_bytes = Encoding.ASCII.GetBytes(plate );
                plate_bytes[42] = 0x0a;
                plate_bytes[43] = 0x0d;
                plate_bytes[44] = 0x0a;
                PrintDialog pd = new PrintDialog();
                pd.PrinterSettings = new PrinterSettings();
                Int32 dwCount = header_01_bytes.Length;
                IntPtr print = Marshal.AllocCoTaskMem((int)header_01_bytes.Length);
                Marshal.Copy(header_01_bytes, 0, print, (int)header_01_bytes.Length);
                RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

                 dwCount = header_02_bytes.Length;
                 print = Marshal.AllocCoTaskMem((int)header_02_bytes.Length);
                Marshal.Copy(header_02_bytes, 0, print, (int)header_02_bytes.Length);
                RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

                byte [] adresa_bytes=new byte [45];
                adresa_bytes = Encoding.ASCII.GetBytes(adresa );
                adresa_bytes[42] = 0x0a;
                adresa_bytes[43] = 0x0d;
                adresa_bytes[44] = 0x0a;
                 dwCount = adresa_bytes.Length;
                 print = Marshal.AllocCoTaskMem((int)adresa_bytes.Length);
                Marshal.Copy(adresa_bytes, 0, print, (int)adresa_bytes.Length);
                RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
                byte[] OIB_bytes = new byte[19];
                OIB_bytes = Encoding.ASCII.GetBytes( string .Concat ("OIB: ", OIB).PadRight(19));
                OIB_bytes[16] = 0x0a;
                OIB_bytes[17] = 0x0d;
                OIB_bytes[18] = 0x0a;
                 dwCount = OIB_bytes.Length;
                 print = Marshal.AllocCoTaskMem((int)OIB_bytes.Length);
                Marshal.Copy(OIB_bytes, 0, print, (int)OIB_bytes.Length);
                RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              byte[]  racun_bytes = new byte[45];
              racun_bytes = Encoding.ASCII.GetBytes(string .Concat ("BROJ RACUNA: ",slijedID ,"/",
                  Properties .Resources .poslovni_prostor ,"/",Properties .Resources.naplatni_uredjaj ).PadRight (45));
              racun_bytes[42] = 0x0a;
              racun_bytes[43] = 0x0d;
              racun_bytes[44] = 0x0a;
              dwCount = racun_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)racun_bytes.Length);
              Marshal.Copy(racun_bytes, 0, print, (int)racun_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

              byte[] vrijeme_bytes = new byte[45];
              vrijeme_bytes = Encoding.ASCII.GetBytes(vrijeme.PadRight(45));
              vrijeme_bytes[42] = 0x0a;
              vrijeme_bytes[43] = 0x0d;
              vrijeme_bytes[44] = 0x0a;
              dwCount = vrijeme_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)vrijeme_bytes.Length);
              Marshal.Copy(vrijeme_bytes, 0, print, (int)vrijeme_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              byte[] nacin_bytes = new byte[44];
              nacin_bytes = Encoding.ASCII.GetBytes("NACIN PLACANJA: GOTOVINA (NOVCANICE)".PadRight (45));
              nacin_bytes[42] = 0x0a;
              nacin_bytes[43] = 0x0d;
              nacin_bytes[44] = 0x0a;
              dwCount = nacin_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)nacin_bytes.Length);
              Marshal.Copy(nacin_bytes, 0, print, (int)nacin_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

              dwCount = plate_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)plate_bytes.Length);
              Marshal.Copy(plate_bytes, 0, print, (int)plate_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              byte[] spec_title_bytes = new byte[44];
              spec_title_bytes = Encoding.ASCII.GetBytes("NAZIV        KOL     CIJENA       IZNOS".PadRight (44));
              spec_title_bytes[42] = 0x0a;
              spec_title_bytes[43] = 0x0d;
              dwCount = spec_title_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)spec_title_bytes.Length);
              Marshal.Copy(spec_title_bytes, 0, print, (int)spec_title_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              byte[] line_bytes = new byte[44];
              line_bytes = Encoding.ASCII.GetBytes("".PadRight(44, '-'));
              line_bytes[42] = 0x0a;
              line_bytes[43] = 0x0d;
              dwCount = line_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)line_bytes.Length);
              Marshal.Copy(line_bytes, 0, print, (int)line_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
            //
              dwCount = sati_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)sati_bytes.Length);
              Marshal.Copy(sati_bytes, 0, print, (int)sati_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

              dwCount = dani_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)dani_bytes.Length);
              Marshal.Copy(dani_bytes, 0, print, (int)dani_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              dwCount = line_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)line_bytes.Length);
              Marshal.Copy(line_bytes, 0, print, (int)line_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              byte[] total_bytes = new byte[44];
              total_bytes = Encoding.ASCII.GetBytes(total.PadRight(45));
              total_bytes[42] = 0x0a;
              total_bytes[43] = 0x0d;
              total_bytes[44] = 0x0a;
              dwCount = total_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)total_bytes.Length);
              Marshal.Copy(total_bytes, 0, print, (int)total_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
                byte [] tax_title_bytes=new byte [44];
                tax_title_bytes =Encoding .ASCII .GetBytes ("VRSTA        STOPA   OSNOVICA     IZNOS".PadRight (44));
                tax_title_bytes[42] = 0x0a;
                tax_title_bytes[43] = 0x0d;
                dwCount = tax_title_bytes.Length;
                print = Marshal.AllocCoTaskMem((int)tax_title_bytes.Length);
                Marshal.Copy(tax_title_bytes, 0, print, (int)tax_title_bytes.Length);
                RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              dwCount = line_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)line_bytes.Length);
              Marshal.Copy(line_bytes, 0, print, (int)line_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              byte[] tax_bytes = new byte[44];
              tax_bytes = Encoding.ASCII.GetBytes(tax.PadRight(44));
              tax_bytes[42] = 0x0a;
              tax_bytes[43] = 0x0d;
              dwCount = tax_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)tax_bytes.Length);
              Marshal.Copy(tax_bytes, 0, print, (int)tax_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
              dwCount = line_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)line_bytes.Length);
              Marshal.Copy(line_bytes, 0, print, (int)line_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

               dwCount = footer_01_bytes.Length;
               print = Marshal.AllocCoTaskMem((int)footer_01_bytes.Length);
               Marshal.Copy(footer_01_bytes, 0, print, (int)footer_01_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

              dwCount = footer_02_bytes.Length;
              print = Marshal.AllocCoTaskMem((int)footer_02_bytes.Length);
              Marshal.Copy(footer_02_bytes, 0, print, (int)footer_02_bytes.Length);
              RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);

                byte[] end_bytes = new byte[]{0x0a, 0x1b, 0x64, 0x04,
                         0x1b,0x69,0x04, 0x0c, 0x04 
            };
                dwCount = end_bytes.Length;
                print = Marshal.AllocCoTaskMem((int)end_bytes.Length);
                Marshal.Copy(end_bytes, 0, print, (int)end_bytes.Length);
                RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
            }
        }
        public void PrintLoggerList(List <string > lines)
        {
            PrintDialog pd = new PrintDialog();
            pd.PrinterSettings = new PrinterSettings();
            
            byte[] line_bytes = new byte[44];
            Int32 dwCount = line_bytes.Length;
            IntPtr print = Marshal.AllocCoTaskMem((int)line_bytes.Length);

            for (int i = 0; i < lines.Count; i++)
            {
               line_bytes = Encoding.ASCII.GetBytes(lines[i].PadRight (44));
                line_bytes[42] = 0x0a;
                line_bytes[43] = 0x0d;
                Marshal.Copy(line_bytes, 0, print, (int)line_bytes.Length);
                RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
           }
            byte[] end_bytes = new byte[]{0x0a, 0x1b, 0x64, 0x04,
                         0x1b,0x69,0x04, 0x0c, 0x04 
            };
            dwCount = end_bytes.Length;
            print = Marshal.AllocCoTaskMem((int)end_bytes.Length);
            Marshal.Copy(end_bytes, 0, print, (int)end_bytes.Length);
            RawPrinterHelper.SendBytesToPrinter(pd.PrinterSettings.PrinterName, print, dwCount);
        }
        public Cijena IzracunCijene(DateTime from, DateTime to, Cjenik c, int pv_ID)
        {
            Cijena res = new Cijena(0.00m, c, pv_ID, 0, 0);
            if (DateTime.Compare(from.AddMinutes(c.besplatne_minute), to) > 0)
            {
                return res;
            }
            else
            {
                TimeSpan sp = to - from;
                int hours = (int)sp.TotalHours + 1;

                int total_hours_parked = 0;
                int total_days_parked = 0;

                if (hours < c.max_sati)
                {
                    res.ukupno = Math.Round((hours * c.cijena_sat), 2);
                    res.sati = hours;
                    res.dani = 0;
                }
                else
                {
                    while (hours > 0)
                    {
                        if (hours > c.max_sati)
                        {
                            total_days_parked++;
                        }
                        else
                        {
                            total_hours_parked = hours;
                        }
                        hours -= 24;
                    }

                    res.dani = total_days_parked;
                    res.sati = total_hours_parked;

                    res.ukupno = Math.Round((total_days_parked * c.cijena_dan + total_hours_parked * c.cijena_sat), 2);
                }
                res.porez_osnovica = Math.Round(((res.ukupno / (c.PDV + 100)) * 100), 2);
                res.porez_iznos = Math.Round(res.ukupno - res.porez_osnovica, 2);

                using (SqlConnection conn = new SqlConnection(connectionstring_remote))
                {
                    using (SqlCommand cmd = new SqlCommand("SelectRacunByParkVehicleID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@parkedvehicleID", pv_ID);
                        try
                        {
                            conn.Open();
                            SqlDataReader dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    try
                                    {

                                        res.ukupno -= Convert.ToDecimal(dr["Ukupno"]);
                                        res.porez_iznos -= Convert.ToDecimal(dr["PorezIznos"]);
                                        res.porez_osnovica -= Convert.ToDecimal(dr["PorezOsnovica"]);
                                        res.sati -= Convert.ToInt32(dr["sati"]);
                                        res.dani -= Convert.ToInt32(dr["dani"]);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            conn.Close();
                        }
                        catch (Exception)
                        {

                        }
                    }
                }


            }

            res.porez_osnovica = Math.Round(((res.ukupno / (c.PDV + 100)) * 100), 2);
            res.porez_iznos = Math.Round(res.ukupno - res.porez_osnovica, 2);
            return res;
        }
        public class Cijena
        {
            public decimal porez_osnovica;
            public decimal porez_iznos;
            public decimal ukupno;
            public Cjenik cjenik;
            public int parkvehicleID;
            public int sati;
            public int dani;
            public Cijena(decimal iznos, Cjenik _cjenik, int _park_vehicle_ID, int _sati, int _dani)
            {
                ukupno = iznos;
                cjenik = _cjenik;
                parkvehicleID = _park_vehicle_ID;
                sati = _sati;
                dani = _dani;

            }

        }
        public class Cjenik
        {
            public int ID;
            public decimal PDV;
            public decimal cijena_sat;
            public decimal cijena_dan;
            public int besplatne_minute;
            public int max_sati;
            public int minute_za_izlaz;
            public Cjenik(int _ID, decimal _PDV, decimal _cijena_sat, decimal _cijena_dan,
                int _max_sati, int _besplatne_minute, int _minute_za_izlaz)
            {
                ID = _ID;
                PDV = _PDV;
                cijena_sat = _cijena_sat;
                cijena_dan = _cijena_dan;
                besplatne_minute = _besplatne_minute;
                max_sati = _max_sati;
                minute_za_izlaz = _minute_za_izlaz;
            }
        }
        public class LicensePlate
        {
            public int park_vehicle_ID;
            public Image licence_plate_image;
            public string licence_plate;
            public DateTime date_time_from;
            public LicensePlate(int _park_vehicle_ID,Image _licence_plate_image,string _licence_plate,DateTime _date_time_from)
            {
                park_vehicle_ID = _park_vehicle_ID;
                licence_plate_image = _licence_plate_image;
                licence_plate = _licence_plate;
                date_time_from = _date_time_from;
            }
        }
        public class ControlsTexts
        {
            public string lbl_licence_plate;
            public string lbl_charge_entrance;
            public string lbl_charge_exit;
            public string lbl_charge_price;
            public string lbl_charge_paid_cash;
            public string lbl_charge_paid_card;
            public string lbl_message_card_error;
            public string lbl_message_cash_error;
            public string cancel;
            public ControlsTexts()
            {
            }
        }
        public class RawPrinterHelper
        {
            // Structure and API declarions:
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public class DOCINFOA
            {
                [MarshalAs(UnmanagedType.LPStr)]
                public string pDocName;
                [MarshalAs(UnmanagedType.LPStr)]
                public string pOutputFile;
                [MarshalAs(UnmanagedType.LPStr)]
                public string pDataType;
            }
            [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

            [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool ClosePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

            [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool EndDocPrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool StartPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool EndPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

            // SendBytesToPrinter()
            // When the function is given a printer name and an unmanaged array
            // of bytes, the function sends those bytes to the print queue.
            // Returns true on success, false on failure.
            public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
            {
                Int32 dwError = 0, dwWritten = 0;
                IntPtr hPrinter = new IntPtr(0);
                DOCINFOA di = new DOCINFOA();
                bool bSuccess = false; // Assume failure unless you specifically succeed.

                di.pDocName = "My C#.NET RAW Document";
                di.pDataType = "RAW";

                // Open the printer.
                if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    // Start a document.
                    if (StartDocPrinter(hPrinter, 1, di))
                    {
                        // Start a page.
                        if (StartPagePrinter(hPrinter))
                        {
                            // Write your bytes.
                            bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                            EndPagePrinter(hPrinter);
                        }
                        EndDocPrinter(hPrinter);
                    }
                    ClosePrinter(hPrinter);
                }
                // If you did not succeed, GetLastError may give more information
                // about why not.
                if (bSuccess == false)
                {
                    dwError = Marshal.GetLastWin32Error();
                }
                return bSuccess;
            }

            public static bool SendFileToPrinter(string szPrinterName, string szFileName)
            {
                // Open the file.
                FileStream fs = new FileStream(szFileName, FileMode.Open);
                // Create a BinaryReader on the file.
                BinaryReader br = new BinaryReader(fs);
                // Dim an array of bytes big enough to hold the file's contents.
                Byte[] bytes = new Byte[fs.Length];
                bool bSuccess = false;
                // Your unmanaged pointer.
                IntPtr pUnmanagedBytes = new IntPtr(0);
                int nLength;

                nLength = Convert.ToInt32(fs.Length);
                // Read the contents of the file into the array.
                bytes = br.ReadBytes(nLength);
                // Allocate some unmanaged memory for those bytes.
                pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                // Copy the managed byte array into the unmanaged array.
                Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
                // Send the unmanaged bytes to the printer.
                bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
                // Free the unmanaged memory that you allocated earlier.
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
                return bSuccess;
            }
            public static bool SendStringToPrinter(string szPrinterName, string szString)
            {
                IntPtr pBytes;
                Int32 dwCount;
                // How many characters are in the string?
                dwCount = szString.Length;
                // Assume that the printer is expecting ANSI text, and then convert
                // the string to ANSI text.
                pBytes = Marshal.StringToCoTaskMemAnsi(szString);
                // Send the converted ANSI string to the printer.
                SendBytesToPrinter(szPrinterName, pBytes, dwCount);
                Marshal.FreeCoTaskMem(pBytes);
                return true;
            }
        }
        private void SetControlText(Control c, string text)
        {
            if (c.InvokeRequired)
            {
                SetControlTextCallback d = new SetControlTextCallback(SetControlText);
                this.Invoke(d, new object[] { c, text });
            }
            else
            {
                c.Text = text;
            }
        }
        private void SetLabelImage(Label l, Image i)
        {
            if (l.InvokeRequired)
            {
                SetLabelImageCallback d = new SetLabelImageCallback(SetLabelImage);
                this.Invoke(d, new object[] { l, i });
            }
            else
            {
                l.Image = i;
            }
        }

        private void HideTextBoxCaret(TextBox t)
        {
            if (t.InvokeRequired)
            {
                HideTextBoxCaretCallback d = new HideTextBoxCaretCallback(HideTextBoxCaret);
                this.Invoke(d, new object[] {t});
            }
            else
            {
                HideCaret(t.Handle);
            }
        }
        private void SetControlVisible(Control c, Boolean v)
        {
            if (c.InvokeRequired)
            {
                SetControlVisibleCallback d = new SetControlVisibleCallback(SetControlVisible);
                this.Invoke(d, new object[] { c, v });
            }
            else
            {
                c.Visible  = v ;
            }
        }
        private void SetControlEnabled(Control c, Boolean e)
        {
            if (c.InvokeRequired)
            {
                SetControlEnabledCallback d = new SetControlEnabledCallback(SetControlEnabled);
                this.Invoke(d, new object[] { c, e });
            }
            else
            {
                c.Enabled = e;
            }
        }
        private void SetControlFocus(Control c)
        {
            if (c.InvokeRequired)
            {
                SetControlFocusCallback d = new SetControlFocusCallback(SetControlFocus);
                this.Invoke(d, new object[] { c });
            }
            else
            {
                c.Focus();
            }
        }

        private void SetTextBoxSelection(TextBox t)
        {
            if (t.InvokeRequired)
            {
                SetTextBoxSelectionCallback d = new SetTextBoxSelectionCallback(SetTextBoxSelection);
                this.Invoke(d, new object[] { t });
            }
            else
            {
                t.SelectionLength = 0;
            }
        }
        private void btnLanguageCroatian_Click(object sender, EventArgs e)
        {
           
            ct_default  = ct_croatian ;
            SetLanguage(ct_croatian );
            if(pnlLicencePlate .Visible ){
                mainform_keypress = false;
           // SetControlFocus(tbxLicencePlate );
            }
        }

        private void btnLanguageItalian_Click(object sender, EventArgs e)
        {

           ct_default =ct_italian ;
            SetLanguage(ct_italian );
            if (pnlLicencePlate.Visible)
            {
                mainform_keypress = false;
              //  SetControlFocus(tbxLicencePlate);
            }
        }

        private void btnLanguageGerman_Click(object sender, EventArgs e)
        {
            ct_default =ct_german ;
            SetLanguage(ct_german );
            if (pnlLicencePlate.Visible)
            {
                mainform_keypress = false;
              //  SetControlFocus(tbxLicencePlate);
            }
        }

        private void btnLanguageEnglish_Click(object sender, EventArgs e)
        {
           ct_default =ct_english ;
            SetLanguage(ct_english );
            if (pnlLicencePlate.Visible)
            {
                mainform_keypress = false;
              //  SetControlFocus(tbxLicencePlate);
            }
        }

        private void tbxLicencePlate_TextChanged(object sender, EventArgs e)
        {
           
            first_license_plate_ID = 0;
            LicensesPlateList.Clear();
            if (tbxLicencePlate.Text.Trim() != string.Empty)
            {
                if(tbxLicencePlate .Text.Trim ().Length >=6 &&
                    tbxLicencePlate.Text.Trim().ToUpper().IndexOf("XXXABB")!=-1)
                {
                        if (tbxLicencePlate.Text.Trim().Length >= 9)
                        {
                if (tbxLicencePlate.Text.Trim().ToUpper ().IndexOf ("XXXABBEND") !=-1 )
                {
                    Environment.Exit(0);
                }
                if (tbxLicencePlate.Text.Trim().ToUpper().IndexOf("XXXABBSET") != -1)
                {
                    mainform_keypress = false;
                    SetControlText(tbxLicencePlate ,"");
                    DisplayPanel(Panels ,pnlSettings );
                    last_panel = 3;
                    SetControlText(tbxSettingsServer ,remote_server_address );
                }
                    }
                }
                else
                {
                    if(pnlLicencePlate .Visible ){
                    SelectParkVehicleByLicensePlate(tbxLicencePlate .Text.Trim ());
                    if (LicensesPlateList.Count > 0)
                    {
                        DisplayLicensePlates(0);
                        SetControlVisible(pnlLicencePlateScroll, true);
                    }
                    else
                    {
                        SetControlVisible(lblLicencePlate01, false );
                        SetControlVisible(lblEntranceDateTime01, false );
                        SetControlVisible(lblLicencePlate02, false );
                        SetControlVisible(lblEntranceDateTime02, false );
                        SetControlVisible(lblLicencePlate03, false );
                        SetControlVisible(lblEntranceDateTime03, false );
                        SetControlVisible(pnlLicencePlateScroll ,false );
                    }
                    }
                }
            }
            else
            {
                SetControlVisible(pnlLicencePlateScroll, false);
            }
        }

        private void btnLicencePlateDown_Click(object sender, EventArgs e)
        {
            first_license_plate_ID += 1;
            DisplayLicensePlates(first_license_plate_ID );
        }

        private void btnLicencePlateUp_Click(object sender, EventArgs e)
        {
            first_license_plate_ID -= 1;
            DisplayLicensePlates(first_license_plate_ID );
        }

        private void frmMainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(mainform_keypress  ){
                if (e.KeyChar != (char)Keys.Back)
                {
                    SetControlText(tbxLicencePlate, string.Concat(tbxLicencePlate.Text, e.KeyChar));
                }
                else
                {
                    SetControlText(tbxLicencePlate ,"");
                }
            }
        }

        private void btnSettingsClose_Click(object sender, EventArgs e)
        {
            if (server_OK)
            {
                mainform_keypress = false;
                DisplayPanel(Panels, pnlLicencePlate);
                last_panel = 1;
            }
            else
            {
                mainform_keypress = true;
                DisplayPanel(Panels ,pnlSpinner );
            }
        }

        private void Keyboard_Click(object sender, EventArgs e)
        {
            if (sender.ToString().IndexOf("BACK") == -1)
            {
                SetControlText(tbxLicencePlate, string.Concat(tbxLicencePlate.Text, sender.ToString().Substring(sender.ToString().IndexOf("Text:") + 5).Trim()));
            }
            else
            {
                if(tbxLicencePlate .Text!=""){
                    SetControlText(tbxLicencePlate ,tbxLicencePlate .Text.Substring (0,tbxLicencePlate .Text .Length -1));
                }
            }
            SetTextBoxSelection(tbxLicencePlate );
        }

        private void btnChargeCancel_Click(object sender, EventArgs e)
        {
            _logger_comm.charging = true;
            credit_paid.Clear();
            last_price.ukupno  = 0;
            last_sum_paid = 0;
            _logger_comm.charge_price = 0;
           
            SetControlText(lblChargePrice, "");
            lbl_charge_paid_state = 0;
            SetControlText(lblChargePaid, "");
            lbl_charge_message_state = 0;
            SetControlText(lblChargeMessage, "");
            SetControlText(tbxLicencePlate, "");

            SetLabelImage(lblChargeLicensePlate, empty_license_plate_image);
            SetControlText(lblChargeEntrance, "");
            SetControlText(lblChargeExit, "");
                                    
            DisplayPanel(Panels, pnlLicencePlate);
            last_panel = 1;
            licence_plate_panel_timer.Stop();
            lpp_count = 0;
        }

        private void lblLicencePlate01_Click(object sender, EventArgs e)
        {
            DisplayLicensePlateCharge(LicensesPlateList [first_license_plate_ID ].park_vehicle_ID  );
            DisplayPanel(Panels,pnlCharge );
            
            _check_payment.check_payment_interval = 0;
            _check_payment.checking_payment = true;
            last_panel = 2;
        }

        private void lblLicencePlate02_Click(object sender, EventArgs e)
        {
            DisplayLicensePlateCharge(LicensesPlateList[first_license_plate_ID+1].park_vehicle_ID);
            DisplayPanel(Panels, pnlCharge);
            _check_payment.check_payment_interval = 0;
            _check_payment.checking_payment = true;
            last_panel = 2;
        }

        private void lblLicencePlate03_Click(object sender, EventArgs e)
        {
            DisplayLicensePlateCharge(LicensesPlateList[first_license_plate_ID+2].park_vehicle_ID);
            DisplayPanel(Panels, pnlCharge);
            _check_payment.check_payment_interval = 0;
            _check_payment.checking_payment = true;
            last_panel = 2;
        }

       
     

      

      
       

       

      

     

      

       

      

    }
}
