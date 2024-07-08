using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameHelper
{
	public abstract class FrameFileSystemInfo : FileSystemInfo
	{
		protected Frame ViaFrame { get; set; }

		public FrameFileSystemInfo(Frame via, string fullName)
		{
			ViaFrame = via;
			base.OriginalPath = fullName.TrimEnd('/');
		}

		public override bool Exists
		{
			get
			{
				if (ViaFrame == null)
				{
					return false;
				}
				string existsResp = ViaFrame.PrintShortLuaResponse($"tostring(true and thesefiles:find(\"{Name}\\n\",1,true))", runBeforePrinting: $"local thesefiles = frame.file.listdir('{Path}')").Result;
				return existsResp == "true";
			}
		}

		public override string FullName => base.OriginalPath;
		public string Path
		{
			get
			{
				return FullName.Split('/').SkipLast(1).Aggregate((a, b) => $"{a}/{b}");
			}
		}

		public override string Name
		{
			get
			{
				return FullName.Split('/').Last();
			}
		}

		public override void Delete()
		{
			DeleteAsync().Wait();
		}

		public virtual async Task<bool> DeleteAsync()
		{
			bool succeeded = await ViaFrame.SendLuaChecked($"frame.file.remove('{FullName}')");
			if (!succeeded)
			{
				Debug.WriteLine($"Failed to delete {FullName}");
			}
			return succeeded;
		}

		public async Task<bool> Rename(string newName)
		{
			bool succeeded = await ViaFrame.SendLuaChecked($"frame.file.rename('{FullName}', '{Path}/{newName}')");
			if (succeeded)
			{
				base.OriginalPath = Path + "/" + newName;
			}
			else
			{
				Debug.WriteLine($"Failed to rename {FullName} to {newName}");
			}
			return succeeded;
		}

		public abstract bool IsFile { get; }
		public abstract bool IsDirectory { get; }
	}

	public class FrameFile : FrameFileSystemInfo
	{
		public FrameFile(Frame via, string fullName) : base(via, fullName)
		{
		}

		public static async Task<FrameFile> WriteFile(Frame frame, string name, string contents)
		{
			FrameFile file = new FrameFile(frame, name);
			if (await file.WriteContents(contents))
			{
				return file;
			}
			return null;
		}

		public async Task<string> GetContents()
		{
			return await ViaFrame.PrintLuaResponse($"readFullFile('{FullName}')");
		}

		public async Task<bool> WriteContents(string contents)
		{
			bool success = true;
			contents = contents.Replace(Environment.NewLine, "\n");
			foreach (var kvp in luaEscapeSequences)
			{
				contents = contents.Replace(kvp.Key.ToString(), kvp.Value);
			}
			string varName = "f" + Name.Split('.').First();
			success &= await ViaFrame.SendLuaAutoRetry($"{varName} = frame.file.open(\"{FullName}\", \"write\")");
			if (!success)
			{
				return false;
			}
			int curIndex = 0;
			int chunkIndex = 0;
			while (curIndex < contents.Length)
			{
				int nextChunkLength = Math.Min(contents.Length - curIndex, (ViaFrame.Connection.MTUSize ?? 23) - 40 - varName.Length);
				if (nextChunkLength <= 0)
				{
					break;
				}
				while (contents[curIndex + nextChunkLength - 1] == '\\' && nextChunkLength > 0)
				{
					nextChunkLength--;
				}
				if (nextChunkLength <= 0)
				{
					break;
				}
				string partToSend = contents.Substring(curIndex, nextChunkLength);
				Debug.WriteLine($"Sending chunk {chunkIndex} of {Name}: {partToSend}");
				success &= await ViaFrame.SendLuaChecked($"{varName}:write(\"{partToSend}\")");
				if (!success)
					break;
				chunkIndex++;
				curIndex += nextChunkLength;
			}
			success &= await ViaFrame.SendLuaAutoRetry($"{varName}:close()");
			if (!success)
			{
				Debug.WriteLine($"Failed to write {FullName}.  Completed {chunkIndex} chunks.");
			}
			else
			{
				Debug.WriteLine(Name + " file written, total of " + chunkIndex + " chunks");
			}
			return success;
		}

		public async Task<bool> AppendContents(string contents)
		{
			bool success = true;
			contents = contents.Replace(Environment.NewLine, "\n");
			foreach (var kvp in luaEscapeSequences)
			{
				contents = contents.Replace(kvp.Key.ToString(), kvp.Value);
			}
			string varName = "f" + Name.Split('.').First();
			success &= await ViaFrame.SendLuaAutoRetry($"{varName} = frame.file.open(\"{FullName}\", \"append\")");
			if (!success)
			{
				return false;
			}
			int curIndex = 0;
			int chunkIndex = 0;
			while (curIndex < contents.Length)
			{
				int nextChunkLength = Math.Min(contents.Length - curIndex, (ViaFrame.Connection.MTUSize ?? 23) - 30 - varName.Length);
				if (nextChunkLength <= 0)
				{
					break;
				}
				while (contents[curIndex + nextChunkLength - 1] == '\\' && nextChunkLength > 0)
				{
					nextChunkLength--;
				}
				if (nextChunkLength <= 0)
				{
					break;
				}
				string partToSend = contents.Substring(curIndex, nextChunkLength);

				success &= await ViaFrame.SendLuaChecked($"{varName}:write(\"{partToSend}\")");
				if (!success)
					break;
				chunkIndex++;
				curIndex += nextChunkLength;
			}
			success &= await ViaFrame.SendLuaAutoRetry($"{varName}:close()");
			if (!success)
			{
				Debug.WriteLine($"Failed to append {FullName}.  Completed {chunkIndex} chunks.");
			}
			else
			{
				Debug.WriteLine(Name + " file appended, total of " + chunkIndex + " chunks");
			}
			return success;
		}

		private static readonly ReadOnlyDictionary<char, string> luaEscapeSequences = new ReadOnlyDictionary<char, string>(new Dictionary<char, string>
		{
			{ '\\', @"\\" },
			{ '\n', @"\n" },
			{ '\r', @"\r" },
			{ '\t', @"\t" },
			{ '\"', "\\\"" },
			{ '[', @"\[" },
			{ ']', @"\]" }
		});

		public override bool IsFile => true;

		public override bool IsDirectory => false;
	}

	public class FrameDirectory : FrameFileSystemInfo
	{
		public override bool IsFile => false;

		public override bool IsDirectory => true;

		public FrameDirectory(Frame via, string fullName) : base(via, fullName)
		{
		}

		public override async Task<bool> DeleteAsync()
		{
			// first get all the contents of the directory
			var allContents = await GetAllContents(false);
			// then delete all the files
			foreach (var info in allContents)
			{				
					if (!await info.DeleteAsync())
					{
						Debug.WriteLine($"Failed to delete {info.FullName}");
						return false;
					}				
			}
			// now delete the directory itself
			return await base.DeleteAsync();
		}

		public async Task<List<FrameFileSystemInfo>> GetAllContents(bool recursive = false)
		{
			string contents = await ViaFrame.PrintLuaResponse($"listFilesInDir('{FullName}')");
			if (contents == null)
			{
				Debug.WriteLine($"Failed to get contents of {FullName}");
				return new List<FrameFileSystemInfo>();
			}
			string[] lines = contents.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			List<FrameFileSystemInfo> allContents = new List<FrameFileSystemInfo>();
			foreach (string line in lines)
			{
				if (line.EndsWith('/'))
				{
					if (line == "./" || line == "../")
					{
						continue;
					}
					allContents.Add(new FrameDirectory(ViaFrame, $"{FullName}/{line}"));
					if (recursive)
					{
						allContents.AddRange(await new FrameDirectory(ViaFrame, $"{FullName}/{line}").GetAllContents(recursive));
					}
				}
				else
				{
					allContents.Add(new FrameFile(ViaFrame, $"{FullName}/{line}"));
				}
			}
			return allContents;
		}

		public async Task<List<FrameFile>> GetAllFiles(bool recursive = false)
		{
			List<FrameFileSystemInfo> allContents = await GetAllContents();
			List<FrameFile> allFiles = new List<FrameFile>();
			foreach (FrameFileSystemInfo info in allContents)
			{
				if (info is FrameFile file)
				{
					allFiles.Add(file);
				}
				else if (recursive && info is FrameDirectory dir)
				{
					allFiles.AddRange(await dir.GetAllFiles(true));
				}
			}
			return allFiles;
		}

		public async Task<List<FrameDirectory>> GetAllDirectories(bool recursive = false)
		{
			List<FrameFileSystemInfo> allContents = await GetAllContents();
			List<FrameDirectory> allDirs = new List<FrameDirectory>();
			foreach (FrameFileSystemInfo info in allContents)
			{
				if (info is FrameDirectory dir)
				{
					allDirs.Add(dir);
					if (recursive)
					{
						allDirs.AddRange(await dir.GetAllDirectories(true));
					}
				}
			}
			return allDirs;
		}

		public async Task<FrameDirectory> CreateSubdirectory(string name)
		{
			bool success = await ViaFrame.SendLuaChecked($"frame.file.mkdir('{FullName}/{name}')");
			if (!success)
			{
				Debug.WriteLine($"Failed to create directory {FullName}/{name}");
				return null;
			}
			return new FrameDirectory(ViaFrame, $"{FullName}/{name}");
		}

		public async Task<bool> Create()
		{

			bool success = await ViaFrame.SendLuaChecked($"frame.file.mkdir('{FullName}')");
			if (!success)
			{
				Debug.WriteLine($"Failed to create directory {FullName}");
			}
			return success;

		}
	}
}
