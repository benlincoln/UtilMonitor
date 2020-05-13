using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenHardwareMonitor.Hardware;
public class Getter
{
    //These two use WMI for getting info such as CPU util and RAM avaliability
    protected PerformanceCounter cpuCounter;
    protected PerformanceCounter ramCounter;
    

    //This uses open source software Open Hardware Monitor
    protected Computer myComputer;
    private int gpuTemp, ramUtil, cpuTemp, gpuUtil;
    private double cpuUtil;

    //Default constructor
    public Getter()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        //Was unable to find a way to get GPU util, was using: https://stackoverflow.com/questions/56830434/c-sharp-get-total-usage-of-gpu-in-percentage
        //gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "");
        ulong totalMemory = getSystemMemory();

    }
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
            //Checks for GPU of either manufacturer, interestingly still lists AMD cards as ATI so will need to test on AMD hardware. Also unclear if this will work on Intel iGPUs
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
            {
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        gpuTemp = Convert.ToInt32(sensor.Value);
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
    public int getGPUUtil()
    {
        return gpuUtil;
    }
    public double getSysRAM()
    {
        //Converts the avaliable ram into a double in megabytes
        return Convert.ToDouble(getSystemMemory())/Math.Pow(1024,2);
    }
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




