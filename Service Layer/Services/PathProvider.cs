using System.Web.Hosting;

namespace Service_Layer.Services
{
    public interface IPathProvider
    {
        string MapPath(string path);
    }

    public class PathProvider : IPathProvider
    {
        public string MapPath(string path)
        {
            return HostingEnvironment.MapPath(path);
        }
    }
}