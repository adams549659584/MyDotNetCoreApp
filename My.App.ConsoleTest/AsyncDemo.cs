using System;
using System.Threading;
using System.Threading.Tasks;

namespace My.App.ConsoleTest
{
    public class AsyncDemo
    {
        public async Task AsyncSleep()
        {
            Console.WriteLine($"step2，线程ID：{Thread.CurrentThread.ManagedThreadId}");
            //await关键字表示等待Task.Run传入的逻辑执行完毕，此时(等待时)AsyncSleep的调用方能继续往下执行
            //Task.Run将开辟一个新线程执行指定逻辑
            await Task.Run(() => Sleep(10));
            Console.WriteLine($"step4，线程ID：{Thread.CurrentThread.ManagedThreadId}");
        }

        private void Sleep(int second)
        {
            Console.WriteLine($"step3，线程ID：{Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(second * 1000);
        }
    }
}