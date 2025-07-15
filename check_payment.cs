using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SelfPay
{
  public   class check_payment
    {
      public System.Timers.Timer check_payment_timer;
      public int check_payment_interval = 0;
      public Boolean checking_payment = false;
      public delegate void PaymentCheckedHandler(object myObject, payment_checked_args payment_checked_sent_Args);
      public event PaymentCheckedHandler payment_checked;
      public class payment_checked_args : EventArgs
      {
          private Boolean payment_not_OK;

          public payment_checked_args(Boolean _payment_not_OK)
          {
              payment_not_OK = _payment_not_OK;
          }

          public Boolean Payment_not_OK
          {
              get
              {
                  return payment_not_OK;
              }
          }
      }
      public check_payment()
      {
          check_payment_timer = new System.Timers.Timer();
          check_payment_timer.Interval = 1000;
          check_payment_timer.Elapsed += new System.Timers.ElapsedEventHandler(check_payment_timer_Elapsed);
      }
      public void Start()
      {
          check_payment_timer.Start();
      }
      public void Stop()
      {
          check_payment_timer.Stop();
      }
      void check_payment_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
      {
          if(checking_payment  ){
              check_payment_interval += 1;
              if(check_payment_interval==120 ){
                  payment_checked_args _args = new payment_checked_args(false );
                  payment_checked(this, _args);
                  checking_payment = false;
                  check_payment_interval = 0;
              }
          }
      }
    }
}
