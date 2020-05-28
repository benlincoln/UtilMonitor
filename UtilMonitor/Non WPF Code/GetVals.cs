using System;
using System.Diagnostics;
using OpenHardwareMonitor.Hardware;
using UtilMonitor.Non_WPF_Code;
public class Getter
{
    //These two use WMI for getting info such as CPU util and RAM avaliability
    private PerformanceCounter cpuCounter;
    private PerformanceCounter ramCounter;
    

    protected Computer myComputer;
    private int gpuTemp, ramUtil, cpuTemp, gpuLoad, maxVal;
    private double cpuUtil, relativePercent;
    public string GPUName, CPUName;
    //private List<string> storageNames = new List<string>();
    //Default constructor
    public Getter()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        //gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "");
        ulong totalMemory = getSystemMemory();
        myComputer = new Computer();
        myComputer.GPUEnabled = true;
        myComputer.CPUEnabled = true;
        myComputer.Open();
        foreach (var hardwareItem in myComputer.Hardware)
        {
            //Gets names for the CPU and GPU. Open HW Monitor does not support intel iGPUs.   
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
            {
                GPUName = hardwareItem.Name;
            }
            else if (hardwareItem.HardwareType == HardwareType.CPU)
            {
                CPUName = hardwareItem.Name;
            }
            //Quits the application thus
            if (GPUName != null && CPUName != null)
            {
                break;
            }

        }
        if (CPUName == null)
        {
            CPUName = "Could not detect CPU";
        }
        if (GPUName == null)
        {
            GPUName = "Could not detect GPU";
        }
        //storageNames = buildDriveList(myComputer);
    }
    //Seemingly does not detect drives, however can adapt this code to accomodate for multi cpu/gpu configs.
    /*private List<string> buildDriveList(Computer computer)
    {
        List<string> drives = new List<string>();
        foreach (var hardwareItem in myComputer.Hardware)
        {
              
            if (hardwareItem.HardwareType == HardwareType.HDD)
            {
                string name = hardwareItem.Name;
                drives.Add(name);
            }
        }
        return drives;
    }*/

    public void update()
    {
        //Updates each value for the getters
        cpuUtil = cpuCounter.NextValue();
        ramUtil = Convert.ToInt32(ramCounter.NextValue());
        // Needs a new contructor each time to get the values
        myComputer = new Computer();
        myComputer.GPUEnabled = true;
        myComputer.CPUEnabled = true;
        myComputer.Open();
        foreach (var hardwareItem in myComputer.Hardware)
        {
            //Checks for GPU of either manufacturer, interestingly still lists AMD cards as ATI.
            // Works on AMD hardware, does not recognise intel iGPUs, some laptops and potentially some OEM MoBos do not seem to allow temp monitoring
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
            {
                //todo: Add a boolean that marks each one as done so can exit the foreach
                GPUName = hardwareItem.Name;
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        gpuTemp = Convert.ToInt32(sensor.Value);
                    }
                    if (sensor.SensorType == SensorType.Load)
                    {
                        //This is called load as opposed to util, as load and utilisation appear to be two different things (MSI Afterburner returns a different value when viewing GPU utilisation)
                        gpuLoad = Convert.ToInt32(sensor.Value);
                    }
                }
            }
            //Like above, could condense into a function.
            else if (hardwareItem.HardwareType == HardwareType.CPU)
            {
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        cpuTemp = Convert.ToInt32(sensor.Value);
                    }
                }
            }
            
            /*else if (hardwareItem.HardwareType == HardwareType.HDD) 
            {

            }*/

        }
        // Essentially destroies the myComputer object.
        myComputer = null;

    }

    //Gets total system memory, returns in bytes
    private static ulong getSystemMemory()
    {
        return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
    }

    //Getters, could potentially use returning a dictionary for this, condensing the getters into a single function
    public int getCPUUtil()
        {
            return Convert.ToInt32(cpuUtil);

        }
   public int getGPUTemp()
    {
        return gpuTemp;
    }
    public int getCPUTemp()
    {
        return cpuTemp;
    }
    public int getRAM()
    {
        return ramUtil;
    }
    public int getGPULoad()
    {
        return gpuLoad;
    }
    public double getSysRAM()
    {
        //Converts the avaliable ram into a double in megabytes
        return Convert.ToDouble(getSystemMemory())/Math.Pow(1024,2);
    }
    

   /* public List<string> getdriveList()
    {
        return storageNames;
    }*/
    
    public double graphCalc(int yMax, string measurement)
    {
        switch (measurement)
        {
            
            case "cpuUtil":
                return yMax - getCPUUtil();
            case "cpuTemp":
                maxVal = Convert.ToInt32(configReadWriter.readConfig("tempMaxCPU"));
                //Reassigns as double to allow percent calculation
                double cpuTemp = getCPUTemp();
                relativePercent = cpuTemp / maxVal;
                relativePercent *= 100;
                return yMax - (relativePercent);
            case "gpuTemp":
                maxVal = Convert.ToInt32(configReadWriter.readConfig("tempMaxGPU"));
                double gpuTemp = getGPUTemp();
                relativePercent = gpuTemp / maxVal;
                relativePercent *= 100;
                return yMax - (relativePercent);
            case "ramUtil":
                return yMax - (getRAM() / getSysRAM())*100;
            case "gpuLoad":
                return yMax - getGPULoad();
            default:
                //Default throws an error as it means the value being measured is not recognised. Could potentially assign cpuUtil as default but this seems more appropriate
                throw new ArgumentException("String measurement parameter in graphCalc not found");
                
        }
        
    }
    }




