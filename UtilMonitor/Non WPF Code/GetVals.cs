using System;
using System.Collections.Generic;
using System.Diagnostics;
//This uses open source software Open Hardware Monitor
using OpenHardwareMonitor.Hardware;
public class Getter
{
    //These two use WMI for getting info such as CPU util and RAM avaliability
    protected PerformanceCounter cpuCounter;
    protected PerformanceCounter ramCounter;
    

    protected Computer myComputer;
    private int gpuTemp, ramUtil, cpuTemp, gpuLoad;
    private double cpuUtil;
    public string GPUName, CPUName;
    //private List<string> storageNames = new List<string>();
    //Default constructor
    public Getter()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        //Was unable to find a way to get GPU util, was using: https://stackoverflow.com/questions/56830434/c-sharp-get-total-usage-of-gpu-in-percentage
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
            //This will need unique identifiers for each HDD, i.e. tested system uses 
            else if (hardwareItem.HardwareType == HardwareType.HDD) 
            {

            }
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
    /*Allows for the different calculations for each type (graph height is 100 which allows for easy percentage calculations,
     however something like GPU temp would have the user enter max values for the graph to make the graph representative for that card 
     (i.e. in my own experience, my RTX2070S runs significantly cooler than my R9 390; thus the scale on the graph should be lower/tighter for the 2070S)   */
    public double graphCalc(int yMax, string measurement)
    {
        switch (measurement)
        {
            case "cpuUtil":
                return yMax - getCPUUtil();
            case "cpuTemp":
                //Treating 90 celcius as "100%" as a placeholder, will allow users to change max value in settings window when implemented 
                return yMax - (getCPUTemp()*1.111);
            case "gpuTemp":
                return yMax - (getGPUTemp() * 1.111);
            case "ramUtil":
                //This calculates and returns the % of ram avaliable
                return yMax - (getRAM() / getSysRAM())*100; 
            default:
                //Default throws an error as it means the value being measured is not recognised. Could potentially assign cpuUtil as default but this seems more appropriate
                throw new ArgumentException("String measurement parameter in graphCalc not found");
                
        }
        
    }
    }




