﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenHardwareMonitor.Hardware;
using UtilMonitor.Non_WPF_Code;
public class Getter
{
    //These two use WMI for getting info such as CPU util and RAM avaliability
    private PerformanceCounter cpuCounter;
    private PerformanceCounter ramCounter;

    Notification noti = new Notification();
    protected Computer myComputer;
    private int maxVal;
    private double relativePercent;
    public string GPUName, CPUName;
    private bool CPUNotified, GPUNotified, RAMNotified;
    private Dictionary<string,string> hardwareInfo = new Dictionary<string, string>();
    //private List<string> storageNames = new List<string>();
    //Default constructor
    public Getter()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        //gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "");
        ulong totalMemory = getSystemMemory();
        hardwareInfo.Add("CPUName", null);
        hardwareInfo.Add("GPUName", null);
        hardwareInfo.Add("FreeRAM", null);
        hardwareInfo.Add("CPUTemp", null);
        hardwareInfo.Add("GPUTemp", null);
        hardwareInfo.Add("CPUUtil", null);
        hardwareInfo.Add("GPULoad", null);
        myComputer = new Computer
        {
            GPUEnabled = true,
            CPUEnabled = true
        };
        myComputer.Open();
        foreach (var hardwareItem in myComputer.Hardware)
        {
            //Gets names for the CPU and GPU. Open HW Monitor does not support intel iGPUs.   
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
            {
                hardwareInfo["GPUName"] = hardwareItem.Name;
                GPUName = hardwareItem.Name;
            }
            else if (hardwareItem.HardwareType == HardwareType.CPU)
            {
                hardwareInfo["CPUName"] = hardwareItem.Name;
                
            }
            //Quits the application thus
            if (hardwareInfo["GPUName"] != null && hardwareInfo["CPUName"] != null)
            {
                break;
            }

        }
        if (hardwareInfo["CPUName"] == null)
        {
            hardwareInfo["CPUName"] = "Could not detect CPU";
        }
        if (hardwareInfo["GPUName"] == null)
        {
            hardwareInfo["GPUName"] = "Could not detect GPU";
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
        hardwareInfo["CPUUtil"] = Convert.ToString(Convert.ToInt32(cpuCounter.NextValue()));
        hardwareInfo["FreeRAM"] = Convert.ToString(ramCounter.NextValue());
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
                //todo: Add a boolean that marks each one as done so can exit the foreach as soon as possible
                GPUName = hardwareItem.Name;
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        hardwareInfo["GPUTemp"] = Convert.ToString(sensor.Value);
                    }
                    if (sensor.SensorType == SensorType.Load)
                    {
                        //This is called load as opposed to util, as load and utilisation appear to be two different things (MSI Afterburner returns a different value when viewing GPU utilisation)
                        hardwareInfo["GPULoad"] = Convert.ToString(Convert.ToInt32(sensor.Value));
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
                        hardwareInfo["CPUTemp"] = Convert.ToString(sensor.Value);
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

    //Getters, could potentially use the settings read/writer for notifications for high temps/ when to warn the user if their ram util goes to certain values (i.e. a user may want to know when their util goes above 70%, not just near maxxed out)
    public int getCPUUtil()
        {
            return Convert.ToInt32(hardwareInfo["CPUUtil"]);
        }
   public int getGPUTemp()
    {
        return Convert.ToInt32(hardwareInfo["GPUTemp"]);
    }
    public int getCPUTemp()
    {
        return Convert.ToInt32(hardwareInfo["CPUTemp"]);
    }
    public int getRAM()
    {
        /* Commented code causes stack overflow error, unsure as to why
         if ((getRAM() / getSysRAM()) * 100 < 5 )
        {
            noti.ShowNotification($"High RAM Utilisation");
            RAMNotified = true;
        }
        else
        {
            RAMNotified = false;
        }*/
        return Convert.ToInt32(hardwareInfo["FreeRAM"]);
    }
    public int getGPULoad()
    {
        return Convert.ToInt32(hardwareInfo["GPULoad"]);
    }
    public int getSysRAM()
    {
        //Converts the avaliable ram into a double in megabytes
        return Convert.ToInt32(Convert.ToDouble(getSystemMemory())/Math.Pow(1024,2));
    }

    public string getCPUName()
    {
        return hardwareInfo["CPUName"];
    }
    public string getGPUName()
    {
        return hardwareInfo["GPUName"];
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
    private void checkNotification(int value, string hardware) {
    }

    }




