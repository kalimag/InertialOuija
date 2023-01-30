using System.Threading.Tasks;

namespace InertialOuija.Ghosts
{
	public class ExternalGhostFile
	{
		public string Path { get; }

		public ExternalGhostInfo Info { get; }

		internal ExternalGhostFile(string path, ExternalGhostInfo info)
		{
			Path = path;
			Info = info;
		}

		public Task<ExternalGhost> LoadAsync()
		{
			return Task.Run(() => ExternalGhost.Load(Path));
		}
	}
}
