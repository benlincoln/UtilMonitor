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
    private bool notified;
    private Dictionary<string,string> hardwareInfo = new Dictionary<string, string>();
    //Default constructor
    public Getter()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        ulong totalMemory = getSystemMemory();
        // Uses a foreach loop to create the empty values in the dictionary
        string[] dictStrings = {"CPUName", "GPUName", "FreeRAM", "CPUTemp" , "GPUTemp", "CPUUtil", "GPULoad", "RAMUtil"};
        foreach (string dictKey in dictStrings)
        {
            hardwareInfo.Add(dictKey, null);
        }
        
        hardwareInfo.Add("SysRAM", Convert.ToString(Convert.ToDouble(getSystemMemory()) / Math.Pow(1024, 2)));
        myComputer = new Computer
        {
            GPUEnabled = true,
            CPUEnabled = true,
            HDDEnabled = true
        };
        myComputer.Open();
        foreach (var hardwareItem in myComputer.Hardware)
        {
            //Gets names for the CPU and GPU. Open HW Monitor does not support intel iGPUs.   
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
            {
                hardwareInfo["GPUName"] = hardwareItem.Name;
                
            }
            else if (hardwareItem.HardwareType == HardwareType.CPU)
            {
                hardwareInfo["CPUName"] = hardwareItem.Name;
                
            }
            //Quits the loop once both are assigned improving the effeciency
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
    //Might have found solution for detecting drives, will implement at a later date.
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
        // Booleans to mark when the program has fetched all the info for the respective components to optimise an otherwise inefficent algorithm
        Boolean GPUDone = false;
        Boolean CPUDone = false;
        foreach (var hardwareItem in myComputer.Hardware)
        {
            //Checks for GPU of either manufacturer, interestingly still lists AMD cards as ATI.
            // Works on AMD hardware, does not recognise intel iGPUs, some laptops and potentially some OEM MoBos do not seem to allow temp monitoring
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
            {
                
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
                GPUDone = true;
            }
            
            else if (hardwareItem.HardwareType == HardwareType.CPU)
            {
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        hardwareInfo["CPUTemp"] = Convert.ToString(sensor.Value);
                        CPUDone = true;
                        break;
                    }
                }
            }
         if (GPUDone && CPUDone)
            {
                //Breaks out of the foreach to try limit ineffeciency 
                break;
            }   
            

        }
        // Essentially destroies the myComputer object.
        myComputer = null;
        // Checks if needs to send a notification for high usage
        hardwareInfo["RAMUtil"] = Convert.ToString((getRAM() / getSysRAM()) * 100);
        // Uses a boolean to stop a new notification every update for continous max load, could potentially use a time frame between notifications to stop notification spam
        string notificationBody = checkNotification(hardwareInfo);
        if (notificationBody != null && notified == false)
        {
            noti.ShowNotification(notificationBody);
            notified = true;
        }
        else
        {
            notified = false;
        }
        
    }

    //Gets total system memory, returns in bytes
    private static ulong getSystemMemory()
    {
        return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
    }

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
        
        return Convert.ToInt32(hardwareInfo["FreeRAM"]);
    }
    public int getGPULoad()
    {
        return Convert.ToInt32(hardwareInfo["GPULoad"]);
    }
    public double getSysRAM()
    {
        //Converts the avaliable ram into a double in megabytes
        return Convert.ToDouble(hardwareInfo["SysRAM"]);
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
                return yMax - Convert.ToDouble(hardwareInfo["RAMUtil"]);
            case "gpuLoad":
                return yMax - getGPULoad();
            default:
                //Default throws an error as it means the value being measured is not recognised. Could potentially assign cpuUtil as default but this seems more appropriate
                throw new ArgumentException("String measurement parameter in graphCalc not found");
                
        }
        
    }
    private string checkNotification(Dictionary<string, string> hardwareDict) {
        List<string> highValues = new List<string>();
        foreach (KeyValuePair<string, string> hardwareItem in hardwareDict)
        {
            string hardware = hardwareItem.Key;
            /*if (hardware == "CPUName" || hardware == "GPUName")
            {
                continue;
            }*/
            
            if (Convert.ToInt32(configReadWriter.readConfig(hardware)) == 0 || configReadWriter.readConfig(hardware) == null)
            {
                // If the setting is 0 the code skip that hardware item
                continue;
            }
            if (Convert.ToInt32(hardwareInfo[hardware]) >= Convert.ToInt32(configReadWriter.readConfig(hardware)) && hardware != "RamUtil")
            {
                // Builds a string for components with high readings, as opposed to two seperate notifications
                highValues.Add(hardware);
                
                
            }
            
        }
        if (highValues.Count > 0)
        {
            string notificationBody = "High values detected for: ";
            foreach (string hardwareAlert in highValues)
            {
                notificationBody += $"{hardwareAlert}: {hardwareDict[hardwareAlert]} ";
                
                return notificationBody;
            } 
        }
        return null;
    }

    }




