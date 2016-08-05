﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Explorer.ViewModel;

namespace Ba2Explorer.Service
{
    public static class ArchiveFilePathService
    {
        private static List<string> m_names = new List<string>();

        public static void GetRoots(ObservableCollection<ArchiveFilePath> roots, ArchiveInfo archive)
        {
            roots.Clear();
            List<int> levelDirHashes = new List<int>();

            foreach (string path in archive.Archive.FileList)
            {
                SplitNames(path);
                if (m_names.Count < 1)
                    continue;
                var root = m_names[1];
                bool isFile = m_names.Count <= 2;
                int rootHash = root.GetHashCode();
                if (!isFile && levelDirHashes.Contains(rootHash))
                    continue; // don't add same directory twice
                roots.Add(new ArchiveFilePath()
                {
                    Path = root,
                    Type = isFile ? FilePathType.File : FilePathType.Directory
                });
                if (!isFile)
                    levelDirHashes.Add(root.GetHashCode());
            }
        }

        /// <summary>
        /// Returns children of specified `filePath `object which is located at `level` in file hierarchy.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="filePath"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static void GetRoots(ObservableCollection<ArchiveFilePath> roots, ArchiveInfo archive, ArchiveFilePath filePath, int level)
        {
            Contract.Assert(filePath.Type == FilePathType.Directory);
            roots.Clear();
            List<int> levelDirHashes = new List<int>();

            roots.Add(new ArchiveFilePath() { Type = FilePathType.GoBack, Path = "..." });

            foreach (string path in archive.Archive.FileList)
            {
                if (!CheckPathAtLevel(path, filePath.Path, level))
                    continue;
                SplitNames(path);
                if (m_names.Count < level + 1)
                    continue;
                var root = m_names[level + 1];
                bool isFile = m_names.Count <= level + 2;
                int rootHash = root.GetHashCode();
                if (!isFile && levelDirHashes.Contains(rootHash))
                    continue; // don't add same directory twice
                roots.Add(new ArchiveFilePath()
                {
                    Path = root,
                    Type = isFile ? FilePathType.File : FilePathType.Directory
                });
                if (!isFile)
                    levelDirHashes.Add(root.GetHashCode());
            }
        }

        private static bool CheckPathAtLevel(string path, string pathToCheck, int level)
        {
            int levelsPassed = 0;
            int pos = 0;
            if (level == 0)
                goto skipLevelDetection;
            foreach (char c in path)
            {
                if (c == '\\')
                {
                    ++levelsPassed;
                    if (levelsPassed == level)
                        break;
                }
                ++pos;
            }
            if (levelsPassed != level)
                return false;
            skipLevelDetection:
            int pcPos = 0;
            ++pos; // skip '\' char.
            while (pcPos < pathToCheck.Length)
            {
                if (path[pos] != pathToCheck[pcPos])
                    return false;
                ++pos;
                ++pcPos;
            }
            return true;
        }

        private static void SplitNames(string path)
        {
            m_names.Clear();

            int startIndex = 0;
            int length = 0;
            foreach (char c in path)
            {
                if (c == '\\')
                {
                    m_names.Add(path.Substring(startIndex, length));
                    startIndex += length + 1; // +1 to skip '\' char
                    length = 0;
                }
                else
                {
                    ++length;
                }
            }
            if (length != 0)
                m_names.Add(path.Substring(startIndex, length));
        }
    }
}