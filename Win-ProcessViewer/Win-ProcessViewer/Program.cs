using System.Diagnostics;
using System.Timers;

namespace Win_ProcessViewer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var monitor = new ProcessMonitor();
            monitor.StartMonitoring();

            Thread monitorThread = new Thread(monitor.StartMonitoring);
            monitorThread.Start();

            while (true)
            {
                Console.WriteLine("按S停止监视...");
                Console.WriteLine("要更改进程名称，请按C");

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.C)
                {
                    monitor.ChangeProcessName();
                }
                else if (key == ConsoleKey.S)
                {
                    monitor.StopMonitoring();
                    break;
                }
            }

            monitorThread.Join(); // 等待监视线程结束
        }
    }
}

public class ProcessMonitor
{
    private System.Timers.Timer _timer;
    private string ProcessNameCache;
    private readonly object _lock = new object();
    private bool _isChangingName = false;

    public ProcessMonitor()
    {
        // 设置定时器以定期检查进程状态
        _timer = new System.Timers.Timer(5000); // 每5秒检查一次
        _timer.Elapsed += CheckProcessStatus;
        ProcessNameCache = "HYP"; // 设置默认值
    }

    public void ChangeProcessName()
    {
        lock (_lock)
        {
            _isChangingName = true;
            Console.WriteLine("更改要监听的进程名称为：");
            string processName = Console.ReadLine();
            if (!string.IsNullOrEmpty(processName))
            {
                ProcessNameCache = processName;
            }
            else
            {
                ProcessNameCache = "InputIsNull";
            }
            Console.WriteLine($"已应用新名称: {ProcessNameCache}");
            _isChangingName = false;
            Monitor.Pulse(_lock); // 通知其他线程名称更改完成
        }
    }

    public void StartMonitoring()
    {
        _timer.Start();
        Console.WriteLine("开始监视进程...");
    }

    public void StopMonitoring()
    {
        _timer.Stop();
        Console.WriteLine("停止监视进程...");
    }

    private void CheckProcessStatus(object sender, ElapsedEventArgs e)
    {
        lock (_lock)
        {
            while (_isChangingName)
            {
                Monitor.Wait(_lock); // 等待名称更改完成
            }

            var processName = ProcessNameCache;
            var processes = Process.GetProcessesByName(processName);

            if (processes.Any())
            {
                Console.WriteLine($"{processName} 正在运行.");
            }
            else
            {
                Console.WriteLine($"{processName} 未运行.");
            }
        }
    }
}