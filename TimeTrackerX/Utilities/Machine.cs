using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerX.Models;

namespace TimeTrackerX.Utilities
{
    public class Machine
    {
        string _machineId = string.Empty;

        public string CreateMachineId()
        {
            _machineId = string.Empty;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _machineId = GenerateWindowsMachineId();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _machineId = GenerateMacOSMachineId();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _machineId = GenerateLinuxMachineId();
                }
                else
                {
                    // Fallback for unsupported platforms
                    _machineId = GenerateFallbackMachineId();
                }

                // Ensure the ID is not empty
                if (string.IsNullOrEmpty(_machineId))
                {
                    _machineId = GenerateFallbackMachineId();
                }

                // Store in GlobalSetting for consistency
                GlobalSetting.Instance.MachineId = _machineId;
            }
            catch (Exception ex)
            {
                _machineId = GenerateFallbackMachineId();
                GlobalSetting.Instance.MachineId = _machineId;
            }

            return _machineId;
        }

        private string GenerateWindowsMachineId()
        {
            try
            {
                string machineId = string.Empty;
                var win32Processor = new System.Management.ManagementObjectSearcher(
                    "SELECT * FROM Win32_Processor"
                );
                var win32BaseBoard = new System.Management.ManagementObjectSearcher(
                    "SELECT * FROM Win32_BaseBoard"
                );

                foreach (var processor in win32Processor.Get())
                {
                    string processorId = Convert.ToString(processor["ProcessorId"]);
                    if (!string.IsNullOrEmpty(processorId))
                    {
                        machineId = processorId;
                        break;
                    }
                }

                foreach (var baseboard in win32BaseBoard.Get())
                {
                    string serialNumber = Convert.ToString(
                        baseboard.GetPropertyValue("SerialNumber")
                    );
                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        machineId += serialNumber;
                        break;
                    }
                }

                return machineId;
            }
            catch (Exception ex)
            {
                return GenerateFallbackMachineId();
            }
        }

        private string GenerateMacOSMachineId()
        {
            try
            {
                // Use system_profiler to get the hardware UUID
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/usr/sbin/system_profiler",
                        Arguments = "SPHardwareDataType",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Parse the hardware UUID
                string hardwareUuid = string.Empty;
                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains("Hardware UUID"))
                    {
                        hardwareUuid = line.Split(':').Last().Trim();
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(hardwareUuid))
                {
                    return HashString(hardwareUuid); // Hash for consistency with Windows ID length
                }

                return GenerateFallbackMachineId();
            }
            catch (Exception ex)
            {
                return GenerateFallbackMachineId();
            }
        }

        private string GenerateLinuxMachineId()
        {
            try
            {
                // Try reading /etc/machine-id (systemd-based systems)
                string machineIdPath = "/etc/machine-id";
                if (File.Exists(machineIdPath))
                {
                    string machineId = File.ReadAllText(machineIdPath).Trim();
                    if (!string.IsNullOrEmpty(machineId))
                    {
                        return HashString(machineId);
                    }
                }

                // Fallback to /sys/class/dmi/id/product_uuid (requires root or appropriate permissions)
                string productUuidPath = "/sys/class/dmi/id/product_uuid";
                if (File.Exists(productUuidPath))
                {
                    string productUuid = File.ReadAllText(productUuidPath).Trim();
                    if (!string.IsNullOrEmpty(productUuid))
                    {
                        return HashString(productUuid);
                    }
                }

                return GenerateFallbackMachineId();
            }
            catch (Exception ex)
            {
                return GenerateFallbackMachineId();
            }
        }

        private string GenerateFallbackMachineId()
        {
            // Generate a GUID as a fallback
            return Guid.NewGuid().ToString("N"); // "N" format removes hyphens
        }

        private string HashString(string input)
        {
            // Hash the input to ensure consistent length and format
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }
                return builder.ToString().Substring(0, 32); // Truncate to 32 chars for consistency
            }
        }
    }
}
