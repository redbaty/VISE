namespace VISE.Models
{
    public class ComputerModel
    {
        public string Ip { get; set; }
        public string Password { get; set; } = "";
        public bool IsPinned { get; set; }

        public ComputerModel()
        {
        }

        public ComputerModel(string ip)
        {
            Ip = ip;
        }

        public ComputerModel(string ip, string password)
        {
            Ip = ip;
            Password = password;
        }
    }
}