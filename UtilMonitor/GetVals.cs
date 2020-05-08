using System;
using System.Diagnostics;
using System.Management;
using OpenHardwareMonitor.Hardware;
public class Getter
{
    //These two use WMI for getting info such as CPU util and RAM avaliability
    protected PerformanceCounter cpuCounter;
    protected PerformanceCounter ramCounter;
    //This uses open source software Open Hardware Monitor
    protected Computer myComputer;
    private string cpuUtil, gpuTemp, ramUtil;

    //Default constructor
    public Getter()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        cpuUtil = null;
        gpuTemp = null;

    }
    public void update()
    {
        //Updates each value for the getters
        cpuUtil = cpuCounter.NextValue() + "%";
        ramUtil = ramCounter.NextValue() + "MB";
        // Needs a new contructor each time to get the values
        myComputer = new Computer();
        myComputer.GPUEnabled = true;
        myComputer.Open();
        foreach (var hardwareItem in myComputer.Hardware)
        {
            if (hardwareItem.HardwareType == HardwareType.GpuNvidia)
            {
                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        gpuTemp = $"{sensor.Value.ToString()}ºC";
                    }
                }
            }
        }
        // Essentially destroies the myComputer object.
        myComputer = null;
    }

    //Getters
   public string getCPUUtil()
        {
            return cpuUtil;

        }
   public string getGPUTemp()
    {
        return gpuTemp;
    }
    public string getRAM()
    {
        return ramUtil;
    }
    }




