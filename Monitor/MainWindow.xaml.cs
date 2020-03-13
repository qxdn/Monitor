using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Information Info { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            //不在任务栏显示
            this.ShowInTaskbar = false;
            Info = new Information();
            Task task = new Task(async () => {
                while (true)
                {
                    this.Dispatcher.Invoke(new Action(delegate
                    {
                        GetInformation(Info);
                        CPULoad.Content = Info.CPULoad.ToString("0.00");
                        CPUTemp.Content = Info.CPUTemp;
                        MEMLoad.Content = Info.MEMLoad.ToString("0.00");
                    }));
                    await Task.Delay(1000);
                }
            });
            task.Start();
        }

        //获取电脑信息
        public void GetInformation(Information information)
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.RAMEnabled = true;
            computer.Accept(updateVisitor);
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                //查找硬件类型为CPU
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //找到温度传感器
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {   
                            //找到CPU Package
                            if("CPU Package" == computer.Hardware[i].Sensors[j].Name)
                            {
                                information.CPUTemp = computer.Hardware[i].Sensors[j].Value ?? 0;                              
                            } 
                        }
                        else if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                        {
                            //CPU Total负载
                            if("CPU Total" == computer.Hardware[i].Sensors[j].Name)
                            {
                                information.CPULoad = computer.Hardware[i].Sensors[j].Value ?? 0;
                            }
                        }
                    }
                }else if(computer.Hardware[i].HardwareType == HardwareType.RAM)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                        {
                            //RAM负载
                            information.MEMLoad = computer.Hardware[i].Sensors[j].Value ?? 0;
                        }
                    }
                }
            }
            computer.Close();
        }
    }

    //openhardwareMonitor
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }
    }

    public class Information
    {
        public float CPULoad { get; set; }

        public float CPUTemp { get; set; }

        public float MEMLoad { get; set; }
    }

}
