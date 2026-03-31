using AltoHttp;
using Claunia.PropertyList;
using Guna.UI2.WinForms;
using iSkorpionA12;
using iSkorpionAiO_RE.Properties;
using LibMobileDevice;
using LibMobileDevice.Event;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Color = System.Drawing.Color;
using File = System.IO.File;




namespace iSkorpionAiO_RE
{
    public partial class Form1 : Form
    {
        #region LINKS
        public static string ramdiskURL = "http://SERVERURL/ramdisk/";
        public static string baseUrl = "https://SERVERURL/A12";
        public static string MDMLink = "http://SERVERURL/mdm";
        public static string PasscodeLink = "http://SERVERURL/backup";
        public static string XSNLink = "http://SERVERURL/xsn";
        public static string SNLink = "http://SERVERURL/sn";
        public static string JBLink = "http://SERVERURL/jailbreak";
        public static string BBLink = "http://SERVERURL/baseband";
        public static string VersionLink = "http://SERVERURL/aio.php";
        public static string DownloadLink = "https://iskorpion.com/downloads";
        public static int Toolversion = 72;
        public static string MegaLink = "https://mega.nz/folder/LqRFUCpA#uUh3E_CEEbpgWYy3NxZW-g";
        public static string TelegramLink = "https://t.me/iSkorpionOfficial";
        public static string TelegramGLink = "https://t.me/iSkorpion_Official";
        public static string TNotif = baseUrl + "/telegramv5.php";
        #endregion
        private newShetouane driverManager;
        private Point windowLocation;
        private LibMobileDevice.iOSDeviceManager manager = new iOSDeviceManager();
        public static string ToolDir = Environment.CurrentDirectory;
        public string typedevice = "";
        public string type = "";
        public static string Libs = ToolDir + "\\x64\\";
        public static string ramdisk;
        public iOSDevice currentiOSDevice;
        public iOSRecoveryDevice recoveryDevice;

        public static string ramdiskFolder = ToolDir + "\\ramdisk\\";
        public static string ramdiskFile;
        public static string purpleFile;
        public static string part1;
        public static string part2;
        public static string part3;
        public static string part4;
        public static string part5;
        public static string part6;
        public static string part7;
        public static string part8;
        public static string part9;


        private bool isSharingActive = false;
        private BackgroundWorker worker = new BackgroundWorker();
        private SshClient sshClient;
        private ScpClient scpClient;
        public static string UCID = "NA";
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(out IntPtr OldValue);



        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
   (
       int nLeftRect,     // x-coordinate of upper-left corner
       int nTopRect,      // y-coordinate of upper-left corner
       int nRightRect,    // x-coordinate of lower-right corner
       int nBottomRect,   // y-coordinate of lower-right corner
       int nWidthEllipse, // height of ellipse
       int nHeightEllipse // width of ellipse
   );

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private const int CornerRadius = 20;


        private DropShadow shadow;


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }

        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;

            InitializeComponent();
            this.Load += Form1_Load;
            InitializeDeviceManagers();
            shadow = new DropShadow();
            shadow.ApplyShadows(this);
           // checkversion();
            //InitializeUsbSharing();
            driverManager = new newShetouane();

            _telegramNotifier = new TelegramNotifier(TNotif, Toolversion.ToString());
            Instance = this;
            string lockdownDir = "C:\\ProgramData\\Apple\\Lockdown\\";
            try
            {
                if (Directory.Exists(lockdownDir))
                {
                    Directory.Delete(lockdownDir, true);
                    Directory.CreateDirectory(lockdownDir);
                }

                MessageBoxManager.Register();
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerAsync();

                KilliProxy();

            }
            catch
            {
                //
            } 
        }


        private void UpdatelabelInfoProgres(string text)
        {

            AddLog($" {text}", Color.Blue);
        }
        private void UpdateProgressLabel(string message)
        {

            AddLog($"{message}", Color.Blue);
        }
        private void UpdateGuna2ProgressBar1Control(int value)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new Action<int>(UpdateGuna2ProgressBar1Control), value);
                return;
            }
            progressBar1.Value = Math.Max(0, Math.Min(100, value));

        }

        private void InitializeDeviceManagers()
        {

            CurrentDeviceData = new DeviceData();
            deviceCleanupManager = new DeviceCleanupManager(
                pythonTargetPath,
                UpdatelabelInfoProgres,
                UpdateProgressLabel,
                UpdateGuna2ProgressBar1Control
            );

            deviceFileManager = new DeviceFileManager(
                pythonTargetPath,
                UpdatelabelInfoProgres,
                UpdateProgressLabel,
                UpdateGuna2ProgressBar1Control
            );

        }
        public void checkversion()
        {

            WebProxy webProxy = null;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string s;
                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    s = webClient.DownloadString(VersionLink);
                    if (webProxy != null)
                    {
                        webClient.Proxy = webProxy;
                    }
                }
                if (int.Parse(s) > Toolversion && CustomMessageBox.Show("Please Update the Tool!", "NEW UPDATE!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    Process.Start(DownloadLink);
                    Environment.Exit(0);
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                CustomMessageBox.Show("Failed to check version !", "Server Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Process.Start(MegaLink);
                Environment.Exit(0);
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            manager.CommonConnectEvent += NormalMode;
            manager.RecoveryConnectEvent += RecoveryMode;
            manager.DfuConnectEvent += DFUMode;
            manager.ListenErrorEvent += ListenError;
            manager.StartListen();
            Debug.WriteLine("started listening for device");
        }

        public static Form1 Instance { get; private set; }

        public void Form1_Load(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = verify_cert;
            ApplyRoundedCorners();
            ShowPanel("A12Panel");


            CleanTMP();

        }

        private void ShowPanel(string panelName)
        {
            // Hide all panels first
            foreach (Control control in this.Controls)
            {
                if (control is Guna2Panel panel)
                {
                    panel.Visible = false;
                }
            }

            // Show the requested panel
            foreach (Control control in this.Controls)
            {
                if (control is Guna2Panel panel && panel.Name == panelName)
                {
                    FooterPanel.Visible = true;
                    HeaderPanel.Visible = true;
                    InfoPanel.Visible = true;
                    panel.Visible = true;
                    break;
                }
            }
        }




        private void ApplyRoundedCorners()
        {
            IntPtr region = CreateRoundRectRgn(0, 0, this.Width, this.Height, CornerRadius, CornerRadius);
            SetWindowRgn(this.Handle, region, true);
            DeleteObject(region); // Clean up the region handle
        }

        private void ListenError(object sender, ListenErrorEventHandlerEventArgs args)
        {
             status2.Text += "\r\n Device Disconnected!";
        }
        private void RecoveryMode(object sender, DeviceRecoveryConnectEventArgs e)
        { 
            statux2("Device Connected in Recovery Mode");
            MODELBL.Text = "Recovery"; 
        }

        private void DFUMode(object sender, DeviceDfuConnectEventArgs e)
        { 
            iRecovery(); 
        }

        public async void readNormalMode()
        {
            try
            {
                Process processx = new Process();
                processx.StartInfo.UseShellExecute = false;
                processx.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "x64", "ideviceinfo.exe");
                processx.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "x64");
                processx.StartInfo.CreateNoWindow = true;
                processx.StartInfo.RedirectStandardOutput = true;
                processx.StartInfo.RedirectStandardError = true;

                // StringBuilder for more efficient string handling
                StringBuilder outputBuilder = new StringBuilder();
                StringBuilder errorBuilder = new StringBuilder();

                bool deviceTrustRequired = false;
                bool deviceConnected = false;

                processx.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                        ProcessOutputLine(e.Data);
                    }
                };

                processx.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorBuilder.AppendLine(e.Data);
                        string errorLine = e.Data;

                        // Check for common error patterns
                        if (errorLine.Contains("Pairing dialog response pending") ||
                            errorLine.Contains("Device is locked") ||
                            errorLine.Contains("User denied trust") ||
                            errorLine.Contains("Please accept the trust dialog"))
                        {
                            deviceTrustRequired = true;
                        }

                        statux2(errorLine);
                    }
                };

                processx.Start();
                processx.BeginOutputReadLine();
                processx.BeginErrorReadLine();

                // Wait for process to exit with timeout (e.g., 30 seconds)
                bool exited = processx.WaitForExit(30000);

                if (!exited)
                {
                    processx.Kill();
                    statux2("Process timed out");
                    return;
                }

                // Handle trust/connection issues
                if (deviceTrustRequired)
                {
                    await HandleDeviceTrustIssue();
                    return;
                }

                // If no device information was retrieved, show connection error
                if (!deviceConnected && processx.ExitCode != 0)
                {
                    await HandleDeviceConnectionError();
                    return;
                }

                // Update UI on the main thread if needed
                await UpdateDeviceStatusUI();
            }
            catch (Exception ex)
            {
                statux2($"Error reading device info: {ex.Message}");
                await ShowErrorMessage($"Failed to read device information: {ex.Message}");
            }
        }

        private void ProcessOutputLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                // Use dictionary for cleaner mapping of property handlers
                var propertyHandlers = new Dictionary<string, Action<string>>
                {
                    ["SerialNumber: "] = (value) =>
                    {
                        SNLBL.Text = value;
                        BATTLBL.Text = GetBatt() + " %";
                        UpdateStorageInfo();
                    },
                    ["ProductVersion: "] = (value) =>
                    {
                        iOSLBL.Text = value;
                        typedevice = DetermineDeviceType(value);
                    },
                    ["SIMStatus: "] = (value) => SIMStat = value,
                    ["ActivationState: "] = (value) =>
                    {
                        ActLBL.Text = value;
                        UpdateActivationState(value);
                    },
                    ["ModelNumber: "] = (value) => ModelLBL.Text = value,
                    ["InternationalMobileEquipmentIdentity: "] = (value) => IMEILBL.Text = value,
                    ["UniqueDeviceID: "] = (value) =>
                    {
                        UDIDLBL.Text = value;
                        currentUdid = value;
                    },
                    ["UniqueChipID: "] = (value) =>
                    {
                        UCIDLBL.Text = value;
                        UCID = value;
                        ECIDLBL.Text = ToHex(value);
                    },
                    ["BuildVersion: "] = (value) => BuildLBL.Text = value,
                    ["BoardId: "] = (value) => BDIDLBL.Text = value == "NA" ? "NA" : "0" + value,
                    ["RegionInfo: "] = (value) => RegionLBL.Text = value,
                    ["HardwareModel: "] = (value) => HardLBL.Text = value.ToLower(),
                    ["ChipID: "] = (value) =>
                    {
                        CPIDLBL.Text = value == "0" ? "NA" : ToHex(value);
                    },
                    ["ProductType: "] = (value) =>
                    {
                        MODELBL.Text = "Normal";
                        ProdLBL.Text = value;
                        NameLBL.Text = DetermineModel(value);
                        isDeviceCurrentlyConnected = true;
                        UpdateCurrentDeviceInfo(value);
                    }
                };

                // Find and execute the appropriate handler
                foreach (var handler in propertyHandlers)
                {
                    if (text.StartsWith(handler.Key))
                    {
                        string value = text.Substring(handler.Key.Length).Trim();
                        handler.Value(value);
                        return;
                    }
                }

                // Handle special cases
                if (text.Contains("Could not connect to lockdownd"))
                {
                    FixLockdownIssue();
                }
            }
            catch (Exception ex)
            {
                statux2($"Error processing output: {ex.Message}");
            }
        }

        private void UpdateActivationState(string activationState)
        {
            bool isActivated = !activationState.ToLower().Contains("unactivated");
            ActivateButton.Enabled = !isActivated;
            guna2GradientButton2.Enabled = isActivated;
        }

        private string DetermineDeviceType(string productVersion)
        {
            var gsmModels = new HashSet<string>
    {
        "iPhone9,3", "iPhone9,4", "iPhone10,4", "iPhone10,5",
        "iPhone10,6", "iPhone8,1", "iPhone8,2"
    };

            return gsmModels.Contains(productVersion) ? "GSM" : "MEID";
        }


        private void UpdateCurrentDeviceInfo(string productType)
        {
            currentProductType = productType;
            currentProductVersion = iOSLBL.Text;
            currentSerialNumber = SNLBL.Text;
            currentActivationState = ActLBL.Text;
            currentImei = IMEILBL.Text;
            currentEcid = ECIDLBL.Text;
            lastDeviceVersion = iOSLBL.Text;
            lastDeviceSN = SNLBL.Text;
            lastDeviceType = productType;
            lastDeviceActivation = ActLBL.Text;
            lastDeviceIMEI = IMEILBL.Text;
            lastDeviceECID = currentEcid ?? "";
        }


        private void FixLockdownIssue()
        {
            string lockdownDir = @"C:\ProgramData\Apple\Lockdown\";
            try
            {
                if (Directory.Exists(lockdownDir))
                {
                    Directory.Delete(lockdownDir, true);
                    Directory.CreateDirectory(lockdownDir);
                }
            }
            catch (Exception ex)
            {
                statux2($"Failed to fix lockdown: {ex.Message}");
            }
        }

        private async Task HandleDeviceTrustIssue()
        {
            var result = await ShowTrustDialogAsync();
            if (result == DialogResult.Retry)
            {
                // Wait a moment for user to tap "Trust" then retry
                await Task.Delay(3000);
                readNormalMode();
            }
        }

        private async Task HandleDeviceConnectionError()
        {

        }

        private async Task UpdateDeviceStatusUI()
        {
            try
            {
                await LoadImageWithZoomAsync(2.0f);
            }
            catch
            {
                // 
            }
        }

        // Example dialog methods (implement according to your UI framework)
        private async Task<DialogResult> ShowTrustDialogAsync()
        {
            return await Task.Run(() =>
            {
                return CustomMessageBox.Show(
                    "Trust This Computer?\n\n" +
                    "Please unlock your iPhone and tap 'Trust' when prompted.\n\n" +
                    "Click Retry after accepting the trust dialog.",
                    "Device Trust Required",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Information
                );
            });
        }

        private async Task ShowErrorMessage(string message)
        {
            await Task.Run(() =>
            {
                CustomMessageBox.Show(message, "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }


        private void UpdateStorageInfo()
        {
            try
            {
                double totalDiskCapacityInGB = getTotalDiskCapacityInGB();
                double totalDataAvailableInGB = getTotalDataAvailableInGB();
                double usedSpace = totalDiskCapacityInGB - totalDataAvailableInGB;

                DiskLBL.Text = $"{usedSpace:F1} GB / {totalDiskCapacityInGB:F1} GB";

                int storagePercentage = (int)Math.Round(usedSpace / totalDiskCapacityInGB * 100.0);
                StorageBar.Value = Math.Max(0, Math.Min(100, storagePercentage));
            }
            catch
            {
                DiskLBL.Text = "Unknown";
                StorageBar.Value = 0;
            }
        }


        public void ReadSN()
        {
            string text = SSH("ioreg -l | grep IOPlatformSerialNumber --color=never").Replace("\"", "").Replace("\t", "").Replace(" ", "").Replace("|IOPlatformSerialNumber=", "").Trim();
            bool flag = text.Length <= 12;
            string result = "";
            if (flag)
            {
                result = text;
            }
            else
            {
                result = text.Remove(0, 13);
            }
            SNLBL.Text = result;
        }

        public bool isDeviceCurrentlyConnected = false;
        private void NormalMode(object sender, DeviceCommonConnectEventArgs args)
        {

            readNormalMode();
            isDeviceCurrentlyConnected = true;
            /* try
             {
                 PostGestalt();
                 PostSISV();
                 MakeAct();
             }
             catch
             {

             }*/
            if (CheckJB())
            {
                JBLBL.Text = "Yes";
                JBLBL.ForeColor = System.Drawing.Color.Green;
                if (SNLBL.Text == "NA")
                {
                    ReadSN();
                }

            }


        }

        public double getTotalDiskCapacityInGB()
        {
            try
            {
                //  var ksldf = currentiOSDevice.
                using (Process process = Process.Start(new ProcessStartInfo
                {

                    FileName = Environment.CurrentDirectory + "\\x64\\ideviceinfo.exe",
                    Arguments = "-q com.apple.disk_usage.factory -k TotalDiskCapacity",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }))
                {
                    string text = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    long num;
                    bool flag = long.TryParse(text.Trim(), out num);
                    if (flag)
                    {
                        return Math.Round((double)num / 1073741824.0, 2);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return 0.0;
        }


        public double getTotalDataAvailableInGB()
        {
            try
            {
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\x64\\ideviceinfo.exe",
                    Arguments = "-q com.apple.disk_usage.factory -k TotalDataAvailable",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }))
                {
                    string text = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    long num;
                    bool flag = long.TryParse(text.Trim(), out num);
                    if (flag)
                    {
                        return Math.Round((double)num / 1073741824.0, 2);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return 0.0;
        }
        public string UCIDx
        {
            get
            {
                try
                {

                    string text = Int64.Parse(ECIDLBL.Text.Replace("0x", ""), NumberStyles.HexNumber).ToString();
                    Debug.WriteLine(text);

                    return text;
                }
                catch
                {
                    return "NA";
                }
            }
        }
        public string ToHex(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return "0x0000000000000000";

                // Remove any existing "0x" prefix if present
                string cleanValue = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    ? value.Substring(2)
                    : value;

                if (long.TryParse(cleanValue, out long numericValue))
                {
                    return "0x" + numericValue.ToString("X16").ToLower();
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error converting UCID to ECID: {ex.Message}");
            }

            return "0x0000000000000000";
        }

        public string cmd(string xprocess, string args, string value)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = Libs + xprocess;
                process.StartInfo.Arguments = args;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
                string text = process.StandardOutput.ReadToEnd();
                if (text.Contains("ERROR"))
                {
                    return "NA";
                }
                string blabla;
                string result = "NA";
                string[] array = text.Split('\n');
                for (int i = 0; i < array.Length; i++)
                {
                    string text2 = array[i].Replace("\r", "");
                    if (text2.StartsWith(value + ": "))
                    {
                        blabla = text2.Replace(value + ": ", "");
                        result = blabla;

                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetBatt()
        {
            try
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = Libs + "ideviceinfo.exe";
                process.StartInfo.Arguments = "-q com.apple.mobile.battery";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
                string text = process.StandardOutput.ReadToEnd();
                if (text.Contains("ERROR"))
                {
                    return "-";
                }
                string blabla;
                string result = "-";
                string[] array = text.Split('\n');
                for (int i = 0; i < array.Length; i++)
                {
                    string text2 = array[i].Replace("\r", "");
                    if (text2.StartsWith("BatteryCurrentCapacity: "))
                    {
                        blabla = text2.Replace("BatteryCurrentCapacity: ", "");
                        result = blabla;

                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public void statux2(string stat)
        {
            try
            {
                status2.Text += "\r\n " + stat + "\r\n";
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }
            catch
            {
                //
            }

        }
        public bool iRecovery()
        {

            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Libs + "irecovery.exe",
                    Arguments = "-q",
                    WorkingDirectory = Libs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                process.Start();
                process.WaitForExit();

                string text = process.StandardOutput.ReadToEnd();
                statux2(text);
                JBLBL.Text = "NA";
                bool result = true;
                string[] array = text.Split('\n');
                for (int i = 0; i < array.Length; i++)
                {
                    string text2 = array[i].Replace("\r", "");
                    if (text2.StartsWith("ECID: "))
                    {

                        string textsn = text2.Replace("ECID: ", "");
                        ECIDLBL.Text = textsn;
                    }

                    else if (text2.StartsWith("PRODUCT: "))
                    {

                        string string_12 = text2.Replace("PRODUCT: ", "");
                        ProdLBL.Text = string_12;
                        NameLBL.Text = DetermineModel(string_12);
                    }
                    else if (text2.StartsWith("MODEL: "))
                    {

                        HardLBL.Text = text2.Replace("MODEL: ", "").ToLower();

                    }
                    else if (text2.StartsWith("PWND: "))
                    {
                        JBLBL.Text = text2.Replace("PWND: ", "");
                        string hj = JBLBL.Text;
                        if (hj.Contains("GASTER"))
                        {
                            JBLBL.ForeColor = System.Drawing.Color.Teal;
                        }
                        else if (hj.Contains("CHECKM8"))
                        {
                            JBLBL.ForeColor = System.Drawing.Color.Orange;
                        }
                        else
                        {
                            JBLBL.Text = "NA";
                            JBLBL.ForeColor = System.Drawing.Color.Crimson;
                        }

                    }

                    else if (text2.StartsWith("CPID: "))
                    {
                        string string_13 = text2.Replace("CPID: ", "");
                        CPIDLBL.Text = string_13;
                    }

                    else if (text2.StartsWith("BDID: "))
                    {
                        string BDID = text2.Replace("BDID: ", "").ToUpper();
                        BDIDLBL.Text = BDID;
                    }
                    else if (text2.StartsWith("MODE: "))
                    {
                        string Modex = text2.Replace("MODE: ", "");
                        MODELBL.Text = Modex;
                        if (Modex.ToLower() == "dfu")
                        {
                            statux2("Device Connected in DFU Mode");
                        }
                        if (Modex.ToLower() == "recovery")
                        {
                            statux2("Device Connected in Recovery Mode");
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void iRec()
        {
            iRecovery();
        }
        public bool iDeviceInfo()
        {

            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = Libs + "ideviceinfo.exe",
                    WorkingDirectory = Libs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                };

                process.Start();
                process.WaitForExit();

                string text = process.StandardOutput.ReadToEnd();
                string textx = process.StandardError.ReadToEnd();
                statux2(textx);

                bool result = true;
                string[] array = text.Split('\n');
                for (int i = 0; i < array.Length; i++)
                {
                    string text2 = array[i].Replace("\r", "");
                    if (text2.StartsWith("SerialNumber: "))
                    {

                        string textsn = text2.Replace("SerialNumber: ", "");
                        SNLBL.Text = textsn;
                        Debug.WriteLine(textsn);
                        Debug.WriteLine("testing;...");
                    }

                    else if (text2.StartsWith("ProductType: "))
                    {

                        string string_12 = text2.Replace("ProductType: ", "");
                        ProdLBL.Text = string_12;
                        NameLBL.Text = DetermineModel(string_12);
                    }
                    else if (text2.StartsWith("HardwareModel: "))
                    {

                        HardLBL.Text = text2.Replace("HardwareModel: ", "").ToLower();

                    }

                    else if (text2.StartsWith("ActivationState: "))
                    {

                        ActLBL.Text = text2.Replace("ActivationState: ", "").ToLower();

                    }
                    else if (text2.StartsWith("ModelNumber: "))
                    {

                        ModelLBL.Text = text2.Replace("ModelNumber: ", "").ToLower();

                    }
                    else if (text2.StartsWith("UniqueChipID: "))
                    {

                        UDIDLBL.Text = text2.Replace("UniqueChipID: ", "").ToLower();

                    }
                    else if (text2.StartsWith("BuildVersion: "))
                    {

                        BuildLBL.Text = text2.Replace("BuildVersion: ", "").ToLower();

                    }

                    else if (text2.StartsWith("ChipID: "))
                    {

                        CPIDLBL.Text = ToHex(text2.Replace("ChipID: ", ""));

                    }

                    else if (text2.StartsWith("InternationalMobileEquipmentIdentity: "))
                    {
                        string string_13 = text2.Replace("InternationalMobileEquipmentIdentity: ", "");
                        IMEILBL.Text = string_13;

                    }

                    else if (text2.StartsWith("BoardId: "))
                    {
                        string BDID = text2.Replace("BoardId: ", "").ToUpper();
                        BDIDLBL.Text = "0" + BDID;
                    }

                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private Dictionary<string, string> modelNames = new Dictionary<string, string>();
        private string GetModelName(string productType)
        {
            if (modelNames.ContainsKey(productType))
            {
                return modelNames[productType];
            }
            return productType;
        }
        public string DetermineModel(string productType)
        {

            string modelName = GetModelName(productType);
            modelName = modelName.Replace("iPhone5,1", "iPhone 5 (GSM)");
            modelName = modelName.Replace("iPhone5,2", "iPhone 5 (GSM+CDMA)");
            modelName = modelName.Replace("iPhone5,3", "iPhone 5C (GSM)");
            modelName = modelName.Replace("iPhone5,4", "iPhone 5C (Global)");
            modelName = modelName.Replace("iPhone6,1", "iPhone 5S (GSM)");
            modelName = modelName.Replace("iPhone6,2", "iPhone 5S (Global)");
            modelName = modelName.Replace("iPhone7,1", "iPhone 6 Plus");
            modelName = modelName.Replace("iPhone7,2", "iPhone 6");
            modelName = modelName.Replace("iPhone8,1", "iPhone 6s");
            modelName = modelName.Replace("iPhone8,2", "iPhone 6s Plus");
            modelName = modelName.Replace("iPhone8,4", "iPhone SE (GSM)");
            modelName = modelName.Replace("iPhone9,1", "iPhone 7");
            modelName = modelName.Replace("iPhone9,2", "iPhone 7 Plus");
            modelName = modelName.Replace("iPhone9,3", "iPhone 7");
            modelName = modelName.Replace("iPhone9,4", "iPhone 7 Plus");
            modelName = modelName.Replace("iPhone10,1", "iPhone 8");
            modelName = modelName.Replace("iPhone10,2", "iPhone 8 MEID");
            modelName = modelName.Replace("iPhone10,3", "iPhone X Global");
            modelName = modelName.Replace("iPhone10,4", "iPhone 8 GSM");
            modelName = modelName.Replace("iPhone10,5", "iPhone 8 Plus");
            modelName = modelName.Replace("iPhone10,6", "iPhone X GSM");
            modelName = modelName.Replace("iPhone11,2", "iPhone XS");
            modelName = modelName.Replace("iPhone11,4", "iPhone XS Max");
            modelName = modelName.Replace("iPhone11,6", "iPhone XS Max Global");
            modelName = modelName.Replace("iPhone11,8", "iPhone XR");
            modelName = modelName.Replace("iPhone12,1", "iPhone 11");
            modelName = modelName.Replace("iPhone12,3", "iPhone 11 Pro");
            modelName = modelName.Replace("iPhone12,5", "iPhone 11 Pro Max");
            modelName = modelName.Replace("iPhone12,8", "iPhone SE 2nd Gen");
            modelName = modelName.Replace("iPhone13,1", "iPhone 12 Mini");
            modelName = modelName.Replace("iPhone13,2", "iPhone 12");
            modelName = modelName.Replace("iPhone13,3", "iPhone 12 Pro");
            modelName = modelName.Replace("iPhone13,4", "iPhone 12 Pro Max");
            modelName = modelName.Replace("iPhone14,2", "iPhone 13 Pro");
            modelName = modelName.Replace("iPhone14,3", "iPhone 13 Pro Max");
            modelName = modelName.Replace("iPhone14,4", "iPhone 13 Mini");
            modelName = modelName.Replace("iPhone14,5", "iPhone 13");
            modelName = modelName.Replace("iPhone14,6", "iPhone SE 3rd Gen");
            modelName = modelName.Replace("iPhone14,7", "iPhone 14");
            modelName = modelName.Replace("iPhone14,8", "iPhone 14 Plus");
            modelName = modelName.Replace("iPhone15,2", "iPhone 14 Pro");
            modelName = modelName.Replace("iPhone15,3", "iPhone 14 Pro Max");
            modelName = modelName.Replace("iPhone15,4", "iPhone 15");
            modelName = modelName.Replace("iPhone15,5", "iPhone 15 Plus");
            modelName = modelName.Replace("iPhone16,1", "iPhone 15 Pro ");
            modelName = modelName.Replace("iPhone16,2", "iPhone 15 Pro Max ");

            modelName = modelName.Replace("iPhone17,3", "iPhone 16 ");
            modelName = modelName.Replace("iPhone17,1", "iPhone 16 Pro");
            modelName = modelName.Replace("iPhone17,2", "iPhone 16 Pro Max ");
            modelName = modelName.Replace("iPhone17,4", "iPhone 16 Plus ");

            modelName = modelName.Replace("iPod1,1", "1st Gen iPod");
            modelName = modelName.Replace("iPod2,1", "2nd Gen iPod");
            modelName = modelName.Replace("iPod3,1", "3rd Gen iPod");
            modelName = modelName.Replace("iPod4,1", "4th Gen iPod");
            modelName = modelName.Replace("iPod5,1", "5th Gen iPod");
            modelName = modelName.Replace("iPod7,1", "6th Gen iPod");
            modelName = modelName.Replace("iPod9,1", "7th Gen iPod");
            modelName = modelName.Replace("iPad1,1", "iPad");
            modelName = modelName.Replace("iPad1,2", "iPad 3G");
            modelName = modelName.Replace("iPad2,1", "2nd Gen iPad");
            modelName = modelName.Replace("iPad2,2", "2nd Gen iPad GSM");
            modelName = modelName.Replace("iPad2,3", "2nd Gen iPad CDMA");
            modelName = modelName.Replace("iPad2,4", "2nd Gen iPad Nueva Revisión");
            modelName = modelName.Replace("iPad3,1", "3rd Gen iPad");
            modelName = modelName.Replace("iPad3,2", "3rd Gen iPad CDMA");
            modelName = modelName.Replace("iPad3,3", "3rd Gen iPad GSM");
            modelName = modelName.Replace("iPad2,5", "iPad mini");
            modelName = modelName.Replace("iPad2,6", "iPad mini GSM+LTE");
            modelName = modelName.Replace("iPad2,7", "iPad mini CDMA+LTE");
            modelName = modelName.Replace("iPad3,4", "4th Gen iPad");
            modelName = modelName.Replace("iPad3,5", "4th Gen iPad GSM+LTE");
            modelName = modelName.Replace("iPad3,6", "4th Gen iPad CDMA+LTE");
            modelName = modelName.Replace("iPad4,1", "iPad Air (WiFi)");
            modelName = modelName.Replace("iPad4,2", "iPad Air (GSM+CDMA)");
            modelName = modelName.Replace("iPad4,3", "1st Gen iPad Air (China)");
            modelName = modelName.Replace("iPad4,4", "iPad mini Retina (WiFi)");
            modelName = modelName.Replace("iPad4,5", "iPad mini Retina (GSM+CDMA)");
            modelName = modelName.Replace("iPad4,6", "iPad mini Retina (China)");
            modelName = modelName.Replace("iPad5,3", "iPad Air 2 (WiFi)");
            modelName = modelName.Replace("iPad6,3", "iPad Pro (9.7 pulgadas, WiFi)");
            modelName = modelName.Replace("iPad6,4", "iPad Pro (9.7 pulgadas, WiFi+LTE)");
            modelName = modelName.Replace("iPad6,7", "iPad Pro (12.9 pulgadas, WiFi)");
            modelName = modelName.Replace("iPad6,8", "iPad Pro (12.9 pulgadas, WiFi+LTE)");
            modelName = modelName.Replace("iPad6,11", "iPad (2017)");
            modelName = modelName.Replace("iPad6,12", "iPad (2017)");
            modelName = modelName.Replace("iPad7,1", "iPad Pro 2da Gen (WiFi)");
            modelName = modelName.Replace("iPad7,2", "iPad Pro 2da Gen (WiFi+Cellular)");
            modelName = modelName.Replace("iPad7,3", "iPad Pro 10.5 pulgadas 2da Gen");
            modelName = modelName.Replace("iPad7,4", "iPad Pro 10.5 pulgadas 2da Gen");
            modelName = modelName.Replace("iPad7,5", "iPad 6ta Gen (WiFi)");
            modelName = modelName.Replace("iPad7,6", "iPad 6ta Gen (WiFi+Cellular)");
            modelName = modelName.Replace("iPad7,11", "iPad 7ma Gen 10.2 pulgadas (WiFi)");
            modelName = modelName.Replace("iPad7,12", "iPad 7ma Gen 10.2 pulgadas (WiFi+Cellular)");
            modelName = modelName.Replace("iPad8,1", "iPad Pro 11 pulgadas 3ra Gen (WiFi)");
            modelName = modelName.Replace("iPad8,2", "iPad Pro 11 pulgadas 3ra Gen (1TB, WiFi)");
            modelName = modelName.Replace("iPad8,3", "iPad Pro 11 pulgadas 3ra Gen (WiFi+Cellular)");
            modelName = modelName.Replace("iPad8,4", "iPad Pro 11 pulgadas 3ra Gen (1TB, WiFi+Cellular)");
            modelName = modelName.Replace("iPad8,5", "iPad Pro 12.9 pulgadas 3ra Gen (WiFi)");
            modelName = modelName.Replace("iPad8,6", "iPad Pro 12.9 pulgadas 3ra Gen (1TB, WiFi)");
            modelName = modelName.Replace("iPad8,7", "iPad Pro 12.9 pulgadas 3ra Gen (WiFi+Cellular)");
            modelName = modelName.Replace("iPad8,8", "iPad Pro 12.9 pulgadas 3ra Gen (1TB, WiFi+Cellular)");
            modelName = modelName.Replace("iPad8,9", "iPad Pro 11 pulgadas 4ta Gen (WiFi)");
            modelName = modelName.Replace("iPad8,10", "iPad Pro 11 pulgadas 4ta Gen (WiFi+Cellular)");
            modelName = modelName.Replace("iPad8,11", "iPad Pro 12.9 pulgadas 4ta Gen (WiFi)");
            modelName = modelName.Replace("iPad8,12", "iPad Pro 12.9 pulgadas 4ta Gen (WiFi+Cellular)");
            modelName = modelName.Replace("iPad11,1", "iPad mini 5ta Gen (WiFi)");
            modelName = modelName.Replace("iPad11,2", "iPad mini 5ta Gen");
            modelName = modelName.Replace("iPad11,3", "iPad Air 3ra Gen (WiFi)");
            modelName = modelName.Replace("iPad11,4", "iPad Air 3ra Gen");
            modelName = modelName.Replace("Watch1,1", "Apple Watch 38mm");
            modelName = modelName.Replace("Watch1,2", "Apple Watch 42mm");
            modelName = modelName.Replace("Watch2,6", "Apple Watch Series 1 38mm");
            modelName = modelName.Replace("Watch2,7", "Apple Watch Series 1 42mm");
            modelName = modelName.Replace("Watch2,3", "Apple Watch Series 2 38mm");
            modelName = modelName.Replace("Watch2,4", "Apple Watch Series 2 42mm");
            modelName = modelName.Replace("Watch3,1", "Apple Watch Series 3 38mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch3,2", "Apple Watch Series 3 42mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch3,3", "Apple Watch Series 3 38mm (GPS)");
            modelName = modelName.Replace("Watch3,4", "Apple Watch Series 3 42mm (GPS)");
            modelName = modelName.Replace("Watch4,1", "Apple Watch Series 4 40mm (GPS)");
            modelName = modelName.Replace("Watch4,2", "Apple Watch Series 4 44mm (GPS)");
            modelName = modelName.Replace("Watch4,3", "Apple Watch Series 4 40mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch4,4", "Apple Watch Series 4 44mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch5,1", "Apple Watch Series 5 40mm (GPS)");
            modelName = modelName.Replace("Watch5,2", "Apple Watch Series 5 44mm (GPS)");
            modelName = modelName.Replace("Watch5,3", "Apple Watch Series 5 40mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch5,4", "Apple Watch Series 5 44mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch5,9", "Apple Watch SE 40mm (GPS)");
            modelName = modelName.Replace("Watch5,10", "Apple Watch SE 44mm (GPS)");
            modelName = modelName.Replace("Watch5,11", "Apple Watch SE 40mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch5,12", "Apple Watch SE 44mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch6,1", "Apple Watch Series 6 40mm (GPS)");
            modelName = modelName.Replace("Watch6,2", "Apple Watch Series 6 44mm (GPS)");
            modelName = modelName.Replace("Watch6,3", "Apple Watch Series 6 40mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch6,4", "Apple Watch Series 6 44mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch6,6", "Apple Watch Series 7 41mm (GPS)");
            modelName = modelName.Replace("Watch6,7", "Apple Watch Series 7 45mm (GPS)");
            modelName = modelName.Replace("Watch6,8", "Apple Watch Series 7 41mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch6,9", "Apple Watch Series 7 45mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch6,10", "Apple Watch SE 40mm (GPS)");
            modelName = modelName.Replace("Watch6,11", "Apple Watch SE 44mm (GPS)");
            modelName = modelName.Replace("Watch6,12", "Apple Watch SE 40mm (GPS+Cellular)");
            modelName = modelName.Replace("Watch6,13", "Apple Watch SE 44mm (GPS+Cellular)");
            return modelName;
        }
        private bool verify_cert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return new X509Certificate2(certificate).Verify();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            this.windowLocation = e.Location;

        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                this.Location = new Point(
                    (this.Location.X - windowLocation.X) + e.X,
                    (this.Location.Y - windowLocation.Y) + e.Y
                );

                this.Update();
            }
        }



        private async void DriversBTN_Click(object sender, EventArgs e)
        {
            DriversBTN.Enabled = false;

            try
            {
                // Use the ORIGINAL working class with renamed methods
                await Task.Run(() => newShetouane.FixAllDrivers());
                CustomMessageBox.Show("All drivers fixed successfully!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DriversBTN.Enabled = true;
            }
        }
        private static void bcdedit(string param)
        {
            StringCollection values = new StringCollection();
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                                       Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess
                                           ? @"Sysnative\bcdedit.exe"
                                           : @"System32\bcdedit.exe"),
                    Arguments = param,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                }
            };
            proc.Start();

            string messagtxt = proc.StandardOutput.ReadToEnd();




        }
        private void TestModeBTN_Click(object sender, EventArgs e)
        {
            bcdedit("/set testsigning on");
            bcdedit("/set nointegritychecks on");
            CustomMessageBox.Show("Tested Mode has been Enabled Successfully!\n" +
              "Now Reboot Windows, for it to take effect.");
        }



        private bool IsDeviceAlreadyPwned()
        {
            string status = JBLBL.Text;
            return status == "GASTER" || status == "CHECKM8" ||
                   status == "SKRPWN" || status == "IKEYM8";
        }

        private void UpdateJailbreakStatus(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => JBLBL.Text = status));
            }
            else
            {
                JBLBL.Text = status;
            }
        }

        // For backward compatibility
        public async Task PWNDFU()
        {
            if (JBLBL.Text == "GASTER" || JBLBL.Text == "CHECKM8" ||
                JBLBL.Text == "SKRPWN" || JBLBL.Text == "IKEYM8")
            {
                statux2("Device Already PWND!");
                return;
            }

            PWNMan.Enabled = false;

            try
            {
                // Use the original class with renamed methods
                var driverManager = new newShetouane();

                // Run gaster pwn with cleanup (this does everything: install libusbK -> run gaster -> restore Apple DFU)
                await Task.Run(() => driverManager.RunGasterPwnWithCleanup());

                // Check if device is in pwndfu mode
                bool isPwnd = await CheckIfDeviceIsPwndAsync();

                if (isPwnd)
                {
                    JBLBL.Text = "GASTER";
                    statux2("✅ Device is now in pwndfu mode!");
                }
                else
                {
                    statux2("❌ Failed to enter pwndfu mode");
                }
            }
            catch (Exception ex)
            {
                statux2($"Error: {ex.Message}");
            }
            finally
            {
                PWNMan.Enabled = true;
            }
        }
        private async Task<bool> CheckIfDeviceIsPwndAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Use irecovery to check if device is pwnd
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = Environment.CurrentDirectory + "\\x64\\irecovery.exe",
                        Arguments = "-q",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return output.Contains("PWND:") ||
                           output.Contains("PWND: GASTER") ||
                           output.Contains("PWND: CHECKM8");
                }
                catch
                {
                    return false;
                }
            });
        }


        private void PWNDFUBTN_Click(object sender, EventArgs e)
        {

            Process.Start("devmgmt.msc");
        }

        private void MDMBTN_Click(object sender, EventArgs e)
        {
            if (ActLBL.Text.ToLower() == "unactivated" || ActLBL.Text == "NA")
            {
                CustomMessageBox.Show("Please Activate Device via 3utools first!\r\n" +
                     "then Reconnect USB in Normal Mode!", "Device not Activated", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            else
            {
                try
                {
                    string snx = SNLBL.Text;
                    string ecidx = ECIDLBL.Text;
                    checker check = new checker();

                    if (!check.checkMDM(snx))
                    {
                        throw new Exception("Device Not Registered!");
                    }
                    MDMBTN.Enabled = false;
                    Thread MDMx = new Thread(DoMDM);
                    MDMx.IsBackground = true;
                    MDMx.Start();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + SNLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    MDMBTN.Enabled = true;
                }

            }



        }
        public void zipass(string path, string outpath)
        {
            try
            {


                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(path))
                {

                    zip.Password = "Shetouane@iSkorpion";

                    zip.ExtractAll(outpath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                }

            }
            catch (Exception exzk)
            {
                statux2(exzk.Message);
                System.IO.Directory.Delete(outpath, true);
            }
        }

        public bool Pair(string argument)
        {
            string path = Directory.GetCurrentDirectory();
            Process proc;

            try
            {
                if (argument == "pair")
                {
                    proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = path + @"\\x64\\idevicepair.exe",
                            Arguments = "pair",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                }
                else
                {
                    proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = path + @"\\x64\\idevicepair.exe",
                            Arguments = "validate",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                }

                try
                {
                    proc.Start();
                    StreamReader reader = proc.StandardOutput;
                    string result = reader.ReadToEnd();

                    Thread.Sleep(2000);
                    try { proc.Kill(); }
                    catch { }
                    if (result.Contains("SUCCESS"))
                    {
                        reader.Dispose();
                        return true;
                    }
                    else { return false; }
                }
                catch { }

            }
            catch (System.ComponentModel.Win32Exception)
            {
                CustomMessageBox.Show("Idevicepair not found");
                return false;
            }

            return false;
        }

        public void PairLoop()
        {
            //Pairing
            if (Pair("pair") == false)
            {
                statux2("Please click trust device on iPhone!");
            }
        }
        public void p(int number, string message = "")
        {

            Invoke(new MethodInvoker(delegate ()
             {
                 lblProgress.Text = message + " %";
                 progressBar1.Value = number;
                 //  statux2(message);
             }));

        }
        public void DoMDM()
        {
            try
            {
                MDMBTN.Enabled = false;


                p(5);

                try
                {


                    PairLoop();
                    progx(10, "Pairloop Valid!");

                    ResetTemporaryFolder();
                    progx(20, "Wait Please...");

                    command1();
                    progx(30, "Generating Files..");

                    command2();
                    progx(60, "Generating Files...");

                    info();
                    progx(80, "Generating Files Server...");

                    manifest();
                    progx(84, "Removing MDM");

                    command3();
                    progx(90, "Preparing Device...");

                    ResetTemporaryFolder();
                    progx(96, "Wait Please...");


                    progx(100, "Successfully MDM!");
                    ENN();
                    CleanTMP();
                }
                catch (Exception ex)
                {
                    ENN();
                    statux2(ex.Message);
                    CleanTMP();
                    return;
                }

                CustomMessageBox.Show("Your Phone Has beeen Successfully MDM Unlocked !\n\nnow will be full restoring. Wait until the restoring process finishes\nWhen it finishes, you can setup the device as Without modem managament lock \r\n" +
                     "Thanks Using our Software ! ", "Successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ENN();
                CleanTMP();
                statux2(ex.Message);
            }
        }

        public void ENN()
        {
            MDMBTN.Enabled = true;
            p(0, "Done");
        }
        public void UploadAntiResetSettings()
        {
            try
            {
                if (iOSLBL.Text.StartsWith("13.") || iOSLBL.Text.StartsWith("14."))
                {
                    this.SSH("mkdir -p /System/Library/PrivateFrameworks/Settings/GeneralSettingsUI.framework && chmod -R 777 /System/Library/PrivateFrameworks/Settings/GeneralSettingsUI.framework");
                    this.scpClient.Upload(new MemoryStream(this.byte_9), "/System/Library/PrivateFrameworks/Settings/GeneralSettingsUI.framework/General.plist");
                    this.scpClient.Upload(new MemoryStream(this.byte_10), "/System/Library/PrivateFrameworks/Settings/GeneralSettingsUI.framework/Reset.plist");
                }
                else
                {
                    this.SSH("mkdir -p /System/Library/PrivateFrameworks/PreferencesUI.framework && chmod -R 777 /System/Library/PrivateFrameworks/PreferencesUI.framework");
                    this.scpClient.Upload(new MemoryStream(this.byte_11), "/System/Library/PrivateFrameworks/PreferencesUI.framework/General.plist");
                    this.scpClient.Upload(new MemoryStream(this.byte_12), "/System/Library/PrivateFrameworks/PreferencesUI.framework/Reset.plist");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public void DoBypassiPAD()
        {
            try
            {

                this.progx(5, "");
                try
                {
                    CleanTMP();
                    this.progx(6, "Cleaning Temp Folder...");
                    this.PairLoop();
                    this.progx(8, "Pairing Device...");
                    connectSSHJB();
                    this.progx(10, "Connecting SSH...");
                    this.Mount();
                    this.progx(12, "Mounting Filesystems...");
                    this.FindActivationRoutes();
                    this.progx(14, "Finding Activation Routes....");
                    this.DeactivateX();
                    this.progx(16, "Deleting Substrate...");
                    this.DeleteSubstrate();
                    this.progx(18, "Deleting Old Activation Files...");
                    this.DeleteActivationFiles();
                    this.progx(20, "Restarting Daemons...");
                    this.RestartAllDeamons();
                    this.progx(22, "Running Snappy...");
                    this.SnappyRename();
                    this.progx(24, "Sending Factory Activation Ticket...");
                }
                catch (Exception ex)
                {
                    progx(0, ex.Message);
                    return;
                }

                try
                {
                    this.SSH("cp /System/Library/PrivateFrameworks/MobileActivation.framework/Support/Certificates/FactoryActivation.pem /System/Library/PrivateFrameworks/MobileActivation.framework/Support/Certificates/RaptorActivation.pem");
                    this.progx(26, "Restarting Lockdown");
                    this.RestartLockdown();
                    this.progx(28, "Creating Activation FOlders");
                    this.CreateActivationFolders();
                    this.progx(29, "Pairing Device...");
                    this.PairLoop();
                    this.progx(30, "Running Bypass Service...");
                    bool activateResult = false;
                    int intento = 1;
                    while (!activateResult && intento <= 5)
                    {
                        this.progx(30 + intento, "Checking Bypass Server...");
                        this.DeactivateX();
                        activateResult = this.ActivateX();
                        intento++;
                        Thread.Sleep(1000);
                    }
                    if (!activateResult)
                    {
                        this.ActivateX();
                    }
                    this.progx(36, "");
                }
                catch (Exception ex2)
                {
                    progx(0, ex2.Message);
                    return;
                }
                try
                {
                    this.SSH("chflags nouchg " + this.Route_Purple);
                    this.progx(38, "Finding Activation Routes...");
                    this.FindActivationRoutes();
                    this.progx(42, "Downloading Activation Files...");
                    this.DownloadActivationFiles();
                    this.progx(45, "Creating Activation Records...");
                    this.CreateActivationRecFile();
                    this.progx(48, "Patching Comm Center");
                    this.ModifyCommcenterDsnFile();
                    this.progx(50, "Patching Data Ark ...");
                    this.ModifyDataArkFile();
                    this.progx(55, "Uploading Activation Files...");
                    this.UploadActivationFiles();
                    this.progx(65, "Installing Substrate...");
                }
                catch (Exception ex3)
                {

                    progx(0, ex3.Message);
                    return;
                }

                try
                {
                    this.InstallSubstrate();
                    this.progx(75, "Uploading Anti Reset Settings...");
                    this.UploadAntiResetSettings();
                    this.progx(82, "Disabling OTA Updates");
                    this.DisableOTAUpdates();
                    this.progx(84, "Deleting Logs...");
                    this.DeleteLogs();
                    this.progx(85, "Skipping Setup...");
                    this.SkipSetup();
                    switch (typedevice)
                    {
                        case "GSM":
                            this.Dobbremove();
                            break;
                        default:
                            FixUntethered();
                            break;
                    }
                    Dobbremove();
                    this.CleanTMP();
                    this.progx(86, "Cleaning Temp Files...");
                    this.progx(90, "Almost done...");
                }
                catch (Exception ex4)
                {
                    if (progressBar1.Value >= 85)
                    {

                        this.progx(100, "");

                        CustomMessageBox.Show("Your Device " + NameLBL.Text + " Successfully Activated! \nWait Some Secone \nEnjoy Your Device!!", "[iOSTool]", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }
                    progx(0, ex4.Message);
                    return;
                }

                try
                {
                    this.ldrestart1();
                    this.progx(94, "Finishing up...");
                    this.CleanTMP();
                    this.progx(99, "Setting Reboot signal..");
                    this.progx(100, "Bypass Done..");
                }
                catch (Exception)
                {
                    this.CleanTMP();
                    HelloBTNJ.Enabled = true;
                }
                reboot();
                this.CleanTMP();
                CustomMessageBox.Show("Your Device " + NameLBL.Text + " Successfully Activated! \nWait Some Secone \nEnjoy Your Device!!", "[iOSTool]", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HelloBTNJ.Enabled = true;
            }
            catch (Exception ex5)
            {
                HelloBTNJ.Enabled = true;
                progx(0, ex5.Message);
            }
            HelloBTNJ.Enabled = true;
        }
        public void info()
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(MDMLink + "/get_information_mdm.php");

            string imeimdm = IMEILBL.Text;
            string serialNumbermdm = SNLBL.Text;
            string UDIDmdm = UDIDLBL.Text;
            string DeviceTypemdm = ProdLBL.Text;
            string ProductVersionmdm = iOSLBL.Text;
            string BuildVersionmdm = BuildLBL.Text;
            string Devicenamemdm = NameLBL.Text;


            //serial
            var postData = "thing1=" + Uri.EscapeDataString((serialNumbermdm != null) ? (serialNumbermdm ?? "") : "");
            //udid
            postData += "&thing2=" + Uri.EscapeDataString((UDIDmdm != null) ? (UDIDmdm ?? "") : "");
            //producttype
            postData += "&thing3=" + Uri.EscapeDataString((DeviceTypemdm != null) ? (DeviceTypemdm ?? "") : "");
            //productversion
            postData += "&thing4=" + Uri.EscapeDataString((ProductVersionmdm != null) ? (ProductVersionmdm ?? "") : "");
            //productversion
            postData += "&thing5=" + Uri.EscapeDataString((BuildVersionmdm != null) ? (BuildVersionmdm ?? "") : "");
            //productversion
            postData += "&thing6=" + Uri.EscapeDataString((Devicenamemdm != null) ? (Devicenamemdm ?? "") : "");
            //imei
            postData += "&thing7=" + Uri.EscapeDataString((imeimdm != null) ? (imeimdm ?? "") : "");


            var data = Encoding.ASCII.GetBytes(postData);

            request.ProtocolVersion = HttpVersion.Version11;
            request.Accept = "/";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");

            string username = "vandetta";
            string password = "a3NaOWIxZ3VxZ1o5WC9KcGI3VXQzV";

            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            request.UserAgent = "AssetCache/233 CFNetwork/1121.1.2 Darwin/19.3.0 (x86_64)";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            System.IO.File.WriteAllText(ikey + UDIDLBL.Text + "\\" + "Info.plist", responseString);

        }

        public void manifest()
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(MDMLink + "/activatebypass_mdm_full.php");

            string serialNumbermdm = SNLBL.Text;
            string UDIDmdm = UDIDLBL.Text;
            string DeviceTypemdm = ProdLBL.Text;
            string ProductVersionmdm = iOSLBL.Text;
            string BuildVersionmdm = BuildLBL.Text;
            string Devicenamemdm = NameLBL.Text;

            //serial
            var postData = "thing1=" + Uri.EscapeDataString((serialNumbermdm != null) ? (serialNumbermdm ?? "") : "");
            //udid
            postData += "&thing2=" + Uri.EscapeDataString((UDIDmdm != null) ? (UDIDmdm ?? "") : "");
            //producttype
            postData += "&thing3=" + Uri.EscapeDataString((DeviceTypemdm != null) ? (DeviceTypemdm ?? "") : "");
            //productversion
            postData += "&thing4=" + Uri.EscapeDataString((ProductVersionmdm != null) ? (ProductVersionmdm ?? "") : "");
            //productversion
            postData += "&thing5=" + Uri.EscapeDataString((BuildVersionmdm != null) ? (BuildVersionmdm ?? "") : "");
            //productversion
            postData += "&thing6=" + Uri.EscapeDataString((Devicenamemdm != null) ? (Devicenamemdm ?? "") : "");


            var data = Encoding.ASCII.GetBytes(postData);

            request.ProtocolVersion = HttpVersion.Version11;
            request.Accept = "/";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");

            string username = "vandetta";
            string password = "a3NaOWIxZ3VxZ1o5WC9KcGI3VXQzV";

            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            request.UserAgent = "AssetCache/233 CFNetwork/1121.1.2 Darwin/19.3.0 (x86_64)";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            System.IO.File.WriteAllText(ikey + UDIDLBL.Text + "\\" + "Manifest.plist", responseString);

        }
        private static string UserTemporaryFolder = Path.Combine(Environment.CurrentDirectory, "tmp");
        private static string ApplicationTemporaryFolderName = "random";
        private static string ApplicationTemporaryFolder = System.IO.Path.Combine(UserTemporaryFolder, ApplicationTemporaryFolderName);
        public static void ResetTemporaryFolder()
        {
            bool isApplicationTempFolder = System.IO.Directory.Exists(ApplicationTemporaryFolder);

            if (isApplicationTempFolder)
            {

                System.IO.Directory.Delete(ApplicationTemporaryFolder, true);

            }


        }
        private void command1()
        {


            // 
        }

        public void command2()
        {
            zipass(k7loush, ikey + UDIDLBL.Text);
        }

        public string ikey = tmp + "random" + "\\";
        public static string drivers = Environment.CurrentDirectory + "\\Drivers\\";

        public static string k7loush = drivers + "net\\Apple_Mobile_Device_USB.cat";
        public void command3()
        {
            string exePath = ToolDir + "\\X64\\idevicebackup2.exe";
            string udid = UDIDLBL.Text;
            string arguments = $"-u {udid} restore {ikey} --password mdm";

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                statux2("Command3 failed: " + ex.Message);
            }
        }


        public static string dumped_text = "";
        public static string dumper = Environment.CurrentDirectory + "\\x64\\shetouane2.dll";
        public static string AppleIDX = "";
        public static string PETToken = "";



        private global::System.ComponentModel.BackgroundWorker backgroundWorker1;
        private global::System.ComponentModel.BackgroundWorker backgroundWorker2;

        private void RamFolder_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(ToolDir + "\\Ramdisk\\"))
            {
                Directory.CreateDirectory(ToolDir + "\\Ramdisk\\");
            }
            Process.Start(ToolDir + "\\Ramdisk\\");
        }

        private void BackupsBTN_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(ToolDir + "\\Backup\\"))
            {
                Directory.CreateDirectory(ToolDir + "\\Backup\\");
            }
            Process.Start(ToolDir + "\\Backup\\");
        }

        private void BackupDown_Click(object sender, EventArgs e)
        {

            string backupfolder = ToolDir + "\\Backup\\";
            string backupfile = ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip";
            string backupfile2 = ToolDir + "\\Backup\\" + SNLBL.Text + ".zip";
            if (!Directory.Exists(backupfolder))
            {
                Directory.CreateDirectory(backupfolder);
            }

            try
            {

                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    webClient.DownloadFile(PasscodeLink + "/Passcode/" + ECIDLBL.Text + ".zip", backupfile);
                }
                status2.Text += "\r\n iOS 15-16 Backup Found & Downloaded Successfully!. \r\n";
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

            }
            catch
            {
                status2.Text += "\r\n iOS 15 - 16 Backup not found! \r\n";
                status2.ForeColor = System.Drawing.Color.BlueViolet; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

            }

            try
            {

                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    webClient.DownloadFile(PasscodeLink + "/Passcode/" + SNLBL.Text + ".zip", backupfile2);
                }
                status2.Text += "\r\n iOS 12-14 Backup Found & Downloaded Successfully!. \r\n";
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }
            catch
            {
                status2.Text += "\r\n iOS 12 - 14 Backup not found! \r\n";
                status2.ForeColor = System.Drawing.Color.BlueViolet; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                return;
            }
        }


        public void RDButtons(bool hehe)
        {

            HelloBTNR.Enabled = hehe;
            PasscodeBTNR.Enabled = hehe;
            BackupRD.Enabled = hehe;
            EraseBTNR.Enabled = hehe;
            ExitRamdiskBTN.Enabled = hehe;
            LogoutBTNR.Enabled = hehe;
        }

        public void JButtons(bool haha)
        {
            HelloBTNJ.Enabled = haha;
            PasscodeActBTNJ.Enabled = haha;
            PasscodeBackBTNJ.Enabled = haha;
            PatchBTN.Enabled = haha;
            EraseBTNJ.Enabled = haha;
            BrokenBBJ.Enabled = haha;
            EnableBBJ.Enabled = haha;
            DisableBBJ.Enabled = haha;
            LogoutBTNJ.Enabled = haha;
            if (ProdLBL.Text == "iPhone9,3" || ProdLBL.Text == "iPhone9,4" || ProdLBL.Text == "iPhone10,4" || ProdLBL.Text == "iPhone10,5" || ProdLBL.Text == "iPhone10,6")
            {
                GSMBTN.Enabled = haha;
            }
            else
            {
                GSMBTN.Enabled = false;
            }

        }

        public static string UDIDX = "b62f4ce4d71acfea78ec9845983de1700f230c9b";

        private HttpDownloader downloader = null;

        private void DownloaderManX(string url, string targettool)
        {
            try
            {
                // Ensure cleanup of previous downloader
                if (downloader != null)
                {
                    downloader.DownloadInfoReceived -= downloader_DownloadInfoReceived;
                    downloader.DownloadCompleted -= downloader_DownloadCompletedManX;
                    downloader.BeforeSendingRequest -= downloader_BeforeSendingRequest;
                    downloader.ProgressChanged -= downloader_ProgressChanged;
                    downloader.ErrorOccured -= downloader_ErrorOccured;
                    //downloader?.StopReset();
                    downloader = null;
                }

                downloader = new HttpDownloader(url, targettool);

                downloader.DownloadInfoReceived += downloader_DownloadInfoReceived;
                downloader.DownloadCompleted += downloader_DownloadCompletedManX;
                downloader.BeforeSendingRequest += downloader_BeforeSendingRequest;
                downloader.ProgressChanged += downloader_ProgressChanged;
                downloader.ErrorOccured += downloader_ErrorOccured;

                downloader.Start();
            }
            catch (Exception hoha)
            {
                statux2($"Download failed: {hoha.Message}");

            }
        }


        void downloader_ProgressChanged(object sender, AltoHttp.ProgressChangedEventArgs e)
        {
            try
            {
                // Check if controls are accessible (if running in UI thread)
                if (lblTotalBytesReceived.IsDisposed || lblSpeed.IsDisposed)
                    return;

                // Use thread-safe invocation if needed
                if (lblTotalBytesReceived.InvokeRequired)
                {
                    lblTotalBytesReceived.Invoke(new Action(() =>
                    {
                        lblTotalBytesReceived.Text = string.Format("{0} / {1}",
                            e.TotalBytesReceived.ToHumanReadableSize(),
                            downloader?.Info?.Length > 0 ? downloader.Info.Length.ToHumanReadableSize() : "Unknown");
                        lblSpeed.Text = e.SpeedInBytes.ToHumanReadableSize() + "/s";

                        decimal prox = (decimal)e.Progress;
                        p((int)Math.Truncate(prox), Math.Truncate(prox).ToString());
                    }));
                }
                else
                {
                    lblTotalBytesReceived.Text = string.Format("{0} / {1}",
                        e.TotalBytesReceived.ToHumanReadableSize(),
                        downloader?.Info?.Length > 0 ? downloader.Info.Length.ToHumanReadableSize() : "Unknown");
                    lblSpeed.Text = e.SpeedInBytes.ToHumanReadableSize() + "/s";

                    decimal prox = (decimal)e.Progress;
                    p((int)Math.Truncate(prox), Math.Truncate(prox).ToString());
                }
            }
            catch (Exception ex)
            {
                // Silent fail for UI updates to prevent crashes
                System.Diagnostics.Debug.WriteLine($"Progress update error: {ex.Message}");
            }
        }
        void downloader_ErrorOccured(object sender, ErrorEventArgs e)
        {
            statux2($"Download error: {e.GetException().Message}");
            //CleanupDownloader();
        }
        void downloader_BeforeSendingRequest(object sender, BeforeSendingRequestEventArgs e)
        {
            try
            {
                // Safe null check with thread safety
                var msg = Program.MSG;
                if (msg?.Headers != null)
                {
                    var request = (HttpWebRequest)e.Request;
                    request.SetHeaders(msg.Headers);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Header error: {ex.Message}");
            }
        }
        void downloader_DownloadInfoReceived(object sender, EventArgs e)
        {

            try
            {
                var saveDirectory = Path.GetDirectoryName(downloader.FullFileName);
                var serverFileName = downloader.Info.ServerFileName;

                var newFilePath = Path.Combine(saveDirectory, serverFileName);
                statux2("Preparing Files..");
                downloader.FullFileName = newFilePath;




            }
            catch (Exception ex1)
            {
                statux2(ex1.Message);
            }

        }
        private void DownloadWinrain(string url, string targettool)
        {
            try
            {
                if (Program.MSG == null)
                {

                    downloader = new HttpDownloader(url, targettool);

                    downloader.DownloadInfoReceived += downloader_DownloadInfoReceived;
                    downloader.DownloadCompleted += downloader_DownloadCompletedWinrain;
                    downloader.BeforeSendingRequest += downloader_BeforeSendingRequest;
                    downloader.ProgressChanged += downloader_ProgressChanged;
                    downloader.ErrorOccured += downloader_ErrorOccured;

                    downloader.Start();

                }


            }
            catch (Exception hoha)
            {
                statux2(hoha.Message);
            }
        }
        public void KilliProxy()
        {
            int num = 0;
            num += 1000;
            Process[] processesByName = Process.GetProcessesByName("iproxy.exe");
            for (int i = 0; i < processesByName.Length; i++)
            {
                processesByName[i].Kill();
            }

            if (System.IO.File.Exists("%USERPROFILE%\\.ssh\\known_hosts"))
            {
                System.IO.File.Delete("%USERPROFILE%\\.ssh\\known_hosts");
            }
        }

        public void StartiProxy(int local, int remote)
        {

            int num = 0;
            num += 1000;
            Process[] processesByName = Process.GetProcessesByName("iproxy");
            for (int i = 0; i < processesByName.Length; i++)
            {
                processesByName[i].Kill();
            }
            Thread.Sleep(2000);
            if (System.IO.File.Exists("%USERPROFILE%\\.ssh\\known_hosts"))
            {
                System.IO.File.Delete("%USERPROFILE%\\.ssh\\known_hosts");
            }
            using (Process process = new Process())
            {
                process.StartInfo.FileName = Libs + "iproxy.exe";
                process.StartInfo.Arguments = remote + " " + local;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

            }
        }

        private void CleanupConnections()
        {
            try
            {
                scpClient?.Dispose();
                sshClient?.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
            finally
            {
                scpClient = null;
                sshClient = null;
            }
        }

        public void ConnectSSH(int remote, int port)
        {
            try
            {
                // Clean up any existing connections
                CleanupConnections();

                AuthenticationMethod[] array = new AuthenticationMethod[]
                {
            new PasswordAuthenticationMethod("root", "alpine")
                };

                ConnectionInfo connectionInfo = new ConnectionInfo("127.0.0.1", port, "root", array);
                connectionInfo.Timeout = TimeSpan.FromSeconds(15.0); // Increased timeout

                sshClient = new SshClient(connectionInfo);
                scpClient = new ScpClient(connectionInfo);

                sshClient.Connect();
                scpClient.Connect();
            }
            catch
            {
                CleanupConnections();
                //
            }
        }

        public string SSH(string command)
        {
            // Validate input parameter
            if (string.IsNullOrWhiteSpace(command))
            {
                JBLBL.Text = "NA";
                return "XD";
            }

            // Check SSH client instance
            if (this.sshClient == null)
            {
                JBLBL.Text = "NA";
                return "XD";
            }

            if (!this.sshClient.IsConnected)
            {
                try
                {
                    this.sshClient.Connect();
                }
                catch
                {
                    // Log the exception if needed
                    JBLBL.Text = "NA";
                    return "XD";
                }
            }

            SshCommand sshCommand = null;
            string result = "XD";

            try
            {
                sshCommand = this.sshClient.CreateCommand(command);
                sshCommand.Execute();

                // Handle null result from command execution
                result = sshCommand.Result ?? string.Empty;

                // If you want to return "XD" for empty results, uncomment:
                // if (string.IsNullOrEmpty(result)) result = "XD";
            }
            catch
            {
                // Log the exception if needed
                try
                {
                    if (this.sshClient?.IsConnected == true)
                    {
                        this.sshClient.Disconnect();
                    }
                }
                catch
                {
                    // Ignore disconnect errors
                }
                result = "XD";
            }
            finally
            {
                // Dispose of the command if it implements IDisposable
                (sshCommand as IDisposable)?.Dispose();
            }

            return result;
        }
        public void ExtractRamdisk()
        {
            if (RD15.Checked)
            {
                if (CPIDLBL.Text.Contains("8003"))
                {
                    ramdiskFile = ramdiskFolder + ProdLBL.Text + "_2.zip";
                }
                else
                {
                    ramdiskFile = ramdiskFolder + ProdLBL.Text + ".zip";
                }

            }
            if (RD16.Checked)
            {
                ramdiskFile = ramdiskFolder + ProdLBL.Text + "-" + HardLBL.Text + "-16.x.zip";
            }
            if (RD17.Checked)
            {
                ramdiskFile = ramdiskFolder + ProdLBL.Text + "-" + HardLBL.Text + "-17.x.zip";
            }
            statux2("Cleaning Temporary Files...");
            cleantmp();
            statux2("Extracting Ramdisk Files...");
            zipass(ramdiskFile, tmp + ProdLBL.Text);
            statux2("Extraction Complete...");

        }

        public void BootRamdisk()
        {
            string tmpx = tmp + ProdLBL.Text;
            string ldevicetree = tmpx + "\\devicetree.img4";
            string ltrustcache = tmpx + "\\trustcache.img4";
            string lramdisk = tmpx + "\\ramdisk.img4";
            string lkernel = tmpx + "\\kernelcache.img4";
            string tst = tmpx + "\\kernel.img4";
            string tst2 = tmpx + "\\kernelcache.img4";
            string cpidx = CPIDLBL.Text;
            string assetx = ToolDir + "\\assets\\" + cpidx + ".asset";
            if (System.IO.File.Exists(tst))
            {
                lkernel = tmpx + "\\kernel.img4";
            }
            if (System.IO.File.Exists(tst2))
            {
                lkernel = tmpx + "\\kernelcache.img4";
            }
            string libec = tmpx + "\\ibec.img4";
            string libss = tmpx + "\\ibss.img4";
            string shsh_shsh = tmpx + "\\shsh.shsh";
            string sepfw = tmpx + "\\sep-fw.img4";

            if (System.IO.File.Exists(shsh_shsh))
            {
                SendBoot(shsh_shsh, "Sending SHSH..", 10);
            }
            SendBoot(libss, "Sending iBSS..", 15);
            SendBoot(libss, "Sending iBSS...", 20);
            SendBoot(libec, "Sending iBEC..", 25);
            SendCMD("go", "Sending iBEC...", 30);
            if (System.IO.File.Exists(assetx))
            {
                SendBoot(assetx, "Sending Boot Logo...", 32);
                SendCMD("\"setpicture 0\"", "Loading Boot Logo...", 33);
                SendCMD("\"bgcolor 0 0 0\"", "Applying Boot Logo...", 34);
            }
            SendBoot(ldevicetree, "Sending Device Tree..", 35);
            SendCMD("devicetree", "Sending Device Tree...", 40);
            SendBoot(lramdisk, "Sending Ramdisk..", 45);
            SendCMD("ramdisk", "Sending Ramdisk...", 50);
            if (System.IO.File.Exists(sepfw))
            {
                SendBoot(sepfw, "Sending SEP Firmware..", 55);
                SendCMD("rsepfirmware", "Sending SEP Firmware...", 60);
            }
            SendBoot(ltrustcache, "Sending TrustCache..", 70);
            SendCMD("firmware", "Sending TrustCache...", 80);
            SendBoot(lkernel, "Sending KernelCache..", 90);
            SendCMD("bootx", "Sending KernelCache...", 100);
        }

        public void SendCMD(string cmd, string message, int prog)
        {
            try
            {
                //CustomMessageBox.Show(nooo);
                Process process = new Process();

                process.StartInfo.FileName = Libs + "irecovery.exe";
                process.StartInfo.Arguments = "-c " + cmd;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Thread.Sleep(3000);
                progx(prog, message);

            }
            catch (Exception ex)
            {
                statux2(ex.Message);
            }
        }
        public void SendBoot(string fileX, string message, int prog)
        {
            try
            {
                //CustomMessageBox.Show(nooo);
                Process process = new Process();
                new Process();
                process.StartInfo.FileName = Libs + "irecovery.exe";
                process.StartInfo.Arguments = "-f " + '\u0022' + fileX + '\u0022';
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Thread.Sleep(3000);
                progx(prog, message);

            }
            catch (Exception ex)
            {
                statux2(ex.Message);
            }
        }
        public static string tmp = ToolDir + "\\tmp\\";
        public void progx(int prog, string message)
        {
            progressBar1.Value = prog;
            statux2(message);
        }
        public void cleantmp()
        {
            string dir = tmp + ProdLBL.Text;

            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }

                // Wait for the directory to be truly gone (poll instead of sleep)
                int retries = 0;
                while (Directory.Exists(dir) && retries++ < 10)
                {
                    Thread.Sleep(300); // total max ~3s
                }

                if (Directory.Exists(dir))
                {
                    throw new Exception("Temporary directory not fully deleted: " + dir);
                }
            }
            catch (Exception ex)
            {
                statux2("Error cleaning temp: " + ex.Message);
                //throw;
            }
        }

        void downloader_StatusChanged(object sender, StatusChangedEventArgs e)
        {

        }
        void downloader_DownloadCompletedMan(object sender, EventArgs e)
        {
            statux2("Ramdisk Prepared successfully! Now Boot device!");

            progressBar1.Value = 0;
            BootX();

        }
        void downloader_DownloadCompletedManX(object sender, EventArgs e)
        {
            try
            {
                // Verify the download actually completed successfully
                if (downloader?.Info == null)
                {
                    statux2("Download completed but info is missing");
                    return;
                }

                // Verify the file exists and has content before proceeding
                if (string.IsNullOrEmpty(downloader.FullFileName) || !File.Exists(downloader.FullFileName))
                {
                    statux2("Download completed but file not found");
                    return;
                }

                // Check file size to ensure it's not empty
                FileInfo fileInfo = new FileInfo(downloader.FullFileName);
                if (fileInfo.Length == 0)
                {
                    statux2("Download completed but file is empty");

                    return;
                }

                statux2("Ramdisk Prepared successfully! Now Boot device!");
                progressBar1.Value = 0;

                // Proceed with boot process
                BootXMan();
            }
            catch (Exception ex)
            {
                statux2($"Boot preparation failed: {ex.Message}");

            }
            finally
            {
                // 
            }
        }
        void downloader_DownloadCompletedWinrain(object sender, EventArgs e)
        {
            statux2("Jailbreak Downloaded, Starting Jailbreak tool now!");
            button3.Enabled = true;
            progressBar1.Value = 0;
            zipass(ToolDir + "\\x64\\WinRa1n2.1.zip", ToolDir + "\\x64\\");
            Winra1n();

        }

        public void Winra1n()
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\x64\\WinRa1n2.1.exe",
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
            process.WaitForExit();
            button3.Enabled = true;
        }
        private void RD15_CheckedChanged(object sender, EventArgs e)
        {
            PWNMan.Enabled = true;
            ManBootBTN.Enabled = true;
        }

        private void RD16_CheckedChanged(object sender, EventArgs e)
        {
            PWNMan.Enabled = true;
            ManBootBTN.Enabled = true;
        }

        private void RD17_CheckedChanged(object sender, EventArgs e)
        {
            PWNMan.Enabled = true;
            ManBootBTN.Enabled = true;
        }

        private void JB1214_CheckedChanged(object sender, EventArgs e)
        {
            JButtons(true);
        }

        private void JB1518_CheckedChanged(object sender, EventArgs e)
        {
            JButtons(false);
            PasscodeActBTNJ.Enabled = true;
            HelloBTNJ.Enabled = true;
            EraseBTNJ.Enabled = true;
            LogoutBTNJ.Enabled = true;
        }

        private void ReadDevice()
        {
            iRec();
            readNormalMode();



            ReadBTN.Enabled = true;
        }
        private void ReadBTN_Click(object sender, EventArgs e)
        {
            ReadBTN.Enabled = false;
            Thread Normal = new Thread(ReadDevice);
            Normal.IsBackground = true;
            Normal.Start();



        }
        public void BootXMan()
        {
            try
            {

                cleantmp();
                Thread.Sleep(2000);
                ExtractRamdisk();
                Thread.Sleep(2000);
                BootRamdisk();
                Thread.Sleep(5000);
                cleantmp();


                progx(0, "Boot Complete.. Now connect SSH");


                RDButtons(true);
            }
            catch (Exception ex)
            {
                statux2(ex.Message);
                cleantmp();
                RDButtons(true);
            }
        }
        public void BootX()
        {
            try
            {
                cleantmp();
                Thread.Sleep(2000);
                ExtractRamdisk();
                Thread.Sleep(2000);
                BootRamdisk();
                Thread.Sleep(2000);
                progx(0, "Boot Complete.. Now connecting SSH");

                connectSSH();
                Thread.Sleep(5000);
                cleantmp();

            }
            catch (Exception ex)
            {
                statux2(ex.Message);
                cleantmp();
                RDButtons(true);
            }

            RDButtons(true);
        }


        // mount partitions

        public void SystemPart()
        {

            part1 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s1");
            part2 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s2");
            part3 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s3");
            part4 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s4");
            part5 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s5");
            part6 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s6");
            part7 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s7");
            part8 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s8");

            try
            {
                if (part1.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1 /mnt1");
                }
                else if (part2.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1s2 /mnt1");
                }
                else if (part3.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1s3 /mnt1");
                }
                else if (part4.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1s4 /mnt1");
                }
                else if (part5.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1s5 /mnt1");
                }
                else if (part6.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1s6 /mnt1");
                }
                else if (part7.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1s7 /mnt1");
                }
                else if (part8.StartsWith("System"))
                {
                    sshell("mount_apfs /dev/disk0s1s8 /mnt1");
                }

            }

            catch
            {
                status2.Text += "\r\n ERROR MOUNTING PARTITIONS!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                return;
            }

        }
        public void PrebootPart()
        {

            part1 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s1");
            part2 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s2");
            part3 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s3");
            part4 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s4");
            part5 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s5");
            part6 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s6");
            part7 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s7");
            part8 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s8");

            try
            {
                if (part1.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1 /mnt6");
                }
                else if (part2.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s2 /mnt6");
                }
                else if (part3.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s3 /mnt6");
                }
                else if (part4.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s4 /mnt6");
                }
                else if (part5.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s5 /mnt6");
                }
                else if (part6.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s6 /mnt6");
                }
                else if (part7.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s7 /mnt6");
                }
                else if (part8.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s8 /mnt6");
                }


            }

            catch
            {
                status2.Text += "\r\n ERROR MOUNTING PARTITIONS!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                return;
            }

        }
        public void xARTPart()
        {

            part1 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s1");
            part2 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s2");
            part3 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s3");
            part4 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s4");
            part5 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s5");
            part6 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s6");
            part7 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s7");
            part8 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s8");


            try
            {

                if (part1.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1 /mnt7");
                }
                else if (part2.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s2 /mnt7");
                }
                else if (part3.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s3 /mnt7");
                }
                else if (part4.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s4 /mnt7");
                }
                else if (part5.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s5 /mnt7");
                }
                else if (part6.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s6 /mnt7");
                }
                else if (part7.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s7 /mnt7");
                }
                else if (part8.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s8 /mnt7");
                }

            }

            catch
            {
                status2.Text += "\r\n ERROR MOUNTING PARTITIONS!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                return;
            }

        }
        public void HardwarePart()
        {

            part1 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s1");
            part2 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s2");
            part3 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s3");
            part4 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s4");
            part5 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s5");
            part6 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s6");
            part7 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s7");
            part8 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk0s1s8");


            try
            {

                if (part1.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1 /mnt4");
                }
                else if (part2.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s2 /mnt4");
                }
                else if (part3.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s3 /mnt4");
                }
                else if (part4.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s4 /mnt4");
                }
                else if (part5.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s5 /mnt4");
                }
                else if (part6.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s6 /mnt4");
                }
                else if (part7.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s7 /mnt4");
                }
                else if (part8.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk0s1s8 /mnt4");
                }

            }

            catch
            {
                status2.Text += "\r\n ERROR MOUNTING PARTITIONS!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                return;
            }

        }

        public void PrebootPart16()
        {

            part1 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s1");
            part2 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s2");
            part3 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s3");
            part4 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s4");
            part5 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s5");
            part6 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s6");
            part7 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s7");
            part8 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s8");

            try
            {
                if (part1.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s1 /mnt6");
                }
                else if (part2.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s2 /mnt6");
                }
                else if (part3.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s3 /mnt6");
                }
                else if (part4.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s4 /mnt6");
                }
                else if (part5.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s5 /mnt6");
                }
                else if (part6.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s6 /mnt6");
                }
                else if (part7.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s7 /mnt6");
                }
                else if (part8.StartsWith("Preboot"))
                {
                    sshell("mount_apfs -R /dev/disk1s8 /mnt6");
                }


            }

            catch
            {
                status2.Text += "\r\n ERROR MOUNTING PARTITIONS!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }

        }
        public string sshexec(string Comando)
        {
            try
            {
                SshCommand sshCommand = sshClient.CreateCommand(Comando);


                sshCommand.Execute();
                Console.WriteLine(sshCommand.Result);
                return sshCommand.Result;


            }
            catch (Renci.SshNet.Common.SshConnectionException)
            {
                status2.Text += "\r\n SSH Failed!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                string errorX = "SSH Failed!";
                return errorX;
            }
            catch (NullReferenceException)
            {
                status2.Text += "\r\n Please make sure to Boot Ramdisk and Connect SSH!\r\n";
                return "Please make sure to Boot Ramdisk and Connect SSH!";
            }
            catch
            {

                return "Error!";
            }
        }
        public void xARTPart16()
        {

            part1 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s1");
            part2 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s2");
            part3 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s3");
            part4 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s4");
            part5 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s5");
            part6 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s6");
            part7 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s7");
            part8 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s8");


            try
            {

                if (part1.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s1 /mnt7");
                }
                else if (part2.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s2 /mnt7");
                }
                else if (part3.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s3 /mnt7");
                }
                else if (part4.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s4 /mnt7");
                }
                else if (part5.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s5 /mnt7");
                }
                else if (part6.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s6 /mnt7");
                }
                else if (part7.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s7 /mnt7");
                }
                else if (part8.StartsWith("xART"))
                {
                    sshell("mount_apfs -R /dev/disk1s8 /mnt7");
                }

            }

            catch
            {
                status2.Text += "\r\n ERROR MOUNTING PARTITIONS!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                return;
            }

        }
        public void HardwarePart16()
        {

            part1 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s1");
            part2 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s2");
            part3 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s3");
            part4 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s4");
            part5 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s5");
            part6 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s6");
            part7 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s7");
            part8 = SSH("/System/Library/Filesystems/apfs.fs/apfs.util -p /dev/disk1s8");


            try
            {

                if (part1.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s1 /mnt4");
                }
                else if (part2.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s2 /mnt4");
                }
                else if (part3.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s3 /mnt4");
                }
                else if (part4.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s4 /mnt4");
                }
                else if (part5.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s5 /mnt4");
                }
                else if (part6.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s6 /mnt4");
                }
                else if (part7.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s7 /mnt4");
                }
                else if (part8.StartsWith("Hardware"))
                {
                    sshell("mount_apfs -R /dev/disk1s8 /mnt4");
                }

            }

            catch
            {
                status2.Text += "\r\n ERROR MOUNTING PARTITIONS!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                return;
            }

        }


        public bool MountParts()
        {
            bool iSkorpion = false;
            string testx = SSH("test -e /mnt2/root && echo Yes || echo No");
            Debug.WriteLine("testing: " + testx);
            if (!testx.Contains("Yes"))
            {
                sshell("mount_filesystems");
                if (!testx.Contains("Yes"))
                {
                    sshell("/sbin/mount_apfs /dev/disk0s1s1 /mnt1");
                    status2.Text += "\r\n MOUNTING SYSTEM PARTITION...\r\n";
                    sshell("test -d /mnt1/System/Library && echo 'YES'");
                    status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

                    xARTPart();

                    status2.Text += "\r\n MOUNTING XART PARTITION...\r\n";
                    status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

                    PrebootPart();
                    status2.Text += "\r\n MOUNTING Preboot PARTITION...\r\n";
                    status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

                    HardwarePart();
                    status2.Text += "\r\n MOUNTING Hardware PARTITION...\r\n";
                    status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();


                    sshell("/usr/libexec/seputil --gigalocker-init");
                    sshell("/usr/sbin/nvram oblit-inprogress=1 rev=2");
                    status2.Text += "\r\n MOUNTING SEP....\r\n";
                    status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                    sshexec("/usr/libexec/seputil --load /mnt6/*/usr/standalone/firmware/sep*");
                    //status2.Text += "\r\n test....\r\n";
                    sshexec("/usr/sbin/nvram -d oblit-inprogress");
                    sshexec("/usr/sbin/nvram -d oblit-inprogress");
                    Thread.Sleep(500);

                    sshell("/sbin/mount_apfs /dev/disk0s1s2 /mnt2");
                }
                if (!testx.Contains("Yes"))
                {
                    sshexec("bash /usr/bin/mount_root");
                    sshexec("bash /usr/bin/mount_data");
                }
                Thread.Sleep(2000);
                string test = SSH("test -e /mnt2/root && echo Yes || echo No");
                Debug.WriteLine("testing: " + test);
                if (test.Contains("Yes"))
                {
                    iSkorpion = true;
                }
            }

            return iSkorpion;



        }

        public bool CheckJB()
        {
            bool iSkorpion = false;

            ConnectSSH(22, 2222);
            string test = SSH("test -e / && echo Yes || echo No");
            if (test.Contains("Yes"))
            {
                iSkorpion = true;
            }
            else
            {
                ConnectSSH(44, 2222);
                string testx = SSH("test -e / && echo Yes || echo No");
                if (testx.Contains("Yes"))
                {
                    iSkorpion = true;
                }
                else
                {
                    iSkorpion = false;
                }

            }
            return iSkorpion;
        }
        public void sshell(string Comando)
        {
            try
            {

                var stream = sshClient.CreateShellStream("xterm", 80, 50, 1024, 1024, 1024);
                Thread.Sleep(100);


                stream.WriteLine(Comando);
                Thread.Sleep(100);

            }
            catch (Renci.SshNet.Common.SshConnectionException)
            {
                status2.Text += "\r\n SSH Failed!\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                string errorX = status2.Text;
                return;

            }
            catch (NullReferenceException)
            {
                status2.Text += "\r\n Please make sure to Boot Ramdisk and Connect SSH!\r\n";
                return;
            }

            catch
            {

                Console.WriteLine("SSH Failed!");
                return;
            }

        }

        private async Task LoadImageWithZoomAsync(float zoomFactor)
        {
            if (string.IsNullOrEmpty(currentProductType)) return;

            string typeIMG = currentProductType.Contains("iPad") ? "iPad" : "iPhone";
            string imageUrl = $"https://statici.icloud.com/fmipmobile/deviceImages-9.0/{typeIMG}/{currentProductType}/online-infobox__3x.png";

            try
            {
                // AddLog($"Loading device image: {typeIMG} - {currentProductType}", Color.Blue);
                using (HttpClient httpClient = new HttpClient())
                {
                    byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                    using (MemoryStream stream = new MemoryStream(imageBytes))
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                        this.Invoke(new Action(() =>
                        {
                            this.pictureBoxDC.BackgroundImageLayout = ImageLayout.Zoom;
                            this.pictureBoxDC.BackgroundImage = new Bitmap(image);
                        }));
                    }
                }
                // AddLog("Device image loaded successfully", Color.Green);
            }
            catch
            {
                //  AddLog($"Failed to load device image: {ex.Message}", Color.Orange);
                // Use default image if loading fails
            }
        }

        public bool MountParts16()
        {
            bool iSkorpion = false;
            string testx = SSH("test -e /mnt2/root && echo Yes || echo No");
            if (!testx.Contains("Yes"))
            {
                sshell("/sbin/mount_apfs /dev/disk1s1 /mnt1");
                status2.Text += "\r\n MOUNTING SYSTEM PARTITION...\r\n";
                sshell("test -d /mnt1/System/Library && echo 'YES'");
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

                xARTPart16();

                status2.Text += "\r\n MOUNTING XART PARTITION...\r\n";
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

                PrebootPart16();
                status2.Text += "\r\n MOUNTING Preboot PARTITION...\r\n";
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();

                HardwarePart16();
                status2.Text += "\r\n MOUNTING Hardware PARTITION...\r\n";
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();


                sshell("/usr/libexec/seputil --gigalocker-init");
                sshell("/usr/sbin/nvram oblit-inprogress=1 rev=2");
                status2.Text += "\r\n MOUNTING SEP....\r\n";
                status2.ForeColor = System.Drawing.Color.Teal; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                sshexec("/usr/libexec/seputil --load /mnt6/*/usr/standalone/firmware/sep*");
                //status2.Text += "\r\n test....\r\n";
                sshexec("/usr/sbin/nvram -d oblit-inprogress");
                sshexec("/usr/sbin/nvram -d oblit-inprogress");
                Thread.Sleep(500);

                sshell("/sbin/mount_apfs /dev/disk1s2 /mnt2");
                Thread.Sleep(3000);
                string test = SSH("test -e /mnt2/root && echo Yes || echo No");
                if (test.Contains("Yes"))
                {
                    iSkorpion = true;
                }
            }

            return iSkorpion;
        }



        private void connectSSHJB()
        {
            StartiProxy(44, 2222);
            Thread.Sleep(1000);

            ConnectSSH(44, 2222);
            string test = SSH("test -e / && echo Yes || echo No");
            if (test.ToLower().Contains("yes"))
            {
                JBLBL.Text = "Checkra1n";
                if (SNLBL.Text == "NA")
                {
                    string result = "NA";
                    string text = SSH("ioreg -l | grep IOPlatformSerialNumber --color=never").Replace("\"", "").Replace("\t", "").Replace(" ", "").Replace("|IOPlatformSerialNumber=", "").Trim();
                    bool flag = text.Length <= 12;
                    if (flag)
                    {
                        result = text;
                    }
                    else
                    {
                        result = text.Remove(0, 13);
                    }
                    SNLBL.Text = result;
                    CustomMessageBox.Show(result);
                }
            }
        }
        private void connectSSH()
        {
            try
            {
                if (ProdLBL.Text.ToLower().Contains("iphone10"))
                {
                    if (!TryConnectSSH(44, 2222))
                    {
                        throw new Exception("Failed to establish SSH connection on any port");
                    }
                }
                else
                {
                    if (!TryConnectSSH(22, 2222) && !TryConnectSSH(44, 2222))
                    {
                        if (!TryConnectSSH(44, 2222))
                        {

                            throw new Exception("Failed to establish SSH connection on any port");
                        }
                    }
                }


                if (RD15.Checked)
                {
                    if (MountParts())
                    {
                        SCP(Libs + "plutil.dll", "/mnt2/tmp/plutil");
                        SSH("chmod +x /mnt2/tmp/plutil");
                        UDIDLBL.Text = SSH("/mnt2/tmp/plutil -key ArcadeDeviceGUID /mnt2/mobile/Library/Preferences/com.apple.appstored.plist").Replace("\t", "").Replace(" ", "").Trim();
                        string asset = Libs + "iskorpion.dll";
                        SCP(asset, "/mnt2/tmp/sn");
                        SSH("chmod +x /mnt2/tmp/sn");

                        string test = SSH("/mnt2/tmp/sn");
                        SNLBL.Text = test.Trim();
                        UCIDLBL.Text = UCIDx;
                        statux2("Partitions Mounted Successfully!");
                    }
                    else
                    {
                        throw new System.Exception("Error Mounting Partitions!");
                    }
                }
                else
                {
                    if (MountParts16())
                    {
                        SCP(Libs + "plutil.dll", "/mnt2/tmp/plutil");
                        SSH("chmod +x /mnt2/tmp/plutil");
                        UDIDLBL.Text = SSH("/mnt2/tmp/plutil -key ArcadeDeviceGUID /mnt2/mobile/Library/Preferences/com.apple.appstored.plist").Replace("\t", "").Replace(" ", "").Trim();
                        string asset = Libs + "iskorpion.dll";
                        SCP(asset, "/mnt2/tmp/sn");
                        SSH("chmod +x /mnt2/tmp/sn");

                        string test = SSH("/mnt2/tmp/sn");
                        SNLBL.Text = test.Trim();
                        UCIDLBL.Text = UCIDx;
                        statux2("Partitions Mounted Successfully!");
                    }
                    else
                    {
                        throw new System.Exception("Error Mounting Partitions!");
                    }
                }
            }
            catch
            {
                //statux2(ex.Message);
                ConnectSSHBTN.Enabled = true;
                RDButtons(true);
            }
            finally
            {
                ConnectSSHBTN.Enabled = true;
                RDButtons(true);
            }
        }

        private bool TryConnectSSH(int remote, int port, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Ensure iProxy is started and ready
                    StartiProxy(remote, port);
                    Thread.Sleep(2000); // Wait for iProxy to stabilize

                    ConnectSSH(remote, port);

                    if (sshClient?.IsConnected == true)
                    {
                        statux2($"SSH connected successfully on attempt {attempt}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    statux2($"Attempt {attempt}/{maxRetries} failed: {ex.Message}");
                    if (attempt < maxRetries)
                    {
                        Thread.Sleep(1000 * attempt); // Exponential backoff
                        CleanupConnections(); // Clean up failed connection
                    }
                }
            }
            return false;
        }

        public string PostSISV()
        {
            UDIDX = UDIDLBL.Text;
            try
            {
                if (ProdLBL.Text == "NA" || UDIDLBL.Text == "NA")
                {
                    throw new Exception("Please Connect iDevice in Normal Mode first!\\r\n" +
                        "Make Sure Device info are read");
                }
                var request = (HttpWebRequest)WebRequest.Create(XSNLink + "/sisv.php");

                var postData = "product=" + Uri.EscapeDataString(ProdLBL.Text);
                postData += "&udid=" + Uri.EscapeDataString(UDIDX);
                postData += "&ecid=" + Uri.EscapeDataString(ECIDLBL.Text);

                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public void RamdiskGestalt()
        {
            RDButtons(false);
            connectSSH();
            string asset = Libs + "adb.dll";
            string gestalt = ToolDir + "\\x64\\iTunes.dll";
            string certx = ToolDir + "\\x64\\iSkorpion.plist";
            string bkpx = ToolDir + "\\Backup\\" + ECIDLBL.Text;
            if (Directory.Exists(bkpx))
            {
                Directory.Delete(bkpx, true);
            }

            if (!Directory.Exists(ToolDir + "\\Backup\\"))
            {
                Directory.CreateDirectory(ToolDir + "\\Backup\\");
            }
            if (!Directory.Exists(bkpx))
            {
                Directory.CreateDirectory(bkpx);
            }
            try
            {
                progressBar1.Value = 10;
                progx(10, "Generating Activation..");

                // ActivationX();

                PostSISV();
                progx(20, "Downloading Records..");
                PostGestalt();
                MakeGestaltR();
                MakeAct();
                progx(30, "Extracting Activation..");
                // iServicesFix();
                Thread.Sleep(3000);


                Thread.Sleep(3000);
                SSH("rm /mnt2/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/com.apple.MobileGestalt.plist");
                progx(40, "Patching Device..");
                SSH("mv /mnt6/*/usr/local/standalone/firmware/Baseband /mnt6/*/usr/local/standalone/firmware/Baseband2");

                progx(50, "Sending Dylibs to device..");
                SSH("mkdir -p /mnt2/tmp/");
                SCP(tmp + ECIDLBL.Text + ".plist", "/mnt2/tmp/com.apple.MobileGestalt.plist");

                SSH("mv -f /mnt2/tmp/com.apple.MobileGestalt.plist /mnt2/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/");
                SSH("chflags uchg /mnt2/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/com.apple.MobileGestalt.plist");

                progx(60, "Blocking OTA..");
                SSH("plutil -key com.apple.OTATaskingAgent -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.obliteration -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.softwareupdated -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.softwareupdateservicesd -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                SSH("rm -rf /mnt2/log/*");

                progx(70, "Fixing iServices..");
                //FindActivationRecord();
                progx(80, "Removing Cache Files..");

                Thread.Sleep(1000);
                SCP(bkpx + "\\IC-Info.sisv", "/mnt2/tmp/IC-Info.sisv");
                SSH("mkdir -p /mnt2/tmp/activation_records");
                SCP(bkpx + "\\activation_record.plist", "/mnt2/tmp/activation_records/activation_record.plist");
                SCP(bkpx + "\\com.apple.commcenter.device_specific_nobackup.plist", "/mnt2/tmp/com.apple.commcenter.device_specific_nobackup.plist");
                string text2 = SSH("find /mnt2/containers/Data/System -iname 'internal'").Replace("Library/internal", "").Replace("\n", "").Replace("//", "/");
                SSH("rm -rf " + text2 + "/Library/activation_records");
                SSH("rm -rf /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("mv -v /mnt2/tmp/com.apple.commcenter.device_specific_nobackup.plist  /mnt2/wireless/Library/Preferences/");
                SSH("chown mobile:mobile /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chmod 755 /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chflags uchg /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("mv -v  /mnt2/tmp/activation_records " + text2 + "/Library/activation_records"); //shiiiiiiiiiii
                SSH("chmod 755 " + text2 + "/Library/activation_records/activation_record.plist");
                SSH("chflags uchg " + text2 + "/Library/activation_records/activation_record.plist");
                progx(90, "Fixing Permissions..");
                SSH("rm -rf /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes");
                SSH("mkdir -m 755 -p /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes");
                SSH("mv -f /mnt2/tmp/IC-Info.sisv /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("chmod -R 755 /mnt2/mobile/Library/FairPlay/");
                SSH("chown -R mobile:mobile /mnt2/mobile/Library/FairPlay");
                SSH("chmod 664 /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                progx(95, "Skipping Setup..");

                SkipSetupX();
                Thread.Sleep(1000);
                progx(100, "Bypass Done..");
                SSH("kill 1");

                CleanTMP();
                CleanBackup15();
                progx(0, "");
                RDButtons(true);
            }
            catch (Exception ex)
            {
                progx(0, ex.Message);
                CleanTMP();
                RDButtons(true);

            }


        }



        private void HelloBTNR_Click(object sender, EventArgs e)
        {
            try
            {
                string snx = SNLBL.Text;
                string ecidx = ECIDLBL.Text;
                checker check = new checker();

                if (!check.checkhello(ecidx))
                {
                    throw new Exception("Device Not Registered!");
                }

                MessageBoxManager.Yes = "No SN Change";

                MessageBoxManager.No = "SN Change";
                DialogResult dialogResult = CustomMessageBox.Show("Which Bypass do you want?\r\n" +
                          "\r\n- No SN Change Change ( iPhone Original Serial Number )\r\n" +
                          "- SN Change (Use Purple Mode to set Serial Number to: C39LN8QDFRC9", "Confirm",
                      MessageBoxButtons.YesNo,
                      MessageBoxIcon.Question,
                      yesText: "No SN Change",
                      noText: "SN Change");
                if (dialogResult == DialogResult.Yes)
                {
                    RDButtons(false);
                    Thread rdx = new Thread(RamdiskGestalt);
                    rdx.IsBackground = true;
                    rdx.Start();
                }
                if (dialogResult == DialogResult.No)
                {
                    Thread rdx = new Thread(RamdiskHello);
                    rdx.IsBackground = true;
                    rdx.Start();
                }
                else
                {

                    MessageBoxManager.Yes = "Yes";
                    MessageBoxManager.No = "No";
                }
                RDButtons(true);
                CleanTMP();
                MessageBoxManager.Yes = "Yes";
                MessageBoxManager.No = "No";
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + ECIDLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //  statux2("Please Register: " + ECIDLBL.Text + "\r\n at: https://iskorpion.com/post/12_ios-15-17-ramdisk-hello-bypass-no-signal.html");
                RDButtons(true);
                CleanTMP();
            }

        }

        public void EraseRamdisk()
        {
            SSH("/usr/sbin/nvram oblit-inprogress=1 rev=2");

            SSH("kill 1");
        }
        private void EraseBTNR_Click(object sender, EventArgs e)
        {
            EraseBTNR.Enabled = false;

            if (ProdLBL.Text.Contains("iPhone10") & (RD16.Checked))
            {
                ConnectSSH(44, 2222);
            }
            else
            {
                ConnectSSH(22, 2222);
            }
            if (sshClient.IsConnected)
            {
                EraseRamdisk();
            }
            else
            {
                statux2("SSH Not Connected!");
            }

        }

        public void SCP(string localFile, string remoteFile)
        {
            Stream stream = System.IO.File.Open(localFile, FileMode.Open);
            Stream stream2 = stream;
            this.scpClient.Upload(stream2, remoteFile);
            stream2.Close();
        }
        public static string plistu = Environment.CurrentDirectory + "\\x64\\plistutil.exe";
        public void Plistx(string source, string output)
        {
           
            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = plistu,
                    Arguments = $"-i \"{source}\" -o \"{output}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                process.Start();
                process.WaitForExit();

            }
            catch (Exception hhs)
            {
               // statux2(hhs.Message);
            }
            Debug.WriteLine("Created Plist");

        }
        public string DeleteLines(string str, int linesToRemove)
        {
            return str.Split(Environment.NewLine.ToCharArray(), linesToRemove + 1).Skip(linesToRemove).FirstOrDefault<string>();
        }
        public string GetPlistProperty(NSDictionary plist, string NombreObjeto, int LineasArriba = 4)
        {
            NSObject nsobject;
            plist.TryGetValue(NombreObjeto, out nsobject);
            return DeleteLines(nsobject.ToXmlPropertyList().ToString(), LineasArriba).Replace("\n", "").Replace("\r", "").Replace("</data>", "").Replace("</plist>", "").Replace("</string>", "").Replace("<string>", "").Trim();
        }
        public void GenerateFP()
        {
            string ActDir = ToolDir + "\\Backup\\" + ECIDLBL.Text;
            string dirFP = ActDir + "\\FairPlay\\iTunes_Control\\iTunes\\";
            string actrec = ActDir + "\\activation_record.plist";
           


            // Only convert if it's binary plist
            string plistToParse = actrec; // Default to original file
            bool isBin = IsBinaryPlist(actrec);
            if (isBin)
            {
                Debug.WriteLine("File is binary, converting to XML...");
                try
                {
                    Plistx(actrec, actrec);
                    plistToParse = actrec; // Use converted file
                } catch
                {
                    //
                }
                
            }
            else
            {
                Debug.WriteLine("File is already XML, using original file");
                plistToParse = actrec; // Use original file directly
            }

            Debug.WriteLine("Before XML");
            NSDictionary xml = (NSDictionary)PropertyListParser.Parse(plistToParse);
            Debug.WriteLine("XML parsed successfully");

            try
            {
                string resu = GetPlistProperty(xml, "FairPlayKeyData");
                string clean = resu.Replace("<plist version=\"1.0\"><data>", "");
                Console.WriteLine(clean);
                Debug.WriteLine("Plist read");
                byte[] FPT = Convert.FromBase64String(clean);
                Debug.WriteLine("Plist to Byte");
                Debug.WriteLine(FPT);
                string FpDec = Encoding.UTF8.GetString(FPT);
                Console.WriteLine(FpDec + "\n \n");
                string rem1 = FpDec.Replace("-----BEGIN CONTAINER-----", "");
                string rem2 = rem1.Replace("-----END CONTAINER-----", "");
                Console.WriteLine(rem2 + "\n \n");

                byte[] rem3 = Convert.FromBase64String(rem2);
                string finalfp = Encoding.ASCII.GetString(rem3);
                Console.WriteLine("this is final: \n " + finalfp);
                if (!Directory.Exists(dirFP))
                {
                    Directory.CreateDirectory(dirFP);
                }
                string pathx = ActDir + "\\FairPlay\\iTunes_Control\\iTunes\\IC-Info.sisv";
                string pathy = ActDir + "\\FairPlay\\iTunes_Control\\iTunes\\IC-Info.sidv";
                if (!System.IO.File.Exists(pathx))
                {
                    System.IO.File.WriteAllBytes(pathx, rem3);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


        }

        // Method to check if plist is binary format
        private bool IsBinaryPlist(string filePath)
        {
            try
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] header = new byte[8];
                    int bytesRead = stream.Read(header, 0, 8);

                    // Binary plist files start with "bplist"
                    if (bytesRead >= 8)
                    {
                        string fileHeader = Encoding.ASCII.GetString(header);
                        return fileHeader.StartsWith("bplist");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking plist format: {ex.Message}");
            }

            return false;
        }


        public void PasscodeRamdisk()
        {
            string asset = assetsDir + "mobile.asset";
            string ECIDX = ECIDLBL.Text;
            string shetouanetooldir = ToolDir;
            string backupfolder = ToolDir + "\\Backup\\" + ECIDX + "\\";
            string backupfile = shetouanetooldir + "\\Backup\\" + ECIDX + ".zip";
            string fairp = backupfolder + "FairPlay\\iTunes_Control\\iTunes\\";

            if (Directory.Exists(backupfolder))
            {
                Directory.Delete(backupfolder, true);
            }
            if (!Directory.Exists(shetouanetooldir + "\\Backup\\"))
            {
                Directory.CreateDirectory(shetouanetooldir + "\\Backup\\");
            }
            try
            {
                progx(10, "Checking Backup...");
                //ProgT.Text = "0%";
                if (!System.IO.File.Exists(backupfile))
                {
                    try
                    {

                        using (WebClient webClient = new WebClient())
                        {
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            webClient.DownloadFile(PasscodeLink + "/Passcode/" + ECIDX + ".zip", backupfile);
                        }

                    }
                    catch (Exception ex)
                    {
                        status2.Text += "\r\n ERROR! " + ex.Message + "\r\n";
                        status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                        return;
                    }
                }
                Thread.Sleep(800);
                System.IO.Compression.ZipFile.ExtractToDirectory(shetouanetooldir + "\\Backup\\" + ECIDX + ".zip", shetouanetooldir + "\\Backup\\" + ECIDX);
                progx(30, "Extracting Backup...");
                Thread.Sleep(800);
                if (!System.IO.File.Exists(shetouanetooldir + "\\Backup\\" + ECIDX + "\\activation_record.plist"))
                {
                    throw new Exception("Activation Files not Found!");
                }
                if (!System.IO.File.Exists(fairp + "IC-Info.sisv"))
                {
                    statux2("\r\nFixing FairPlay..\r\n");
                    ModifyactBKP();
                }
                if (!System.IO.File.Exists(fairp + "IC-Info.sisv"))
                {
                    statux2("\r\nFixing FairPlay..\r\n");
                    ModifyactBKP();
                }
                progx(50, "Uploading Backup...");
                sshexec("mkdir /mnt2/tmp");

                method_10(assetsDir + "pl", "/mnt2/tmp/");
                sshexec("chmod +x /mnt2/tmp/plutil");
                // sshexec("/mnt2/tmp/plutil -convert xml1 com.apple.commcenter.device_specific_nobackup.plist");
                // sshexec("/mnt2/tmp/plutil -convert xml1 data_ark.plist");
                // sshexec("/mnt2/tmp/plutil -convert xml1 activation_record.plist");

                statux2("Block OTA Update and Reset ...");
                sshexec("/mnt2/tmp/plutil -key com.apple.OTATaskingAgent -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                sshexec("/mnt2/tmp/plutil -key com.apple.mobile.obliteration -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                sshexec("/mnt2/tmp/plutil -key com.apple.mobile.softwareupdated -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                sshexec("/mnt2/tmp/plutil -key com.apple.softwareupdateservicesd -type bool -value true /mnt2/db/com.apple.xpc.launchd/disabled.plist");
                statux2("Block Done!");



                method_10(shetouanetooldir + "\\Backup\\" + ECIDX, "/mnt2/tmp");


                ondeviceactfiles();

                //
                //shiiiiiiiiiiiii
                shellsync("rm -rf " + actrecpath);
                shellsync("rm " + arkplist);
                shellsync("mkdir " + actrecpath);
                shellsync("mkdir /mnt2/tmp");
                shellsync("mv -f /mnt2/tmp/activation_record.plist " + activationrecordplist);
                progx(85, "Fixing Activation..");
                shellsync("mv -f /mnt2/tmp/data_ark.plist " + arkplist);
                if (RD17.Checked)
                {
                    shellsync("mv -f /mnt2/tmp/FairPlay /mnt8/Library/");
                }
                else
                {
                    shellsync("mv -f /mnt2/tmp/FairPlay /mnt2/mobile/Library/");
                }


                progx(85, "Fixing Fairplay..");
                shellsync("mv -f /mnt2/tmp/com.apple.commcenter.device_specific_nobackup.plist /mnt2/wireless/Library/Preferences/");

                progx(85, "Skipping Setup..");
                Thread.Sleep(5000);
                skipsetup();

                sshexec("rm -rf /mnt2/root/Library/Preferences/com.apple.MobileAsset.plist");
                method_9(asset, "/mnt2/root/Library/Preferences/com.apple.MobileAsset.plist");
                sshexec("/bin/chmod 600 /mnt2/root/Library/Preferences/com.apple.MobileAsset.plist");
                sshexec("rm -rf /mnt1/System/Library/LaunchDaemons/com.apple.mobile.obliteration.plist");

                sshexec("/bin/launchctl unload /mnt1/System/Library/LaunchDaemons/com.apple.CommCenter.plist");
                sshexec("chflags -R nouchg /mnt6/*/usr/local/standalone/firmware/");
                fairplaymethod();
                sshexec("/sbin/reboot");
                progx(100, "Bypass Complete!");
                status2.Text += "\r\n \r\n DEVICE HAS BEEN ACTIVATED SUCCESSFULLY!\r\n";
                status2.ForeColor = System.Drawing.Color.LimeGreen; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
                progx(0, " ");
            }
            catch (Exception ex)
            {
                method_19();

                progressBar1.Value = 0;
                progx(0, ex.Message);

            }
            method_19();
        }

        public void method_19()
        {
            try
            {
                if (Directory.Exists(ToolDir + "\\Backup\\" + ECIDLBL.Text))
                {
                    Directory.Delete(ToolDir + "\\Backup\\" + ECIDLBL.Text, true);
                }
                Thread.Sleep(500);

            }
            catch (Exception)
            {
            }
        }
        public void fairplaymethod()
        {
            if (RD17.Checked)
            {

                sshexec("chmod -R 755 /mnt8/Library/FairPlay/");
                sshexec("/usr/sbin/chown -R mobile:staff /mnt8/Library/FairPlay");
                sshexec("chmod 664 /mnt8/Library/FairPlay/iTunes_Control/iTunes/*.*");

            }
            else
            {
                sshexec("chflags -R nouchg /mnt6/*/usr/local/standalone/firmware/");
                sshexec("chmod -R 755 /mnt2/mobile/Library/FairPlay/");
                sshexec("/usr/sbin/chown -R mobile:staff /mnt2/mobile/Library/FairPlay");
                sshexec("chmod 664 /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                sshexec("rm -rf /mnt2/mobile/Library/Logs/* > /dev/null 2>&1");
            }

        }
        public void skipsetup()
        {
            string asset2 = assetsDir + "resource.asset";
            try
            {
                sshexec("launchctl unload /mnt2/System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
                sshexec("launchctl load /mnt2/System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
                sshexec("rm -rf /mnt2/root/Library/Preferences/com.apple.MobileAsset.plist");
                if (RD17.Checked)
                {
                    sshexec("rm -rf /mnt8/Library/Preferences/com.apple.purplebuddy.plist");
                    method_9(asset2, "/mnt8/Library/Preferences/com.apple.purplebuddy.plist");
                    sshexec("chmod 600 /mnt8/Library/Preferences/com.apple.purplebuddy.plist");
                    sshexec("/usr/bin/uicache --all && chflags uchg /var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                }
                else
                {
                    sshexec("rm -rf /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                    method_9(asset2, "/mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                    sshexec("chmod 600 /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                    sshexec("/usr/bin/uicache --all && chflags uchg /var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                }



                sshexec("chmod 755 " + arkplist);
                sshexec("chflags uchg " + arkplist);
                sshexec("chflags uchg " + activationrecordplist);


            }
            catch
            {
                status2.Text += "\r\n ERROR SKIPPING SETUP SCREEN!\r\n";
                status2.ForeColor = System.Drawing.Color.BlueViolet;

            }

        }
        public void method_9(string fileToUpload, string remotePath)
        {
            try
            {
                ScpClient scpClient = new ScpClient("127.0.0.1", 2222, "root", "alpine");
                try
                {
                    scpClient.Connect();
                    scpClient.Upload(new FileInfo(fileToUpload), remotePath);
                    scpClient.Disconnect();
                }
                finally
                {
                    ScpClient scpClient2 = scpClient;
                    if (scpClient2 != null)
                    {
                        ((IDisposable)scpClient2).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                string text = "Failed to send file.";
                Exception ex2 = ex;
                status2.Text += "\r\n ERROR! " + text + ex2.Message + "\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }
        }

        public void ondeviceactfiles()
        {
            findactpath = sshexec("find /mnt2/containers/Data/System -iname 'internal'").Replace("Library/internal", "").Replace("\n", "").Replace("//", "8");
            internalpath = findactpath + "Library/internal/";
            actrecpath = findactpath + "Library/activation_records/";
            prefpath = "/mnt2/wireless/Library/Preferences/";
            arkplist = internalpath + "data_ark.plist";
            activationrecordplist = actrecpath + "activation_record.plist";
            podrecordplist = actrecpath + "pod_record.plist";
            commcenterplist = prefpath + "com.apple.commcenter.device_specific_nobackup.plist";
            if (RD17.Checked)
            {
                fairplayfolder = "/mnt8/Library/FairPlay";
            }
            else
            {
                fairplayfolder = "/mnt2/mobile/Library/FairPlay";
            }


        }

        public string shellsync(string string_8)
        {
            try
            {
                SshCommand sshCommand = sshClient.CreateCommand(string_8);
                IAsyncResult asyncResult = sshCommand.BeginExecute();
                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(500);
                }
                string text = sshCommand.EndExecute(asyncResult);
                string errrr = sshCommand.Error;
                return text;
            }
            catch (Exception ex)
            {

                string err = ex.Message;
                return err;

            }

        }
        public void method_10(string dirToUpload, string remotePath)
        {
            try
            {
                ScpClient scpClient = new ScpClient("127.0.0.1", 2222, "root", "alpine");
                try
                {
                    scpClient.Connect();
                    scpClient.Upload(new DirectoryInfo(dirToUpload), remotePath);
                    scpClient.Disconnect();
                }
                finally
                {
                    ScpClient scpClient2 = scpClient;
                    if (scpClient2 != null)
                    {
                        ((IDisposable)scpClient2).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                string text = "Failed to send file.";
                Exception ex2 = ex;
                status2.Text += "\r\n ERROR! " + text + ex2.Message + "\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }
        }

        public void plistx(string source, string output)
        {
            if (System.IO.File.Exists(output))
            {
                System.IO.File.Delete(output);
            }
            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = plistu,
                    Arguments = " -i " + source + " -o " + output,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                process.Start();
                process.WaitForExit();
                Thread.Sleep(2000);
            }
            catch (Exception hhs)
            {
                statux2(hhs.Message + "\r\n");
            }
            Debug.WriteLine("Created Plist");

        }

        public void ModifyactBKP()
        {
            string ActDir = Environment.CurrentDirectory + "\\Backup\\" + ECIDLBL.Text;
            string dirFP = ActDir + "\\FairPlay\\iTunes_Control\\iTunes\\";
            string actrec = ActDir + "\\activation_record.plist";
            string convd = Environment.CurrentDirectory + "\\data\\cc\\";
            string conva = convd + "test.plist";

            if (Directory.Exists(convd))
            {
                Directory.Delete(convd, true);
            }
            if (!Directory.Exists(convd))
            {
                Directory.CreateDirectory(convd);
            }
            plistx(actrec, conva);


            Debug.WriteLine("Before XML");
            NSDictionary xml = (NSDictionary)PropertyListParser.Parse(conva);
            Debug.WriteLine("XML ERROR");
            try
            {

                string resu = GetPlistProperty(xml, "FairPlayKeyData");
                string clean = resu.Replace("<plist version=\"1.0\"><data>", "");
                Console.WriteLine(clean);
                Debug.WriteLine("Plist read");
                byte[] FPT = Convert.FromBase64String(clean);
                Debug.WriteLine("Plist to Byte");
                Debug.WriteLine(FPT);
                string FpDec = Encoding.UTF8.GetString(FPT);
                Console.WriteLine(FpDec + "\n \n");
                string rem1 = FpDec.Replace("-----BEGIN CONTAINER-----", "");
                string rem2 = rem1.Replace("-----END CONTAINER-----", "");
                Console.WriteLine(rem2 + "\n \n");

                byte[] rem3 = Convert.FromBase64String(rem2);
                string finalfp = Encoding.ASCII.GetString(rem3);
                Console.WriteLine("this is final: \n " + finalfp);
                if (!Directory.Exists(dirFP))
                {
                    Directory.CreateDirectory(dirFP);
                }
                string pathx = ActDir + "\\FairPlay\\iTunes_Control\\iTunes\\IC-Info.sisv";
                string pathy = ActDir + "\\FairPlay\\iTunes_Control\\iTunes\\IC-Info.sidv";
                if (!System.IO.File.Exists(pathx))
                {
                    System.IO.File.WriteAllBytes(pathx, rem3);
                }



            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void ExitRamdiskBTN_Click(object sender, EventArgs e)
        {
            ExitRamdiskBTN.Enabled = false;

            ConnectSSH(22, 2222);
            if (sshClient.IsConnected)
            {
                SSH("kill 1");
            }
            else
            {
                ConnectSSH(44, 2222);
                if (sshClient.IsConnected)
                {
                    SSH("kill 1");
                }
                else
                {
                    statux2("SSH Not Connected!");
                }

            }
        }

        private void PasscodeBTNR_Click(object sender, EventArgs e)
        {
            try
            {
                string snx = SNLBL.Text;
                string ecidx = ECIDLBL.Text;
                checker check = new checker();

                if (!check.checkpasscode(ecidx))
                {
                    throw new Exception("Device Not Registered!");
                }
                RDButtons(false);
                Thread passact = new Thread(PasscodeRamdisk);
                passact.IsBackground = true;
                passact.Start();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + ECIDLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                // statux2("Please Register: " + ECIDLBL.Text + "\r\n at: https://iskorpion.com/post/10_ramdisk-passcode-bypass-signal-ios-15-17.html");
            }




        }

        public void PasscodeRamdiskBKP()
        {
            string backupfile = ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip";
            string backupfolder = ToolDir + "\\Backup\\" + ECIDLBL.Text + "\\";
            string fairp = ToolDir + "\\Backup\\" + ECIDLBL.Text + "\\FairPlay\\iTunes_Control\\iTunes\\";
            string sark = backupfolder + "data_ark.plist";
            string sact = backupfolder + "activation_record.plist";
            string scom = backupfolder + "com.apple.commcenter.device_specific_nobackup.plist";
            string sfp = fairp + "IC-Info.sisv";
            string ecid = ECIDLBL.Text;
            connectSSH();
            progx(10, "Creating backup folder..");
            if (Directory.Exists(backupfolder))
            {
                Directory.Delete(backupfolder, true);
            }
            if (!Directory.Exists(backupfolder))
            {
                Directory.CreateDirectory(backupfolder);
            }
            if (!Directory.Exists(fairp))
            {
                Directory.CreateDirectory(fairp);
            }
            try
            {
                if (System.IO.File.Exists(backupfile))
                {
                    statux2("Backup File Already Exists..");
                }
                progx(20, "Location backup files..");
                if (FindActivationRecord() != "OK")
                {
                    statux2("Device not compatible");
                    throw new Exception("couldn't find activation files on device..");
                }
                progx(30, "Downloading backup files..");
                if (DownloadSCP("/mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist", ToolDir + "\\Backup\\" + ecid + "\\com.apple.commcenter.device_specific_nobackup.plist") == "HOST_SCP_FAILED")
                {
                    throw new Exception("Failed to backup Comm Center..");
                }
                string text3 = SSH("find /mnt2/containers/Data/System -name 'activation_record.plist'").Replace("\n", "").Replace("//", "/");
                string text3x = SSH("find /mnt2/containers/Data/System -name 'data_ark.plist'").Replace("\n", "").Replace("//", "/");
                SSH("chflags nouchg " + text3);
                SSH("chmod 755 " + text3);
                SSH("chmod 777 " + text3);
                progx(50, "Download Activation Record..");
                if (this.DownloadSCP(text3 ?? "", ToolDir + "\\Backup\\" + ecid + "\\activation_record.plist") != "OK")
                {
                    throw new Exception("Failed to backup Activation Record..");
                }
                progx(70, "Getting Fairplay keydata..");
                DownloadSCP(text3x ?? "", ToolDir + "\\Backup\\" + ecid + "\\data_ark.plist");
                SSH("chflags nouchg /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("chmod 755 /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("chmod 777 /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                if (this.DownloadSCP("/mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv", fairp + "\\IC-Info.sisv") == "HOST_SCP_FAILED")
                {
                    GenerateFP();
                }
                progx(70, "Finalizing Backup..");
                ZipFile.CreateFromDirectory(ToolDir + "\\Backup\\" + ecid, ToolDir + "\\Backup\\" + ecid + ".zip");
                bool flag = Directory.Exists(Environment.CurrentDirectory + "\\Backup\\" + ecid);
                if (flag)
                {
                    Directory.Delete(Environment.CurrentDirectory + "\\Backup\\" + ecid, true);
                }
                progx(90, "Uploading backup to server..");
                uploadBackup();

                progx(100, "Backup Done..");


            }
            catch (Exception ex)
            {

                try
                {
                    if (Directory.Exists(backupfolder))
                    {
                        Directory.Delete(backupfolder, true);
                    }
                }
                catch
                {

                }
                statux2(ex.Message);

                progx(0, "");
            }

        }

        private void uploadBackup()
        {
            string encode = ".zip";
            string Method = "POST";
            string Link_Web = PasscodeLink + "/index.php";
            string fileAddress = Directory.GetCurrentDirectory() + "\\Backup\\";
            try
            {
                string str = ECIDLBL.Text;
                string zipcompression = encode;
                string Api = Link_Web;
                string method = Method;
                System.Net.WebClient client = new System.Net.WebClient();
                string ishetouane = fileAddress;
                client.Credentials = CredentialCache.DefaultCredentials;

                client.UploadFile(Api, method, ishetouane + str + zipcompression);
                client.Dispose();
            }
            catch (Exception err)
            {
                status2.Text += "\r\n Error 5: " + err.Message + "\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }
        }
        private void uploadBackup2()
        {
            string encode = ".zip";
            string Method = "POST";
            string Link_Web = PasscodeLink + "/index.php";
            string fileAddress = Directory.GetCurrentDirectory() + "\\Backup\\";
            try
            {
                string str = SNLBL.Text;
                string zipcompression = encode;
                string Api = Link_Web;
                string method = Method;
                System.Net.WebClient client = new System.Net.WebClient();
                string ishetouane = fileAddress;
                client.Credentials = CredentialCache.DefaultCredentials;

                client.UploadFile(Api, method, ishetouane + str + zipcompression);
                client.Dispose();
            }
            catch (Exception err)
            {
                status2.Text += "\r\n Error 5: " + err.Message + "\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }
        }
        public string DownloadSCP(string route_ssh, string route_local)
        {
            bool flag = !this.scpClient.IsConnected;
            if (flag)
            {
                try
                {
                    this.sshClient.Connect();
                }
                catch
                {
                    return "HOST_SCP_FAILED";
                }
            }
            try
            {
                this.scpClient.Download(route_ssh ?? "", new FileInfo(route_local ?? ""));
            }
            catch
            {
                return "HOST_SCP_FAILED";
            }
            return "OK";
        }

        public string FindActivationRecord()
        {
            try
            {
                SSH("chflags nouchg /mnt2/mobile/Library/FairPlay/ && chflags nouchg /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("chflags -R nouchg /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("chmod -R 755 /mnt2/mobile/Library/FairPlay/");
                SSH("chmod -R 755 /mnt2/containers/Data/System/");
                SSH("chmod -R 755 /mnt2/containers/Shared/SystemGroup/systemgroup.com.apple.icloud.findmydevice.managed/");
                SSH("chmod -R 755 /mnt2/mobile/Library/Preferences/");
                SSH("rm -rf /mnt2/mobile/Library/Preferences/com.apple.dataaccess.launchd");
                SSH("history -c");
                string text = SSH("find /mnt2/containers/Data/System -name activation_record.plist");
                string text2 = text;
                text2 = text2.Trim();
                if (text2.Trim() == "")
                {
                    SSH("ACTV=$(find /mnt2/containers/Data/System -name 'activation_record.plist') && chflags nouchg $ACTV && chmod 755 $ACTV");
                    text2 = SSH("find /var/tmp -name 'activation_record.plist'");
                    if (text2.Trim() == "")
                    {
                        return "NO_ACTIVATION_RECORD";
                    }
                }
                SSH("history -c");
                text = SSH("test -f /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist ; echo $?");
                string text3 = text;
                text3 = text3.Trim();
                if (text3.Trim() == "")
                {
                    return "NO_COMMCENTER_OS";
                }
            }
            catch
            {
            }
            return "OK";
        }
        private void BackupRD_Click(object sender, EventArgs e)
        {
            Thread bkx = new Thread(PasscodeRamdiskBKP);
            bkx.IsBackground = true;
            bkx.Start();

        }

        public void sshfolderdownload(string dirToDownload, string localPath)
        {
            try
            {
                string path = localPath + Path.GetFileName(dirToDownload);
                using (ScpClient scpClient = new ScpClient("127.0.0.1", 2222, "root", "alpine"))
                {
                    scpClient.Connect();
                    scpClient.Download(dirToDownload, new DirectoryInfo(path));
                    scpClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                statux2(ex.Message);
            }
        }
        private void doAccountRD()
        {
            try
            {
                connectSSH();
                string accountfolder = ToolDir + "\\Accounts\\" + ECIDLBL.Text + "\\Accounts\\";
                string FolDR = ToolDir + "\\Accounts";
                string FolDR2 = ToolDir + "\\Accounts\\" + ECIDLBL.Text + "\\";
                string txt = "/mnt2/mobile/Library/Accounts";
                SSH("chmod -R 777 " + txt);
                if (!Directory.Exists(FolDR))
                {
                    Directory.CreateDirectory(FolDR);
                }
                if (!Directory.Exists(FolDR2))
                {
                    Directory.CreateDirectory(FolDR2);
                }
                if (!Directory.Exists(accountfolder))
                {
                    Directory.CreateDirectory(accountfolder);
                }

                sshfolderdownload(txt, FolDR2);

                SSH("rm -rf /mnt2/mobile/Library/Accounts");
                SSH("mkdir /mnt2/mobile/Library/Accounts");
                progx(100, "iCloud Logout Done!");
                SSH("/sbin/reboot");

                progx(0, "");
            }
            catch (Exception EX)
            {

                status2.Text += "\r\n ERROR 7: " + EX.Message + "\r\n";
                status2.ForeColor = System.Drawing.Color.Red; status2.SelectionStart = status2.TextLength; status2.ScrollToCaret();
            }
        }
        private void LogoutBTNR_Click(object sender, EventArgs e)
        {
            try
            {
                string snx = SNLBL.Text;
                string ecidx = ECIDLBL.Text;
                checker check = new checker();

                if (!check.checkaccount(ecidx))
                {
                    throw new Exception("Device Not Registered!");
                }
                RDButtons(false);
                Thread accs = new Thread(doAccountRD);
                accs.IsBackground = true;
                accs.Start();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + ECIDLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                // statux2("Please Register: " + ECIDLBL.Text + "\r\n at: https://iskorpion.com/post/18_ios-12-15-logout-icloud-open-menu-not-fmi-off.html");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Purple purple = new Purple();
            purple.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\x64\\LibDFUHelper.dll",
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
            process.WaitForExit();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            if (System.IO.File.Exists(ToolDir + "\\x64\\WinRa1n2.1.exe"))
            {
                Winra1n();
            }
            else
            {
                DownloadWinrain(DownloadLink + "/WinRa1n2.1.zip", ToolDir + "\\x64\\WinRa1n2.1.zip");
            }


        }

        public void ExitRecovery()
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\x64\\irecovery.exe",
                    Arguments = "-n",
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            ExitRecovery();
        }

        public void CleanTMP()
        {
            try
            {
                if (Directory.Exists(Environment.CurrentDirectory + "\\tmp"))
                {
                    Directory.Delete(Environment.CurrentDirectory + "\\tmp", true);
                }
                Thread.Sleep(1000);
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\tmp");
            }
            catch (Exception)
            {
            }
        }
        public void FindActivationRoutes()
        {
            string commandX = SSH("find /private/var/containers/Data/System/ -iname 'internal'");
            Folder_Activa = commandX.Replace("Library/internal", "").Replace("\n", "").Replace("//", "/");
            Folder_ArkInt = Folder_Activa + "Library/internal/";
            Folder_ActRec = Folder_Activa + "Library/activation_records/";
            Folder_LBPref = "/var/mobile/Library/Preferences/";
            Folder_WLPref = "/var/wireless/Library/Preferences/";
            Folder_LBLock = "/var/root/Library/Lockdown/";
            Route_ActRec = Folder_ActRec + "activation_record.plist";
            Route_CommCe = Folder_WLPref + "com.apple.commcenter.device_specific_nobackup.plist";
            Route_ArkInt = Folder_ArkInt + "data_ark.plist";
            Route_DatArk = Folder_LBLock + "data_ark.plist";
            Route_Purple = Folder_LBPref + "com.apple.purplebuddy.plist";
        }
        private string Folder_Activa = "";
        private string Folder_ArkInt = "";
        private string Folder_ActRec = "";
        private string Folder_LBPref = "/var/mobile/Library/Preferences/";
        private string Folder_WLPref = "/var/wireless/Library/Preferences/";
        private string Folder_LBLock = "/var/root/Library/Lockdown/";
        private string Route_ActRec = "///activation_record.plist";
        private string Route_CommCe = "///com.apple.commcenter.device_specific_nobackup.plist";
        private string Route_DatArk = "///data_ark.plist";
        private string Route_ArkInt = "///data_ark.plist";
        private string Route_Purple = "///com.apple.purplebuddy.plist";

        public void DeleteActivationFiles()
        {
            this.SSH("chflags nouchg " + this.Route_ActRec + " && rm " + this.Route_ActRec);
            this.SSH(string.Concat(new string[]
            {
                "chflags nouchg ",
                this.Route_CommCe,
                " && rm ",
                this.Route_CommCe,
                " && chflags nouchg ",
                this.Route_DatArk,
                " && rm ",
                this.Route_DatArk,
                " && chflags nouchg ",
                this.Route_ArkInt,
                " && rm ",
                this.Route_ArkInt,
                " && chflags nouchg ",
                this.Route_Purple,
                " && rm ",
                this.Route_Purple
            }));
        }
        public void DeleteSubstrate()
        {
            this.SSH("rm /Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib && rm /Library/MobileSubstrate/DynamicLibraries/iuntethered.plist");
            this.SSH("rm -rf /Library/MobileSubstrate/DynamicLibraries/");
            this.SSH("rm -rf /Library/Frameworks/CydiaSubstrate.framework");
            this.SSH("rm /Library/MobileSubstrate/MobileSubstrate.dylib");
            this.SSH("rm /Library/MobileSubstrate/DynamicLibraries && rm /Library/MobileSubstrate/ServerPlugins");
            this.SSH("rm /usr/bin/cycc && rm /usr/bin/cynject");
            this.SSH("rm /usr/include/substrate.h");
            this.SSH("rm /usr/lib/cycript0.9/com/saurik/substrate/MS.cy");
            this.SSH("rm -rf /usr/lib/substrate");
            this.SSH("rm /usr/lib/libsubstrate.dylib");
            this.SSH("rm /usr/libexec/substrate && rm /usr/libexec/substrated");
        }

        public void RestartAllDeamons()
        {
            this.SSH("launchctl unload -F /System/Library/LaunchDaemons/* && launchctl load -w -F /System/Library/LaunchDaemons/*");
        }

        public static string GSMEID = "MEID";

        public void FixUntethered()
        {
            connectSSHJB();
            SSH("launchctl unload /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
            SSH("launchctl load /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
            SSH("chflags -R nouchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
            this.scpClient.Upload(new MemoryStream(nobackup_plist), "/var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
            SSH("chflags uchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
            this.scpClient.Upload(new MemoryStream(this.byte_17), "/af2ccdService.dylib");
            this.scpClient.Upload(new MemoryStream(mon6), "/af2ccdService.plist");
            SSH("cd / ; mv af2ccdService.dylib /Library/MobileSubstrate/DynamicLibraries/af2ccdService.dylib");
            SSH("cd / ; mv af2ccdService.plist /Library/MobileSubstrate/DynamicLibraries/af2ccdService.plist");
        }
        public void Mount()
        {
            this.SSH("mount -o rw,union,update /");
        }
        public void Dobbrestore()
        {
            try
            {
                connectSSHJB();
                Mount();

                if (iOSLBL.Text.StartsWith("13.") || iOSLBL.Text.StartsWith("14."))
                {

                    SSH("cp /private/preboot/active /./");
                    SSH("mount -o rw,union,update /private/preboot");
                    SSH("key=$(cat /./active); mv /private/preboot/$key/usr/local/standalone/firmware/Baseband2 /private/preboot/$key/usr/local/standalone/firmware/Baseband");
                    SSH("key=$(cat /./Library/Preferences/SystemConfiguration/com.apple.radios.plist); if test -z $key; then echo '' &>log; rm /log; else chflags nouchg /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; plutil -AirPlaneMode -remove /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; killall CommCenter; fi ");
                    SSH("mount -o rw,union,update /");
                    this.scpClient.Upload(new MemoryStream(this.byte_18), "/usr/bin/ldrunps");
                    SSH("chmod +x /usr/bin/ldrunps");
                    SSH("mv /usr/local/standalone/firmware/Baseband2 /usr/local/standalone/firmware/Baseband");
                }
                else
                {
                    SSH("mount -o rw,union,update /");
                    this.scpClient.Upload(new MemoryStream(this.byte_18), "/usr/bin/ldrunps");
                    SSH("chmod +x /usr/bin/ldrunps");
                    SSH("mv /usr/local/standalone/firmware/Baseband2 /usr/local/standalone/firmware/Baseband");
                }
                SSH("ldrestart");
                statux2("Baseband Successfully Restore");

            }
            catch (Exception e)
            {
                statux2(e.Message);

            }
        }
        public void Dobbremove()
        {
            try
            {
                connectSSHJB();
                Mount();

                if (SNLBL.Text.StartsWith("13.") || SNLBL.Text.StartsWith("14."))
                {
                    SSH("mount -o rw,union,update /");
                    this.scpClient.Upload(new MemoryStream(this.byte_18), "/usr/bin/ldrunps");
                    SSH("chmod +x /usr/bin/ldrunps");
                    SSH("mv /usr/local/standalone/firmware/Baseband /usr/local/standalone/firmware/Baseband2");
                    SSH("cp /private/preboot/active /./");
                    SSH("mount -o rw,union,update /private/preboot");
                    SSH("key=$(cat /./active); mv /private/preboot/$key/usr/local/standalone/firmware/Baseband /private/preboot/$key/usr/local/standalone/firmware/Baseband2");
                    SSH("key=$(cat /./Library/Preferences/SystemConfiguration/com.apple.radios.plist); if test -z $key; then plutil -create /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; plutil -AirPlaneMode -1 /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; plutil -binary /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; chflags uchg /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; else plutil -AirPlaneMode -remove /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; plutil -AirPlaneMode -1 /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; chflags uchg /./Library/Preferences/SystemConfiguration/com.apple.radios.plist; fi ");
                    SSH("path=\"/System/Library/LaunchDaemons\"; launchctl unload -w -F $path*; launchctl load -w -F $path*");
                    SSH("mv /usr/local/standalone/firmware/Baseband /usr/local/standalone/firmware/Baseband2");
                    SSH("chmod +x /Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");

                }
                else
                {
                    SSH("mount -o rw,union,update /");
                    this.scpClient.Upload(new MemoryStream(this.byte_18), "/usr/bin/ldrunps");
                    SSH("chmod +x /usr/bin/ldrunps");
                    SSH("chmod +x /Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");
                    SSH("mv /usr/local/standalone/firmware/Baseband /usr/local/standalone/firmware/Baseband2");

                }
                SSH("ldrestart");
                CleanTMP();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                CleanTMP();
                statux2(e.Message);
            }
        }
        private void SkipSetup()
        {
            if (RD17.Checked)
            {
                SSH("rm /mnt1/System/Library/LifecyclePolicy/DomainAttributes/com.apple.purplebuddy.plist");
                this.scpClient.Upload(new MemoryStream(this.byte_16), "/mnt1/System/Library/LifecyclePolicy/DomainAttributes/com.apple.purplebuddy.plist");
                this.SSH("chmod 600 /var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
            }
            else
            {
                this.scpClient.Upload(new MemoryStream(this.byte_16), "/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                this.SSH("chmod 600 /var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
            }

        }

        private void SkipSetupX()
        {
            if (RD17.Checked)
            {
                SSH("rm /mnt1/System/Library/LifecyclePolicy/DomainAttributes/com.apple.purplebuddy.plist");
                this.scpClient.Upload(new MemoryStream(this.byte_16), "/mnt1/System/Library/LifecyclePolicy/DomainAttributes/com.apple.purplebuddy.plist");
                this.SSH("chmod 600 /vmnt1/System/Library/LifecyclePolicy/DomainAttributes/com.apple.purplebuddy.plist");
            }
            else
            {
                this.scpClient.Upload(new MemoryStream(this.byte_16), "/mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                this.SSH("chmod 600 /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
            }

        }
        public void DeleteLogs()
        {
            this.SSH("rm -rf /private/var/mobile/Library/Logs/* > /dev/null 2>&1");
            this.SSH("rm -rf /private/var/mobile/Library/Logs/* > /dev/null 2>&1");
        }
        public void DisableOTAUpdates()
        {

            try
            {
                this.SSH("launchctl unload -w /System/Library/LaunchDaemons/com.apple.mobile.obliteration.plist && launchctl unload -w /System/Library/LaunchDaemons/com.apple.OTACrashCopier.plist && launchctl unload -w /System/Library/LaunchDaemons/com.apple.mobile.softwareupdated.plist && launchctl unload -w /System/Library/LaunchDaemons/com.apple.OTATaskingAgent.plist");
                this.SSH("rm -rf /System/Library/LaunchDaemons/com.apple.softwareupdateservicesd.plist && rm -rf /System/Library/LaunchDaemons/com.apple.mobile.softwareupdated.plist &&  rm -rf /System/Library/LaunchDaemons/com.apple.mobile.obliteration &&  rm -rf /System/Library/LaunchDaemons/com.apple.OTATaskingAgent");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Disable OTA: " + e.Message);
            }
        }
        public void InstallSubstrate()
        {
            this.scpClient.Upload(new MemoryStream(this.byte_13), "/sbin/lzma");
            this.scpClient.Upload(new MemoryStream(this.byte_14), "/lib.tar");
            progx(70, "");
            this.scpClient.Upload(new MemoryStream(this.byte_15), "/foo.tar.lzma");
            this.SSH("tar -xvf /lib.tar -C / && chmod 777 /sbin/lzma && lzma -d -v /foo.tar.lzma && tar -xvf /foo.tar -C /");

            this.SSH("/usr/libexec/substrate");
            this.SSH("rm /sbin/lzma && rm /lib.tar && rm /foo.tar && rm /foo.tar.lzma");
            this.scpClient.Upload(new MemoryStream(this.byte_6), "/Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");
            this.scpClient.Upload(new MemoryStream(this.byte_7), "/Library/MobileSubstrate/DynamicLibraries/iuntethered.plist");
        }

        private byte[] nobackup_plist = Convert.FromBase64String("PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPCFET0NUWVBFIHBsaXN0IFBVQkxJQyAiLS8vQXBwbGUvL0RURCBQTElTVCAxLjAvL0VOIiAiaHR0cDovL3d3dy5hcHBsZS5jb20vRFREcy9Qcm9wZXJ0eUxpc3QtMS4wLmR0ZCI+CjxwbGlzdCB2ZXJzaW9uPSIxLjAiPgo8ZGljdD4KCTxrZXk+a1Bvc3Rwb25lbWVudFRpY2tldDwva2V5PgoJPGRpY3Q+CgkJPGtleT5BY3Rpdml0eVVSTDwva2V5PgoJCTxzdHJpbmc+aHR0cHM6Ly9hbGJlcnQuYXBwbGUuY29tL2RldmljZXNlcnZpY2VzL2FjdGl2aXR5PC9zdHJpbmc+CgkJPGtleT5BY3RpdmF0aW9uVGlja2V0PC9rZXk+CgkJPHN0cmluZz5QRDk0Yld3Z2RtVnljMmx2YmowaU1TNHdJaUJsYm1OdlpHbHVaejBpVlZSR0xUZ2lQejRLUENGRVQwTlVXVkJGSUhCc2FYTjBJRkJWUWt4SlF5QWlMUzh2UVhCd2JHVXZMMFJVUkNCUVRFbFRWQ0F4TGpBdkwwVk9JaUFpYUhSMGNEb3ZMM2QzZHk1aGNIQnNaUzVqYjIwdlJGUkVjeTlRY205d1pYSjBlVXhwYzNRdE1TNHdMbVIwWkNJK0NqeHdiR2x6ZENCMlpYSnphVzl1UFNJeExqQWlQZ284WkdsamRENEtDVHhyWlhrK1FXTjBhWFpoZEdsdmJsSmxjWFZsYzNSSmJtWnZQQzlyWlhrK0NnazhaR2xqZEQ0S0NRazhhMlY1UGtGamRHbDJZWFJwYjI1U1lXNWtiMjF1WlhOelBDOXJaWGsrQ2drSlBITjBjbWx1Wno0ek1HSTJNR1prTUMwMk5qYzBMVFEzTnpndFltSXhOQzFtTkdaaE9UUTBNV1EwWXpnOEwzTjBjbWx1Wno0S0NRazhhMlY1UGtGamRHbDJZWFJwYjI1VGRHRjBaVHd2YTJWNVBnb0pDVHh6ZEhKcGJtYytWVzVoWTNScGRtRjBaV1E4TDNOMGNtbHVaejRLQ1FrOGEyVjVQa1pOYVZCQlkyTnZkVzUwUlhocGMzUnpQQzlyWlhrK0Nna0pQSFJ5ZFdVdlBnb0pQQzlrYVdOMFBnb0pQR3RsZVQ1Q1lYTmxZbUZ1WkZKbGNYVmxjM1JKYm1adlBDOXJaWGsrQ2drOFpHbGpkRDRLQ1FrOGEyVjVQa0ZqZEdsMllYUnBiMjVTWlhGMWFYSmxjMEZqZEdsMllYUnBiMjVVYVdOclpYUThMMnRsZVQ0S0NRazhkSEoxWlM4K0Nna0pQR3RsZVQ1Q1lYTmxZbUZ1WkVGamRHbDJZWFJwYjI1VWFXTnJaWFJXWlhKemFXOXVQQzlyWlhrK0Nna0pQSE4wY21sdVp6NVdNand2YzNSeWFXNW5QZ29KQ1R4clpYaytRbUZ6WldKaGJtUkRhR2x3U1VROEwydGxlVDRLQ1FrOGFXNTBaV2RsY2o0eE1qTTBOVFkzUEM5cGJuUmxaMlZ5UGdvSkNUeHJaWGsrUW1GelpXSmhibVJOWVhOMFpYSkxaWGxJWVhOb1BDOXJaWGsrQ2drSlBITjBjbWx1Wno0NFEwSXhNRGN3UkRrMVFqbERSVVUwUXpnd01EQXdOVVV5TVRrNVFrSTRSa0l4T0ROQ01ESTNNVE5CTlRKRVFqVkZOelZEUVRKQk5qRTFOVE0yTVRneVBDOXpkSEpwYm1jK0Nna0pQR3RsZVQ1Q1lYTmxZbUZ1WkZObGNtbGhiRTUxYldKbGNqd3ZhMlY1UGdvSkNUeGtZWFJoUGdvSkNVVm5hR1JEZHowOUNna0pQQzlrWVhSaFBnb0pDVHhyWlhrK1NXNTBaWEp1WVhScGIyNWhiRTF2WW1sc1pVVnhkV2x3YldWdWRFbGtaVzUwYVhSNVBDOXJaWGsrQ2drSlBITjBjbWx1Wno0eE1qTTBOVFkzT0RreE1qTTBOVFk4TDNOMGNtbHVaejRLQ1FrOGEyVjVQazF2WW1sc1pVVnhkV2x3YldWdWRFbGtaVzUwYVdacFpYSThMMnRsZVQ0S0NRazhjM1J5YVc1blBqRXlNelExTmpjNE9URXlNelExUEM5emRISnBibWMrQ2drSlBHdGxlVDVUU1UxVGRHRjBkWE04TDJ0bGVUNEtDUWs4YzNSeWFXNW5QbXREVkZOSlRWTjFjSEJ2Y25SVFNVMVRkR0YwZFhOT2IzUkpibk5sY25SbFpEd3ZjM1J5YVc1blBnb0pDVHhyWlhrK1UzVndjRzl5ZEhOUWIzTjBjRzl1WlcxbGJuUThMMnRsZVQ0S0NRazhkSEoxWlM4K0Nna0pQR3RsZVQ1clExUlFiM04wY0c5dVpXMWxiblJKYm1adlVGSk1UbUZ0WlR3dmEyVjVQZ29KQ1R4cGJuUmxaMlZ5UGpBOEwybHVkR1ZuWlhJK0Nna0pQR3RsZVQ1clExUlFiM04wY0c5dVpXMWxiblJKYm1adlUyVnlkbWxqWlZCeWIzWnBjMmx2Ym1sdVoxTjBZWFJsUEM5clpYaytDZ2tKUEdaaGJITmxMejRLQ1R3dlpHbGpkRDRLQ1R4clpYaytSR1YyYVdObFEyVnlkRkpsY1hWbGMzUThMMnRsZVQ0S0NUeGtZWFJoUGdvSlRGTXdkRXhUTVVOU1ZXUktWR2xDUkZKV1NsVlRWVnBLVVRCR1ZWSlRRbE5TVmtaV1VsWk9WVXhUTUhSTVV6QkxWRlZzU2xGdWFFVlJNRTVDVlhwQ1JGRldSa0lLQ1dReVpGcFVXR2hOVmtWR2VWRnRaRTlXYTBwQ1ZGWlNTMUpWYTNwVWJYUlNUVVUxUmxKVVZrMVdWbXQ2VkdwQ1VtUkZOVVpWVkVKT1lWUkJNRlZXVmt0Ulp6QkxDZ2xVUmxKeVpVWktjVmRyVmxOU1JXdzFWV3RXUzFJd05YRlNWWGhPVVZkMFNGRlVSbFpTVlVwdlZGVk9WMVpyTVRSUk0zQkNVMnRLYmxSc1drTlJWMlJWVVZkMFR3b0pZV3BhZUZOVmJIUlVibXhYVTIxV01VNXNUVEpWYWs0MFVWY3hUMVJYTldGalJFcEhURE5vUlZOSVJqVmlWbXhWVDFab1QxSkZkekJqUmxKYVlqRm5NbUY2UW1zS0NWRnJNVk5UV0dSR1VWWnNSVlpzUmxKVFFUQkxVbGhrYzFKSFVsbFJiWGhxWW14S2QxbHRNRFJsUlZZMlVWWktRMW93TlZkUmEwWjJWa1ZPY2xKdVpHcFNNMmh6Q2dsVFZWWnpaRlpzTlU1SWFFVmxhMFpQVVcxa1QxWnJTa0pqTVZKRFlsZDRVbGxWWXpWa1VUQkxWMnhTUkZGdE5UWlJWVFZEV2pKMGVHRkhkSEJTZW13elRVVktRZ29KVVZVeFFrMUZaRVJWTTBaSVZUQnNhVTB3VWxKU1ZVcERWVlpXUWxGVVVraFJhMFpFVERKNGVXSkhWbEpVYW1SM1VWRXdTMDB5YUVoV1Zsa3dVMFpzVTFsWGRIWUtDV0ZyYXpSUFYzZDRZVVpLZG1ScVFsUk9SRUpQVFVoQmVVMVVhSEpVVjI5NVlrUkdUMkV6VVhkV1dFSnhWMnM1UlU1V1ZsZGxWR1JEVDBWc1QxRnJTbTFSTW14TkNnbE5aekJMV25rNGRreDVkSHBhVlZab1ZqRmpNR0ZFV1hkVU0wcE9aRzVLYkZGV1FUQk5SMHBzVlRKYVVGbHVjRTFXUjNoWVV6SkdWMk5YUm5KTlYxSkdWR3BTU2dvSlRrZDRUVlJZYUhCbFZGVnlZak53U1ZwcVdtbFdkekJMVkdsMGJsZEZTbFZOTWpsNFdraFdSRkY2UmxkV2VsWktWMjVhTWxwRlVsTldSV3gzWVVab05tRXlSVXNLQ1ZWVlZrZFJWVVpRVVcxd1VsRllaRzVYVjNSRVdqRnNSbEZZU2xWaE1WcEZaREJHVjAxSWJIUlphelZXVW0xNE1FMHllRXhqTUhSQ1drRXdTMkp1WXpCVFJscFBDZ2xhTUVaMVVraG9hV1JSTUV0UlZVcFhWMVZTTWxOR2FFSk5SRVpOVjBaT1MxRjVkSFJrYW1kNVZGWlNTV1F5U2s1T1JWRjJWMnhKY2xKRmFGcFJWMWt5V1hsek5Rb0pZVmMxVGxKdFVrOVBSMnhhVjBoU1NXRkZPWGRqVjNNd1lWZDRUbFIzTUV0Wk1uUnVXV3RzTmxNd2IzbE9XRkpQV1RGS1ZXTXdPWGRXVlU1Q1pEQldRbEZYUmtJS0NVeFRNSFJNVXpGR1ZHdFJaMUV3VmxOV1JXeEhVMVZPUWxaRlZXZFZhMVpTVmxWV1ZGWkRNSFJNVXpCMENnazhMMlJoZEdFK0NnazhhMlY1UGtSbGRtbGpaVWxFUEM5clpYaytDZ2s4WkdsamRENEtDUWs4YTJWNVBsTmxjbWxoYkU1MWJXSmxjand2YTJWNVBnb0pDVHh6ZEhKcGJtYytSbEl4VURKSFNEaEtPRmhJUEM5emRISnBibWMrQ2drSlBHdGxlVDVWYm1seGRXVkVaWFpwWTJWSlJEd3ZhMlY1UGdvSkNUeHpkSEpwYm1jK1pEazRPVEl3T1RaalpqTTBNVEZsWVRnM1pEQXdNalF5WVdNeE16QXdNRE5tTXpReE1XVTBNand2YzNSeWFXNW5QZ29KUEM5a2FXTjBQZ29KUEd0bGVUNUVaWFpwWTJWSmJtWnZQQzlyWlhrK0NnazhaR2xqZEQ0S0NRazhhMlY1UGtKMWFXeGtWbVZ5YzJsdmJqd3ZhMlY1UGdvSkNUeHpkSEpwYm1jK01UaEdNREE4TDNOMGNtbHVaejRLQ1FrOGEyVjVQa1JsZG1salpVTnNZWE56UEM5clpYaytDZ2tKUEhOMGNtbHVaejVwVUdodmJtVThMM04wY21sdVp6NEtDUWs4YTJWNVBrUmxkbWxqWlZaaGNtbGhiblE4TDJ0bGVUNEtDUWs4YzNSeWFXNW5Qa0k4TDNOMGNtbHVaejRLQ1FrOGEyVjVQazF2WkdWc1RuVnRZbVZ5UEM5clpYaytDZ2tKUEhOMGNtbHVaejVOVEV4T01qd3ZjM1J5YVc1blBnb0pDVHhyWlhrK1QxTlVlWEJsUEM5clpYaytDZ2tKUEhOMGNtbHVaejVwVUdodmJtVWdUMU04TDNOMGNtbHVaejRLQ1FrOGEyVjVQbEJ5YjJSMVkzUlVlWEJsUEM5clpYaytDZ2tKUEhOMGNtbHVaejVwVUdodmJtVXdMREE4TDNOMGNtbHVaejRLQ1FrOGEyVjVQbEJ5YjJSMVkzUldaWEp6YVc5dVBDOXJaWGsrQ2drSlBITjBjbWx1Wno0eE5DNHdMakE4TDNOMGNtbHVaejRLQ1FrOGEyVjVQbEpsWjJsdmJrTnZaR1U4TDJ0bGVUNEtDUWs4YzNSeWFXNW5Qa3hNUEM5emRISnBibWMrQ2drSlBHdGxlVDVTWldkcGIyNUpibVp2UEM5clpYaytDZ2tKUEhOMGNtbHVaejVNVEM5QlBDOXpkSEpwYm1jK0Nna0pQR3RsZVQ1U1pXZDFiR0YwYjNKNVRXOWtaV3hPZFcxaVpYSThMMnRsZVQ0S0NRazhjM1J5YVc1blBrRXhNak0wUEM5emRISnBibWMrQ2drSlBHdGxlVDVUYVdkdWFXNW5SblZ6WlR3dmEyVjVQZ29KQ1R4MGNuVmxMejRLQ1FrOGEyVjVQbFZ1YVhGMVpVTm9hWEJKUkR3dmEyVjVQZ29KQ1R4cGJuUmxaMlZ5UGpFeU16UTFOamM0T1RFeU16UThMMmx1ZEdWblpYSStDZ2s4TDJScFkzUStDZ2s4YTJWNVBsSmxaM1ZzWVhSdmNubEpiV0ZuWlhNOEwydGxlVDRLQ1R4a2FXTjBQZ29KQ1R4clpYaytSR1YyYVdObFZtRnlhV0Z1ZER3dmEyVjVQZ29KQ1R4emRISnBibWMrUWp3dmMzUnlhVzVuUGdvSlBDOWthV04wUGdvSlBHdGxlVDVUYjJaMGQyRnlaVlZ3WkdGMFpWSmxjWFZsYzNSSmJtWnZQQzlyWlhrK0NnazhaR2xqZEQ0S0NRazhhMlY1UGtWdVlXSnNaV1E4TDJ0bGVUNEtDUWs4ZEhKMVpTOCtDZ2s4TDJScFkzUStDZ2s4YTJWNVBsVkpTME5sY25ScFptbGpZWFJwYjI0OEwydGxlVDRLQ1R4a2FXTjBQZ29KQ1R4clpYaytRbXgxWlhSdmIzUm9RV1JrY21WemN6d3ZhMlY1UGdvSkNUeHpkSEpwYm1jK1ptWTZabVk2Wm1ZNlptWTZabVk2Wm1ZOEwzTjBjbWx1Wno0S0NRazhhMlY1UGtKdllYSmtTV1E4TDJ0bGVUNEtDUWs4YVc1MFpXZGxjajR5UEM5cGJuUmxaMlZ5UGdvSkNUeHJaWGsrUTJocGNFbEVQQzlyWlhrK0Nna0pQR2x1ZEdWblpYSStNekkzTmpnOEwybHVkR1ZuWlhJK0Nna0pQR3RsZVQ1RmRHaGxjbTVsZEUxaFkwRmtaSEpsYzNNOEwydGxlVDRLQ1FrOGMzUnlhVzVuUG1abU9tWm1PbVptT21abU9tWm1PbVptUEM5emRISnBibWMrQ2drSlBHdGxlVDVWU1V0RFpYSjBhV1pwWTJGMGFXOXVQQzlyWlhrK0Nna0pQR1JoZEdFK0Nna0pUVWxKUkRCM1NVSkJha05EUVRoM1JVbFFORU16YzNGUmRGQXhVekpvZDBKYWVrTnZTR056YjBneWVFNTFOV01yWVRSUk5EVnZTakZOUzBZekNna0pRa1ZGUlRKbE9UTmxiMVpQZUhWbU1HVkxVRlZ4VGtWbk5ucE5iRUp6VG5FcmFuSXJVbkZOUVhoVGFGWkJMMk5VTlc5dWEzSXdkQ3RGTUVoTENna0pibE5rZGtoTk1pOUdaWFJ5VDNGcFQwazBSSFpJVUVsRVZ6QkVNblZCVVZFemFXOWlVSGRoUVd4R2JGaElVRmR5T0UxS0x5dDNVVkZIVkd4dUNna0pSVmhQTVRaT2RESnJWVVVyZHk4dlFteEhkMVE0VjNoU1pYa3ZTVTQxU1cxTmJHdFplbHBzU25wYWNrODNkVmwwYkhCbFozazJLM2hKYVd3eUNna0pRakpZYkhrMGFVZDRVbHBwVWxkNU5YTkxjRkZ2TWxsNmIwcEZiVzFYVTI1bWFuVXdZMVV5TDNKaU9VWkNkblZXYVM5clYxTkdia0ZyZERSNUNna0pjVkYzTkdzd2FXSjBjRFZYSzFsVlEwTnZabTh6ZVdWdWFrMHlWV013Yml0NVNFeHlVMjB3UlRsUFVETndkRXhVTjNaSGNuSm1hM0l6V0ZKcENna0pkSGRFY0dSQ1QzTnpLMWg2U0VGUldFdDFjRzg1V0dreFVXMU9iR3AxVkdveGFrcFpielpOYzFreU9VUllPVVZhY0ZkRWRtcEpjMGw1VEhkNENna0pRalJqYlVsVFZXWTRRbTV5VWxGSE9VUkJNMDFsWXpaaWFGUmtVRUpqZFV0WFpIQkNibTVETWxZNFYzQm1UWEJ3VlVRMlUyUm5kVzVwZWpaNkNna0pURWN3Tm1OR1IzZHZVWFp1V1hoUmExVnJhMnBrV1dSNk5HODVlWE01TDNaeFEySnhabkJ1TkhSalpFa3lNV001WjI5TmQweG9SSE5vWW1zMUNna0pVRU5hUW5Ob05VWTBVMUpTYVdkQlYzSkJVME5CZWprNE1rSTNiemh3UTBOYUwycFpLM2xhUTNwQmIzSjZTRzV6UjJaMmQwdHBTbEJCVFdwcENna0paVEEwUnpScVNrMDRjRXBSVVU1dVdtRmhVQ3QwUm1Wc1pHaEVSMUZ1YnpBMGRtWktSRmt6T0VaR1RTdGhaVU4zZWxKeVF5OURVR0pyWlZwUkNna0pOWFI1TlRkTVNYTnpNVWh5VW1VelNURmpLMFpNTlhCdVptd3ZhRXN4UWpGMVFUUkhSRFJXYkZreFUweE1NWGsxYWpSSGRVWlVNMWhUZUhwaUNna0pXbEl2Wm1KRWExVjVWSE5VTTNJMmVHZG9XblJOTkVKWVNXOWhOakphUkVNelNWQnRUMko0UzJKb2JHRkxRVFJ0U3pKek0xRkNORlpqTmxNdkNna0piVFoxWVRaUWFrd3ZRakUxUXpCalRHcHlNVU5OYjB4MEx6YzRURlZSVjIxR1JYVjVTa2hrZG5SVE5uaEliV3RNUkc5RlpXMXRNSGxEY0dKcUNna0pNbWhyUm10MmQzZElTbGcyU0RGaVVtMUtXUzlIVW1ZMVVYVklXRFZLZGxrM1pHaE9ZMll6TkVObWFWRXhSSGR3WjJWS1VrdzVlVE4yU0cwdkNna0paa0ZTVjBKeFdETmtXalYxVlVwWGNVTnpNa2x2TUZkSVJHZHFNVGgzY1c1dlVFdzJRblJIY2pWaFdFSkZlR0YzV2twR1QxWk9jVlpqVjJsUENna0pPRTlMTXpodVNERkthR2N4Vms0NFVVUkJlbGhtVEVwalEydzBVRU42TW01c1ZscFNNRGwxV25GME5scFBhWEZqVlVOeVozaFpiVGRJUWt0YUNna0pPUzlCUm1JeVZteExVRlJaVFROdWVYQkRlR2g1TW1OTVFub3dLM1JDSzBWNlYwaFRiamx6VTNGTWVsTjFlRkJPZEdJeFkyMUZNbm81T0ZOb0Nna0pNazFVVnpKYVdrNDJOV2R2WWt4clNVNXdiemRVYjFSQk1tNTBjSFkxWmpCcWRsaHBWblpJVjFWMWRtaFVTVmxMWkc0dkt6QTBjek5KUTBWTENna0pRVmxKUTBOUE5qZ3Zha3h1Y0RWUVVFUnVSbVZzUTNaMWQwZEZSVEZrYjBsTU56WjZVbGxOT1dscldUSkhSVkI1Tlc1WGRXMXlkWHA0VTJSQ0Nna0pNVVJCTm5OT2VVeFFhbk4yUW5CbllVVm5XbUkwT1VwWFNqbEVSVTV2WVdaS2VHUTRkbEJvUm5wT1JIWkVMME5SS3pVNFZHdENZbVl3V0VWTENna0phMnhJUnpkek9GWTBTa1JzWVM5ak1UQmpTRGN5V1M4d0wwbE9VaTlrVVZrMVYzRlNhSE5pU0VWRmFsQlZla2REVEdOVlBRb0pDVHd2WkdGMFlUNEtDUWs4YTJWNVBsZHBabWxCWkdSeVpYTnpQQzlyWlhrK0Nna0pQSE4wY21sdVp6NW1aanBtWmpwbVpqcG1aanBtWmpwbVpqd3ZjM1J5YVc1blBnb0pQQzlrYVdOMFBnbzhMMlJwWTNRK0Nqd3ZjR3hwYzNRKzwvc3RyaW5nPgoJCTxrZXk+UGhvbmVOdW1iZXJOb3RpZmljYXRpb25VUkw8L2tleT4KCQk8c3RyaW5nPmh0dHBzOi8vYWxiZXJ0LmFwcGxlLmNvbS9kZXZpY2VzZXJ2aWNlcy9waG9uZUhvbWU8L3N0cmluZz4KCQk8a2V5PkFjdGl2YXRpb25TdGF0ZTwva2V5PgoJCTxzdHJpbmc+QWN0aXZhdGVkPC9zdHJpbmc+Cgk8L2RpY3Q+CjwvZGljdD4KPC9wbGlzdD4=");
        private byte[] mon6 = Convert.FromBase64String("PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPCFET0NUWVBFIHBsaXN0IFBVQkxJQyAiLS8vQXBwbGUvL0RURCBQTElTVCAxLjAvL0VOIiAiaHR0cDovL3d3dy5hcHBsZS5jb20vRFREcy9Qcm9wZXJ0eUxpc3QtMS4wLmR0ZCI+CjxwbGlzdCB2ZXJzaW9uPSIxLjAiPgo8ZGljdD4KCTxrZXk+RmlsdGVyPC9rZXk+Cgk8ZGljdD4KCQk8a2V5PkV4ZWN1dGFibGVzPC9rZXk+CgkJPGFycmF5PgoJCQk8c3RyaW5nPm1vYmlsZWFjdGl2YXRpb25kPC9zdHJpbmc+CgkJPC9hcnJheT4KCTwvZGljdD4KPC9kaWN0Pgo8L3BsaXN0Pgo=");

        private byte[] byte_6 = Convert.FromBase64String(GClass7.smethod_0("iunthD"));
        private byte[] byte_7 = Convert.FromBase64String(GClass7.smethod_0("iunthP"));
        private byte[] byte_8 = Convert.FromBase64String(GClass7.smethod_0("ldrestart"));
        private byte[] byte_9 = Convert.FromBase64String(GClass7.smethod_0("i13update"));
        private byte[] byte_10 = Convert.FromBase64String(GClass7.smethod_0("i13reset"));
        private byte[] byte_11 = Convert.FromBase64String(GClass7.smethod_0("i12update"));
        private byte[] byte_12 = Convert.FromBase64String(GClass7.smethod_0("i12reset"));
        private byte[] byte_13 = Convert.FromBase64String(GClass7.smethod_0("lzma"));
        private byte[] byte_14 = Convert.FromBase64String(GClass7.smethod_0("Library"));
        private byte[] byte_15 = Convert.FromBase64String(GClass7.smethod_0("sub"));
        private byte[] byte_16 = Convert.FromBase64String(GClass7.smethod_0("purple"));
        private byte[] byte_17 = Convert.FromBase64String(GClass7.smethod_0("mon5"));
        private byte[] byte_18 = Convert.FromBase64String(GClass7.smethod_0("ldrunps"));
        public static string ShetouaneDir = "/tmp/Backup";
        public void UploadActivationFiles()
        {
            this.SSH("rm -rf " + ShetouaneDir);
            this.SSH(string.Concat(new string[]
            {
                "mkdir ",
                ShetouaneDir,
                " && chown -R mobile:mobile ",
                ShetouaneDir,
                " && chmod -R 755 ",
                ShetouaneDir
            }));

            this.SSH("mkdir -p " + this.Folder_WLPref + " && chmod 775 " + this.Folder_WLPref);
            this.ConvertPlist(tmpDir + "com.apple.commcenter.device_specific_nobackup.plist", 2);

            this.UploadLocalFile(tmpDir + "com.apple.commcenter.device_specific_nobackup.plist", ShetouaneDir + "/Route_CommCe.plist");
            this.SSH(string.Concat(new string[]
            {
                "mv -f ",
                ShetouaneDir,
                "/Route_CommCe.plist ",
                this.Route_CommCe,
                " && chflags uchg ",
                this.Route_CommCe
            }));
            this.SSH("mkdir -p " + this.Folder_ActRec + " && chmod 775 " + this.Folder_ActRec);
            this.ConvertPlist(tmpDir + "activation_record.plist", 2);
            this.UploadLocalFile(tmpDir + "activation_record.plist", ShetouaneDir + "/Route_ActRec.plist");
            this.SSH(string.Concat(new string[]
            {
                "mv -f ",
                ShetouaneDir,
                "/Route_ActRec.plist ",
                this.Route_ActRec,
                " && chflags uchg ",
                this.Route_ActRec
            }));

            this.ConvertPlist(tmpDir + "data_ark.plist", 2);
            this.UploadLocalFile(tmpDir + "data_ark.plist", ShetouaneDir + "/Route_DataArk.plist");

            this.SSH(string.Concat(new string[]
            {
                "cp -f ",
                ShetouaneDir,
                "/Route_DataArk.plist ",
                this.Route_ArkInt,
                " && mv -f ",
                ShetouaneDir,
                "/Route_DataArk.plist ",
                this.Route_DatArk,
                " && chflags uchg ",
                this.Route_ArkInt,
                " && chflags uchg ",
                this.Route_DatArk
            }));
        }

        public void UploadLocalFile(string localFile, string remoteFile)
        {
            Stream stream = System.IO.File.Open(localFile, FileMode.Open);
            this.scpClient.Upload(stream, remoteFile);
            stream.Close();
        }

        public bool ConvertPlist(string fileName, int method)
        {
            bool result;
            try
            {
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.FileName = ToolDir + "\\x64\\win-plutil.exe";
                if (method == 1)
                {
                    string str = string.Format("\"{0}\"", fileName);
                    processStartInfo.Arguments = "-convert xml1 " + str;
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                    result = true;
                }
                else
                {
                    string str2 = string.Format("\"{0}\"", fileName);
                    processStartInfo.Arguments = "-convert binary1 " + str2;
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Convert Error: " + ex.Message);
            }
            return result;
        }
        public void ModifyDataArkFile()
        {
            NSDictionary DataArkDictionary = (NSDictionary)PropertyListParser.Parse(new FileInfo(tmpDir + "data_ark.plist"));
            try
            {
                DataArkDictionary.Remove("-UCRTOOBForbidden");
                DataArkDictionary.Remove("ActivationState");
                DataArkDictionary.Remove("-ActivationState");
                DataArkDictionary.Add("ActivationState", "Activated");
            }
            catch (Exception)
            {
            }
            PropertyListParser.SaveAsXml(DataArkDictionary, new FileInfo(tmpDir + "data_ark.plist"));
        }
        public string GetActivationRecordFromServer()
        {
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(JBLink + "/gsm_record.php?serial=" + SNLBL.Text);
            httpWebRequest.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);
            httpWebRequest.Timeout = 12000;
            string responseBodyFromRemoteServer;
            using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseBodyFromRemoteServer = reader.ReadToEnd();
                }
            }
            return responseBodyFromRemoteServer;
        }
        private string WildcardTicket;
        public void ModifyCommcenterDsnFile()
        {
            NSDictionary ComCenterDictionary = (NSDictionary)PropertyListParser.Parse(new FileInfo(tmpDir + "com.apple.commcenter.device_specific_nobackup.plist"));
            try
            {
                ComCenterDictionary.Remove("kPostponementTicket");
            }
            catch
            {
            }

            ComCenterDictionary.Add("kPostponementTicket", new NSDictionary
            {
                {
                    "ActivationState",
                    "Activated"
                },
                {
                    "ActivationTicket",
                    this.WildcardTicket
                },
                {
                    "ActivityURL",
                    "https://albert.apple.com/deviceservices/activity"
                },
                {
                    "PhoneNumberNotificationURL",
                    "https://albert.apple.com/deviceservices/phoneHome"
                }
            });
            PropertyListParser.SaveAsXml(ComCenterDictionary, new FileInfo(tmpDir + "com.apple.commcenter.device_specific_nobackup.plist"));
        }
        public void CreateActivationRecFile()
        {
            string serverResponse = this.GetActivationRecordFromServer();
            serverResponse = serverResponse.ToString().Replace("\n", "").Replace("\r", "").Replace("\t", "");
            System.IO.File.WriteAllText(tmpDir + "act_rec.plist.tmp", serverResponse);
            NSDictionary actRecDictionary = (NSDictionary)PropertyListParser.Parse(new FileInfo(tmpDir + "act_rec.plist.tmp"));
            this.WildcardTicket = this.GetPlistProperty(actRecDictionary, "WildcardTicketToRemove", 4);
            actRecDictionary.Remove("WildcardTicketToRemove");
            PropertyListParser.SaveAsXml(actRecDictionary, new FileInfo(tmpDir + "activation_record.plist"));
        }
        public void DownloadActivationFiles()
        {
            if (!this.scpClient.IsConnected)
            {
                this.scpClient.Connect();
            }
            this.downloadFile(this.Route_CommCe, tmpDir + "com.apple.commcenter.device_specific_nobackup.plist");
            this.downloadFile(this.Route_ArkInt, tmpDir + "data_ark.plist");
        }

        public static string tmpDir = Environment.CurrentDirectory + "\\tmp\\";
        public void downloadFile(string remoteFile = "", string localFile = "")
        {
            Stream _stream = System.IO.File.Create(localFile);
            try
            {
                this.scpClient.Download(remoteFile, _stream);
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
                throw;
            }
            _stream.Close();
        }
        public bool ActivateX()
        {
            string UrlActivation = JBLink + "/activation.php";
            using (Process process = new Process())
            {
                process.StartInfo.FileName = ToolDir + "\\x64\\ideviceactivation.exe";
                process.StartInfo.Arguments = "activate -s " + UrlActivation;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            string text = "";
            using (Process process2 = new Process())
            {
                process2.StartInfo.FileName = ToolDir + "\\x64\\ideviceactivation.exe";
                process2.StartInfo.Arguments = "state";
                process2.StartInfo.UseShellExecute = false;
                process2.StartInfo.RedirectStandardOutput = true;
                process2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process2.StartInfo.CreateNoWindow = true;
                process2.Start();
                text = process2.StandardOutput.ReadToEnd();
                process2.WaitForExit();
            }
            return text.Contains("Activated");
        }
        public void DeactivateX()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = ToolDir + "\\x64\\ideviceactivation.exe";
                process.StartInfo.Arguments = "deactivate";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
        }
        public void CreateActivationFolders()
        {
            this.SSH(string.Concat(new string[]
            {
                "mkdir -p ",
                this.Folder_WLPref,
                " && mkdir -p ",
                this.Folder_ActRec,
                " && mkdir -p ",
                this.Folder_ArkInt,
                " && mkdir -p ",
                this.Folder_LBLock
            }));
        }
        public void RestartLockdown()
        {
            this.SSH("launchctl unload /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist > /dev/null 2>&1 && launchctl load /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist > /dev/null 2>&1");
        }
        private void HelloBTNJ_Click(object sender, EventArgs e)
        {
            HelloBTNJ.Enabled = false;
            if (JB1518.Checked)
            {
                try
                {
                    string snx = SNLBL.Text;
                    string ecidx = ECIDLBL.Text;
                    checker check = new checker();

                    if (check.checkhello(ecidx))
                    {

                        DialogResult dialogResult = CustomMessageBox.Show("Which Bypass do you want?\r\n" +
                            "\r\n- No SN Change Change ( iPhone Original Serial Number )\r\n" +
                            "- SN Change (Use Purple Mode to set Serial Number to: C39LN8QDFRC9", "Confirm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        yesText: "No SN Change",
                        noText: "SN Change");
                        if (dialogResult == DialogResult.Yes)
                        {
                            Thread rdx = new Thread(JailbreakHello);
                            rdx.IsBackground = true;
                            rdx.Start();
                        }
                        if (dialogResult == DialogResult.No)
                        {
                            if (SNLBL.Text != "C39LN8QDFRC9")
                            {
                                CustomMessageBox.Show("Please Change Serial Number to: \r\n C39LN8QDFRC9");
                                AddLog("Please Change Serial Number to: C39LN8QDFRC9");
                                Clipboard.SetText("C39LN8QDFRC9");
                            }
                            else
                            {
                                Thread rdx = new Thread(JailbreakHelloSN);
                                rdx.IsBackground = true;
                                rdx.Start();
                            }

                        }
                        else
                        {

                            MessageBoxManager.Yes = "Yes";
                            MessageBoxManager.No = "No";
                        }

                    }
                    else
                    {
                        HelloBTNJ.Enabled = true;
                        throw new Exception("Device Not Registered!");

                    }

                }
                catch (Exception ex)
                {
                    HelloBTNJ.Enabled = true;
                    CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + ECIDLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //  statux2("Please Register: " + ECIDLBL.Text + "\r\n at: https://iskorpion.com/post/12_ios-15-17-ramdisk-hello-bypass-no-signal.html");
                }


            }
            else
            {
                try
                {
                    string snx = SNLBL.Text;
                    string ecidx = ECIDLBL.Text;
                    checker check = new checker();

                    if (!check.checkmeid(snx))
                    {
                        HelloBTNJ.Enabled = true;
                        throw new Exception("Device Not Registered!");
                    }
                    Thread jbx = new Thread(DoBypassiPAD);
                    jbx.IsBackground = true;
                    jbx.Start();
                }
                catch (Exception ex)
                {
                    HelloBTNJ.Enabled = true;
                    CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + SNLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //  statux2("Please Register: " + SNLBL.Text + "\r\n at: https://iskorpion.com/post/17_ios-12-14-full-meid-gsm-no-signal.html");
                }

            }


        }

        public void RamdiskHello()
        {
            try
            {
                connectSSH();
                GenerateSN();
                string ecid = ECIDLBL.Text;
                if (!Directory.Exists(ToolDir + "\\Backup"))
                {
                    Directory.CreateDirectory(ToolDir + "\\Backup");
                }


                if (!System.IO.File.Exists(ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip"))
                {
                    using (System.Net.WebClient webClient = new System.Net.WebClient())
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        webClient.DownloadFile(SNLink + "/zips/" + ECIDLBL.Text + ".zip", ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip");
                    }
                }


                bool flag = Directory.Exists(Environment.CurrentDirectory + "\\Backup\\" + ecid);
                if (flag)
                {
                    Directory.Delete(Environment.CurrentDirectory + "\\Backup\\" + ecid, true);
                }
                string text = SSH("find /mnt2/containers/Data/System -name 'internal'").Replace("\n", "").Replace("//", "/");
                string text2 = Environment.CurrentDirectory + "\\Backup\\" + ecid + ".zip";
                string text3 = Environment.CurrentDirectory + "\\Backup\\" + ecid;
                ZipFile.ExtractToDirectory(text2, text3);
                progx(20, "Checking Backup...");
                string text4 = SSH("find /mnt2/containers/Data/System -iname 'internal'").Replace("Library/internal", "").Replace("\n", "")
                    .Replace("//", "/");
                SSH("chflags nouchg /mnt2/tmp/");
                SSH("rm -R /mnt2/tmp/" + ecid);
                SSH("rm -rf /mnt2/mobile/Media/" + ecid);
                progx(30, "Uploading Backup Via Scp");
                SSH("mkdir /mnt2/tmp/" + ecid);
                SSH("mkdir -p /mnt2/tmp/" + ecid);
                this.scpClient.Upload(new DirectoryInfo(Environment.CurrentDirectory + "\\Backup\\" + ecid), "/mnt2/tmp/" + ecid);
                SSH("mv -f /mnt2/tmp/" + ecid + " /mnt2/mobile/Media/" + ecid);
                SSH("/usr/sbin/chown -R mobile:mobile /mnt2/mobile/Media/" + ecid);
                progx(40, "Activating Backup...");
                SSH("chmod 644 /mnt2/mobile/Media/" + ecid + "/activation_record.plist");
                SSH("cp -Rfv /mnt2/mobile/Media/" + ecid + "/mnt2/mobile/Media/XS");
                SSH("chmod 644 /mnt2/mobile/Media/" + ecid + "/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chmod 644 /mnt2/mobile/Media/" + ecid + "/fpkd");
                SSH("chflags nouchg " + text + "/data_ark.plist");
                SSH("mkdir " + text4 + "/Library/activation_records");
                SSH("mkdir -p" + text4 + "/Library/activation_records");
                progx(60, "...");
                SSH(string.Concat(new string[] { "mv -f /mnt2/mobile/Media/", ecid, "/activation_record.plist ", text4, "/Library/activation_records/activation_record.plist" }));
                SSH("chmod 755 " + text4 + "/Library/activation_records/activation_record.plist");
                SSH("chmod 777 " + text4 + "/Library/activation_records/activation_record.plist");
                SSH("chflags uchg " + text4 + "/Library/activation_records/activation_record.plist");
                SSH("chflags nouchg /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("mv -f /mnt2/mobile/Media/" + ecid + "/com.apple.commcenter.device_specific_nobackup.plist /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("/usr/sbin/chown root:mobile /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chmod 755 /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                progx(70, "...");
                SSH("chflags uchg /mnt2/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chmod -R 777 /mnt2/mobile/Library/FairPlay/");
                SSH("chmod -R 775 /mnt2/mobile/Library/FairPlay/");
                SSH("chmod -R 775 /mnt2/mobile/Library/FairPlay/");
                SSH("chflags nouchg /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("chflags nouchg /mnt2/mobile/Library/FairPlay/iTunes_Control/");
                SSH("chflags nouchg /mnt2/mobile/Library/FairPlay/");
                SSH("chflags nouchg /mnt2/mobile/Library/FairPlay");
                SSH("rm -rf /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("rm -rf /mnt2/mobile/Library/FairPlay/");
                SSH("mkdir /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("mkdir -p /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("mkdir -m 755 -p /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("mkdir /mnt2/mobile/Library/FairPlay/");
                SSH("mkdir -p /mnt2/mobile/Library/FairPlay/iTunes_Control/");
                SSH("mkdir -p /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("chmod 755 /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("mv -f /mnt2/mobile/Media/" + ecid + "/fpkd /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("chmod 00664 /mnt2/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");

                SSH("/usr/sbin/chown -R mobile:mobile /mnt2/mobile/Library/FairPlay");
                SSH("mv /mnt6/*/usr/local/standalone/firmware/Baseband /mnt6/*/usr/local/standalone/firmware/Baseband2");
                string text5 = SSH("test -e /mnt6/*/usr/local/standalone/firmware/Baseband2 && echo 'YES'").Replace("\n", "").Replace("//", "/");
                if (text5 != "YES")
                {
                    SSH("mv -f $(find /mnt6/*/usr/local -iname Baseband) $(find /mnt6/*/usr/local -iname Baseband)/../Baseband2");
                }
                SSH("chflags nouchg /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SSH("rm -rf /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SSH("chflags nouchg /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                AirPlane();
                try
                {
                    this.scpClient.Upload(new FileInfo(Environment.CurrentDirectory + "\\x64\\adb.dll"), "/mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                }
                catch
                {
                    SSH("echo '<plist version=\"1.0\"><dict><key>SetupVersion</key><integer>11</integer><key>UserChoseLanguage</key><true/><key>PBDiagnostics4Presented</key><true/><key>PrivacyPresented</key><true/><key>WebKitAcceleratedDrawingEnabled</key><false/><key>PaymentMiniBuddy4Ran</key><true/><key>RestoreChoice</key><true/><key>Mesa2Presented</key><true/><key>GuessedCountry</key><dict><key>countries</key><array><string>CA</string></array><key>at</key><date>2019-11-18T20:06:16Z</date></dict><key>Locale</key><string>en_CA</string><key>setupMigratorVersion</key><integer>10</integer><key>WebKitShrinksStandaloneImagesToFit</key><true/><key>SetupFinishedAllSteps</key><true/><key>HSA2UpgradeMiniBuddy3Ran</key><true/><key>lastPrepareLaunchSentinel</key><array><date>2019-11-20T00:05:01Z</date><integer>0</integer></array><key>AppleIDPB10Presented</key><true/><key>PrivacyContentVersion</key><integer>2</integer><key>UserInterfaceStyleModePresented</key><true/><key>Language</key><string>en-CA</string><key>SetupLastExit</key><date>2019-11-18T20:08:55Z</date><key>chronicle</key><dict><key>features</key><array></array></dict><key>SiriOnBoardingPresented</key><true/><key>AssistantPresented</key><true/><key>WebDatabaseDirectory</key><string>/var/mobile/Library/Caches</string><key>WebKitOfflineWebApplicationCacheEnabled</key><true/><key>SetupState</key><string>SetupUsingAssistant</string><key>AutoUpdatePresented</key><true/><key>ScreenTimePresented</key><true/><key>Passcode4Presented</key><true/><key>SetupDone</key><true/><key>WebKitLocalStorageDatabasePathPreferenceKey</key><string>/var/mobile/Library/Caches</string></dict></plist>' &> /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                }
                SSH("chflags uchg /mnt2/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SSH("killall -9 backboardd");
                progx(100, "...");
                SSH("/sbin/reboot");
                progx(0, "");
            }
            catch (Exception ex)
            {
                statux2(ex.Message);
                progx(0, "");
            }
        }

        private void AirPlane()
        {
            if (AirPlaneMode.Checked)
            {
                SSH("chflags nuchg /mnt2/preferences/SystemConfiguration/com.apple.radios.plist");
                SSH("rm -rf /mnt2/preferences/SystemConfiguration/com.apple.radios.plist");

                SSH("/mnt2/tmp/plutil -create /mnt2/preferences/SystemConfiguration/com.apple.radios.plist");
                SSH("/mnt2/tmp/plutil -key AirplaneMode -true /mnt2/preferences/SystemConfiguration/com.apple.radios.plist");
                SSH("chflags uchg /mnt2/preferences/SystemConfiguration/com.apple.radios.plist");

            }
        }

        public void jbactfiles()
        {

            findactpath = SSH("find /private/var/containers/Data/System -iname 'internal'").Replace("Library/internal", "").Replace("\n", "").Replace("//", "8");



            internalpath = findactpath + "Library/internal/";
            actrecpath = findactpath + "Library/activation_records/";
            prefpath = "/var/wireless/Library/Preferences/";
            arkplist = internalpath + "data_ark.plist";
            activationrecordplist = actrecpath + "activation_record.plist";
            podrecordplist = actrecpath + "pod_record.plist";
            commcenterplist = prefpath + "com.apple.commcenter.device_specific_nobackup.plist";
            fairplayfolder = "/private/var/mobile/Library/FairPlay";

        }

        public static string findactpath;
        public static string internalpath;
        public static string actrecpath;
        public static string prefpath;
        public static string arkplist;
        public static string activationrecordplist;
        public static string podrecordplist;
        public static string commcenterplist;
        public static string fairplayfolder;
        private Process ideviceinfo = null;

        public string PostGestalt()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(ProdLBL?.Text) || string.IsNullOrEmpty(SNLBL?.Text))
                {
                    return "Error: Product Type and Serial Number are required";
                }

                string ProdT = ProdLBL.Text.Trim();

                // Determine device class and product name based on ProductType
                string DevClass = "";
                string DevProd = "";

                // Check ProductType string
                if (ProdT.StartsWith("iPhone"))
                {
                    DevClass = "iPhone";
                    DevProd = "iPhone";
                }
                else if (ProdT.StartsWith("iPad"))
                {
                    DevClass = "iPad";
                    DevProd = "iPad";
                }
                else
                {
                    return "Error: Invalid Product Type. Must start with 'iPhone' or 'iPad'";
                }

                string iOSV = iOSLBL?.Text?.Trim() ?? "";
                string DevArch = "arm64";
                string DevBuild = BuildLBL?.Text?.Trim() ?? "";
                string DevModel = ModelLBL?.Text?.Trim() ?? DetermineModel(ProdT);
                string DevHard = HardLBL?.Text?.Trim()?.ToUpper() ?? "";
                string DevPlat = CPIDLBL?.Text?.Trim()?.Replace("0x", "") ?? "";
                string DevSerial = SNLBL.Text.Trim();
                string DevEcid = ECIDLBL?.Text?.Trim() ?? "";

                // Validate Serial Number format
                if (!Regex.IsMatch(DevSerial, @"^[A-Z0-9]{10,15}$"))
                {
                    return "Error: Invalid Serial Number format";
                }

                // Build POST data
                var postData = "ProductType=" + Uri.EscapeDataString(ProdT);
                postData += "&iOSVersion=" + Uri.EscapeDataString(iOSV);
                postData += "&DeviceClass=" + Uri.EscapeDataString(DevClass);
                postData += "&Arch=" + Uri.EscapeDataString(DevArch);
                postData += "&BuildNumber=" + Uri.EscapeDataString(DevBuild);
                postData += "&Model=" + Uri.EscapeDataString(DevModel);
                postData += "&Hardware=" + Uri.EscapeDataString(DevHard);
                postData += "&Platform=" + Uri.EscapeDataString(DevPlat);
                postData += "&SerialNumber=" + Uri.EscapeDataString(DevSerial);
                postData += "&ProductName=" + Uri.EscapeDataString(DevProd);
                postData += "&ecid=" + Uri.EscapeDataString(DevEcid);

                byte[] data = Encoding.UTF8.GetBytes(postData);

                // Make request
               // string baseUrl = "http://SERVERURL/xsn"; // Or use XSNLink variable
                var request = (HttpWebRequest)WebRequest.Create($"{XSNLink}/gestalt.php");

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Timeout = 30000; // 30 second timeout
                request.UserAgent = "DeviceInfoClient/1.0";

                // Write data
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                // Get response
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string responseString = streamReader.ReadToEnd();

                    // Check for error in response
                    if (responseString.Contains("Error:") || response.StatusCode != HttpStatusCode.OK)
                    {
                        return $"Server Error: {responseString}";
                    }
                    Debug.WriteLine(responseString);
                    return responseString;
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)webEx.Response)
                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        string errorText = reader.ReadToEnd();
                        return $"HTTP Error {(int)errorResponse.StatusCode}: {errorText}";
                    }
                }
                Debug.WriteLine($"Network Error: {webEx.Message}");
                return $"Network Error: {webEx.Message}";
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                return $"Error: {ex.Message}";
            }
        }

        public void MakeAct()
        {
            string shetouaneX = Environment.CurrentDirectory + "\\Backup\\" + ECIDLBL.Text + "\\";
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Backup\\" + ECIDLBL.Text))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Backup\\" + ECIDLBL.Text);
            }


            try
            {
                DownloadString(XSNLink + "/getRecord.php?&SerialNumber=" + ECIDLBL.Text, Environment.CurrentDirectory + "\\backup\\" + ECIDLBL.Text + "\\activation_record.plist");
                DownloadString(XSNLink + "/getSisv.php?&SerialNumber=" + ECIDLBL.Text, Environment.CurrentDirectory + "\\backup\\" + ECIDLBL.Text + "\\IC-Info.sisv");
                DownloadString(XSNLink + "/getComm.php?&SerialNumber=" + ECIDLBL.Text, Environment.CurrentDirectory + "\\backup\\" + ECIDLBL.Text + "\\com.apple.commcenter.device_specific_nobackup.plist");

            }
            catch
            {

            }

        }
        private static readonly HttpClient client = new HttpClient();


        public void JailbreakHello()
        {
            connectSSHJB();
            string asset = Libs + "adb.dll";
            string gestalt = ToolDir + "\\x64\\iTunes.dll";
            string certx = ToolDir + "\\x64\\iSkorpion.plist";
            string bkpx = ToolDir + "\\Backup\\" + SNLBL.Text;
            if (Directory.Exists(bkpx))
            {
                Directory.Delete(bkpx, true);
            }
            if (!Directory.Exists(ToolDir + "\\Backup\\"))
            {
                Directory.CreateDirectory(ToolDir + "\\Backup\\");
            }
            try
            {
                progressBar1.Value = 10;
                progx(10, "Generating Activation..");

                //ActivationX();
                PostSISV();
                MakeAct();
                progx(20, "Downloading Records..");
                PostGestalt();
                MakeGestalt();
                progx(30, "Extracting Activation..");
                // iServicesFix();
                Thread.Sleep(5000);
                SSH("rm /private/var/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/com.apple.MobileGestalt.plist");
                progx(40, "Patching Device..");
                SSH("key=$(cat /private/preboot/active); mv /private/preboot/$key/usr/local/standalone/firmware/Baseband /private/preboot/$key/usr/local/standalone/firmware/Baseband2");

                //scpClient.Upload(new FileInfo(gestalt), "/private/var/mobile/iskorpion");
                // scpClient.Upload(new FileInfo(certx), "/private/var/mobile/iSkorpion.plist");
                SSH("ldid -S/private/var/mobile/iSkorpion.plist /private/var/mobile/iskorpion");
                //sshexec("/cores/binpack/bin/chmod +x /private/var/mobile/iskorpion");

                progx(50, "Sending Dylibs to device..");
                SSH("mkdir -p /var/tmp/shetouane/");
                SCP("backup\\" + SNLBL.Text + "\\purple_buddy.plist", "/var/tmp/shetouane/com.apple.MobileGestalt.plist");
                SSH("mv -f /var/tmp/shetouane/com.apple.MobileGestalt.plist /private/var/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/");
                SSH("chflags uchg /private/var/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/com.apple.MobileGestalt.plist");
                //sshexec($"/./private/var/mobile/iskorpion");
                progx(60, "Blocking OTA..");
                SSH("plutil -key com.apple.OTATaskingAgent -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.obliteration -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.softwareupdated -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.softwareupdateservicesd -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("rm -rf /private/var/log/*");

                progx(70, "Fixing iServices..");
                // sshexec("/cores/binpack/bin/mkdir /private/var/tmp/shetouane/");

                SCP("backup\\" + ECIDLBL.Text + "\\activation_record.plist", "/var/tmp/shetouane/activation_record.plist");
                SCP("backup\\" + ECIDLBL.Text + "\\IC-Info.sisv", "/var/tmp/shetouane/IC-Info.sisv");
                SSH("chown -R mobile:mobile /var/tmp/shetouane");
                SSH("chmod -R 755 /var/tmp/shetouane");

                jbactfiles();
                progx(80, "Removing Cache Files..");

                SSH("mkdir " + actrecpath);
                SSH("mv -f /var/tmp/shetouane/activation_record.plist " + activationrecordplist);


                // SSH("chflags -R uchg " + actrecpath);
                progx(90, "Fixing Permissions..");
                SSH("rm -rf /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes");
                SSH("mkdir -m 755 -p /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes");
                SSH("mv -f /var/tmp/shetouane/IC-Info.sisv /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("chmod 664 /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("chown -R mobile:mobile /private/var/mobile/Library/FairPlay");
                progx(95, "Skipping Setup..");
                Thread.Sleep(1000);
                jbskipsetup();
                progx(99, "Restarting SpringBoard");
                SSH("launchctl stop com.apple.mobileactivationd");
                SSH("launchctl start com.apple.mobileactivationd");
                SSH("killall 9 SpringBoard");
                progx(100, "Bypass Done..");
                Thread.Sleep(1000);
               // reboot();
                /*DialogResult dialogResult =CustomMessageBox.Show("Do you want to restart your iDevice?", "Restart Device", MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    //sshexec("kill 1");
                    reboot();
                }
                else
                {
                    statux2("User chose not to reboot");

                }*/

                progx(0, "");
                HelloBTNJ.Enabled = true;
            }
            catch (Exception ex)
            {
                progx(0, ex.Message);
                HelloBTNJ.Enabled = true;
                if (Directory.Exists(bkpx))
                {
                    Directory.Delete(bkpx, true);
                }
            }
            if (Directory.Exists(bkpx))
            {
                Directory.Delete(bkpx, true);
            }
            HelloBTNJ.Enabled = true;
        }



        public void jbskipsetup()
        {
            string asset2 = Libs + "adb.dll";
            try
            {
                SSH("chflags -R nouchg /private/var/mobile/Library/Preferences");
                SSH("rm -rf /private/var/root/Library/Preferences/com.apple.MobileAsset.plist");
                SSH("rm -rf /private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SCP(asset2, "/var/tmp/shetouane/purplebuddy.plist");

                SSH("mv /var/tmp/shetouane/purplebuddy.plist /private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SSH("chown mobile /private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SSH("chmod 600 /private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SSH("uicache --all && chflags uchg /private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                SSH("chmod 755 " + arkplist);
                SSH("chflags uchg " + arkplist);
                SSH("chflags uchg " + activationrecordplist);



            }
            catch
            {
                status2.Text += "\r\n ERROR SKIPPING SETUP SCREEN!\r\n";
                status2.ForeColor = System.Drawing.Color.BlueViolet;

            }

        }
        public void DownloadString(string address, string pathX)
        {

            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile(address, pathX);
            }

        }
        public void MakeGestalt()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\backup\\" + SNLBL.Text))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\backup\\" + SNLBL.Text);
            }
            Debug.WriteLine(XSNLink + "/getGestalt.php?&SerialNumber=" + SNLBL.Text);
            DownloadString(XSNLink + "/getGestalt.php?&SerialNumber=" + SNLBL.Text, Environment.CurrentDirectory + "\\backup\\" + SNLBL.Text + "\\purple_buddy.plist");



            //  File.WriteAllText(Environment.CurrentDirectory + "\\backup\\" + SNLBL.Text + "\\purple_buddy.plist", xml);
        }
        public void MakeGestaltR()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\backup\\" + ECIDLBL.Text))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\backup\\" + ECIDLBL.Text);
            }
            DownloadString(XSNLink + "/getGestalt.php?&SerialNumber=" + SNLBL.Text, tmp + ECIDLBL.Text + ".plist");



            //  File.WriteAllText(Environment.CurrentDirectory + "\\backup\\" + SNLBL.Text + "\\purple_buddy.plist", xml);
        }

        public void GenerateSN()
        {
            string SN = "C39LN8QDFRC9";
            if (SNLBL.Text == SN)
            {
                WebProxy webProxy = null;
                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    string s;
                    using (System.Net.WebClient webClient = new System.Net.WebClient())
                    {
                        s = webClient.DownloadString(SNLink + "/gen.php?udid=" + UDIDLBL.Text + "&ucid=" + UCIDLBL.Text + "&productType=" + ProdLBL.Text);
                        if (webProxy != null)
                        {
                            webClient.Proxy = webProxy;
                        }
                    }

                }
                catch (Exception ex)
                {
                    statux2(ex.Message);
                    progx(0, "");
                }
            }
            else
            {
                statux2("Please make sure you change your SN to: C39LN8QDFRC9\r\n" +
                    "SN has been copied to your clipboard");
                Clipboard.SetText(SN);
            }
        }
        public void JailbreakHelloSN()
        {
            connectSSHJB();
            string asset = Libs + "adb.dll";
            string gestalt = ToolDir + "\\x64\\iTunes.dll";
            string certx = ToolDir + "\\x64\\iSkorpion.plist";
            string bkpx = ToolDir + "\\Backup\\" + SNLBL.Text;
            string ecid = ECIDLBL.Text;
            if (Directory.Exists(bkpx))
            {
                Directory.Delete(bkpx, true);
            }
            if (!Directory.Exists(ToolDir + "\\Backup\\"))
            {
                Directory.CreateDirectory(ToolDir + "\\Backup\\");
            }


            try
            {
                progressBar1.Value = 10;
                progx(10, "Generating Activation..");
                if (!System.IO.File.Exists(ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip"))
                {
                    using (System.Net.WebClient webClient = new System.Net.WebClient())
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        webClient.DownloadFile(SNLink + "/zips/" + ECIDLBL.Text + ".zip", ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip");
                    }
                }
                //ActivationX();
                //PostSISV();
                //MakeAct();
                progx(20, "Downloading Records..");

                GenerateSN();
                Thread.Sleep(3000);

                // PostGestalt();
                //MakeGestalt();
                progx(30, "Extracting Activation..");
                string text2 = Environment.CurrentDirectory + "\\Backup\\" + ecid + ".zip";
                string text3 = Environment.CurrentDirectory + "\\Backup\\" + ecid;
                ZipFile.ExtractToDirectory(text2, text3);
                // iServicesFix();

                // SSH("rm /private/var/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/com.apple.MobileGestalt.plist");
                progx(40, "Patching Device..");
                SSH("key=$(cat /private/preboot/active); mv /private/preboot/$key/usr/local/standalone/firmware/Baseband /private/preboot/$key/usr/local/standalone/firmware/Baseband2");

                //scpClient.Upload(new FileInfo(gestalt), "/private/var/mobile/iskorpion");
                // scpClient.Upload(new FileInfo(certx), "/private/var/mobile/iSkorpion.plist");
                // SSH("ldid -S/private/var/mobile/iSkorpion.plist /private/var/mobile/iskorpion");
                //sshexec("/cores/binpack/bin/chmod +x /private/var/mobile/iskorpion");
                SSH("mkdir -p /var/tmp/shetouane/");
                SSH("mkdir -p /var/tmp/FairPlay/iTunes_Control/iTunes/");

                progx(50, "Sending Dylibs to device..");

                SCP("backup\\" + ecid + "\\data_ark.plist", "/var/tmp/shetouane/data_ark.plist");
                SCP("backup\\" + ecid + "\\activation_record.plist", "/var/tmp/shetouane/activation_record.plist");
                SCP("backup\\" + ecid + "\\com.apple.commcenter.device_specific_nobackup.plist", "/var/tmp/shetouane/com.apple.commcenter.device_specific_nobackup.plist");
                SCP("backup\\" + ecid + "\\FairPlay\\iTunes_Control\\iTunes\\IC-Info.sisv", "/var/tmp/shetouane/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");

                // SSH("chflags uchg /private/var/containers/Shared/SystemGroup/systemgroup.com.apple.mobilegestaltcache/Library/Caches/com.apple.MobileGestalt.plist");
                //sshexec($"/./private/var/mobile/iskorpion");
                progx(60, "Blocking OTA..");
                SSH("plutil -key com.apple.OTATaskingAgent -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.obliteration -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.softwareupdated -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.softwareupdateservicesd -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("/cores/binpack/bin/rm -rf /private/var/log/*");

                progx(70, "Fixing iServices..");
                // sshexec("/cores/binpack/bin/mkdir /private/var/tmp/shetouane/");

                SSH("chown -R mobile:mobile /var/tmp/shetouane");
                SSH("chmod -R 755 /var/tmp/shetouane");

                jbactfiles();
                progx(80, "Removing Cache Files..");

                SSH("mkdir " + actrecpath);
                SSH("rm " + arkplist);
                SSH("mv -f /var/tmp/shetouane/data_ark.plist " + arkplist);
                SSH("mv -f /var/tmp/shetouane/activation_record.plist " + activationrecordplist);
                SSH("rm " + commcenterplist);
                SSH("mv -f /var/tmp/shetouane/com.apple.commcenter.device_specific_nobackup.plist " + commcenterplist);

                // SSH("chflags -R uchg " + actrecpath);
                progx(90, "Fixing Permissions..");
                SSH("rm -rf /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes");
                SSH("mkdir -m 755 -p /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes");
                SSH("mv -f /var/tmp/shetouane/FairPlay/iTunes_Control/iTunes/IC-Info.sisv /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes/");
                SSH("chmod 664 /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("chown -R mobile:mobile /private/var/mobile/Library/FairPlay");
                progx(95, "Skipping Setup..");
                Thread.Sleep(1000);
                jbskipsetup();
                progx(99, "Restarting SpringBoard");
                SSH("launchctl stop com.apple.mobileactivationd");
                SSH("launchctl start com.apple.mobileactivationd");
                SSH("killall 9 SpringBoard");
                progx(100, "Bypass Done..");
                Thread.Sleep(1000);

                DialogResult dialogResult = CustomMessageBox.Show("Do you want to restart your iDevice?", "Restart Device", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    sshexec("kill 1");
                }
                else
                {
                    statux2("User chose not to reboot");

                }

                progx(0, "");
                HelloBTNJ.Enabled = true;
            }
            catch (Exception ex)
            {
                progx(0, ex.Message);
                HelloBTNJ.Enabled = true;
                if (Directory.Exists(bkpx))
                {
                    Directory.Delete(bkpx, true);
                }
            }
            if (Directory.Exists(bkpx))
            {
                Directory.Delete(bkpx, true);
            }
            HelloBTNJ.Enabled = true;
        }
        public void PasscodeJB15()
        {
            connectSSHJB();
            string asset = Libs + "adb.dll";

            if (!Directory.Exists(ToolDir + "\\Backup\\"))
            {
                Directory.CreateDirectory(ToolDir + "\\Backup\\");
            }
            try
            {
                progx(10, "Generating Activation..");

                if (!System.IO.File.Exists(ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip"))
                {
                    using (System.Net.WebClient webClient = new System.Net.WebClient())
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        webClient.DownloadFile(PasscodeLink + "/Passcode/" + ECIDLBL.Text + ".zip", ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip");
                    }
                }
                progx(20, "Extracting Activation..");
                if (Directory.Exists(ToolDir + "\\backup\\" + ECIDLBL.Text))
                {
                    Directory.Delete(ToolDir + "\\backup\\" + ECIDLBL.Text, true);
                }
                Thread.Sleep(800);
                System.IO.Compression.ZipFile.ExtractToDirectory(ToolDir + "\\Backup\\" + ECIDLBL.Text + ".zip", ToolDir + "\\Backup\\" + ECIDLBL.Text);
                progx(30, "Sending Activation Files to Device..");
                Thread.Sleep(800);
                if (!System.IO.File.Exists(ToolDir + "\\Backup\\" + ECIDLBL.Text + "\\activation_record.plist"))
                {
                    throw new Exception("Activation Files not Found!");
                }
                SSH("mkdir -p /var/tmp/shetouane");


                progx(40, "Patching Device..");

                SSH("launchctl unload -F /System/Library/LaunchDaemons/* && launchctl load -w -F /System/Library/LaunchDaemons/*");
                progx(50, "Waiting for Respring..");
                Thread.Sleep(5000);
                jbskipsetup();
                progx(60, "Blocking OTA/RESET..");
                SSH("plutil -key com.apple.OTATaskingAgent -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.obliteration -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.mobile.softwareupdated -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("plutil -key com.apple.softwareupdateservicesd -type bool -value true /private/var/db/com.apple.xpc.launchd/disabled.plist");
                SSH("rm -rf /var/log/*");
                progx(70, "Deleting old activation files..");
                SCP(ToolDir + "\\Backup\\" + ECIDLBL.Text, "/var/tmp/shetouane");
                SSH("chown -R mobile:mobile /var/tmp/shetouane/");
                SSH("chmod -R 775 /var/tmp/shetouane/");
                jbactfiles();
                progx(80, "Activating Device..");
                SSH("mkdir " + actrecpath);
                SSH("mv -f /var/tmp/shetouane/activation_record.plist " + activationrecordplist);

                SSH("rm " + arkplist);
                SSH("mv -f /var/tmp/shetouane/data_ark.plist " + arkplist);
                SSH("mv -f /var/tmp/shetouane/FairPlay /private/var/mobile/Library/");
                progx(85, "Fixing Untethered Bypass..");
                SSH("chflags nouchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("rm /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("cp /var/tmp/shetouane/com.apple.commcenter.device_specific_nobackup.plist /var/wireless/Library/Preferences/");
                SSH("chflags uchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                progx(90, "Finalizing Bypass..");
                SSH("rm -rf /private/var/root/Library/Preferences/com.apple.MobileAsset.plist");
                SCP(asset, "/private/var/root/Library/Preferences/com.apple.MobileAsset.plist");
                SSH("chmod 600 /private/var/root/Library/Preferences/com.apple.MobileAsset.plist");
                SSH("launchctl unload -w /System/Library/LaunchDaemons/com.apple.mobile.obliteration.plist");
                SSH("launchctl unload -w /System/Library/LaunchDaemons/com.apple.mobile.obliteration.plist");
                SSH("rm -rf /System/Library/LaunchDaemons/com.apple.mobile.obliteration.plist");
                progx(95, "Fixing Calls..");
                SSH("launchctl unload /System/Library/LaunchDaemons/com.apple.CommCenter.plist");
                SSH("chflags -R nouchg /private/preboot/$(cat /private/preboot/active)/usr/local/standalone/firmware/");
                progx(99, "Fixing iCloud Login..");
                SSH("chmod -R 755 /private/var/mobile/Library/FairPlay/");
                SSH("chown -R mobile:staff /private/var/mobile/Library/FairPlay");
                SSH("chmod 664 /private/var/mobile/Library/FairPlay/iTunes_Control/iTunes/IC-Info.sisv");
                SSH("rm -rf /private/var/mobile/Library/Logs/* > /dev/null 2>&1");
                SSH("killall -9 SpringBoard mobileactivationd");
                SSH("rm -rf /tmp");
                progx(100, "Rebooting Device..");
                Thread.Sleep(2000);
                SSH("kill 1");
                progx(0, "Bypass Done..");
            }
            catch (Exception ex)
            {
                CleanBackup15();
                progx(0, ex.Message);
            }
            CleanBackup15();
        }
        public void CleanBackup15()
        {
            try
            {
                if (Directory.Exists(ToolDir + "\\Backup\\" + ECIDLBL.Text))
                {
                    Directory.Delete(ToolDir + "\\Backup\\" + ECIDLBL.Text, true);
                }
                Thread.Sleep(500);

            }
            catch (Exception)
            {
            }
        }
        private void downloadBackup()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(PasscodeLink + "/Passcode/" + SNLBL.Text + ".zip", ToolDir + "\\backup\\" + SNLBL.Text + ".zip");
                }
                //continue Bypass 
            }
            catch (Exception iSKORPIONERROR)
            {
                statux2(iSKORPIONERROR.Message);
                return;
            }
        }
        public void PasscodeActivate()
        {

            connectSSHJB();
            string SN = SNLBL.Text;
            string Download = ".\\Backup\\";
            string FolDR = ".\\Backup\\" + SN;
            string BackupDev = ".\\Backup\\" + SN + ".zip";
            bool ConectadoScp = scpClient.IsConnected;
            if (System.IO.File.Exists(BackupDev))
            {
                //continue
            }
            else
            {
                Directory.CreateDirectory(Download);
                downloadBackup();
            }
            if (Directory.Exists(FolDR))
            {
                Directory.Delete(FolDR, true);
            }
            try
            {
                progx(5, "Checking iDevicepair");
                PairLoop();
                if (!sshClient.IsConnected) { sshClient.Connect(); }
                SSH("mount -o rw,union,update /");
                if (!scpClient.IsConnected)
                {
                    scpClient.Connect();
                }
                string sourceArchiveFileName = Environment.CurrentDirectory + "\\Backup\\" + SN + ".zip";
                string destinationDirectoryName = Environment.CurrentDirectory + "\\Backup\\" + SN;
                ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);
                progx(20, "Checking Backup...");
                string text2 = SSH("find /private/var/containers/Data/System/ -iname 'internal'").Replace("Library/internal", "").Replace("\n", "").Replace("//", "/");
                SSH("rm -rf /var/tmp/" + SN);
                SSH("rm -rf /var/mobile/Media/" + SN);
                progx(30, "Uploading Backup Via Scp");
                SSH("mkdir /var/tmp/" + SN);
                scpClient.Upload(new DirectoryInfo(Environment.CurrentDirectory + "\\Backup\\" + SN), "/var/tmp/" + SN);
                SSH("mv -f /var/tmp/" + SN + " /var/mobile/Media/" + SN);
                SSH("chown -R mobile:mobile /var/mobile/Media/" + SN);
                progx(40, "Preparing Device...");
                SSH("chmod -R 755 /var/mobile/Media/" + SN);
                SSH("chmod 644 /var/mobile/Media/" + SN + "/1");
                SSH("chmod 644 /var/mobile/Media/" + SN + "/2");
                SSH("chmod 644 /var/mobile/Media/" + SN + "/3");
                SSH("killall backboardd");
                Thread.Sleep(6000);
                SSH("mv -f /var/mobile/Media/" + SN + "/FairPlay /var/mobile/Library/FairPlay");
                SSH("chmod 755 /var/mobile/Library/FairPlay");
                SSH("chflags nouchg " + text2 + "/Library/internal/data_ark.plist");
                progx(50, "Checking Device...");
                SSH(string.Concat(new string[]
                {
                    "mv -f /var/mobile/Media/",
                    SN,
                    "/2 ",
                    text2,
                    "/Library/internal/data_ark.plist"
                }));
                SSH("chmod 755 " + text2 + "/Library/internal/data_ark.plist");
                SSH("chflags uchg " + text2 + "/Library/internal/data_ark.plist");
                SSH("mkdir " + text2 + "/Library/activation_records");
                SSH(string.Concat(new string[]
                {
                    "mv -f /var/mobile/Media/",
                    SN,
                    "/1 ",
                    text2,
                    "/Library/activation_records/activation_record.plist"
                }));
                SSH("chmod 755 " + text2 + "/Library/activation_records/activation_record.plist");
                SSH("chflags uchg " + text2 + "/Library/activation_records/activation_record.plist");
                SSH("chflags nouchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("mv -f /var/mobile/Media/" + SN + "/3 /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chown root:mobile /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chmod 755 /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("chflags uchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                SSH("launchctl unload /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
                SSH("launchctl load /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
                progx(60, "Activating...");
                SSH("tar -xvf /./Files -C /./");
                SSH("rm /./Files");
                Fix();
                progx(100, "Success!");
                reboot();

                progx(100, "Bypass Done...");
                Thread.Sleep(2000);
                PasscodeActBTNJ.Enabled = true;
                progx(0, "");

            }
            catch (Exception e)
            {

                progx(0, e.Message);

                PasscodeActBTNJ.Enabled = true;


            }
        }

        public void reboot()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = ToolDir + ".\\x64\\idevicediagnostics.exe";
                process.StartInfo.Arguments = "restart";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
        }
        public void InstalarDepBasicas()
        {
            connectSSHJB();
            this.UploadLocalFile(assetsDir + "uikit.tar", "/uikit.tar");
            this.SSH("cd / && tar -xvf uikit.tar && chmod 755 /usr/bin/*");
            this.UploadLocalFile(assetsDir + "9A", "/boot.tar.lzma");
            this.scpClient.Upload(new MemoryStream(this.byte_13), "/sbin/lzma");
            this.SSH("chmod 777 /sbin/lzma");
            this.SSH("lzma -d -v /boot.tar.lzma");
            this.SSH("tar -C / -xvf /boot.tar && rm -r /boot.tar");
            this.SSH("chmod +x /usr/libexec/substrate && /usr/libexec/substrate");
            this.SSH("chmod +x /usr/libexec/substrated && /usr/libexec/substrated");
            this.SSH("killall - 9 backboardd");
        }
        public void Fix()
        {

            connectSSHJB();
            try
            {
                PairLoop();
                Mount();
                InstalarDepBasicas();
                progx(70, "Installing Basic Dylibs..");
                SSH("Launchctl unload mobilactivationd.plist");
                bool rv2 = scpClient.IsConnected;
                if (!rv2)
                {
                    scpClient.Connect();
                }
                SSH("Launchctl unload mobilactivationd.plist");

                this.progx(75, "Installing Basic Dylibs...");
                scpClient.Upload(new MemoryStream(this.byte_7), "/Library/MobileSubstrate/DynamicLibraries/iuntethered.plist");
                SSH("chmod +x /Library/MobileSubstrate/DynamicLibraries/iuntethered.plist");
                Thread.Sleep(1000);
                scpClient.Upload(new MemoryStream(this.byte_6), "/Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");
                SSH("chmod +x /Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");
                scpClient.Upload(new FileInfo(".\\assets\\uikit.tar"), "/uikit.tar");
                SSH("cd / && tar -xvf uikit.tar && chmod 755 /usr/bin/*");
                SSH("killall -9 SpringBoard mobileactivationd");
                progx(84, "Activating Device..");
                Skip();
                progx(85, "Skip Config...");
                RestartAllDeamons();
                Thread.Sleep(2000);

            }
            catch (Exception ex2)
            {
                statux2(ex2.Message);
                PasscodeActBTNJ.Enabled = true;
                progx(0, "");
            }
        }
        public void Skip()
        {

            try
            {
                this.SSH("cd /System/Library && launchctl unload -w -F LaunchDaemons* && ls ./");
                this.SSH("chflags -R nouchg /private/var/mobile/Library/Preferences");
                this.scpClient.Upload(new MemoryStream(this.byte_16), "/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                this.SSH("chown mobile/private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                this.SSH("chmod 00600 /private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                this.SSH("uicache -a && chflags uchg /private/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                this.SSH("cd /System/Library && launchctl load -w -F LaunchDaemons* && ls ./");
                this.SSH("killall backboardd");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("ERROR" + ex.Message, "ERROR:(");
            }
        }
        private void PasscodeActBTNJ_Click(object sender, EventArgs e)
        {
            if (JB1518.Checked)
            {
                try
                {
                    string snx = SNLBL.Text;
                    string ecidx = ECIDLBL.Text;
                    checker check = new checker();

                    if (!check.checkpasscode(ecidx))
                    {
                        throw new Exception("Device Not Registered!");
                    }
                    PasscodeActBTNJ.Enabled = false;
                    Thread jbp = new Thread(PasscodeJB15);
                    jbp.IsBackground = true;
                    jbp.Start();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + ECIDLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    // statux2("Please Register: " + ECIDLBL.Text + "\r\n at: https://iskorpion.com/post/10_ramdisk-passcode-bypass-signal-ios-15-17.html");
                }

            }
            else
            {
                try
                {
                    string snx = SNLBL.Text;
                    string ecidx = ECIDLBL.Text;
                    checker check = new checker();

                    if (!check.check_pass(snx))
                    {
                        throw new Exception("Device Not Registered!");
                    }
                    PasscodeActBTNJ.Enabled = false;
                    Thread jbp = new Thread(PasscodeActivate);
                    jbp.IsBackground = true;
                    jbp.Start();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + SNLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    // statux2("Please Register: " + SNLBL.Text + "\r\n at: https://iskorpion.com/post/15_ios-12-14-full-passcode-bypass-signal.html");
                }


            }
        }

        private void EraseBTNJ_Click(object sender, EventArgs e)
        {
            EraseBTNJ.Enabled = false;
            DialogResult = CustomMessageBox.Show(
                   "Are you sure you want to Erase your Device?", "Erase?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult == DialogResult.Yes)
            {
                connectSSHJB();
                SSH("/usr/sbin/nvram oblit-inprogress=1 rev=2");

                SSH("kill 1");
            }
            else
            {
                EraseBTNJ.Enabled = true;
            }
        }

        public void jbaccount()
        {
            connectSSHJB();
            progx(10, "Finding iCloud Account..");
            string strpath = "/private/var/mobile/Library/Preferences/com.apple.icloud.findmydeviced.FMIPAccounts.plist";
            string pathtemp = ToolDir + "\\tmp";
            progx(40, "Bypassing iCloud Restriction..");
            if (!Directory.Exists(pathtemp))
            {
                Directory.CreateDirectory(pathtemp);
            }

            SSH("chflags nouchg " + strpath);
            progx(70, "Removing iCloud Account..");
            SSH("chflags uchg " + strpath);
            SSH("rm -rf /private/var/mobile/Library/Accounts");
            progx(90, "Fixing Files Permissions..");
            SSH("mkdir /private/var/mobile/Library/Accounts");
            progx(100, "Rebooting Device..");
            SSH("killall -9 backboardd");

            SSH("kill 1");
            progx(0, "Account Bypass Done..");

        }

        private void LogoutBTNJ_Click(object sender, EventArgs e)
        {

            if (JB1518.Checked)
            {
                try
                {
                    string snx = SNLBL.Text;
                    string ecidx = ECIDLBL.Text;
                    checker check = new checker();

                    if (!check.checkaccount(ecidx))
                    {
                        throw new Exception("Device Not Registered!");
                    }
                    Thread acc = new Thread(jbaccount);
                    acc.IsBackground = true;
                    acc.Start();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + ECIDLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    // statux2("Please Register: " + ECIDLBL.Text + "\r\n at: https://iskorpion.com/post/18_ios-12-15-logout-icloud-open-menu-not-fmi-off.html");
                }

            }
            else
            {
                try
                {
                    string snx = SNLBL.Text;
                    string ecidx = ECIDLBL.Text;
                    checker check = new checker();

                    if (!check.checkaccount(snx))
                    {
                        throw new Exception("Device Not Registered!");
                    }
                    Thread acc = new Thread(jbaccount);
                    acc.IsBackground = true;
                    acc.Start();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + SNLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //  statux2("Please Register: " + SNLBL.Text + "\r\n at: https://iskorpion.com/post/18_ios-12-15-logout-icloud-open-menu-not-fmi-off.html");
                }
            }
        }

        public void DoPatch()
        {
            connectSSHJB();
            try
            {
                progx(5, "Mount Device Start...");
                SSH("mount -uw -o union /dev/disk0s1s1");

                scpClient.Upload(new FileInfo(".\\utils\\chflags"), "/usr/bin/chflags");
                SSH("chmod 755 chflags");
                progx(15, "Upload Dylibs....");
                scpClient.Upload(new FileInfo(".\\utils\\include\\libap.c"), "/tmp/aplsud1");
                progx(20, "Upload Dylibs..");
                SSH("chmod +x /tmp/aplsud1");
                progx(25, "Upload Dylibs");
                SSH("tmp/aplsud1");
                progx(30, "Please Wait...");
                SSH("chflags -R nouchg /private/var/mobile/Library/UserConfigurationProfiles");
                progx(35, "Upload Dylibs Scp!");
                SSH("chflags -R nouchg /usr/share/firmware/wifi");
                SSH("rm /private/var/mobile/Library/UserConfigurationProfiles/EffectiveUserSettings.plist");
                SSH("rm /private/var/mobile/Library/UserConfigurationProfiles/PublicInfo/PublicEffectiveUserSettings.plist");
                scpClient.Upload(new FileInfo(".\\utils\\include\\libals.cpp"), "/usr/libexec/aplsud");
                progx(40, "Device Patching, Wait please!!");
                scpClient.Upload(new FileInfo(".\\utils\\E.bin"), "/u.bin");
                scpClient.Upload(new FileInfo(".\\utils\\include\\settings.h"), "/private/var/mobile/Library/UserConfigurationProfiles/EffectiveUserSettings.plist");
                scpClient.Upload(new FileInfo(".\\utils\\include\\peus.h"), "/private/var/mobile/Library/UserConfigurationProfiles/PublicInfo/PublicEffectiveUserSettings.plist");
                SSH("chmod 755 /private/var/mobile/Library/UserConfigurationProfiles/EffectiveUserSettings.plist");
                progx(45, "Upload Dylibs....");
                SSH("chmod 755 /private/var/mobile/Library/UserConfigurationProfiles/PublicInfo/PublicEffectiveUserSettings.plist");
                SSH("chflags -R uchg /private/var/mobile/Library/UserConfigurationProfiles");
                SSH("snappy -f / -r `snappy -f / -l | sed -n 2p` -t orig-fs");
                SSH("tar -xvf /u.bin  -C /./");
                progx(50, "Device Almost Ready!!");
                SSH("cd /./ && chmod 777 ./usr/lib/libhistory.8.dylib ./usr/lib/libncurses.6.dylib ./usr/lib/libreadline.8.dylib ./sbin/launchctl ./bin/bash");
                SSH("rm /u.bin");
                SSH("rm -rf /private/etc/rc.d");
                SSH("mkdir -p /private/etc/rc.d");
                progx(60, "Device Almost Ready!!...");
                SSH("echo '#!/bin/bash' &>/private/etc/rc.d/substrate; echo '/sbin/launchctl load -w /./Library/LaunchDaemons/com.apple.aplsud.plist' >>/private/etc/rc.d/substrate; chmod +x /private/etc/rc.d/substrate");
                SSH("chmod 777 /usr/libexec/aplsud");
                SSH("rm -rf /tmp/aplsud1");
                scpClient.Upload(new FileInfo(".\\utils\\include\\cpap.h"), "/./Library/LaunchDaemons/com.apple.aplsud.plist");
                string CMD = "@echo off\nx64\\idevice_id.exe -l | x64\\openssl.exe base64 -d | x64\\openssl.exe base64 -A | x64\\openssl.exe enc -aes-256-ecb -K 3731333437343337373732313741323534333241343632443441363134453634 | x64\\openssl.exe base64 -A | x64\\openssl.exe enc -aes-256-ecb -K 3731333437343337373732313741323534333241343632443441363134453634 | x64\\openssl.exe base64 -A";
                System.IO.File.WriteAllText(".\\x64\\hash.cmd", CMD);
                Process proceso = new Process();
                {
                    proceso.StartInfo = new ProcessStartInfo
                    {
                        FileName = ".\\x64\\hash.cmd",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                }
                ;
                proceso.Start();
                StreamReader Info = proceso.StandardOutput;
                string Retorno = Info.ReadToEnd();
                proceso.WaitForExit();
                progx(70, "Download Hash.cmd...");
                System.IO.File.WriteAllText("x64\\hash.dll", Retorno);
                scpClient.Upload(new FileInfo("x64\\hash.dll"), "/usr/share/firmware/wifi/DefaulPlatform.plist");
                progx(80, "Device Almost Ready!!");
                SSH("chflags uchg /usr/share/firmware/wifi/DefaulPlatform.plist");
                progx(90, "Upload Hash Scp!");
                SSH("/private/etc/rc.d/substrate");
                progx(100, "Your Device Successfully Patched!!");

                statux2("Device has been patched successfully!, now Jailbreak again in Normal mode and do Backup.");
            }
            catch (Exception ex)
            {

                CustomMessageBox.Show("Oops!, there was an error patching device!, re-jailbreak and try again :(\nCause of error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                p(0, "Error...");
            }
        }
        public void SnappyRename()
        {
            try
            {
                // Get the snapshot list and clean it up
                string snapshotOutput = SSH("snappy -f / -l");
                if (string.IsNullOrWhiteSpace(snapshotOutput))
                {
                    return; // Fail silently
                }

                string snapshotName = DeleteLines(snapshotOutput, 1)
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Trim();

                if (string.IsNullOrWhiteSpace(snapshotName))
                {
                    return; // Fail silently
                }

                // Execute the rename command
                string command = $"snappy -f / -r {snapshotName} --to orig-fs";
                string renameResult = SSH(command);

                // Optional: Add minimal logging for debugging without throwing
                // Console.WriteLine($"Renamed snapshot: {snapshotName}");
            }
            catch
            {
                // Completely silent failure - no exceptions, no logging
                return;
            }
        }
        private void PatchBTN_Click(object sender, EventArgs e)
        {
            Thread ptch = new Thread(DoPatch);
            ptch.IsBackground = true;
            ptch.Start();
        }
        public static string SIMStat = "kCTSIMSupportSIMStatusReady";
        public void DoBypassSignal()
        {
            switch (SIMStat)
            {
                case "kCTSIMSupportSIMStatusReady":
                    //continue
                    break;
                default:
                    CustomMessageBox.Show("Please, Insert Working Sim-Card of your device!!!", "important", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
            try
            {

                progx(5, "");
                try
                {
                    this.CleanTMP();
                    progx(6, "Cleaning Temp files...");
                    this.PairLoop();
                    progx(8, "Pairing Device...");
                    connectSSHJB();
                    progx(10, "Mounting Partitions...");
                    this.Mount();
                    progx(12, "Finding Activation Routes...");
                    this.FindActivationRoutes();
                    progx(14, "Deactivating Device...");
                    this.DeactivateX();
                    progx(16, "Deleting Substrate...");
                    this.DeleteSubstrate();
                    progx(18, "Deleting old Activation Files...");

                    this.DeleteActivationFiles();
                    progx(20, "Restarting All Daemons..");
                    this.RestartAllDeamons();
                    progx(22, "Running Snappy..");
                    this.SnappyRename();
                    progx(24, "Sending Factory Activation Ticket...");
                }
                catch (Exception ex)
                {
                    statux2(ex.Message);

                    return;
                }

                try
                {
                    this.SSH("cp /System/Library/PrivateFrameworks/MobileActivation.framework/Support/Certificates/FactoryActivation.pem /System/Library/PrivateFrameworks/MobileActivation.framework/Support/Certificates/RaptorActivation.pem");
                    progx(26, "Restarting Lockdown..");
                    this.RestartLockdown();
                    progx(28, "Creating Activation Folders...");
                    this.CreateActivationFolders();
                    progx(29, "Pairing Device...");
                    this.PairLoop();
                    progx(30, "Running Bypass Service...");
                    bool activateResult = false;
                    int intento = 1;
                    while (!activateResult && intento <= 5)
                    {
                        progx(30 + intento, "Checking Bypass Server..");
                        this.DeactivateX();
                        activateResult = this.ActivateX();
                        intento++;
                        Thread.Sleep(1000);
                    }
                    if (!activateResult)
                    {
                        this.ActivateX();
                    }
                    progx(36, "Skipping Setup...");
                }
                catch (Exception ex2)
                {
                    statux2(ex2.Message);

                    return;
                }
                try
                {
                    this.SSH("chflags nouchg " + this.Route_Purple);
                    progx(38, "Finding Activation Routes...");
                    this.FindActivationRoutes();
                    progx(42, "Download Activation Files..");
                    this.DownloadActivationFiles();
                    progx(45, "Creating Activation Records...");
                    this.CreateActivationRecFile();
                    progx(48, "Patching Baseband...");
                    this.ModifyCommcenterDsnFile();
                    progx(50, "Patching Data Ark....");
                    this.ModifyDataArkFile();
                    progx(55, "Sending Activation Files...");
                    this.UploadActivationFiles();
                    progx(65, "Installing Substrate...");
                }
                catch (Exception ex3)
                {
                    statux2(ex3.Message);

                    return;
                }

                try
                {
                    this.InstallSubstrate();
                    progx(75, "Disabling Reset/Erase...");
                    this.UploadAntiResetSettings();
                    progx(82, "Disabling OTA Update...");
                    this.DisableOTAUpdates();
                    progx(84, "Deleting Logs..");
                    this.DeleteLogs();
                    progx(85, "Patching Setup.app...");
                    this.SkipSetup();
                    progx(86, "Almost Done..");

                    progx(90, "...");
                }
                catch (Exception ex4)
                {
                    if (progressBar1.Value >= 85)
                    {
                        progx(100, "Rebooting Device...");
                        reboot();

                        this.CleanTMP();
                        return;
                    }
                    statux2(ex4.Message);
                    return;
                }

                try
                {
                    this.ldrestart1();
                    progx(94, "Cleaning Device...");
                    this.CleanTMP();

                    progx(100, "Completed.. Rebooting...");

                }
                catch (Exception)
                {

                }
                reboot();
                progx(0, " Device has Been Bypassed Successfully!");
            }
            catch (Exception ex5)
            {
                GSMBTN.Enabled = true;
                progx(0, ex5.Message);
            }
        }
        public void ldrestart1()
        {
            this.scpClient.Upload(new MemoryStream(this.byte_8), "/usr/bin/ldrestart");
            if (!this.sshClient.IsConnected)
            {
                this.sshClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(15.0);
                this.sshClient.Connect();
            }
            try
            {
                this.sshClient.CreateCommand("chmod 755 /usr/bin/ldrestart && /usr/bin/ldrestart").Execute();
                this.sshClient.Disconnect();
            }
            catch (Exception)
            {
                if (System.IO.File.Exists("%USERPROFILE%\\.ssh\\known_hosts"))
                {
                    System.IO.File.Delete("%USERPROFILE%\\.ssh\\known_hosts");
                }
            }
        }
        private void GSMBTN_Click(object sender, EventArgs e)
        {
            try
            {
                string snx = SNLBL.Text;
                checker check = new checker();

                if (!check.checkgsm(snx))
                {
                    throw new Exception("Device Not Registered!");
                }
                GSMBTN.Enabled = false;
                Thread sig = new Thread(DoBypassSignal);
                sig.IsBackground = true;
                sig.Start();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

        }

        private void DisableBBJ_Click(object sender, EventArgs e)
        {
            Thread bbr = new Thread(Dobbremove);
            bbr.IsBackground = true;
            bbr.Start();

        }

        private void EnableBBJ_Click(object sender, EventArgs e)
        {
            Thread bbr = new Thread(Dobbrestore);
            bbr.IsBackground = true;
            bbr.Start();
        }
        private void downloadFairplay()
        {
            try
            {
                this.Mount();
                string serialNumber = SNLBL.Text;
                this.scpClient.Download("/private/var/mobile/Library/FairPlay", new DirectoryInfo(Environment.CurrentDirectory + "\\Backup\\" + serialNumber + "\\FairPlay\\"));
                this.SSH("chmod 755 /private/var/mobile/Library/FairPlay/");
            }
            catch
            {
                this.downloadFairplay2();
            }
        }

        // Token: 0x0600013F RID: 319 RVA: 0x00017960 File Offset: 0x00015D60
        private void downloadFairplay2()
        {
            try
            {
                this.Mount();
                this.SSH("chmod 755 /private/var/mobile/Library/FairPlay/");
                string serialNumber = SNLBL.Text;
                this.scpClient.Download("/private/var/mobile/Library/FairPlay", new DirectoryInfo(Environment.CurrentDirectory + "\\Backup\\" + serialNumber + "\\FairPlay\\"));
                this.SSH("chmod 755 /private/var/mobile/Library/FairPlay/");
            }
            catch
            {
            }
        }

        public void BackupJB()
        {
            progx(5, "Checking SSH");
            connectSSHJB();
            if (SNLBL.Text == "NA")
            {
                string text = SSH("ioreg -l | grep IOPlatformSerialNumber --color=never").Replace("\"", "").Replace("\t", "").Replace(" ", "").Replace("|IOPlatformSerialNumber=", "").Trim();
                bool flag = text.Length <= 12;
                string result;
                if (flag)
                {
                    result = text;
                }
                else
                {
                    result = text.Remove(0, 13);
                }
                SNLBL.Text = result;
            }
            string serialNumber = SNLBL.Text;
            string path = ".\\Backup\\" + serialNumber;

            try
            {
                this.SSH("mount -o rw,union,update /");
                bool flag = System.IO.File.Exists(path);
                if (flag)
                {
                    Directory.Delete(path, true);
                }
                if (!flag)
                {
                    Directory.CreateDirectory(path);
                }
                bool flag2 = System.IO.File.Exists(".\\Backup\\" + serialNumber + ".zip");
                if (flag2)
                {
                    CustomMessageBox.Show("Backup already exists ;)", "Backup Exists", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    progx(0, "Backup Exists");
                }
                else
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\Backup\\" + serialNumber + "\\FairPlay\\");
                    string str = SSH("find /private/var/containers/Data/System/ -iname 'internal'").Replace("Library/internal", "").Replace("\n", "").Replace("//", "/");
                    this.scpClient.Download(str + "Library/internal/data_ark.plist", new FileInfo(Environment.CurrentDirectory + "\\Backup\\" + serialNumber + "\\2"));
                    this.progx(30, "Download Activation Records...");
                    this.scpClient.Download(str + "Library/activation_records/activation_record.plist", new FileInfo(Environment.CurrentDirectory + "\\Backup\\" + serialNumber + "\\1"));
                    this.scpClient.Download("/private/var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist", new FileInfo(Environment.CurrentDirectory + "\\Backup\\" + serialNumber + "\\3"));
                    this.downloadFairplay();
                    this.progx(60, "Saving Activations Records...");
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\Backup\\" + serialNumber);
                    string sourceDirectoryName = Environment.CurrentDirectory + "\\Backup\\" + serialNumber;
                    string destinationArchiveFileName = Environment.CurrentDirectory + "\\Backup\\" + serialNumber + ".zip";
                    ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName);
                    bool flag3 = Directory.Exists(Environment.CurrentDirectory + "\\Backup\\" + serialNumber);
                    bool flag4 = flag3;
                    if (flag4)
                    {
                        Directory.Delete(Environment.CurrentDirectory + "\\Backup\\" + serialNumber, true);
                    }
                    this.progx(80, "Uploading Backup to Server!");
                    this.uploadBackup2();
                    this.progx(90, "Done..!");

                    this.progx(100, "Success!");
                    statux2("Your backup has been done successfully\nnow you can ERASE the device");
                    PasscodeBackBTNJ.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {

                }
                statux2(ex.Message);
                PasscodeBackBTNJ.Enabled = true;
            }
        }
        private void PasscodeBackBTNJ_Click(object sender, EventArgs e)
        {
            PasscodeBackBTNJ.Enabled = false;
            Thread bkp = new Thread(BackupJB);
            bkp.IsBackground = true;
            bkp.Start();
        }
        public void BBServer()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BBLink + "/index.php?serialNumber=" + SNLBL.Text + "&uniqueDiviceID=" + UDIDLBL.Text + "&productType=" + ProdLBL.Text);
            string serialNumberbb = SNLBL.Text;
            string UDIDbb = UDIDLBL.Text;
            string DeviceTypebb = ProdLBL.Text;
            string postData = "thing1=" + Uri.EscapeDataString((serialNumberbb != null) ? (serialNumberbb ?? "") : "");
            postData = postData + "&thing2=" + Uri.EscapeDataString((UDIDbb != null) ? (UDIDbb ?? "") : "");
            postData = postData + "&thing3=" + Uri.EscapeDataString((DeviceTypebb != null) ? (DeviceTypebb ?? "") : "");
            byte[] data = Encoding.ASCII.GetBytes(postData);
            request.ProtocolVersion = HttpVersion.Version11;
            request.Accept = "/";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");
            string username = "vandetta";
            string password = "a3NaOWIxZ3VxZ1o5WC9KcGI3VXQzV";
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            request.UserAgent = "AssetCache/233 CFNetwork/1121.1.2 Darwin/19.3.0 (x86_64)";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = (long)data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            string responseString = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()).ReadToEnd();
            System.IO.File.WriteAllText(tmpDir + "activation_record.plist", responseString);
        }

        public static string assetsDir = Environment.CurrentDirectory + "\\assets\\";
        public void DoBroken()
        {
            try
            {

                this.progx(5, "Starting...");
                try
                {
                    this.PairLoop();
                    this.progx(10, "Pairloop Valid");
                    connectSSHJB();
                    this.progx(20, "SSH Connected! Mount");
                    this.Mount();
                    this.progx(30, "Mount Valid");
                    this.SSH("rm -rf /Library/MobileSubstrate/DynamicLibraries/");
                    this.SSH("rm -rf /Library/Frameworks/CydiaSubstrate.framework");
                    this.SSH("rm /Library/MobileSubstrate/MobileSubstrate.dylib");
                    this.SSH("rm /Library/MobileSubstrate/DynamicLibraries");
                    this.SSH("rm /Library/MobileSubstrate/ServerPlugins");
                    this.SSH("rm /usr/bin/cycc");
                    this.SSH("rm /usr/bin/cynject");
                    this.SSH("rm /usr/include/substrate.h");
                    this.SSH("rm /usr/lib/cycript0.9/com/saurik/substrate/MS.cy");
                    this.SSH("rm -rf /usr/lib/substrate");
                    this.SSH("rm /usr/lib/libsubstrate.dylib");
                    this.SSH("rm /usr/libexec/substrate");
                    this.SSH("rm /usr/libexec/substrated");
                    this.progx(40, "Download Dylibs...");

                    string str3 = this.SSH("find /private/var/containers/Data/System/ -iname 'internal'").Replace("Library/internal", "").Replace("\n", "").Replace("//", "/");
                    this.progx(50, "Wait Please...");
                    this.SSH("chflags nouchg " + str3 + "Library/internal/data_ark.plist");
                    this.SSH("chflags nouchg " + str3 + "Library/activation_records/activation_record.plist");
                    this.SSH("chflags nouchg /var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                    this.SSH("chflags nouchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                    this.SSH("chflags nouchg /var/root/Library/Lockdown/data_ark.plist");
                    this.SSH("rm " + str3 + "Library/internal/data_ark.plist");
                    this.SSH("rm " + str3 + "Library/activation_records/activation_record.plist");
                    this.SSH("rm /var/mobile/Library/Preferences/com.apple.purplebuddy.plist");

                    this.SSH("rm /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                    this.SSH("rm /var/root/Library/Lockdown/data_ark.plist");
                    this.SSH("launchctl unload -F /System/Library/LaunchDaemons/*");
                    this.SSH("launchctl load -w -F /System/Library/LaunchDaemons/*");
                    this.progx(60, "Connect Server Response");
                    this.BBServer();
                    //this.PairLoop();
                    this.SSH("rm -r /System/Library/PrivateFrameworks/MobileActivation.framework/Support/Certificates/RaptorActivation.pem");
                    this.progx(70, "DeviceActivating...");
                    this.UploadLocalFile(assetsDir + "raptor", "/System/Library/PrivateFrameworks/MobileActivation.framework/Support/Certificates/RaptorActivation.pem");
                    this.SSH("snappy -f / -r `snappy -f / -l | sed -n 2p` -t orig-fs");
                    this.progx(80, "Upload Dylibs");
                    this.UploadLocalFile(assetsDir + "uikit.tar", "/uikit.tar");
                    this.SSH("cd / && tar -xvf uikit.tar && chmod 755 /usr/bin/*");
                    this.UploadLocalFile(assetsDir + "9A", "/tmp/boot.tar.lzma");
                    this.UploadLocalFile(assetsDir + "BC", "/sbin/lzma");
                    this.SSH("chmod 755 /sbin/lzma");
                    this.SSH("lzma -d -v /tmp/boot.tar.lzma");
                    this.SSH("tar -C / -xvf /tmp/boot.tar && rm -r /tmp/boot.tar");
                    this.SSH("chmod 755 /usr/libexec/substrate && /usr/libexec/substrate");
                    this.SSH("chmod 755 /usr/libexec/substrated && /usr/libexec/substrated");
                    this.SSH("killall -9 backboardd");
                    this.progx(90, "Config Untethered");
                    this.UploadLocalFile(assetsDir + "12", "/Library/MobileSubstrate/DynamicLibraries/untethered.dylib");
                    this.UploadLocalFile(assetsDir + "34", "/Library/MobileSubstrate/DynamicLibraries/untethered.plist");
                    this.SSH("chmod +x /Library/MobileSubstrate/DynamicLibraries/untethered.dylib");
                    this.SSH("killall -9 SpringBoard mobileactivationd");
                    this.progx(92, "Respring...");
                    this.UploadLocalFile(tmpDir + "activation_record.plist", "/var/tmp/activation_record.plist");
                    this.SSH("chflags nouchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                    this.UploadLocalFile(assetsDir + "com.apple.commcenter.device_specific_nobackup.plist", "/var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                    this.SSH("chflags uchg /var/wireless/Library/Preferences/com.apple.commcenter.device_specific_nobackup.plist");
                    this.SSH("chmod -R 755 /var/containers/Data/System");
                    string str4 = this.SSH("find /private/var/containers/Data/System/ -iname 'internal'").Replace("Library/internal", "").Replace("\n", "").Replace("//", "/");
                    this.progx(94, "");
                    //this.SkipSetup();
                    this.SSH("mkdir -p " + str4 + "Library/activation_records");
                    this.SSH("cp -r /var/tmp/activation_record.plist " + str4 + "Library/activation_records/activation_record.plist");
                    this.SSH("launchctl unload /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
                    this.UploadLocalFile(Libs + "adb.dll", "/var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                    this.SSH("chmod 600 /var/mobile/Library/Preferences/com.apple.purplebuddy.plist");
                    this.SSH("launchctl load /System/Library/LaunchDaemons/com.apple.mobileactivationd.plist");
                    this.UploadLocalFile(assetsDir + "56", "/Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");
                    this.UploadLocalFile(assetsDir + "78", "/Library/MobileSubstrate/DynamicLibraries/iuntethered.plist");
                    this.progx(96, "Upload Dylibs...");
                    this.SSH("chmod +x /Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");
                    this.SSH("rm -r /Library/MobileSubstrate/DynamicLibraries/untethered.dylib");
                    this.SSH("rm -r /Library/MobileSubstrate/DynamicLibraries/untethered.plist");
                    this.SSH("rm -r /Library/MobileSubstrate/DynamicLibraries/iuntethered.dylib");
                    this.SSH("rm -r /Library/MobileSubstrate/DynamicLibraries/iuntethered.plist");
                    this.SSH("rm -r /var/mobile/Library/Logs/mobileactivationd");

                    this.progx(100, "Successfully!");

                }
                catch (Exception ex)
                {
                    statux2(ex.Message);
                    BrokenBBJ.Enabled = true;
                    return;
                }
                progx(0, "Broken Baseband Bypass done!");
                BrokenBBJ.Enabled = true;
            }
            catch (Exception ex2)
            {
                statux2(ex2.Message);
                BrokenBBJ.Enabled = true;
            }
        }
        private void BrokenBBJ_Click(object sender, EventArgs e)
        {
            try
            {
                string snx = SNLBL.Text;
                string ecidx = ECIDLBL.Text;
                checker check = new checker();

                if (!check.checkBroken(snx))
                {
                    throw new Exception("Device Not Registered!");
                }
                BrokenBBJ.Enabled = false;
                Thread bbb = new Thread(DoBroken);
                bbb.IsBackground = true;
                bbb.Start();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message + "\r\n Please Register: " + SNLBL.Text, "Device Not Registered!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                // statux2("Please Register: " + SNLBL.Text + "\r\n at: https://iskorpion.com/post/16_ios-12-14-broken-baseband.html");
            }

        }

        private void SNLBL_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(SNLBL.Text);
            statux2("Serial Number has been copied successfully!\r\n" + SNLBL.Text);
        }

        private void ECIDLBL_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ECIDLBL.Text);
            statux2("ECID has been copied successfully!\r\n" + ECIDLBL.Text);
        }


        private void UDIDLBL_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(UDIDLBL.Text);
        }

        private void ManBootBTN_Click(object sender, EventArgs e)
        {

            UDIDX = UDIDLBL.Text;
            ManBootBTN.Enabled = false;
            if (JBLBL.Text == "NA")
            {
                CustomMessageBox.Show("Please PWNDFU First");
                ManBootBTN.Enabled = true;
            }
            else
            {
                if (RD15.Checked)
                {
                    if (CPIDLBL.Text.Contains("8003"))
                    {
                        ramdisk = ProdLBL.Text + "_2.zip";
                    }
                    else
                    {
                        ramdisk = ProdLBL.Text + ".zip";
                    }


                }
                if (RD16.Checked)
                {
                    ramdisk = ProdLBL.Text + "-" + HardLBL.Text + "-16.x.zip";
                }
                if (RD17.Checked)
                {
                    ramdisk = ProdLBL.Text + "-" + HardLBL.Text + "-17.x.zip";
                }

                if (System.IO.File.Exists(ramdiskFolder + ramdisk))
                {
                    statux2("Ramdisk File Already Exists.. Booting..");
                    Thread btx = new Thread(BootXMan);
                    btx.IsBackground = true;
                    btx.Start();
                    //BootX();
                }
                else
                {
                    ManBootBTN.Enabled = false;
                    try
                    {
                        DownloaderManX(ramdiskURL + ramdisk, ramdiskFolder + ramdisk);
                    }
                    catch (Exception ex)
                    {
                        statux2(ex.Message);
                    }

                }
            }
        }

        private void ConnectSSHBTN_Click(object sender, EventArgs e)
        {
            ConnectSSHBTN.Enabled = false;
            connectSSH();
        }
        public async Task<bool> EnsureDeviceInDFUAndConnected()
        {
            try
            {
                // Run irecovery to check device
                statux2("Checking device connection...");

                // Try multiple times

                // Check if in DFU mode
                if (MODELBL.Text.ToUpper() == "DFU")
                {
                    AddLog("✓ Device in DFU mode and ready");
                    return true;
                }
                else
                {
                    AddLog("⚠️ Device connected but not in DFU mode");
                    return false;
                }

            }
            catch (Exception ex)
            {
                AddLog($"Connection check error: {ex.Message}");
                return false;
            }
        }
        private async void PWNMan_Click(object sender, EventArgs e)
        {
            try
            {
                PWNMan.Enabled = false;
                statux2("=== Starting Pwn Process ===");

                // 1. Verify device is in DFU mode
                bool deviceReady = await EnsureDeviceInDFUAndConnected();
                if (!deviceReady)
                {
                    statux2("❌ Please enter DFU mode first!");
                    return;
                }

                // 2. Run pwn
                statux2("Pwning Device...");
                await PWNDFU();

                // 3. Wait a moment
                await Task.Delay(2000);

                // 4. Verify pwn
                iRecovery();

                // Check if pwn was successful
                if (JBLBL.Text.Contains("GASTER") || JBLBL.Text.Contains("CHECKM8"))
                {
                    statux2($"✅ PwnDFU Done! Device: {JBLBL.Text}");
                }
                else
                {
                    statux2("⚠️ Device may not be properly pwned");
                }
            }
            catch (Exception ex)
            {
                statux2($"Error: {ex.Message}");
            }
            finally
            {
                PWNMan.Enabled = true;
            }
        }


        private void A12B_Click(object sender, EventArgs e)
        {
            ShowPanel("A12Panel");

            // A12Panel.BringToFront();
        }

        private void MDMB_Click(object sender, EventArgs e)
        {
            MDMPanel.BringToFront();
            ShowPanel("MDMPanel");
        }

        private void RamdiskB_Click(object sender, EventArgs e)
        {
            RamdiskPanel.BringToFront();
            ShowPanel("RamdiskPanel");
        }

        private void JailbreakB_Click(object sender, EventArgs e)
        {
            JailbreakPanel.BringToFront();
            ShowPanel("JailbreakPanel");
        }

        private void ToolBoxB_Click(object sender, EventArgs e)
        {
            ToolsPanel.BringToFront();
            ShowPanel("ToolsPanel");
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        private void DisableButtons()
        {
            ActivateButton.Enabled = false;
            guna2GradientButton2.Enabled = false;
            // AddLog("Buttons disabled during process", Color.Gray);
        }

        private void AddLog(string message, Color? color = null)
        {
            if (status2.InvokeRequired)
            {
                status2.Invoke(new Action<string, Color?>(AddLog), message, color);
                return;
            }

            try
            {
                Color logColor = color ?? Color.Black;
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string logEntry = $"[{timestamp}] {message}";

                // Save current selection
                int originalSelectionStart = status2.SelectionStart;
                int originalSelectionLength = status2.SelectionLength;

                // Append new line if not empty
                if (!string.IsNullOrEmpty(status2.Text))
                {
                    status2.AppendText(Environment.NewLine);
                }

                // Append the log entry
                status2.AppendText(logEntry);

                // Apply color to the new text
                if (logColor != System.Drawing.Color.Black)
                {
                    int startIndex = status2.Text.Length - logEntry.Length;
                    status2.Select(startIndex, logEntry.Length);
                    //LogsBox.SelectionColor = logColor;
                    status2.SelectionLength = 0;
                }

                // Auto-scroll to bottom
                status2.SelectionStart = status2.Text.Length;
                status2.ScrollToCaret();

                // Restore original selection if needed
                if (originalSelectionLength > 0)
                {
                    status2.Select(originalSelectionStart, originalSelectionLength);
                }

                // Debug output
                Debug.WriteLine($"LOG: {logEntry}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logging error: {ex.Message}");
            }
        }

        private void ShowError(string message, string title)
        {
            AddLog($"Error: {title} - {message}", Color.Red);
            //labelInfoProgres.Text = "❌ " + title;
            progressBar1.Value = 0;
            CustomMessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private DeviceCleanupManager deviceCleanupManager;
        public DeviceData CurrentDeviceData { get; private set; }
        private DeviceFileManager deviceFileManager;
        private static readonly string pythonTargetPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "iSkorpion", "python"
        );

        public bool isProcessRunning = false;

        private void UpdateUIProgress(int progressValue, string progressText, string statusText)
        {
            this.Invoke(new Action(() =>
            {
                progressBar1.Value = progressValue;
                // if (progressText != null) labelInfoProgres.Text = progressText;
                // if (statusText != null) labelInfoProgres.Text = statusText;
            }));
            AddLog($"Progress: {progressValue}% - {statusText}", Color.Blue);
        }
        private async void ActivateButton_Click(object sender, EventArgs e)
        {
            if (RegionLBL.Text.Contains("CH"))
            {
                CustomMessageBox.Show($"{NameLBL.Text} - China variant requires RJ45 Adapter or Apple Configurator!\r\n Please, if it doesn't activate contact admin for refund!", "Device might not be supported");
                // return;
            }
            
            isProcessRunning = true;
            DisableButtons();
            ActivateButton.Text = "Processing...";
            AddLog("=== Starting Activation Process ===", Color.DarkBlue);
            string currentSerialNumber = SNLBL.Text;
            try
            {
                // Validate device connection
                if (!isDeviceCurrentlyConnected || string.IsNullOrEmpty(UDIDLBL.Text))
                {
                    AddLog("Activation: No device detected", Color.Red);
                    ShowError("No iOS device detected.\n\nPlease connect your device and try again.", "Device Not Found");
                    return;
                }

                AddLog($"Activation started for: {NameLBL.Text} ({SNLBL.Text})", Color.Blue);

                // WiFi warning
                var wifiWarning = CustomMessageBox.Show(
                    "IMPORTANT: Ensure your device is connected to WiFi before continuing.\n\nClick OK to continue or Cancel to abort.",
                    "WiFi Connection Required", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                if (wifiWarning != DialogResult.OK)
                {
                    AddLog("Activation cancelled by user (WiFi warning)", Color.Orange);
                    return;
                }

                AddLog("WiFi confirmation received", Color.Green);


                checker check = new checker();

                if (!check.checkA12(SNLBL.Text, ProdLBL.Text))
                {
                    Clipboard.SetText(currentSerialNumber);
                    AddLog($"Activation: Serial {currentSerialNumber} not authorized", Color.Red);
                    CustomMessageBox.Show($"Your serial {currentSerialNumber} is not authorized.\n\nPlease register it on the website.",
                        "Authorization Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddLog("Serial authorization confirmed", Color.Green);

                Debug.WriteLine("CUrrent UDID: " + currentUdid);
                // Configure device cleanup manager
                deviceCleanupManager.SetDeviceUdid(currentUdid);

                UpdateUIProgress(10, "Preparing your device...", "Setting up activation...");
                AddLog("Device preparation started", Color.Blue);

                // Clean downloads and extract GUID
                var (cleanupSuccess, extractedGuid) = await deviceCleanupManager.ClearDownloadsAndDoubleReboot();

                if (cleanupSuccess && !string.IsNullOrEmpty(extractedGuid))
                {
                    CurrentDeviceData.Guid = extractedGuid;
                    AddLog($"GUID extracted: {extractedGuid}", Color.Green);
                    UpdateUIProgress(30, "Processing information...", "Validating with server...");

                    // API workflow
                    var (apiWorkflowSuccess, savedFilePath) = await StartApiWorkflow();

                    if (!apiWorkflowSuccess)
                    {
                        AddLog("API workflow failed", Color.Red);
                        UpdateUIProgress(0, "Unable to complete activation", "Please check your connection and try again");
                        return;
                    }

                    // File management
                    UpdateUIProgress(60, "Finalizing setup...", "Applying changes to device...");
                    AddLog("Starting device file management", Color.Blue);

                    bool fileManagementSuccess = await deviceFileManager.PerformDeviceFileManagement(
                        currentSerialNumber,
                        currentUdid,
                        savedFilePath
                    );

                    if (fileManagementSuccess)
                    {
                        AddLog("Device file management completed successfully", Color.Green);
                        await FinalizeActivation();
                    }
                    else
                    {
                        AddLog("Device file management failed", Color.Red);
                        UpdateUIProgress(0, "Setup incomplete", "Please try again or contact support");
                    }
                }
                else
                {
                    AddLog("Device cleanup or GUID extraction failed", Color.Red);
                    // Handle cleanup failure
                }
            }
            catch (Exception ex)
            {
                AddLog($"Activation error: {ex.Message}", Color.Red);
                CleanUP();
                ShowError($"An error occurred: {ex.Message}", "Error");
                await _telegramNotifier.SendActivationErrorAsync(NameLBL.Text + " (" + ProdLBL.Text + ")", SNLBL.Text, iOSLBL.Text);
            }
            finally
            {
                isProcessRunning = false;
                ActivateButton.Enabled = true;
                guna2GradientButton2.Enabled = false;
                ActivateButton.Text = "Activate Device";
                AddLog("Activation process completed", Color.Gray);
                CleanUP();
                // Refresh device info and button states
                readNormalMode();

            }
        }

        private TelegramNotifier _telegramNotifier;

        private void CleanUP()
        {
            try
            {
                if (Directory.Exists(pythonTargetPath))
                {
                    Directory.Delete(pythonTargetPath, true);
                }
                CloseExitAPP("SEC");
                CloseExitAPP("pymobiledevice3");
                CloseExitAPP("ideviceinfo");
            }
            catch
            {


            }


        }
        private void CloseExitAPP(string processName)
        {
            // AddLog($"Closing process: {processName}", Color.Gray);
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                    Debug.WriteLine($"Killed process: {process.ProcessName}", Color.Gray);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to kill process {processName}: {ex.Message}", Color.Orange);
                }
            }
        }

        private static string currentUdid = null;
        private static string currentProductType = "";
        private static string currentProductVersion = "";
        private static string currentSerialNumber = "";
        private static string currentActivationState = "";
        private static string currentImei = "";
        private static string currentEcid = "";

        // Device persistence
        private string lastConnectedUdid = null;
        private string lastDeviceModel = "";
        private string lastDeviceType = "";
        private string lastDeviceSN = "";
        private string lastDeviceVersion = "";
        private string lastDeviceActivation = "";
        private string lastDeviceECID = "";
        private string lastDeviceIMEI = "";



        private async Task<string> ExecuteProcessAsync(string fileName, string arguments)
        {
            try
            {
                // AddLog($"Executing: {fileName} {arguments}", Color.Gray);
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.WriteLine($"Warning: {error}", Color.Orange);
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: {ex.Message}", Color.Red);
                return null;
            }
        }


        private async Task<bool> GetDeviceInfo(string udid)
        {
            try
            {
                string ideviceInfoPath = Path.Combine(Libs, "ideviceinfo.exe");
                if (!System.IO.File.Exists(ideviceInfoPath))
                {
                    AddLog("ideviceinfo.exe not found", Color.Red);
                    return false;
                }

                // Fetch all info shit
                string output = await ExecuteProcessAsync(ideviceInfoPath, $"");


                if (string.IsNullOrWhiteSpace(output))
                {
                    AddLog("No device info received", Color.Orange);
                    return false;
                }

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var deviceInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var line in lines)
                {
                    int idx = line.IndexOf(':');
                    if (idx > 0 && idx < line.Length - 1)
                    {
                        string key = line.Substring(0, idx).Trim();
                        string value = line.Substring(idx + 1).Trim();
                        deviceInfo[key] = value;
                    }
                }

                // Use helper to safely extract keys
                currentProductType = GetDictValue(deviceInfo, "ProductType", "Unknown");
                currentProductVersion = GetDictValue(deviceInfo, "ProductVersion", "Unknown");
                currentSerialNumber = GetDictValue(deviceInfo, "SerialNumber", "Unknown");
                currentActivationState = GetDictValue(deviceInfo, "ActivationState", "Unknown");
                currentImei = GetDictValue(deviceInfo, "InternationalMobileEquipmentIdentity", "Unknown");
                currentEcid = GetDictValue(deviceInfo, "UniqueChipID", "Unknown");
                currentUdid = GetDictValue(deviceInfo, "UniqueDeviceID", "Unknown");
                devicedata.SerialNumber = currentSerialNumber;
                devicedata.Model = currentProductType;
                devicedata.Udid = currentUdid;
                // AddLog("test:" + devicedata.Udid);
                //  AddLog($"Device Info - Type: {currentProductType}, iOS: {currentProductVersion}, Serial: {currentSerialNumber}", Color.DarkBlue);
                Debug.WriteLine($"Product: {currentProductType}, Version: {currentProductVersion}, Serial: {currentSerialNumber}");
                return true;
            }
            catch (Exception ex)
            {
                // AddLog($"GetDeviceInfo error: {ex.Message}", Color.Red);
                Debug.WriteLine($"GetDeviceInfo error: {ex.Message}");
                return false;
            }
        }
        public DeviceData devicedata = new DeviceData();
        private static string GetDictValue(Dictionary<string, string> dict, string key, string defaultValue)
        {
            string value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static string Win64Path = Libs;

        private async Task<string> GetDeviceUdid()
        {
            try
            {
                string ideviceIdPath = Path.Combine(Win64Path, "idevice_id.exe");
                if (!System.IO.File.Exists(ideviceIdPath))
                {
                    AddLog("idevice_id.exe not found", Color.Red);
                    return null;
                }

                string output = await ExecuteProcessAsync(ideviceIdPath, "-l");
                if (!string.IsNullOrEmpty(output))
                {
                    string[] udids = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    return udids.Length > 0 ? udids[0] : null;
                }
            }
            catch (Exception ex)
            {
                AddLog($"GetDeviceUdid error: {ex.Message}", Color.Red);
            }
            return null;
        }

        private async Task<bool> CheckActivationViaDeviceProperty(int maxRetries = 5, int delayMs = 6000)
        {
            AddLog($"Checking activation status (max {maxRetries} attempts)...", Color.Blue);
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    AddLog($"Activation check attempt {attempt}/{maxRetries}", Color.Gray);
                    // Refresh device info
                    string udid = await GetDeviceUdid();
                    await GetDeviceInfo(udid);

                    if (!string.IsNullOrEmpty(currentActivationState))
                    {
                        if (currentActivationState.Equals("Activated", StringComparison.OrdinalIgnoreCase))
                        {
                            AddLog("Device is activated!", Color.Green);
                            return true;
                        }
                        else if (currentActivationState.Equals("Unactivated", StringComparison.OrdinalIgnoreCase))
                        {
                            AddLog("Device is still unactivated", Color.Orange);
                            return false;
                        }
                    }

                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    AddLog($"Activation check error: {ex.Message}", Color.Orange);
                    await Task.Delay(delayMs);
                }
            }
            AddLog("Activation check timeout reached", Color.Orange);
            return false;
        }

        public async Task<string> SkipSetupXX(string arguments)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string iOSPath = Path.Combine(Libs, @"ios.exe");

            Process process = new Process();
            process.StartInfo.FileName = iOSPath;
            process.StartInfo.Arguments = arguments; // Usar el parámetro
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true; // Redirigir la salida estándar
            process.StartInfo.RedirectStandardError = true; // Redirigir la salida de error
            process.StartInfo.CreateNoWindow = true; // Esto ocultará la ventana de la consola

            Console.WriteLine($"[DEBUG] Ejecutando ios.exe con argumentos: {arguments}");

            process.Start();

            // Leer la salida estándar y de error del proceso
            string output = await process.StandardOutput.ReadToEndAsync();
            string errorOutput = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            // Combinar ambas salidas
            string combinedOutput = output + errorOutput;

            // Mostrar la salida en la consola de Visual Studio
            Console.WriteLine($"ios.exe Output: {output}");
            Console.WriteLine($"ios.exe Error Output: {errorOutput}");

            // Retornar la salida combinada para análisis más detallado
            return combinedOutput.Trim();
        }
        private async Task FinalizeActivation()
        {
            AddLog("Finalizing activation...", Color.Blue);
            UpdateUIProgress(100, "Verifying activation status...", "Checking if device is activated...");
            await Task.Delay(8000);

            bool isActivated = await CheckActivationViaDeviceProperty(maxRetries: 6, delayMs: 5000);

            if (isActivated)
            {
                AddLog("Device activation verified successfully!", Color.Green);
                //labelActivaction.Text = "Activated";
                readNormalMode(); // Refresh all device info

                await _telegramNotifier.SendActivationSuccessAsync(NameLBL.Text + $"({ProdLBL.Text})", SNLBL.Text, iOSLBL.Text);
                /* if (A12SkipSetup.Checked)
                 {
                     await SkipSetupXX("prepare --skip-all");
                     await Task.Delay(10000);
                 }*/
                UpdateUIProgress(100, "Activation Successful! 🎉", "Device is Activated.. 🎉");
                if (AutoOTA.Checked)
                {
                    await OTABlock();
                }
                
                if (!AutoOTA.Checked && SkipSX.Checked)
                {
                    await SkipSetupXX("prepare --skip-all");
                    //await Task.Delay(10000);
                }

                await deviceCleanupManager.RebootDeviceOnly();
                

                CustomMessageBox.Show($"{NameLBL.Text} - iOS {iOSLBL.Text} Activated Successfully! 🎉\n\nYou can now use your device normally.",
                    "Activation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                await deviceCleanupManager.RebootDeviceOnly();
                if (isActivated)
                {

                    AddLog("Device activation verified successfully!", Color.Green);
                    ActLBL.Text = "Activated";
                    readNormalMode(); // Refresh all device info

                    await _telegramNotifier.SendActivationSuccessAsync(NameLBL.Text + $"({ProdLBL.Text})", SNLBL.Text, iOSLBL.Text);
                    //  await SkipSetupXX("prepare --skip-all");
                    // await Task.Delay(10000);
                    await deviceCleanupManager.RebootDeviceOnly();
                    UpdateUIProgress(100, "Activation Successful! 🎉", "Device is Activated.. 🎉");

                    CustomMessageBox.Show($"{NameLBL.Text} - iOS {iOSLBL.Text} Activated Successfully! 🎉\n\nYou can now use your device normally.",
                        "Activation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    AddLog("Activation process completed (verification pending)", Color.Orange);

                    await _telegramNotifier.SendActivationErrorAsync(NameLBL.Text, SNLBL.Text, iOSLBL.Text);

                    UpdateUIProgress(100, "❌ Activation process Failed ❌", "Activation process completed! ❌");
                    await deviceCleanupManager.RebootDeviceOnly();

                    CustomMessageBox.Show("Activation process Failed! ❌\n\nPlease wait a few minutes then reboot your device manually activation may take a few minutes to fully complete.",
                        "Process Complete - Verification Pending", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
        }

        public static string variant = "normal";
        public string GetDownloadUrl(string modelName, string guid = null)
        {
            if (RegionLBL.Text.Contains("CH"))
            {
                variant = "china";
            }

            return $"{baseUrl}/index.php?GUID={guid}&name=sqlitedb&variant={variant}";
        }


        private string InjectKeyIntoXml(string xmlData, string keyName, string keyValue)
        {
            try
            {
                // Load the XML
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);

                // Find the <dict> node (where all the key-value pairs are)
                XmlNode dictNode = xmlDoc.SelectSingleNode("//dict");
                if (dictNode == null)
                {
                    AddLog("Could not find dict node in XML", Color.Yellow);
                    return xmlData; // Return original if we can't modify
                }

                // Check if key already exists (it shouldn't since -x doesn't include it)
                bool keyExists = false;
                foreach (XmlNode node in dictNode.ChildNodes)
                {
                    if (node.Name == "key" && node.InnerText == keyName)
                    {
                        keyExists = true;
                        break;
                    }
                }

                if (!keyExists)
                {
                    // Add the key
                    XmlElement keyElement = xmlDoc.CreateElement("key");
                    keyElement.InnerText = keyName;
                    dictNode.AppendChild(keyElement);

                    // Add the string value
                    XmlElement stringElement = xmlDoc.CreateElement("string");
                    stringElement.InnerText = keyValue;
                    dictNode.AppendChild(stringElement);
                }

                // Return the modified XML
                return xmlDoc.OuterXml;
            }
            catch (Exception ex)
            {
                AddLog($"Error injecting key into XML: {ex.Message}", Color.Yellow);
                return xmlData; // Return original if modification fails
            }
        }

        // NEW METHOD: Get specific key value only
        private async Task<string> GetSpecificKeyValue(string keyName)
        {
            try
            {
                // Get the path to ideviceinfo.exe
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string ideviceinfoPath = Path.Combine(appDir, "x64", "ideviceinfo.exe");

                if (!File.Exists(ideviceinfoPath))
                    return null;

                // Run ideviceinfo with -k flag to get specific key
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ideviceinfoPath,
                    Arguments = $"-k {keyName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Read output with timeout
                    string output = await Task.Run(() =>
                    {
                        Task<string> readOutput = process.StandardOutput.ReadToEndAsync();
                        Task<string> readError = process.StandardError.ReadToEndAsync();

                        if (Task.WaitAll(new Task[] { readOutput, readError }, 5000)) // 5 second timeout
                        {
                            return readOutput.Result;
                        }
                        else
                        {
                            process.Kill();
                            return null;
                        }
                    });

                    process.WaitForExit();

                    if (string.IsNullOrEmpty(output))
                        return null;

                    // Clean the output - sometimes contains extra whitespace or "KeyName: "
                    output = output.Trim();

                    // Remove the key name prefix if present
                    if (output.StartsWith(keyName + ":"))
                        output = output.Substring(keyName.Length + 1).Trim();

                    return output;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SendGuidToApi(string guid)
        {
            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    // Get device info XML (original method)
                    string deviceInfoXml = await GetDeviceInfoXml();

                    if (string.IsNullOrEmpty(deviceInfoXml))
                    {
                        AddLog("Failed to get device info", Color.Red);
                        return false;
                    }

                    // Get the additional key we want
                    string enclosureColor = await GetSpecificKeyValue("DeviceEnclosureColor");

                    if (!string.IsNullOrEmpty(enclosureColor))
                    {
                        // Inject the key into the XML
                        deviceInfoXml = InjectKeyIntoXml(deviceInfoXml, "DeviceEnclosureColor", enclosureColor);
                        AddLog($"✓ Added DeviceEnclosureColor: {enclosureColor}", Color.Green);
                    }
                    else
                    {
                        AddLog("⚠️ Could not get DeviceEnclosureColor", Color.Yellow);
                    }

                    // Convert XML to Base64URL
                    string base64urlInfo = ConvertToBase64URL(deviceInfoXml);

                    // Prepare POST data
                    var postData = new Dictionary<string, string>
            {
                { "guid", guid },
                { "info", base64urlInfo }
            };

                    // Create HTTP client
                    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) })
                    {
                        // Prepare request content
                        var content = new FormUrlEncodedContent(postData);

                        // Make POST request
                        var response = await client.PostAsync($"{baseUrl}/index.php", content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();

                            // Log response
                            // AddLog($"API Response: {responseBody}", Color.Green);

                            // Check if response contains a download URL (success)
                            bool success = responseBody.Contains("http") &&
                                           (responseBody.Contains("downloads") ||
                                            responseBody.Contains("sqlitedb") ||
                                            responseBody.StartsWith("http"));

                            if (success)
                            {
                                // Extract download URL
                                string downloadUrl = responseBody.Trim();
                                AddLog($"✅ Activation file ready", Color.Green);
                                return true;
                            }
                            else
                            {
                                AddLog($"❌ API returned unexpected response: {responseBody}", Color.Orange);
                            }
                        }
                        else
                        {
                            string errorContent = await response.Content.ReadAsStringAsync();
                            AddLog($"❌ API Error {response.StatusCode}: {errorContent}", Color.Red);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    retryCount++;
                    AddLog($"HTTP Error attempt {retryCount}: {ex.Message}", Color.Orange);
                }
                catch (TaskCanceledException ex)
                {
                    retryCount++;
                    AddLog($"Timeout attempt {retryCount}: {ex.Message}", Color.Orange);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    AddLog($"General Error attempt {retryCount}: {ex.Message}", Color.Orange);
                }

                if (retryCount >= maxRetries)
                    break;

                await Task.Delay(1000 * retryCount);
            }

            AddLog("All API attempts failed", Color.Red);
            return false;
        }

        // Helper method to get device info XML using ideviceinfo.exe
        private async Task<string> GetDeviceInfoXml()
        {
            try
            {
                // Get the path to ideviceinfo.exe
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string ideviceinfoPath = Path.Combine(appDir, "x64", "ideviceinfo.exe");

                if (!File.Exists(ideviceinfoPath))
                {
                    AddLog($"ideviceinfo.exe not found at: {ideviceinfoPath}", Color.Red);
                    return null;
                }

                // Run ideviceinfo with -x flag to get XML output
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ideviceinfoPath,
                    Arguments = "-x",  // XML output
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Read output with timeout
                    string output = await Task.Run(() =>
                    {
                        Task<string> readOutput = process.StandardOutput.ReadToEndAsync();
                        Task<string> readError = process.StandardError.ReadToEndAsync();

                        if (Task.WaitAll(new Task[] { readOutput, readError }, 10000)) // 10 second timeout
                        {
                            return readOutput.Result;
                        }
                        else
                        {
                            process.Kill();
                            return null;
                        }
                    });

                    process.WaitForExit();

                    if (string.IsNullOrEmpty(output))
                    {
                        AddLog("Failed to get device info from ideviceinfo", Color.Red);
                        return null;
                    }

                    // Validate XML
                    if (!output.Contains("<?xml") || !output.Contains("<plist"))
                    {
                        AddLog("Invalid XML output from ideviceinfo", Color.Red);
                        return null;
                    }

                    AddLog($"Device info XML obtained ({output.Length} chars)", Color.Green);
                    return output;
                }
            }
            catch (Exception ex)
            {
                AddLog($"Error getting device info: {ex.Message}", Color.Red);
                return null;
            }
        }

        // Convert to Base64URL format
        private string ConvertToBase64URL(string xmlData)
        {
            try
            {
                // Convert to bytes
                byte[] bytes = Encoding.UTF8.GetBytes(xmlData);

                // Convert to base64
                string base64 = Convert.ToBase64String(bytes);

                // Convert to Base64URL
                string base64url = base64
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", "");

                return base64url;
            }
            catch (Exception ex)
            {
                AddLog($"Error converting to Base64URL: {ex.Message}", Color.Red);
                return null;
            }
        }

        // Store download URL for later use


        private async Task<(bool success, string filePath)> StartApiWorkflow()
        {
            try
            {
                AddLog("Starting API workflow...", Color.Blue);
                string ecid = currentEcid;
                string serial = currentSerialNumber;
                string guid = CurrentDeviceData.Guid;

                if (string.IsNullOrEmpty(ecid) || string.IsNullOrEmpty(serial))
                {
                    AddLog("API Workflow: Missing ECID or Serial", Color.Red);
                    UpdateUIProgress(0, "ECID or Serial not available", "Missing device data");
                    return (false, null);
                }

                AddLog($"API Workflow - ECID: {ecid}, Serial: {serial}, GUID: {guid}", Color.DarkBlue);

                // Send GUID to API
                string deviceModel = NameLBL.Text;
                AddLog($"Sending GUID to API for model: {deviceModel} ({ProdLBL.Text})", Color.Blue);
                bool sendResult = await SendGuidToApi(guid);

                if (!sendResult)
                {
                    AddLog("Failed to send GUID to API", Color.Red);
                    UpdateUIProgress(0, "Failed to communicate with server", "API Error");
                    return (false, null);
                }

                AddLog("GUID sent to API successfully", Color.Green);

                // Download SQLite file
                var downloadUrl = GetDownloadUrl(deviceModel, CurrentDeviceData.Guid);
                Debug.WriteLine(downloadUrl);
                string downloadedFilePath = Path.Combine(pythonTargetPath, "downloads.28.sqlitedb");

                if (!Directory.Exists(pythonTargetPath))
                {
                    Directory.CreateDirectory(pythonTargetPath);
                    AddLog($"Created directory: {pythonTargetPath}", Color.Green);
                }

                await Task.Delay(3000);
                AddLog($"Downloading SQLite file...", Color.Blue);

                if (!await DownloadFileWithProgressAsync(downloadUrl, downloadedFilePath))
                {
                    AddLog("Failed to download SQLite file", Color.Red);
                    UpdateUIProgress(0, "Failed to download activation file", "Download Error");
                    return (false, null);
                }

                AddLog($"SQLite file downloaded successfully.", Color.Green);
                UpdateUIProgress(100, "SQLite database downloaded successfully", "API workflow completed!");
                return (true, downloadedFilePath);
            }
            catch (Exception ex)
            {
                AddLog($"API Workflow error: {ex.Message}", Color.Red);
                UpdateUIProgress(0, $"Workflow failed: {ex.Message}", "API Workflow Error");
                return (false, null);
            }
        }
        private readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private async Task<bool> DownloadFileWithProgressAsync(string url, string localPath)
        {
            const int maxRetries = 3;
            AddLog($"Downloading Activation File...", Color.Blue);

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                    AddLog("File downloaded successfully", Color.Green);
                    return true;
                }
                catch (Exception ex)
                {
                    AddLog($"Download attempt {retryCount + 1} failed: {ex.Message}", Color.Orange);
                    // try { if (System.IO.File.Exists(localPath)) System.IO.File.Delete(localPath); } catch { }
                    if (retryCount >= maxRetries - 1) return false;
                    await Task.Delay(1000 * (retryCount + 1));
                }
            }
            AddLog("All download attempts failed", Color.Red);
            return false;
        }



        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Process.Start(TelegramGLink);
        }

        private async void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            await OTABlockSystem();
        }

        private async Task OTABlockSystem()
        {
            if (string.IsNullOrEmpty(currentProductType))
            {
                AddLog("OTA Block: No device connected", Color.Red);
                CustomMessageBox.Show("Please connect the device first.", "Device Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string serial = currentSerialNumber;
            AddLog($"Starting OTA Block for serial: {serial}", Color.Blue);
            checker check = new checker();
            if (check.checkA12(SNLBL.Text, ProdLBL.Text))
            {
                await OTABlock();
            }
            else
            {
                CustomMessageBox.Show($"Your {NameLBL.Text} with Serial: {SNLBL.Text} is not AUTHORIZED! " +
                    $"\r\n Please Register first!", "UNAUTHORIZED Device", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }



        }

        private void UpdateProgressUI(int value)
        {
            progressBar1.Value = value;
        }
        private int totalProgress = 0;
        private async Task ProgressTask(int targetValue)
        {
            AddLog($"Progress task started: {targetValue}%", Color.Gray);
            int finalTarget = Math.Min(targetValue, 100);
            if (totalProgress >= finalTarget) return;

            while (totalProgress < finalTarget)
            {
                totalProgress++;
                if (progressBar1.InvokeRequired)
                    progressBar1.Invoke(new Action(() => UpdateProgressUI(totalProgress)));
                else
                    UpdateProgressUI(totalProgress);
                await Task.Delay(15);
            }
            AddLog($"Progress task completed: {targetValue}%", Color.Gray);
        }


        private static async Task<string> ShellCMDAsync(string command, string argument)
        {
            using (Process process = new Process())
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = argument,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.StartInfo = processStartInfo;
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                string result = string.IsNullOrEmpty(error) ? output : error;
                return result;
            }
        }

        private async Task<bool> OTABlock()
        {
            try
            {
                AddLog("=== Starting OTA Blocking Process ===", Color.DarkBlue);
                //  InsertLabelText("Starting OTA blocking process...", Color.Black);
                await ProgressTask(10);

                string udid = currentUdid;
                string productType = currentProductType;
                string productVersion = currentProductVersion;

                AddLog($"Device: {productType} iOS {productVersion} UDID: {udid}", Color.DarkBlue);
                //  InsertLabelText($"Device detected: {productType} - iOS {productVersion}", Color.Black);
                await ProgressTask(30);

                // Verify OTA directory exists
                string RutaOTA = Path.Combine(ToolDir, "OTA", "swp");
                if (!Directory.Exists(RutaOTA))
                {
                    AddLog($"OTA directory not found: {RutaOTA}", Color.Red);
                    //   InsertLabelText("Error: OTA blocking directory not found.", Color.Red);
                    return false;
                }

                AddLog($"OTA directory verified: {RutaOTA}", Color.Green);
                await ProgressTask(60);
                //  InsertLabelText("Applying OTA blocking configuration...", Color.Black);

                // Execute blocking command
                string idevicebackup2Path = Path.Combine(Libs, "idevicebackup2.exe");
                string ideviceCommand = $"-u {udid} --source ad09186179f31a88dd6ee2c8f2d034025f54c82a restore --system --skip-apps \"{RutaOTA}\"";

                AddLog($"Executing OTA block command...", Color.Blue);
                string output = await ShellCMDAsync(idevicebackup2Path, ideviceCommand);

                if (output.Contains("error") || output.Contains("failed"))
                {
                    AddLog($"OTA block command failed: {output}", Color.Red);
                    //   InsertLabelText("Error occurred during OTA blocking.", Color.Red);
                    return false;
                }

                await ProgressTask(100);
                AddLog("OTA blocking completed successfully!", Color.Green);
                //  InsertLabelText("OTA blocking process completed successfully!", Color.Green);

                CustomMessageBox.Show("OTA / Reset blocking process completed successfully!", "OTA Block Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"OTA Block error: {ex.Message}", Color.Red);
                //  InsertLabelText($"Error: {ex.Message}", Color.Red);
                CustomMessageBox.Show($"An error occurred: {ex.Message}", "OTA Block Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {

            Environment.Exit(0);
        }

        private void AutoOTA_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoOTA.Checked)
            {
                SkipSX.Checked = false;
            }
           
        }

        private void SkipSX_CheckedChanged(object sender, EventArgs e)
        {
            if (SkipSX.Checked)
            {
                AutoOTA.Checked = false;
            }
            

        }
    }
}
