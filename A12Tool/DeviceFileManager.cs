using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iSkorpionA12;

namespace iSkorpionA12
{
    public class DeviceFileManager
    {
        #region Fields

        private readonly string pythonTargetPath;
        private readonly Action<string> statusUpdateCallback;
        private readonly Action<string> progressUpdateCallback;
        private readonly Action<int> progressBarUpdateCallback;
        private string deviceUdid;

        #endregion

        #region Constructor

        public DeviceFileManager(string pythonPath, Action<string> statusCallback, Action<string> progressCallback, Action<int> progressBarCallback)
        {
            pythonTargetPath = pythonPath;
            statusUpdateCallback = statusCallback;
            progressUpdateCallback = progressCallback;
            progressBarUpdateCallback = progressBarCallback;
        }

        #endregion

        #region Public Methods

        public void SetDeviceUdid(string udid)
        {
            deviceUdid = udid;
            Console.WriteLine($"[DEVICE UDID] Set to: {udid}");
        }

        /// <summary>
        /// Main workflow for device file management with 2 full retry attempts
        /// </summary>
        public async Task<bool> PerformDeviceFileManagement(string serial, string udid, string sqliteFilePath)
        {
            const int MAX_FULL_ATTEMPTS = 2; // 2 intentos completos

            SetDeviceUdid(udid);


            for (int fullAttempt = 1; fullAttempt <= MAX_FULL_ATTEMPTS; fullAttempt++)
            {
                try
                {
                   

                    UpdateProgress($"Starting activation process (Attempt {fullAttempt}/{MAX_FULL_ATTEMPTS})...");
                    UpdateProgressBar(5);

                    // Execute full activation process
                    bool result = await PerformActivationProcess(serial, udid, sqliteFilePath);

                    if (result)
                    {
                   
                        return true;
                    }

             

                    if (fullAttempt < MAX_FULL_ATTEMPTS)
                    {
                        Console.WriteLine($"[RETRY] Will retry entire process in 5 seconds...");
                        UpdateProgress($"Attempt {fullAttempt} failed. Retrying entire process...");
                        await Task.Delay(5000);
                    }
                }
                catch (Exception ex)
                {
                

                    if (fullAttempt < MAX_FULL_ATTEMPTS)
                    {
                        Console.WriteLine($"[RETRY] Exception occurred. Will retry entire process in 5 seconds...");
                        UpdateProgress($"Error on attempt {fullAttempt}. Retrying...");
                        await Task.Delay(5000);
                    }
                    else
                    {
                        UpdateStatus("Error during file management");
                        UpdateProgress($"Error: {ex.Message}");
                        UpdateProgressBar(0);

                        MessageBox.Show(
                            $"An error occurred during activation:\n\n{ex.Message}",
                            "Activation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }

            // All attempts exhausted


            UpdateProgress("Activation failed after all attempts");
            UpdateProgressBar(0);
            UpdateStatus("Activation process failed");

          

            MessageBox.Show(
                $"Device activation failed after {MAX_FULL_ATTEMPTS} attempts.\n\n" +
                "Please try:\n" +
                "• Restart the device manually\n" +
                "• Ensure device is connected to WiFi\n" +
                "• Check device storage space\n" +
                "• Try the activation process again later\n\n" +
                "If the problem persists, contact support.",
                "Activation Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return false;
        }

        #endregion

        #region Private Methods - Main Activation Process

        /// <summary>
        /// Performs the complete activation process (all 12 steps)
        /// </summary>
        private async Task<bool> PerformActivationProcess(string serial, string udid, string sqliteFilePath)
        {

            UpdateProgress("Deleting existing files...");

            try
            {
                await ExecuteAfcCommand("rm", "/Downloads/downloads.28.sqlitedb");
                await ExecuteAfcCommand("rm", "/Downloads/downloads.28.sqlitedb-shm");
                await ExecuteAfcCommand("rm", "/Downloads/downloads.28.sqlitedb-wal");
                Console.WriteLine("[STEP 1] ✅ Files deleted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STEP 1] ⚠️ Delete warning: {ex.Message}");
            }

            UpdateProgressBar(20);

            UpdateProgress("Uploading database...");

            if (string.IsNullOrEmpty(sqliteFilePath) || !File.Exists(sqliteFilePath))
            {
                Console.WriteLine($"[STEP 2] ❌ File not found: {sqliteFilePath}");
                UpdateStatus("Error: File not found");
                return false;
            }

            FileInfo fileInfo = new FileInfo(sqliteFilePath);
            Console.WriteLine($"[STEP 2] File size: {fileInfo.Length / 1024.0:F2} KB");

            await ExecuteAfcCommand("put", $"\"{sqliteFilePath}\" \"/Downloads/downloads.28.sqlitedb\"");
            Console.WriteLine("[STEP 2] ✅ File uploaded successfully");
            UpdateProgressBar(30);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 3: Wait 6 seconds
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 3] Waiting 6 seconds...");
            UpdateProgress("Waiting for device to process...");
            await Task.Delay(6000);
            UpdateProgressBar(35);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 4: First reboot
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 4] Performing first reboot...");
            UpdateProgress("Rebooting device (1/3)...");
            await ExecuteDiagnosticsCommand("restart");
            UpdateProgressBar(40);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 5: Wait for reconnection (35 seconds)
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 5] Waiting for device reconnection...");
            await WaitForReconnection("Reconnecting after first reboot", 35);
            UpdateProgressBar(45);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 6: Wait 10 seconds
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 6] Waiting 10 seconds...");
            UpdateProgress("Waiting for system stabilization...");
            await Task.Delay(10000);
            UpdateProgressBar(50);
            bool meta = await CheckMetaForDuration(60);

            if (meta)
            {
                Console.WriteLine("[STEP 56] ✅ Metadata is valid!");
            }
            // ════════════════════════════════════════════════════════════════════════
            // STEP 7: Second reboot
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 7] Performing second reboot...");
            UpdateProgress("Rebooting device (2/3)...");
            await ExecuteDiagnosticsCommand("restart");
            UpdateProgressBar(55);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 8: Wait for reconnection (35 seconds)
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 8] Waiting for device reconnection...");
            await WaitForReconnection("Reconnecting after second reboot", 35);
            UpdateProgressBar(60);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 9: Search for epub (50 seconds)
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 9] Searching for activation file (50 seconds)...");
            UpdateProgress("Checking activation status...");

            bool found = await CheckAssetForDuration(50);

            if (found)
            {
                Console.WriteLine("[STEP 9] ✅ Activation file found!");
                return await HandleSuccess(serial);
            }

            Console.WriteLine("[STEP 9] ❌ Activation file not found after 50 seconds");
            UpdateProgressBar(70);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 10: Reboot if not found
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 10] Rebooting device for retry...");
            UpdateProgress("Rebooting for retry...");
            await ExecuteDiagnosticsCommand("restart");
            UpdateProgressBar(75);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 11: Wait for reconnection (35 seconds)
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("[STEP 11] Waiting for device reconnection...");
            await WaitForReconnection("Reconnecting for retry", 35);
            UpdateProgressBar(80);

            // ════════════════════════════════════════════════════════════════════════
            // STEP 12: Search for epub (30 seconds) - with 2 retries
            // ════════════════════════════════════════════════════════════════════════
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                Console.WriteLine($"[STEP 12] Search attempt {attempt}/2 (30 seconds)...");
                UpdateProgress($"Checking activation (attempt {attempt}/2)...");

                found = await CheckAssetForDuration(30);

                if (found)
                {
                    Console.WriteLine($"[STEP 12] ✅ Activation file found on attempt {attempt}!");
                    return await HandleSuccess(serial);
                }

                Console.WriteLine($"[STEP 12] ❌ Attempt {attempt}/2 failed");

                if (attempt < 2)
                {
                    Console.WriteLine("[STEP 12] Preparing for final retry...");
                    UpdateProgress("Preparing final retry...");
                    await Task.Delay(5000);
                    UpdateProgressBar(85);
                }
            }

            UpdateProgressBar(90);

            // ════════════════════════════════════════════════════════════════════════
            // FAILURE: Activation file not found after all steps
            // ════════════════════════════════════════════════════════════════════════
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("[ACTIVATION PROCESS] ❌ FAILED - Activation file not found");
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            return false;
        }

        #endregion

        #region Private Methods - Asset Detection

        /// <summary>
        /// Checks for asset3.epub or asset.epub for the specified duration
        /// </summary>
        private async Task<bool> CheckAssetForDuration(int durationSeconds)
        {
            int checks = durationSeconds / 2; // Check every 2 seconds
            bool assetFound = false;
            bool asset1Found = false;

            for (int i = 1; i <= checks; i++)
            {
                try
                {
                    int elapsed = i * 2;
                    UpdateProgress($"Checking activation files ({elapsed}s/{durationSeconds}s)...");
                    Console.WriteLine($"[CHECK ASSET] Check #{i}/{checks} ({elapsed}s)");

                    // First, list the directory to see what's there
                    string result = await GetResultafcCommand("ls", "/Books");

                    if (!result.StartsWith("[ERROR]") && !result.StartsWith("[EXCEPTION]"))
                    {
                        // Check for asset.epub if not already found
                        if (!assetFound && (result.Contains("asset.epub") || result.Contains("asset3.epub")))
                        {
                            string fileName = result.Contains("asset3.epub") ? "asset3.epub" : "asset.epub";
                            Console.WriteLine($"[CHECK ASSET] Found {fileName} in directory listing, checking if it's a valid file...");

                            string infoResult = await GetResultafcCommand("info", $"/Books/{fileName}");

                            if (!infoResult.StartsWith("[ERROR]") && !infoResult.StartsWith("[EXCEPTION]"))
                            {
                                if (infoResult.Contains("st_ifmt: S_IFREG"))
                                {
                                    long fileSize = ExtractFileSizeFromInfo(infoResult);
                                    if (fileSize > 0)
                                    {
                                        Console.WriteLine($"[CHECK ASSET] ✅ {fileName} is a valid file with size {fileSize} bytes at {elapsed}s!");
                                        assetFound = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[CHECK ASSET] ⚠️ {fileName} is a file but has zero size at {elapsed}s");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"[CHECK ASSET] ⚠️ {fileName} is not a regular file at {elapsed}s");
                                }
                            }
                        }

                        // Check for asset1.epub if not already found
                        if (!asset1Found && result.Contains("asset1.epub"))
                        {
                            Console.WriteLine($"[CHECK ASSET] Found asset1.epub in directory listing, checking if it's a valid file...");

                            string infoResult = await GetResultafcCommand("info", "/Books/asset1.epub");

                            if (!infoResult.StartsWith("[ERROR]") && !infoResult.StartsWith("[EXCEPTION]"))
                            {
                                if (infoResult.Contains("st_ifmt: S_IFREG"))
                                {
                                    long fileSize = ExtractFileSizeFromInfo(infoResult);
                                    if (fileSize > 0)
                                    {
                                        Console.WriteLine($"[CHECK ASSET] ✅ asset1.epub is a valid file with size {fileSize} bytes at {elapsed}s!");
                                        asset1Found = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[CHECK ASSET] ⚠️ asset1.epub is a file but has zero size at {elapsed}s");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"[CHECK ASSET] ⚠️ asset1.epub is not a regular file at {elapsed}s");
                                }
                            }
                        }

                        // If both files are found, we're done
                        if (assetFound && asset1Found)
                        {
                            Console.WriteLine($"[CHECK ASSET] ✅ Both asset.epub and asset1.epub detected at {elapsed}s!");
                            UpdateProgress("activation files detected ✅");
                            return true;
                        }

                        // Report current status
                        string status = "";
                        if (assetFound) status += "asset.epub ✓ ";
                        if (asset1Found) status += "asset1.epub ✓ ";

                        if (!string.IsNullOrEmpty(status))
                        {
                            Console.WriteLine($"[CHECK ASSET] Check #{i} - {status.Trim()}");
                        }
                        else
                        {
                            Console.WriteLine($"[CHECK ASSET] Check #{i} - No asset files found yet");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[CHECK ASSET] Check #{i} - Directory listing failed: {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHECK ASSET] ⚠️ Check #{i} exception: {ex.Message}");
                }

                if (i < checks)
                {
                    await Task.Delay(2000); // Wait 2 seconds before next check
                }
            }

            // Final status
            if (assetFound && asset1Found)
            {
                Console.WriteLine($"[CHECK ASSET] ✅ Both files found after {durationSeconds}s!");
                UpdateProgress("activation files detected ✅");
                return true;
            }
            else if (assetFound || asset1Found)
            {
                string foundFiles = "";
                if (assetFound) foundFiles += "asset.epub";
                if (asset1Found) foundFiles += (foundFiles != "" ? ", " : "") + "asset1.epub";
                Console.WriteLine($"[CHECK ASSET] ❌ Only found {foundFiles} (both required) after {durationSeconds}s");
            }
            else
            {
                Console.WriteLine($"[CHECK ASSET] ❌ Neither asset.epub nor asset1.epub found after {durationSeconds}s");
            }

            return false;
        }

        private async Task<bool> CheckMetaForDuration(int durationSeconds)
        {
            int checks = durationSeconds / 2; // Check every 2 seconds
            bool metadataFound = false;
            bool metadata1Found = false;

            for (int i = 1; i <= checks; i++)
            {
                try
                {
                    int elapsed = i * 2;
                    UpdateProgress($"Checking Metadata ({elapsed}s/{durationSeconds}s)...");
                    Console.WriteLine($"[CHECK METADATA] Check #{i}/{checks} ({elapsed}s)");

                    // First, list the directory to see what's there
                    string result = await GetResultafcCommand("ls", "/iTunes_Control/iTunes");

                    if (!result.StartsWith("[ERROR]") && !result.StartsWith("[EXCEPTION]"))
                    {
                        // Check for iTunesMetadata.plist if not already found
                        if (!metadataFound && result.Contains("iTunesMetadata.plist"))
                        {
                            string fileName = "iTunesMetadata.plist";
                            Console.WriteLine($"[CHECK METADATA] Found {fileName} in directory listing, checking if it's a valid file...");

                            string infoResult = await GetResultafcCommand("info", $"/iTunes_Control/iTunes/{fileName}");

                            if (!infoResult.StartsWith("[ERROR]") && !infoResult.StartsWith("[EXCEPTION]"))
                            {
                                if (infoResult.Contains("st_ifmt: S_IFREG"))
                                {
                                    long fileSize = ExtractFileSizeFromInfo(infoResult);

                                    if (fileSize > 0)
                                    {
                                        Console.WriteLine($"[CHECK METADATA] ✅ {fileName} is a valid file with size {fileSize} bytes at {elapsed}s!");
                                        metadataFound = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[CHECK METADATA] ⚠️ {fileName} is a file but has zero size ({fileSize} bytes) at {elapsed}s");
                                    }
                                }
                                else if (infoResult.Contains("st_ifmt: S_IFDIR"))
                                {
                                    Console.WriteLine($"[CHECK METADATA] ⚠️ {fileName} is a directory (not file yet) at {elapsed}s");
                                }
                                else
                                {
                                    Console.WriteLine($"[CHECK METADATA] ⚠️ {fileName} has unknown file type: {infoResult}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[CHECK METADATA] ⚠️ Could not get info for {fileName}: {infoResult}");
                            }
                        }

                        // Check for iTunesMetadata1.plist if not already found
                        if (!metadata1Found && result.Contains("iTunesMetadata1.plist"))
                        {
                            string fileName = "iTunesMetadata1.plist";
                            Console.WriteLine($"[CHECK METADATA] Found {fileName} in directory listing, checking if it's a valid file...");

                            string infoResult = await GetResultafcCommand("info", $"/iTunes_Control/iTunes/{fileName}");

                            if (!infoResult.StartsWith("[ERROR]") && !infoResult.StartsWith("[EXCEPTION]"))
                            {
                                if (infoResult.Contains("st_ifmt: S_IFREG"))
                                {
                                    long fileSize = ExtractFileSizeFromInfo(infoResult);

                                    if (fileSize > 0)
                                    {
                                        Console.WriteLine($"[CHECK METADATA] ✅ {fileName} is a valid file with size {fileSize} bytes at {elapsed}s!");
                                        metadata1Found = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[CHECK METADATA] ⚠️ {fileName} is a file but has zero size ({fileSize} bytes) at {elapsed}s");
                                    }
                                }
                                else if (infoResult.Contains("st_ifmt: S_IFDIR"))
                                {
                                    Console.WriteLine($"[CHECK METADATA] ⚠️ {fileName} is a directory (not file yet) at {elapsed}s");
                                }
                                else
                                {
                                    Console.WriteLine($"[CHECK METADATA] ⚠️ {fileName} has unknown file type: {infoResult}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[CHECK METADATA] ⚠️ Could not get info for {fileName}: {infoResult}");
                            }
                        }

                        // If both files are found, we're done
                        if (metadataFound && metadata1Found)
                        {
                            Console.WriteLine($"[CHECK METADATA] ✅ Both iTunesMetadata.plist and iTunesMetadata1.plist detected at {elapsed}s!");
                            UpdateProgress("metadata files detected ✅");
                            return true;
                        }

                        // Report current status
                        string status = "";
                        if (metadataFound) status += "iTunesMetadata.plist ✓ ";
                        if (metadata1Found) status += "iTunesMetadata1.plist ✓ ";

                        if (!string.IsNullOrEmpty(status))
                        {
                            Console.WriteLine($"[CHECK METADATA] Check #{i} - {status.Trim()}");
                        }
                        else
                        {
                            Console.WriteLine($"[CHECK METADATA] Check #{i} - No metadata files found yet");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[CHECK METADATA] Check #{i} - Directory listing failed: {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CHECK METADATA] ⚠️ Check #{i} exception: {ex.Message}");
                }

                if (i < checks)
                {
                    await Task.Delay(2000); // Wait 2 seconds before next check
                }
            }

            // Final status
            if (metadataFound && metadata1Found)
            {
                Console.WriteLine($"[CHECK METADATA] ✅ Both files found after {durationSeconds}s!");
                UpdateProgress("metadata files detected ✅");
                return true;
            }
            else if (metadataFound || metadata1Found)
            {
                string foundFiles = "";
                if (metadataFound) foundFiles += "iTunesMetadata.plist";
                if (metadata1Found) foundFiles += (foundFiles != "" ? ", " : "") + "iTunesMetadata1.plist";
                Console.WriteLine($"[CHECK METADATA] ❌ Only found {foundFiles} (both required) after {durationSeconds}s");
            }
            else
            {
                Console.WriteLine($"[CHECK METADATA] ❌ Neither iTunesMetadata.plist nor iTunesMetadata1.plist found after {durationSeconds}s");
            }

            return false;
        }

        /// <summary>
        /// Extracts file size from AFC info command output
        /// </summary>
        /// <param name="infoResult">The output from 'info' command</param>
        /// <returns>File size in bytes, or 0 if not found</returns>
        private long ExtractFileSizeFromInfo(string infoResult)
        {
            try
            {
                // Look for the st_size line in the info output
                // Example: "st_size: 4096"
                using (StringReader reader = new StringReader(infoResult))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Trim().StartsWith("st_size:"))
                        {
                            string[] parts = line.Split(':');
                            if (parts.Length >= 2)
                            {
                                string sizeStr = parts[1].Trim();
                                if (long.TryParse(sizeStr, out long fileSize))
                                {
                                    return fileSize;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXTRACT SIZE] Error extracting file size: {ex.Message}");
            }

            return 0;
        }


        #endregion

        #region Private Methods - Success Handling

        /// <summary>
        /// Handles success scenario - final reboot, cleanup, and notification
        /// </summary>
        /// 



        private Task WaitForExitAsync(Process process)
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);

            if (process.HasExited)
            {
                tcs.TrySetResult(null);
            }

            return tcs.Task;
        }

        private async Task<string> RunProcessWithTimeout(string fileName, string arguments, int timeoutMs)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, "x64", fileName),
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;

                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            outputBuilder.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            errorBuilder.AppendLine(e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Use Task.WhenAny to implement timeout
                    var processTask = WaitForExitAsync(process);
                    var timeoutTask = Task.Delay(timeoutMs);

                    var completedTask = await Task.WhenAny(processTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        // Process timed out
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                await Task.Delay(100); // Give it a moment to terminate
                            }
                        }
                        catch (Exception killEx)
                        {
                            Debug.WriteLine($"⚠️ Error killing timed out process: {killEx.Message}");
                        }

                        throw new TimeoutException($"Process {fileName} timed out after {timeoutMs}ms");
                    }

                    var errorOutput = errorBuilder.ToString();
                    if (!string.IsNullOrEmpty(errorOutput) && !errorOutput.Contains("No device found"))
                    {
                        Debug.WriteLine($"Process error output: {errorOutput}");
                    }

                    return outputBuilder.ToString();
                }
            }
            catch (TimeoutException)
            {
                throw; // Re-throw timeout exceptions
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Process error: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }


        private async Task<bool> IsDirectoryAfcWithTimeout(string path, int timeoutMs)
        {
            try
            {
                // Use 'ls' on the parent directory to get proper file info
                var parentPath = Path.GetDirectoryName(path)?.Replace("\\", "/") ?? "/";
                var fileName = Path.GetFileName(path);

                Debug.WriteLine($"🔍 Checking if directory: {path} (parent: {parentPath}, file: {fileName})");

                // List parent directory contents
                var listResult = await RunProcessWithTimeout("afcclient.exe", $"ls \"{parentPath}\"", timeoutMs);

                if (string.IsNullOrEmpty(listResult) || listResult.Contains("ERROR"))
                {
                    Debug.WriteLine($"❌ Cannot list parent directory: {parentPath}");
                    return false;
                }

                // Parse the listing to find our item
                var items = listResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    var itemName = item.Trim();
                    if (string.IsNullOrEmpty(itemName) || itemName == "." || itemName == "..")
                        continue;

                    if (itemName == fileName)
                    {
                        // Try to get info about the item to determine if it's a directory
                        var infoResult = await RunProcessWithTimeout("afcclient.exe", $"info \"{path}\"", timeoutMs);

                        if (!string.IsNullOrEmpty(infoResult) && !infoResult.Contains("ERROR"))
                        {
                            // Check for directory indicators in info output
                            bool isDirectory = infoResult.Contains("st_ifmt: S_IFDIR") ||
                                              infoResult.ToLower().Contains("directory") ||
                                              infoResult.Contains("st_mode: 0040755") || // Directory permission
                                              infoResult.Contains("st_mode: 0040777");   // Directory permission

                            Debug.WriteLine($"📊 Item {path} is directory: {isDirectory}");
                            return isDirectory;
                        }

                        // Fallback: try to list the item itself
                        var listItemResult = await RunProcessWithTimeout("afcclient.exe", $"ls \"{path}\"", timeoutMs);
                        bool canList = !string.IsNullOrEmpty(listItemResult) &&
                                      !listItemResult.Contains("ERROR") &&
                                      !listItemResult.Contains("not a directory") &&
                                      !listItemResult.Contains("is not a valid directory");

                        Debug.WriteLine($"📊 Item {path} can be listed (directory): {canList}");
                        return canList;
                    }
                }

                Debug.WriteLine($"❌ Item not found in directory listing: {path}");
                return false;
            }
            catch (TimeoutException)
            {
                Debug.WriteLine($"⏰ Timeout checking if directory: {path}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error checking if directory {path}: {ex.Message}");
                return false;
            }
        }


        private async Task<bool> RecursivelyDeleteFolderContents(string folderPath, int currentDepth = 0)
        {
            const int MAX_RECURSION_DEPTH = 50;

            if (currentDepth > MAX_RECURSION_DEPTH)
            {
                Debug.WriteLine($"🚨 Maximum recursion depth reached for: {folderPath}");
                return false;
            }

            try
            {
                Debug.WriteLine($"🔍 Recursively deleting contents of: {folderPath} (depth: {currentDepth})");

                // Check if folder exists first with timeout
                var listResult = await RunProcessWithTimeout("afcclient.exe", $"ls \"{folderPath}\"", 10000);
                if (string.IsNullOrEmpty(listResult) ||
                    listResult.Contains("ERROR: Could not connect to lockdownd") ||
                    listResult.Contains("No device found") ||
                    listResult.Contains("ERROR") ||
                    listResult.Contains("No such file or directory"))
                {
                    Debug.WriteLine($"ℹ️ Folder doesn't exist or cannot access: {folderPath}");
                    return true; // Not an error if folder doesn't exist
                }

                // Check if listing is actually empty
                var items = listResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Where(item => !string.IsNullOrEmpty(item.Trim()) &&
                                                  item.Trim() != "." &&
                                                  item.Trim() != "..")
                                     .ToArray();

                if (items.Length == 0)
                {
                    Debug.WriteLine($"ℹ️ Folder is empty: {folderPath}");
                    return true;
                }

                var success = true;
                var processedItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var item in items)
                {
                    var itemName = item.Trim();
                    if (string.IsNullOrEmpty(itemName) || itemName == "." || itemName == "..")
                        continue;

                    // Prevent processing the same item multiple times
                    if (processedItems.Contains(itemName))
                    {
                        Debug.WriteLine($"⚠️ Skipping duplicate item: {itemName}");
                        continue;
                    }

                    processedItems.Add(itemName);

                    var fullPath = $"{folderPath}/{itemName}";
                    var displayPath = fullPath.TrimStart('/');

                    try
                    {
                        // Check if it's a directory with timeout
                        bool isDirectory = await IsDirectoryAfcWithTimeout(fullPath, 5000);

                        if (isDirectory)
                        {
                            // Special handling for iTunes_Control/iTunes - keep this folder but delete its contents
                            if (fullPath == "/iTunes_Control/iTunes")
                            {
                                Debug.WriteLine($"📁 Keeping iTunes folder, deleting its contents: {displayPath}");
                                var recursiveSuccessx = await RecursivelyDeleteFolderContents(fullPath, currentDepth + 1);
                                if (!recursiveSuccessx)
                                {
                                    Debug.WriteLine($"⚠️ Failed to delete iTunes contents: {displayPath}");
                                    success = false;
                                }
                                continue; // Skip deleting the iTunes folder itself
                            }

                            Debug.WriteLine($"📁 Processing directory: {displayPath}");

                            // First recursively delete contents of this directory
                            var recursiveSuccess = await RecursivelyDeleteFolderContents(fullPath, currentDepth + 1);
                            if (!recursiveSuccess)
                            {
                                Debug.WriteLine($"⚠️ Failed to delete contents of: {displayPath}");
                                success = false;
                                continue;
                            }

                            // Now delete the empty subdirectory itself (but NOT the main folders)
                            // Don't delete the main target folders: /Downloads, /Books, /iTunes_Control
                            if (folderPath != "/" &&
                                fullPath != "/Downloads" &&
                                fullPath != "/Books" &&
                                fullPath != "/iTunes_Control")
                            {
                                var deleteDirResult = await RunProcessWithTimeout("afcclient.exe", $"rm \"{fullPath}\"", 5000);
                                if (deleteDirResult?.Contains("ERROR") == true)
                                {
                                    Debug.WriteLine($"⚠️ Failed to delete subdirectory: {displayPath} - {deleteDirResult}");
                                    success = false;
                                }
                                else
                                {
                                    Debug.WriteLine($"🗑️ Deleted subdirectory: {displayPath}");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"⏭️ Keeping main folder: {displayPath}");
                            }
                        }
                        else
                        {
                            // It's a file, delete it directly with timeout
                            Debug.WriteLine($"📄 Deleting file: {displayPath}");
                            var deleteResult = await RunProcessWithTimeout("afcclient.exe", $"rm \"{fullPath}\"", 5000);
                            if (deleteResult?.Contains("ERROR") == true)
                            {
                                Debug.WriteLine($"⚠️ Failed to delete file: {displayPath} - {deleteResult}");
                                success = false;
                            }
                            else
                            {
                                Debug.WriteLine($"🗑️ Deleted file: {displayPath}");
                            }
                        }
                    }
                    catch (Exception itemEx)
                    {
                        Debug.WriteLine($"❌ Error processing item {displayPath}: {itemEx.Message}");
                        success = false;
                        // Continue with other items instead of getting stuck
                    }
                }

                Debug.WriteLine($"✅ Emptied folder: {folderPath}");
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error recursively deleting folder contents {folderPath}: {ex}");
                return false;
            }
        }


        private async Task<bool> ExecuteAfcCommandDir(string command, string folderPath)
        {
            const int MAX_RECURSION_DEPTH = 50;
            int currentDepth = 0;

            if (currentDepth > MAX_RECURSION_DEPTH)
            {
                Debug.WriteLine($"🚨 Maximum recursion depth reached for: {folderPath}");
                return false;
            }

            try
            {
                Debug.WriteLine($"🔍 Recursively deleting contents of: {folderPath} (depth: {currentDepth})");

                // Check if folder exists first with timeout
                var listResult = await RunProcessWithTimeout("afcclient.exe", $"{command} \"{folderPath}\"", 10000);
                if (string.IsNullOrEmpty(listResult) ||
                    listResult.Contains("ERROR: Could not connect to lockdownd") ||
                    listResult.Contains("No device found") ||
                    listResult.Contains("ERROR") ||
                    listResult.Contains("No such file or directory"))
                {
                    Debug.WriteLine($"ℹ️ Folder doesn't exist or cannot access: {folderPath}");
                    return true; // Not an error if folder doesn't exist
                }

                // Check if listing is actually empty
                var items = listResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Where(item => !string.IsNullOrEmpty(item.Trim()) &&
                                                  item.Trim() != "." &&
                                                  item.Trim() != "..")
                                     .ToArray();

                if (items.Length == 0)
                {
                    Debug.WriteLine($"ℹ️ Folder is empty: {folderPath}");
                    return true;
                }

                var success = true;
                var processedItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var item in items)
                {
                    var itemName = item.Trim();
                    if (string.IsNullOrEmpty(itemName) || itemName == "." || itemName == "..")
                        continue;

                    // Prevent processing the same item multiple times
                    if (processedItems.Contains(itemName))
                    {
                        Debug.WriteLine($"⚠️ Skipping duplicate item: {itemName}");
                        continue;
                    }

                    processedItems.Add(itemName);

                    var fullPath = $"{folderPath}/{itemName}";
                    var displayPath = fullPath.TrimStart('/');

                    try
                    {
                        // Check if it's a directory with timeout
                        bool isDirectory = await IsDirectoryAfcWithTimeout(fullPath, 5000);

                        if (isDirectory)
                        {
                            // Special handling for iTunes_Control/iTunes - keep this folder but delete its contents
                            if (fullPath == "/iTunes_Control/iTunes")
                            {
                                Debug.WriteLine($"📁 Keeping iTunes folder, deleting its contents: {displayPath}");
                                var recursiveSuccessx = await RecursivelyDeleteFolderContents(fullPath, currentDepth + 1);
                                if (!recursiveSuccessx)
                                {
                                    Debug.WriteLine($"⚠️ Failed to delete iTunes contents: {displayPath}");
                                    success = false;
                                }
                                continue; // Skip deleting the iTunes folder itself
                            }

                            Debug.WriteLine($"📁 Processing directory: {displayPath}");

                            // First recursively delete contents of this directory
                            var recursiveSuccess = await RecursivelyDeleteFolderContents(fullPath, currentDepth + 1);
                            if (!recursiveSuccess)
                            {
                                Debug.WriteLine($"⚠️ Failed to delete contents of: {displayPath}");
                                success = false;
                                continue;
                            }

                            // Now delete the empty subdirectory itself (but NOT the main folders)
                            // Don't delete the main target folders: /Downloads, /Books, /iTunes_Control
                            if (folderPath != "/" &&
                                fullPath != "/Downloads" &&
                                fullPath != "/Books" &&
                                fullPath != "/iTunes_Control")
                            {
                                var deleteDirResult = await RunProcessWithTimeout("afcclient.exe", $"rm \"{fullPath}\"", 5000);
                                if (deleteDirResult?.Contains("ERROR") == true)
                                {
                                    Debug.WriteLine($"⚠️ Failed to delete subdirectory: {displayPath} - {deleteDirResult}");
                                    success = false;
                                }
                                else
                                {
                                    Debug.WriteLine($"🗑️ Deleted subdirectory: {displayPath}");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"⏭️ Keeping main folder: {displayPath}");
                            }
                        }
                        else
                        {
                            // It's a file, delete it directly with timeout
                            Debug.WriteLine($"📄 Deleting file: {displayPath}");
                            var deleteResult = await RunProcessWithTimeout("afcclient.exe", $"rm \"{fullPath}\"", 5000);
                            if (deleteResult?.Contains("ERROR") == true)
                            {
                                Debug.WriteLine($"⚠️ Failed to delete file: {displayPath} - {deleteResult}");
                                success = false;
                            }
                            else
                            {
                                Debug.WriteLine($"🗑️ Deleted file: {displayPath}");
                            }
                        }
                    }
                    catch (Exception itemEx)
                    {
                        Debug.WriteLine($"❌ Error processing item {displayPath}: {itemEx.Message}");
                        success = false;
                        // Continue with other items instead of getting stuck
                    }
                }

                Debug.WriteLine($"✅ Emptied folder: {folderPath}");
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error recursively deleting folder contents {folderPath}: {ex}");
                return false;
            }
        }

        private async Task<bool> HandleSuccess(string serial)
        {
            try
            {
              

                UpdateProgress("Activation files are being processed! Finalizing...");
                UpdateProgressBar(93);

                // Final reboot
                Console.WriteLine("[SUCCESS] Performing final reboot...");
                await Task.Delay(10000);
                await ExecuteDiagnosticsCommand("restart");
                
                UpdateProgressBar(95);
                
                // Wait for reconnection
                Console.WriteLine("[SUCCESS] Waiting for final reconnection...");
                await WaitForReconnection("Final reconnection", 35);
                UpdateProgressBar(97);

                // Cleanup downloads folder
                Console.WriteLine("[SUCCESS] Cleaning up temporary files...");
                UpdateProgress("Cleaning up...");
                try
                {
                   

                    await ExecuteAfcCommandDir("rm", "/Downloads/downloads.28.sqlitedb");
                    await ExecuteAfcCommandDir("rm", "/Books/asset3.epub");
                    Console.WriteLine("[SUCCESS] ✅ Temporary files cleared");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SUCCESS] ⚠️ Cleanup warning: {ex.Message}");
                }
                UpdateProgressBar(98);

                // Send Telegram notification
               
                UpdateProgressBar(100);

                // Update UI
               /* UpdateProgress("Device activated successfully!");
                UpdateStatus("Activation complete!");*/

                await Task.Delay(4000);  // 👈 AQUÍ ESTÁ LA ESPERA

    

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SUCCESS] ⚠️ Error in finalization: {ex.Message}");
                Console.WriteLine($"[SUCCESS] Stack trace: {ex.StackTrace}");
                // Even if there's an error here, we consider it a success since asset file was found
                Console.WriteLine("[SUCCESS] Treating as success despite finalization error");
                return true;
            }
        }

        #endregion

        #region Private Methods - AFC Commands

        private async Task<string> GetResultafcCommand(string command, string path)
        {
            try
            {
                string pythonExe = Path.Combine(Environment.CurrentDirectory,"x64","afcclient.exe");
                string arguments = $"{command} --udid {deviceUdid} {path}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (process.ExitCode != 0)
                        {
                            return $"[ERROR] {error}";
                        }
                        return output.Trim();
                    }
                }
                return "[AFC COMMAND] Process failed";
            }
            catch (Exception ex)
            {
                return $"[EXCEPTION] {ex.Message}";
            }
        }

        private async Task ExecuteAfcCommand(string command, string path)
        {
            const int maxRetries = 3;
            const int retryDelayMs = 5000;
            int attempt = 0;

            while (attempt <= maxRetries)
            {
                try
                {
                    if (string.IsNullOrEmpty(deviceUdid))
                    {
                        throw new InvalidOperationException("Device UDID not set");
                    }

                    string pythonExe = Path.Combine(Environment.CurrentDirectory, "x64", "afcclient.exe");
                    string arguments = $"{command} --udid {deviceUdid} {path}";

                    var processInfo = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    using (var process = Process.Start(processInfo))
                    {
                        if (process != null)
                        {
                            process.WaitForExit();

                            if (process.ExitCode == 0)
                            {
                                Console.WriteLine($"[AFC] ✅ {command} {path}");
                                return; // Success, exit the method
                            }
                            else
                            {
                                string error = process.StandardError.ReadToEnd();
                                Console.WriteLine($"[AFC] ⚠️ {command} {path} - {error}");

                                // Only retry on non-zero exit code if we have attempts left
                                if (attempt < maxRetries)
                                {
                                    attempt++;
                                    Console.WriteLine($"[AFC] 🔄 Retry {attempt}/{maxRetries} in {retryDelayMs / 1000} seconds...");
                                    await Task.Delay(retryDelayMs);
                                    continue;
                                }
                                else
                                {
                                    // Final attempt failed, throw exception
                                    throw new InvalidOperationException($"AFC command failed after {maxRetries} retries: {error}");
                                }
                            }

                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    attempt++;

                    if (attempt > maxRetries)
                    {
                        Console.WriteLine($"[AFC] ❌ Exception after {maxRetries} retries: {ex.Message}");
                        throw;
                    }
                    else
                    {
                        Console.WriteLine($"[AFC] 🔄 Retry {attempt}/{maxRetries} after exception in {retryDelayMs / 1000} seconds: {ex.Message}");
                        await Task.Delay(retryDelayMs);
                    }
                }
            }
        }

        #endregion

        #region Private Methods - Diagnostics

        private async Task ExecuteDiagnosticsCommand(string command)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceUdid))
                {
                    throw new InvalidOperationException("Device UDID not set");
                }

                string pythonExe = Path.Combine(Environment.CurrentDirectory, "x64", "idevicediagnostics.exe");
                string arguments = $"{command} --udid {deviceUdid}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        Console.WriteLine($"[DIAGNOSTICS] ✅ {command} executed");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DIAGNOSTICS] ❌ Exception in {command}: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Private Methods - Device Connection

        public async Task WaitForReconnection(string message, int seconds)
        {
            Console.WriteLine($"[RECONNECT] {message}");
            Console.WriteLine($"[RECONNECT] Waiting {seconds} seconds...");

            // Countdown
            for (int i = seconds; i > 0; i--)
            {
                UpdateProgress($"{message} ({i}s remaining)...");
                await Task.Delay(1000);
            }

            // Verify connection with retries
            Console.WriteLine($"[RECONNECT] Verifying device connection...");
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                if (await IsDeviceConnected())
                {
                    Console.WriteLine($"[RECONNECT] ✅ Device connected and verified");
                    UpdateProgress("Device connected!");
                    await Task.Delay(1000);
                    return;
                }
                Console.WriteLine($"[RECONNECT] Verification attempt {attempt}/5...");
                await Task.Delay(2000);
            }

            Console.WriteLine($"[RECONNECT] ⚠️ Could not verify connection (continuing anyway)");
            UpdateProgress("Connection verification timed out (continuing)...");
        }

        private async Task<bool> IsDeviceConnected()
        {
            try
            {
                if (string.IsNullOrEmpty(deviceUdid))
                {
                    return false;
                }

                string pythonExe = Path.Combine(Environment.CurrentDirectory, "x64", "afcclient.exe");
                string arguments = $"ls --udid {deviceUdid} /";

                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        bool finished = process.WaitForExit(10000); // 10 second timeout

                        if (!finished)
                        {
                            try { process.Kill(); } catch { }
                            return false;
                        }

                        return process.ExitCode == 0;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

 
        #endregion
         
        #region Private Methods - UI Updates

        private void UpdateStatus(string message)
        {
            try
            {
                statusUpdateCallback?.Invoke(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UI UPDATE] Status error: {ex.Message}");
            }
        }

        private void UpdateProgress(string message)
        {
            try
            {
                progressUpdateCallback?.Invoke(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UI UPDATE] Progress error: {ex.Message}");
            }
        }

        private void UpdateProgressBar(int value)
        {
            try
            {
                progressBarUpdateCallback?.Invoke(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UI UPDATE] Progress bar error: {ex.Message}");
            }
        }

        #endregion
    }
}