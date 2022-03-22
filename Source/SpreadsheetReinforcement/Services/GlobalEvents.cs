namespace SpreadsheetReinforcement.Services
{
    using System;
    using Tools;

    public static class GeneralEvents
    {
        public static event EventHandler<StringArg> HideMainForm;

        public static void SendHideMainForm(StringArg message)
        {
            HideMainForm?.Invoke("MessangeHandler", message);
        }

        public static event EventHandler<StringArg> ShowMainForm;

        public static void SendShowMainForm(StringArg message)
        {
            ShowMainForm?.Invoke("MessangeHandler", message);
        }

        public static event EventHandler<StringArg> NewStatusMessage;

        public static void SendNewStatusMessage(StringArg message)
        {
            NewStatusMessage?.BeginInvoke(null, message, null, null);
        }
    }
}