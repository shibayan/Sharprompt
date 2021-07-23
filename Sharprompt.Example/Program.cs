using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Sharprompt.Example.Models;

namespace Sharprompt.Example
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                    .UseConsoleLifetime()
                    .UseEnvironment("CLI")
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddLogging(
                          builder =>
                          {
                              builder.AddFilter("Microsoft", LogLevel.Warning)
                                     .AddFilter("System", LogLevel.Warning);
                          });
                        services.AddHostedService<MainProgram>();
                    }).Build();

            await host.RunAsync();
        }
    }

    internal class MainProgram : IHostedService
    {

        private readonly IHostApplicationLifetime _appLifetime;
        private readonly CancellationToken _stopApp;
        private Task _menutask;
        public MainProgram(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
            _stopApp = _appLifetime.ApplicationStopping;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _menutask = Task.Run(() =>
            {
                ShowMenu();
            }, stoppingToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            if (_menutask != null)
            {
                await _menutask;
                _menutask.Dispose();
            }
        }
        public void ShowMenu()
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (!_stopApp.IsCancellationRequested)
            {
                Console.Clear();

                var type = Prompt.Select<ExampleType>("Choose prompt example", _stopApp);

                if (_stopApp.IsCancellationRequested)
                {
                    continue;
                }
                switch (type)
                {
                    case ExampleType.Input:
                        RunInputSample();
                        break;
                    case ExampleType.Confirm:
                        RunConfirmSample();
                        break;
                    case ExampleType.Password:
                        RunPasswordSample();
                        break;
                    case ExampleType.Select:
                        RunSelectSample();
                        break;
                    case ExampleType.MultiSelect:
                        RunMultiSelectSample();
                        break;
                    case ExampleType.SelectWithEnum:
                        RunSelectEnumSample();
                        break;
                    case ExampleType.MultiSelectWithEnum:
                        RunMultiSelectEnumSample();
                        break;
                    case ExampleType.List:
                        RunListSample();
                        break;
                    case ExampleType.FolderSelect:
                        RunFolderSample();
                        break;
                    case ExampleType.FileSelect:
                        RunFileSample();
                        break;
                    case ExampleType.AutoForms:
                        RunAutoFormsSample();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (!_stopApp.IsCancellationRequested)
                {
                    Prompt.AnyKey(_stopApp);
                }
            }
        }

        private void RunInputSample()
        {
            var name = Prompt.Input<string>("What's your name?", _stopApp, "teste", validators: new[] { Validators.Required(), Validators.MinLength(3) });
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine($"Hello, {name}!");
            }
        }

        private void RunConfirmSample()
        {
            var answer = Prompt.Confirm("Are you ready?", _stopApp, true);
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine($"Your answer is {answer}");
            }
        }

        private void RunPasswordSample()
        {
            var secret = Prompt.Password("Type new password", _stopApp, new[] { Validators.Required(), Validators.MinLength(8) });
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine("Password OK");
            }
        }

        private void RunListSample()
        {
            var opt = new ListOptions<string>
            {
                Message = "Please add item(s)",
                RemoveAllMatch = true
            };
            var value = Prompt.List(opt, _stopApp);
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine($"You picked {string.Join(", ", value)}");
            }
        }
        private void RunSelectSample()
        {
            var city = Prompt.Select("Select your city", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, _stopApp, defaultValue: "Singapore", pageSize: 3);
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine($"Hello, {city}!");
            }
        }

        private void RunMultiSelectSample()
        {
            var options = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, _stopApp, pageSize: 3, defaultValues: new[] { "Tokyo" });
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine($"You picked {string.Join(", ", options)}");
            }
        }

        private void RunSelectEnumSample()
        {
            var envalue = Prompt.Select<MyEnum>("Select enum value", _stopApp, defaultValue: MyEnum.Bar);
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine($"You selected {envalue}");
            }
        }

        private void RunMultiSelectEnumSample()
        {
            var multvalue = Prompt.MultiSelect("Select enum value", _stopApp, defaultValues: new[] { MyEnum.Bar });
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine($"You picked {string.Join(", ", multvalue)}");
            }
        }

        private void RunAutoFormsSample()
        {
            var model = Prompt.AutoForms<MyFormModel>(_stopApp);
            if (!_stopApp.IsCancellationRequested)
            {
                Console.WriteLine("Forms OK");
            }
        }

        private void RunFolderSample()
        {
            var folder = Prompt.FileBrowser(FileBrowserChoose.Folder, "Select/New folder", _stopApp, pageSize: 5, promptCurrentPath: false);
            if (!_stopApp.IsCancellationRequested)
            {
                var dirfound = folder.NotFound ? "not found" : "found";
                Console.WriteLine($"You picked, {Path.Combine(folder.PathValue,folder.SelectedValue)} and {dirfound}");
            }
        }

        private void RunFileSample()
        {
            var file = Prompt.FileBrowser(FileBrowserChoose.File, "Select/New file", _stopApp, pageSize: 10, allowNotSelected:true);
            if (!_stopApp.IsCancellationRequested)
            {
                if (string.IsNullOrEmpty(file.SelectedValue))
                {
                    Console.WriteLine("You chose nothing!");
                }
                else
                {
                    var filefound = file.NotFound ? "not found" : "found";
                    Console.WriteLine($"You picked, {Path.Combine(file.PathValue, file.SelectedValue)} and {filefound}");
                }
            }
        }
    }
}
