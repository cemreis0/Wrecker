using System.IO.Ports;

namespace GroundConsole.Configuration
{
    public class SerialPortConfig
    {
        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; }
        public StopBits StopBits { get; set; }
        public Parity ParityBits { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }

        public static SerialPortConfig GetConfigurationFromUser()
        {
            Console.WriteLine("==================================================\n");
            Console.WriteLine("     Wrecker Flight Controller Systems            \n");
            Console.WriteLine("     (c) 2025                                     \n");
            Console.WriteLine("==================================================\n");

            Console.WriteLine("Devam etmek için herhangi bir tuşa basın...");
            Console.ReadKey(true);

            var config = new SerialPortConfig();

            config.PortName = GetPortName();
            config.BaudRate = GetBaudRate();
            config.StopBits = GetStopBits();
            config.ParityBits = GetParity();
            config.ReadTimeout = GetTimeout("Okuma", [500, 1000, 3000, 5000, 10000]);
            config.WriteTimeout = GetTimeout("Yazma", [500, 1000, 3000, 5000, 10000]);

            return config;
        }

        private static string GetPortName()
        {
            var ports = SerialPort.GetPortNames();
            return GetSelection("COM portunu seçin", ports);
        }

        private static int GetBaudRate()
        {
            string[] baudRates = ["9600", "115200", "14400", "19200", "38400", "57600"];
            var selected = GetSelection("Baud hızını seçin", baudRates);
            return int.Parse(selected);
        }

        private static StopBits GetStopBits()
        {
            string[] stopBits = ["1", "1.5", "2"];
            var selected = GetSelection("Stop biti sayısını seçin", stopBits);
            return Enum.TryParse(selected, out StopBits result) ? result : StopBits.One;
        }

        private static Parity GetParity()
        {
            string[] parities = ["None", "Odd", "Even"];
            var selected = GetSelection("Pariteyi seçin", parities);
            return Enum.TryParse(selected, out Parity result) ? result : Parity.None;
        }

        private static int GetTimeout(string type, int[] options)
        {
            string[] optionsStr = Array.ConvertAll(options, opt => opt.ToString());
            var selected = GetSelection($"{type} zaman aşımını seçin (ms)", optionsStr);
            return int.Parse(selected);
        }

        private static string GetSelection(string prompt, string[] options)
        {
            int currentSelection = 0;
            while (true)
            {
                Console.Clear();

                Console.WriteLine("\nOk tuşlarıyla seçim yapın. Enter ile seçimi onaylayın:\n");
                Console.WriteLine($"{prompt}:");
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == currentSelection) Console.WriteLine($"> {options[i]}");
                    else Console.WriteLine($"  {options[i]}");
                }

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow) currentSelection = (currentSelection - 1 + options.Length) % options.Length;
                if (key == ConsoleKey.DownArrow) currentSelection = (currentSelection + 1) % options.Length;
                if (key == ConsoleKey.Enter) break;
            }

            return options[currentSelection];
        }
    }
}
