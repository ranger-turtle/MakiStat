using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace MakiStatBackend;

	/*BONUS Optimize checker to make language based hierarchy of resources where language specific resources can be stored 
	 separately to discard them easily when language-specific skeleton data will be changed*/
	/*TODO fix the bug making registering multiple instances of the same resource whose paths differ only with the case of some letters (standard library does not have
	  case-sensitive file path checking so far */
	internal class ModificationChecker
	{
		internal const int ShaHashLength = 16;
		private byte[] cachedSkeletonHash;
		private struct ResourceEntry
		{
			//Size: 16 bytes
			public byte[] Hash { get; set; }
			//Paths of output pages which are generated using this hash
			public HashSet<string> PathsOfPagesUsingResource { get; set; }
		}

		//M - Maki; S - Sei; M - Modification; C - Checker
		private const string modificationCheckerFileMagicNumber = "MSMC";
		private const string modificationCheckerFileName = "check.msmc";

		//This dictionary uses resource path as a key
		private readonly Dictionary<string, ResourceEntry> resourceInfo = new();
		// Pages checked by the checker
		private readonly HashSet<string> registeredPagePaths = new();

		private readonly HashSet<string> pathsOfPagesForGeneration = new();

		public required string OutputPath { get; init; }

		private bool saveToFile;

		public void LoadModificationData(string skeletonContent, bool forceRebuild = false)
		{
			byte[] actualSkeletonHash = GenerateHash(skeletonContent);
			//if skeleton is not changed when the hashes are cached
			if (cachedSkeletonHash is null)
			{
				cachedSkeletonHash = actualSkeletonHash;
				if (File.Exists(modificationCheckerFileName))
				{
					using FileStream fileStream = new(modificationCheckerFileName, FileMode.Open);
					using BinaryReader binaryReader = new(fileStream);
					if (binaryReader.ReadString() == modificationCheckerFileMagicNumber)
					{
						byte[] savedSkeletonHash = binaryReader.ReadFileHash();
						if (Enumerable.SequenceEqual(savedSkeletonHash, cachedSkeletonHash)) //If skeleton not changed
						{
							ReadRegisteredPages(binaryReader);
							ReadHashes(binaryReader);
							AddPathsOfPagesUsingCheckedResources();
						}
						else
							ClearCheckoutData(skeletonContent);//When skeleton is changed, entire website needs to be regenerated.
					}
					else
						ClearCheckoutData(skeletonContent);
				}
				else if (cachedSkeletonHash is not null)
					AddPathsOfPagesUsingCheckedResources();
			}
			else if (cachedSkeletonHash is not null)
			{
				if (cachedSkeletonHash.SequenceEqual(actualSkeletonHash) && !forceRebuild)
					AddPathsOfPagesUsingCheckedResources();
				else
					ClearCheckoutData(skeletonContent);
			}
		}

		private void ReadRegisteredPages(BinaryReader binaryReader)
		{
			int registeredPageNumber = binaryReader.ReadInt32();
			for (int i = 0; i < registeredPageNumber; i++)
			{
				string readPagePath = binaryReader.ReadString();
				registeredPagePaths.Add(readPagePath);
			}
		}

		private void ReadHashes(BinaryReader binaryReader)
		{
			int resourceHashNum = binaryReader.ReadInt32();
			for (int i = 0; i < resourceHashNum; i++)
			{
				string resourcePath = binaryReader.ReadString();
				byte[] resourceHash = binaryReader.ReadFileHash();
				int pageTemplatesUsingCurrentResourceNumber = binaryReader.ReadInt32();
				HashSet<string> pageUsedForResourcePaths = new();
				for (int j = 0; j < pageTemplatesUsingCurrentResourceNumber; j++)
				{
					string pagePath = binaryReader.ReadString();
					if (File.Exists(Path.Combine(OutputPath, pagePath)))
						pageUsedForResourcePaths.Add(pagePath);
				}
				resourceInfo.Add(resourcePath, new ResourceEntry()
				{
					Hash = resourceHash,
					PathsOfPagesUsingResource = pageUsedForResourcePaths
				});
			}
		}

		private void AddPathsOfPagesUsingCheckedResources()
		{
			List<string> keysToDelete = new();
			foreach (string resourcePath in resourceInfo.Keys)
			{
				ResourceEntry currentResourceEntry = resourceInfo[resourcePath];
				byte[] currentResourceHash = null;
				if (File.Exists(resourcePath)) // If non-page template resource mentioned in file still exists
					currentResourceHash = GenerateHash(File.ReadAllText(resourcePath));
				else
					keysToDelete.Add(resourcePath);
				if (currentResourceHash is null || !Enumerable.SequenceEqual(currentResourceHash, currentResourceEntry.Hash))
					//if non-page resource does not exist or it is changed
				{
					foreach (string pagePath in currentResourceEntry.PathsOfPagesUsingResource)
					{
						pathsOfPagesForGeneration.Add(pagePath);
						// HashSet does not have AddRange
					}
				}
			}
			if (keysToDelete.Count > 0)
			{
				foreach (string keyToDelete in keysToDelete)
					resourceInfo.Remove(keyToDelete);
				saveToFile = true;
			}
		}

		public void AddResourceToModificationChecking(string pagePath, string resourcePath, string resourceContent)
		{
			byte[] resourceHash = GenerateHash(resourceContent);

			pagePath = SetSlashesOnly(pagePath);
			resourcePath = SetSlashesOnly(resourcePath);
			if (resourceInfo.TryGetValue(resourcePath, out ResourceEntry entry))
				// If resource is registered in checker
			{
				entry.PathsOfPagesUsingResource.Add(pagePath);
				resourceInfo[resourcePath] = new()
				{
					Hash = resourceHash,
					PathsOfPagesUsingResource = entry.PathsOfPagesUsingResource
				};
			}
			else
			{
				resourceInfo.Add(resourcePath, new()
				{
					Hash = resourceHash,
					PathsOfPagesUsingResource = new() { pagePath }
				});
			}
			saveToFile = true;
		}

		/// <summary>
		/// Adds page template path to the list of page template paths when it is not modified.
		/// Method reads page template as modified when its hash is not saved in msmc file at all
		/// or its current hash and the one from the msmc file are not equal
		/// </summary>
		/// <param name="outputPageDest"></param>
		public bool CheckIfPageIsNotModified(string relativePagePath, string outputPageDest)
		{
			relativePagePath = SetSlashesOnly(relativePagePath);
			return pathsOfPagesForGeneration.Contains(relativePagePath) || !registeredPagePaths.Contains(relativePagePath) || !File.Exists(outputPageDest);
		}

		public void SaveLastWebsiteStateIfNeeded()
		{
			if (saveToFile)
			{
				using FileStream fileStream = new(modificationCheckerFileName, FileMode.Create);
				using BinaryWriter binaryWriter = new(fileStream);
				binaryWriter.Write(modificationCheckerFileMagicNumber);
				binaryWriter.Write(cachedSkeletonHash);
				binaryWriter.Write(registeredPagePaths.Count);
				foreach (string registeredPagePath in registeredPagePaths)
				{
					binaryWriter.Write(registeredPagePath);
				}
				binaryWriter.Write(resourceInfo.Count);
				foreach (KeyValuePair<string, ResourceEntry> dictionaryEntry in resourceInfo)
				{
					binaryWriter.Write(dictionaryEntry.Key);
					ResourceEntry entry = dictionaryEntry.Value;
					binaryWriter.Write(entry.Hash);
					binaryWriter.Write(entry.PathsOfPagesUsingResource.Count);
					foreach (string outputPagePath in entry.PathsOfPagesUsingResource)
					{
						binaryWriter.Write(outputPagePath);
					}
				}
				pathsOfPagesForGeneration.Clear();
				saveToFile = false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte[] GenerateHash(string fileContent) => MD5.HashData(Encoding.UTF8.GetBytes(fileContent));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterPageForChecking(string pagePath)
		{
			pagePath = SetSlashesOnly(pagePath);
			registeredPagePaths.Add(pagePath);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string SetSlashesOnly(string path) => path.Replace("\\", "/"); // I decided to replace with UNIX slashes, since they are supported even by modern Windows

		private void ClearCheckoutData(string skeletonContent)
		{
			registeredPagePaths.Clear();
			resourceInfo.Clear();
			pathsOfPagesForGeneration.Clear();
			cachedSkeletonHash = GenerateHash(skeletonContent);
		}
	}
	file static class BinaryReaderHelper
	{
		internal static byte[] ReadFileHash(this BinaryReader binaryReader)
		{
			byte[] hash = new byte[ModificationChecker.ShaHashLength];
			for (int i = 0; i < hash.Length; i++)
			{
				hash[i] = binaryReader.ReadByte();
			}
			return hash;
		}
	}
