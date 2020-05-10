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
    private string gpuTemp, ramUtil, cpuTemp, gpuUtil;
    private double cpuUtil;

    //Default constructor
    public Getter()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        //Was unable to find a way to get GPU util, was using: https://stackoverflow.com/questions/56830434/c-sharp-get-total-usage-of-gpu-in-percentage
        //gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "");
        cpuUtil = 0;
        gpuTemp = null;
        cpuTemp = null;
        gpuUtil = null;

    }
    public void update()
    {
        //Updates each value for the getters
        cpuUtil = cpuCounter.NextValue();
        ramUtil = ramCounter.NextValue() + "MB";
        // Needs a new contructor each time to get the values
        myComputer = new Computer();
        myComputer.GPUEnabled = true;
        myComputer.CPUEnabled = true;
        myComputer.Open();
        foreach (var hardwareItem in myComputer.Hardware)
        {
            //Checks for GPU of either manufacturer, interestingly still lists AMD cards as ATI so will need to test on AMD hardware.
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAti)
            {
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        gpuTemp = $"{sensor.Value.ToString()}ºC";
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
                        cpuTemp = $"{sensor.Value.ToString()}ºC";
                    }
                }

            }
        }
        // Essentially destroies the myComputer object.
        myComputer = null;

    }

    //Getters, could potentially use returning a dictionary for this, condensing the getters into a single function
   public int getCPUUtil()
        {
            return Convert.ToInt32(cpuUtil);

        }
   public string getGPUTemp()
    {
        return gpuTemp;
    }
    public string getCPUTemp()
    {
        return cpuTemp;
    }
    public string getRAM()
    {
        return ramUtil;
    }
    public string getGPUUtil()
    {
        return gpuUtil;
    }
    }




